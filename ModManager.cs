using Narod.SteamGameFinder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AdofaiModManager
{
    public partial class ModManager : Form
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.xml");
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml");
        public static string OnlineModsUrl = "https://api.aloptys.top/AdofaiMods/ModList.json";

        private bool isPathLoading = false;

        public ModManager()
        {
            InitializeComponent();
            SetInitialLanguage();
            LoadLanguageFromSettings();
            Setting.LoadLanguage(Setting.CurrentLanguage);
            ModList.SelectedIndexChanged += ModList_SelectedIndexChanged;
            DisableModList.SelectedIndexChanged += DisableModList_SelectedIndexChanged;
            OnlineModsList.SelectedIndexChanged += OnlineModsList_SelectedIndexChanged;
            Select.SelectedIndexChanged += Select_SelectedIndexChanged;
            GamePath.TextChanged += GamePath_TextChanged;
            HideModInfo();
            InstallButton.Enabled = false;
            DisableButton.Enabled = false;
            OnlineInstall.Enabled = false;
            OnlineInstall.Visible = false;
            Loading.Visible = false;
            progressBarForOnline.Visible = false;
            Refresh.Visible = false;
            LoadConfig();
            LoadApiSettings();
            ApplyTranslation();
            AllowDrop = true;
            DragEnter += ModManager_DragEnter;
            DragDrop += ModManager_DragDrop;
        }

        private void SetInitialLanguage()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    var doc = XDocument.Load(SettingsPath);
                    var languageElement = doc.Element("Settings")?.Element("Language");
                    if (languageElement != null)
                    {
                        Setting.CurrentLanguage = languageElement.Value;
                        return;
                    }
                }
                catch { }
            }

            string systemLang = System.Globalization.CultureInfo.CurrentUICulture.Name.ToLower();
            if (systemLang.StartsWith("zh"))
                Setting.CurrentLanguage = "zh-cn";
            else
                Setting.CurrentLanguage = "en-us";
        }

        private void LoadLanguageFromSettings()
        {
            if (!File.Exists(SettingsPath)) return;

            try
            {
                var doc = XDocument.Load(SettingsPath);
                var root = doc.Element("Settings");
                if (root == null) return;

                var languageElement = root.Element("Language");
                if (languageElement != null)
                    Setting.CurrentLanguage = languageElement.Value;
            }
            catch { }
        }

        private void ApplyTranslation()
        {
            this.Text = Setting.T("ModManager");
            FindGame.Text = Setting.T("Find Game Folder");
            BrowersButton.Text = Setting.T("Browser");
            InstallButton.Text = Setting.T("Install");
            DisableButton.Text = Setting.T("Disable");
            Installed.Text = Setting.T("Installed");
            Disable.Text = Setting.T("Disabled");
            OnlineMods.Text = Setting.T("Online");
            ModName.Text = Setting.T("Name");
            ModVersion.Text = Setting.T("Version");
            DisableModName.Text = Setting.T("Name");
            DisableModVersion.Text = Setting.T("Version");
            OnlineModName.Text = Setting.T("Name");
            OnlineModVersion.Text = Setting.T("Version");
            OPENMODSFOLDER.Text = Setting.T("Open Mod Folder");
            ModHomePage.Text = Setting.T("HomePage");
            Refresh.Text = Setting.T("Refresh");
            Loading.Text = Setting.T("Loading");
            Option.Text = Setting.T("Option");
            settingToolStripMenuItem.Text = Setting.T("Setting");
            patchToolStripMenuItem.Text = Setting.T("Patch");
            installModManagerToolStripMenuItem.Text = Setting.T("Install Mod Manager");
            uninstallModManagerToolStripMenuItem.Text = Setting.T("Uninstall Mod Manager");
            AMMVersion.Text = "Adofai Mod Manager v0.0.3";
        }

        public string ModsPath = "";
        public string DisableModsPath = "";
        private List<ModInfo> modInfos = new List<ModInfo>();
        private List<ModInfo> disableModInfos = new List<ModInfo>();
        private List<OnlineModInfo> onlineModInfos = new List<OnlineModInfo>();

        private string GameManagedPath => Path.Combine(GamePath.Text, "A Dance of Fire and Ice_Data", "Managed", "UnityModManager");
        private string GameWinHttpPath => Path.Combine(GamePath.Text, "winhttp.dll");
        private string LocalModManagerPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModManager", "UnityModManager");
        private string LocalWinHttpPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ModManager", "winhttp.dll");

        private void HideModInfo()
        {
            ModTitleLabel.Visible = false;
            ModVersionLabel.Visible = false;
            ModAuthorLabel.Visible = false;
            ModHomePage.Visible = false;
            ModDescription.Visible = false;
            OPENMODSFOLDER.Visible = false;
            OnlineInstall.Visible = false;
        }

        private void ShowModInfo(ModInfo modInfo, bool isOnline = false)
        {
            ModTitleLabel.Visible = true;
            ModVersionLabel.Visible = true;
            ModAuthorLabel.Visible = true;
            ModDescription.Visible = true;

            ModTitleLabel.Text = modInfo.DisplayName;
            ModVersionLabel.Text = Setting.T("Version") + " " + modInfo.Version;
            ModAuthorLabel.Text = Setting.T("by") + " " + modInfo.Author;

            if (!isOnline)
            {
                ModHomePage.Visible = !string.IsNullOrEmpty(modInfo.HomePage);
                OPENMODSFOLDER.Visible = true;
                OnlineInstall.Visible = false;
                ModDescription.Visible = false;
            }
        }

        private void LoadApiSettings()
        {
            if (!File.Exists(SettingsPath)) return;

            try
            {
                var doc = XDocument.Load(SettingsPath);
                var root = doc.Element("Settings");
                if (root == null) return;

                var useDefaultElement = root.Element("UseDefaultApi");
                var apiUrlElement = root.Element("ApiUrl");

                bool useDefault = useDefaultElement != null && useDefaultElement.Value == "true";
                string apiUrl = apiUrlElement?.Value;

                if (!useDefault && !string.IsNullOrEmpty(apiUrl))
                    OnlineModsUrl = apiUrl;
            }
            catch { }
        }

        private void LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return;

            try
            {
                var doc = XDocument.Load(ConfigPath);
                var root = doc.Element("Config");
                if (root == null) return;

                var gamePathElement = root.Element("GamePath");
                if (gamePathElement != null && Directory.Exists(gamePathElement.Value))
                {
                    string gamePath = gamePathElement.Value;
                    isPathLoading = true;
                    GamePath.Text = gamePath;
                    isPathLoading = false;

                    ModsPath = Path.Combine(gamePath, "Mods");
                    DisableModsPath = Path.Combine(ModsPath, "DisableMods");
                    InstallButton.Enabled = true;
                    DisableButton.Enabled = true;
                    LoadModsList();
                    LoadDisableModsList();
                }
            }
            catch { }
        }

        private void SaveConfig()
        {
            try
            {
                var doc = new XDocument(
                    new XElement("Config",
                        new XElement("GamePath", GamePath.Text)
                    )
                );
                doc.Save(ConfigPath);
            }
            catch { }
        }

        private void GamePath_TextChanged(object sender, EventArgs e)
        {
            if (isPathLoading) return;

            string path = GamePath.Text.Trim();

            if (string.IsNullOrEmpty(path))
            {
                DisableModManagement();
                return;
            }

            if (Directory.Exists(path))
            {
                ModsPath = Path.Combine(path, "Mods");
                DisableModsPath = Path.Combine(ModsPath, "DisableMods");
                InstallButton.Enabled = true;
                DisableButton.Enabled = true;
                SaveConfig();
                LoadModsList();
                LoadDisableModsList();
            }
            else
            {
                InstallButton.Enabled = false;
                DisableButton.Enabled = false;
            }
        }

        private void DisableModManagement()
        {
            InstallButton.Enabled = false;
            DisableButton.Enabled = false;
            ModList.Items.Clear();
            modInfos.Clear();
            DisableModList.Items.Clear();
            disableModInfos.Clear();
            HideModInfo();
        }

        private void FindGame_Click(object sender, EventArgs e)
        {
            var steamGameLocator = new SteamGameLocator();

            try
            {
                string gameInstallDir = steamGameLocator.getGameInfoByFolder("A Dance of Fire and Ice").steamGameLocation;
                isPathLoading = true;
                GamePath.Text = gameInstallDir;
                isPathLoading = false;

                ModsPath = Path.Combine(gameInstallDir, "Mods");
                DisableModsPath = Path.Combine(ModsPath, "DisableMods");
                InstallButton.Enabled = true;
                DisableButton.Enabled = true;
                SaveConfig();
                LoadModsList();
                LoadDisableModsList();
            }
            catch
            {
                MessageBox.Show(Setting.T("Steam is not installed on this computer.") + "\n" + Setting.T("Please install Steam first."), Setting.T("Steam not found"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BrowersButton_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    isPathLoading = true;
                    GamePath.Text = selectedPath;
                    isPathLoading = false;

                    ModsPath = Path.Combine(selectedPath, "Mods");
                    DisableModsPath = Path.Combine(ModsPath, "DisableMods");
                    InstallButton.Enabled = true;
                    DisableButton.Enabled = true;
                    SaveConfig();
                    LoadModsList();
                    LoadDisableModsList();
                }
            }
        }

        private void LoadModsList()
        {
            ModList.Items.Clear();
            modInfos.Clear();

            ModsPath = Path.Combine(GamePath.Text, "Mods");
            DisableModsPath = Path.Combine(ModsPath, "DisableMods");

            if (!Directory.Exists(ModsPath))
            {
                Directory.CreateDirectory(ModsPath);
                return;
            }

            var ModsFolder = new DirectoryInfo(ModsPath);

            foreach (var mod in ModsFolder.GetDirectories())
            {
                if (mod.Name == "DisableMods") continue;

                string infoPath = Path.Combine(mod.FullName, "Info.json");
                string folderPath = mod.FullName;

                if (!File.Exists(infoPath))
                {
                    var item = new ListViewItem(new[] { mod.Name, Setting.T("Unknown") });
                    item.Tag = folderPath;
                    ModList.Items.Add(item);
                    continue;
                }

                try
                {
                    string jsonContent = File.ReadAllText(infoPath);
                    ModInfo modInfo = JsonConvert.DeserializeObject<ModInfo>(jsonContent);

                    if (modInfo != null)
                    {
                        var item = new ListViewItem(modInfo.DisplayName);
                        item.SubItems.Add(modInfo.Version);
                        item.Tag = folderPath;
                        modInfo.FolderPath = folderPath;
                        ModList.Items.Add(item);
                        modInfos.Add(modInfo);
                    }
                }
                catch
                {
                    var item = new ListViewItem(new[] { mod.Name, Setting.T("Unknown") });
                    item.Tag = folderPath;
                    ModList.Items.Add(item);
                }
            }
        }

        private void LoadDisableModsList()
        {
            DisableModList.Items.Clear();
            disableModInfos.Clear();

            if (!Directory.Exists(DisableModsPath)) return;

            var DisableModsFolder = new DirectoryInfo(DisableModsPath);

            foreach (var mod in DisableModsFolder.GetDirectories())
            {
                string infoPath = Path.Combine(mod.FullName, "Info.json");
                string folderPath = mod.FullName;

                if (!File.Exists(infoPath))
                {
                    var item = new ListViewItem(new[] { mod.Name, Setting.T("Unknown") });
                    item.Tag = folderPath;
                    DisableModList.Items.Add(item);
                    continue;
                }

                try
                {
                    string jsonContent = File.ReadAllText(infoPath);
                    ModInfo modInfo = JsonConvert.DeserializeObject<ModInfo>(jsonContent);

                    if (modInfo != null)
                    {
                        var item = new ListViewItem(modInfo.DisplayName);
                        item.SubItems.Add(modInfo.Version);
                        item.Tag = folderPath;
                        modInfo.FolderPath = folderPath;
                        DisableModList.Items.Add(item);
                        disableModInfos.Add(modInfo);
                    }
                }
                catch
                {
                    var item = new ListViewItem(new[] { mod.Name, Setting.T("Unknown") });
                    item.Tag = folderPath;
                    DisableModList.Items.Add(item);
                }
            }
        }

        private async void LoadOnlineModsList()
        {
            OnlineModsList.Items.Clear();
            onlineModInfos.Clear();

            Loading.Visible = true;
            progressBarForOnline.Visible = true;
            progressBarForOnline.Style = ProgressBarStyle.Marquee;

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "AdofaiModManager");
                    string jsonContent = await client.DownloadStringTaskAsync(OnlineModsUrl);
                    var onlineMods = JsonConvert.DeserializeObject<List<OnlineModInfoRaw>>(jsonContent);

                    if (onlineMods != null)
                    {
                        var latestMods = new Dictionary<string, OnlineModInfoRaw>();

                        foreach (var mod in onlineMods)
                        {
                            if (string.IsNullOrEmpty(mod.name) || string.IsNullOrEmpty(mod.version)) continue;

                            string key = mod.name.ToLower();

                            if (!latestMods.ContainsKey(key) || CompareVersions(mod.version, latestMods[key].version) > 0)
                                latestMods[key] = mod;
                        }

                        foreach (var kvp in latestMods)
                        {
                            var mod = kvp.Value;
                            string displayName = mod.name;
                            string version = mod.version;
                            string author = mod.cachedUsername ?? Setting.T("Unknown");
                            string downloadUrl = !string.IsNullOrEmpty(mod.parsedDownload) ? mod.parsedDownload : mod.download;

                            var onlineModInfo = new OnlineModInfo
                            {
                                DisplayName = displayName,
                                Version = version,
                                Author = author,
                                DownloadUrl = downloadUrl
                            };

                            var item = new ListViewItem(displayName);
                            item.SubItems.Add(version);
                            item.Tag = onlineModInfo;
                            OnlineModsList.Items.Add(item);
                            onlineModInfos.Add(onlineModInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Setting.T("Failed to load online mods:") + " " + ex.Message, Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Loading.Visible = false;
                progressBarForOnline.Visible = false;
                progressBarForOnline.Style = ProgressBarStyle.Blocks;
            }
        }

        private int CompareVersions(string v1, string v2)
        {
            try
            {
                var parts1 = v1.Split('.');
                var parts2 = v2.Split('.');

                int maxLength = Math.Max(parts1.Length, parts2.Length);

                for (int i = 0; i < maxLength; i++)
                {
                    int num1 = i < parts1.Length && int.TryParse(parts1[i], out int p1) ? p1 : 0;
                    int num2 = i < parts2.Length && int.TryParse(parts2[i], out int p2) ? p2 : 0;

                    if (num1 > num2) return 1;
                    if (num1 < num2) return -1;
                }

                return 0;
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
            }
        }

        private ModInfo GetInstalledModByDisplayName(string displayName)
        {
            foreach (var mod in modInfos)
            {
                if (mod.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    return mod;
            }
            return null;
        }

        private void UpdateOnlineInstallButtonState()
        {
            if (Select.SelectedTab != OnlineMods || OnlineModsList.SelectedItems.Count == 0)
            {
                OnlineInstall.Visible = false;
                OnlineInstall.Enabled = false;
                return;
            }

            var selectedItem = OnlineModsList.SelectedItems[0];
            if (!(selectedItem.Tag is OnlineModInfo onlineModInfo))
            {
                OnlineInstall.Visible = false;
                OnlineInstall.Enabled = false;
                return;
            }

            var installedMod = GetInstalledModByDisplayName(onlineModInfo.DisplayName);

            if (installedMod != null)
            {
                if (CompareVersions(installedMod.Version, onlineModInfo.Version) >= 0)
                {
                    OnlineInstall.Text = Setting.T("Installed");
                    OnlineInstall.Enabled = false;
                }
                else
                {
                    OnlineInstall.Text = Setting.T("Update");
                    OnlineInstall.Enabled = true;
                }
                OnlineInstall.Visible = true;
            }
            else
            {
                OnlineInstall.Text = Setting.T("Install");
                OnlineInstall.Enabled = true;
                OnlineInstall.Visible = true;
            }
        }

        private void Select_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideModInfo();
            DisableButton.Enabled = false;
            OnlineInstall.Visible = false;
            OnlineInstall.Enabled = false;

            if (Select.SelectedTab == Installed)
            {
                DisableButton.Text = Setting.T("Disable");
                DisableButton.Enabled = ModList.SelectedItems.Count > 0;
                DisableButton.Visible = true;
                InstallButton.Visible = true;
                Refresh.Visible = false;
            }
            else if (Select.SelectedTab == Disable)
            {
                DisableButton.Text = Setting.T("Enable");
                DisableButton.Enabled = DisableModList.SelectedItems.Count > 0;
                DisableButton.Visible = true;
                InstallButton.Visible = true;
                Refresh.Visible = false;
            }
            else if (Select.SelectedTab == OnlineMods)
            {
                DisableButton.Visible = false;
                InstallButton.Visible = false;
                Refresh.Visible = true;

                if (OnlineModsList.Items.Count == 0)
                    LoadOnlineModsList();

                UpdateOnlineInstallButtonState();
            }
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Select.SelectedTab == Installed)
                DisableButton.Enabled = ModList.SelectedItems.Count > 0;

            if (ModList.SelectedItems.Count == 0)
            {
                if (Select.SelectedTab == Installed) HideModInfo();
                return;
            }

            var selectedItem = ModList.SelectedItems[0];

            if (selectedItem.Tag is string folderPath && modInfos.Exists(m => m.FolderPath == folderPath))
            {
                var modInfo = modInfos.Find(m => m.FolderPath == folderPath);
                ShowModInfo(modInfo);
            }
            else
            {
                ShowBasicModInfo(selectedItem.Text, selectedItem.SubItems[1].Text);
            }
        }

        private void DisableModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Select.SelectedTab == Disable)
                DisableButton.Enabled = DisableModList.SelectedItems.Count > 0;

            if (DisableModList.SelectedItems.Count == 0)
            {
                if (Select.SelectedTab == Disable) HideModInfo();
                return;
            }

            var selectedItem = DisableModList.SelectedItems[0];

            if (selectedItem.Tag is string folderPath && disableModInfos.Exists(m => m.FolderPath == folderPath))
            {
                var modInfo = disableModInfos.Find(m => m.FolderPath == folderPath);
                ShowModInfo(modInfo);
            }
            else
            {
                ShowBasicModInfo(selectedItem.Text, selectedItem.SubItems[1].Text);
            }
        }

        private void OnlineModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnlineModsList.SelectedItems.Count == 0)
            {
                if (Select.SelectedTab == OnlineMods) HideModInfo();
                UpdateOnlineInstallButtonState();
                return;
            }

            var selectedItem = OnlineModsList.SelectedItems[0];

            if (selectedItem.Tag is OnlineModInfo onlineModInfo)
            {
                var tempModInfo = new ModInfo
                {
                    DisplayName = onlineModInfo.DisplayName,
                    Version = onlineModInfo.Version,
                    Author = onlineModInfo.Author,
                    HomePage = ""
                };
                ShowModInfo(tempModInfo, true);
                UpdateOnlineInstallButtonState();
            }
        }

        private void ShowBasicModInfo(string name, string version)
        {
            ModTitleLabel.Visible = true;
            ModVersionLabel.Visible = true;
            ModAuthorLabel.Visible = false;
            ModHomePage.Visible = false;
            OPENMODSFOLDER.Visible = true;
            OnlineInstall.Visible = false;

            ModTitleLabel.Text = name;
            ModVersionLabel.Text = version;
        }

        private void OPENMODSFOLDER_Click(object sender, EventArgs e)
        {
            string folderPath = null;

            if (Select.SelectedTab == Installed && ModList.SelectedItems.Count > 0)
                folderPath = ModList.SelectedItems[0].Tag as string;
            else if (Select.SelectedTab == Disable && DisableModList.SelectedItems.Count > 0)
                folderPath = DisableModList.SelectedItems[0].Tag as string;

            if (folderPath != null && Directory.Exists(folderPath))
            {
                try
                {
                    System.Diagnostics.Process.Start(folderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Setting.T("Failed to open Mods folder:") + " " + ex.Message, Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ModHomePage_Click(object sender, EventArgs e)
        {
            string folderPath = null;
            List<ModInfo> currentList = null;

            if (Select.SelectedTab == Installed && ModList.SelectedItems.Count > 0)
            {
                folderPath = ModList.SelectedItems[0].Tag as string;
                currentList = modInfos;
            }
            else if (Select.SelectedTab == Disable && DisableModList.SelectedItems.Count > 0)
            {
                folderPath = DisableModList.SelectedItems[0].Tag as string;
                currentList = disableModInfos;
            }

            if (!string.IsNullOrEmpty(folderPath) && currentList != null)
            {
                var modInfo = currentList.Find(m => m.FolderPath == folderPath);
                if (modInfo != null && !string.IsNullOrEmpty(modInfo.HomePage))
                    System.Diagnostics.Process.Start(modInfo.HomePage);
            }
        }

        public class ModInfo
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Author { get; set; }
            public string Version { get; set; }
            public string ManagerVersion { get; set; }
            public string AssemblyName { get; set; }
            public string EntryMethod { get; set; }
            public string HomePage { get; set; }
            public string Repository { get; set; }
            public string FolderPath { get; set; }
        }

        public class OnlineModInfo
        {
            public string DisplayName { get; set; }
            public string Version { get; set; }
            public string Author { get; set; }
            public string DownloadUrl { get; set; }
        }

        public class OnlineModInfoRaw
        {
            public string name { get; set; }
            public string version { get; set; }
            public string download { get; set; }
            public string parsedDownload { get; set; }
            public string cachedUsername { get; set; }
        }

        private void ModManager_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1 && Path.GetExtension(files[0]).ToLower() == ".zip")
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void ModManager_DragDrop(object sender, DragEventArgs e)
        {
            if (!InstallButton.Enabled)
            {
                MessageBox.Show(Setting.T("Please locate the game folder first."), Setting.T("Game not found"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            string zipPath = files[0];

            var result = MessageBox.Show(Setting.T("Install this Mod?") + "\n\n" + Path.GetFileName(zipPath), Setting.T("Install Mod"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            InstallModFromZip(zipPath);
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = Setting.T("ZIP files") + " (*.zip)|*.zip";
                openFileDialog.Title = Setting.T("Select Mod ZIP File");

                if (openFileDialog.ShowDialog() != DialogResult.OK) return;

                InstallModFromZip(openFileDialog.FileName);
            }
        }

        private void InstallModFromZip(string zipPath)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                ZipFile.ExtractToDirectory(zipPath, tempPath);

                string infoJsonPath = FindInfoJson(tempPath);

                if (infoJsonPath == null)
                {
                    MessageBox.Show(Setting.T("Invalid Mod File!") + "\n" + Setting.T("Info.json not found."), Setting.T("Install Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Directory.Delete(tempPath, true);
                    return;
                }

                string modFolderPath = Path.GetDirectoryName(infoJsonPath);
                string modInfoJson = File.ReadAllText(infoJsonPath);
                ModInfo modInfo = JsonConvert.DeserializeObject<ModInfo>(modInfoJson);

                if (modInfo == null || string.IsNullOrEmpty(modInfo.Id))
                {
                    MessageBox.Show(Setting.T("Invalid Info.json!") + "\n" + Setting.T("Missing Mod Id."), Setting.T("Install Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Directory.Delete(tempPath, true);
                    return;
                }

                string targetPath = Path.Combine(ModsPath, modInfo.Id);

                if (Directory.Exists(targetPath))
                {
                    var overwriteResult = MessageBox.Show(Setting.T("Mod already exists. Overwrite?"), Setting.T("Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (overwriteResult == DialogResult.No)
                    {
                        Directory.Delete(tempPath, true);
                        return;
                    }
                    Directory.Delete(targetPath, true);
                }

                CopyDirectory(modFolderPath, targetPath);
                Directory.Delete(tempPath, true);

                LoadModsList();

                if (Select.SelectedTab == OnlineMods)
                    UpdateOnlineInstallButtonState();
                else
                    MessageBox.Show(Setting.T("Install complete!"), Setting.T("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Setting.T("Failed to install Mod:") + " " + ex.Message, Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }

        private string FindInfoJson(string directory)
        {
            string directPath = Path.Combine(directory, "Info.json");
            if (File.Exists(directPath)) return directPath;

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                string result = FindInfoJson(subDir);
                if (result != null) return result;
            }

            return null;
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(targetDir, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        private void DisableButton_Click(object sender, EventArgs e)
        {
            if (Select.SelectedTab == Installed)
                MoveModToDisabled();
            else if (Select.SelectedTab == Disable)
                MoveModToEnabled();
        }

        private void MoveModToDisabled()
        {
            if (ModList.SelectedItems.Count == 0) return;

            string folderPath = ModList.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;

            string folderName = Path.GetFileName(folderPath);

            if (!Directory.Exists(DisableModsPath))
                Directory.CreateDirectory(DisableModsPath);

            string targetPath = Path.Combine(DisableModsPath, folderName);

            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);

            Directory.Move(folderPath, targetPath);

            RefreshModLists();
        }

        private void MoveModToEnabled()
        {
            if (DisableModList.SelectedItems.Count == 0) return;

            string folderPath = DisableModList.SelectedItems[0].Tag as string;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;

            string folderName = Path.GetFileName(folderPath);
            string targetPath = Path.Combine(ModsPath, folderName);

            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);

            Directory.Move(folderPath, targetPath);

            RefreshModLists();
        }

        private void RefreshModLists()
        {
            LoadModsList();
            LoadDisableModsList();
            HideModInfo();
        }

        private async void OnlineInstall_Click(object sender, EventArgs e)
        {
            if (OnlineModsList.SelectedItems.Count == 0) return;

            var selectedItem = OnlineModsList.SelectedItems[0];
            if (!(selectedItem.Tag is OnlineModInfo onlineModInfo)) return;

            string downloadUrl = onlineModInfo.DownloadUrl;
            if (string.IsNullOrEmpty(downloadUrl))
            {
                MessageBox.Show(Setting.T("No download URL available."), Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!InstallButton.Enabled)
            {
                MessageBox.Show(Setting.T("Please locate the game folder first."), Setting.T("Game not found"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Loading.Visible = true;
            progressBarForOnline.Visible = true;
            progressBarForOnline.Style = ProgressBarStyle.Marquee;
            OnlineInstall.Enabled = false;

            string tempZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");

            try
            {
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempZipPath);
                }

                InstallModFromZip(tempZipPath);

                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Setting.T("Failed to download Mod:") + " " + ex.Message, Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }
            finally
            {
                Loading.Visible = false;
                progressBarForOnline.Visible = false;
                progressBarForOnline.Style = ProgressBarStyle.Blocks;
            }
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var settingForm = new Setting())
            {
                settingForm.ShowDialog();
                ApplyTranslation();
                if (!string.IsNullOrEmpty(GamePath.Text) && Directory.Exists(GamePath.Text))
                {
                    LoadModsList();
                    LoadDisableModsList();
                    if (OnlineModsList.Items.Count > 0)
                    {
                        OnlineModsList.Items.Clear();
                        onlineModInfos.Clear();
                        if (Select.SelectedTab == OnlineMods)
                            LoadOnlineModsList();
                    }
                }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            LoadOnlineModsList();
        }

        private bool isOK2InstallModManager()
        {
            if (string.IsNullOrEmpty(GamePath.Text) || !Directory.Exists(GamePath.Text))
                return false;

            bool hasModManagerFolder = Directory.Exists(LocalModManagerPath)
                && Directory.GetFiles(LocalModManagerPath, "*", SearchOption.AllDirectories).Length > 0;

            bool hasWinHttpDll = File.Exists(LocalWinHttpPath);

            if (!hasModManagerFolder && !hasWinHttpDll)
                return false;

            bool NeedModManager = !Directory.Exists(GameManagedPath);
            bool NeedWinHttp = !File.Exists(GameWinHttpPath);

            return (hasModManagerFolder && NeedModManager) || (hasWinHttpDll && NeedWinHttp);
        }

        private void InstallModManager()
        {
            try
            {
                bool installed = false;
                int copiedCount = 0;
                var failedFiles = new List<string>();

                bool NeedModManager = !Directory.Exists(GameManagedPath);
                bool NeedWinHttp = !File.Exists(GameWinHttpPath);

                if (NeedModManager && Directory.Exists(LocalModManagerPath))
                {
                    var allFiles = Directory.GetFiles(LocalModManagerPath, "*", SearchOption.AllDirectories);
                    if (allFiles.Length > 0)
                    {
                        if (!Directory.Exists(GameManagedPath))
                            Directory.CreateDirectory(GameManagedPath);

                        if (!Directory.Exists(ModsPath))
                            Directory.CreateDirectory(ModsPath);

                        foreach (var file in allFiles)
                        {
                            string relativePath = file.Substring(LocalModManagerPath.Length + 1);
                            string destFile = Path.Combine(GameManagedPath, relativePath);
                            string destDir = Path.GetDirectoryName(destFile);

                            if (!Directory.Exists(destDir))
                                Directory.CreateDirectory(destDir);

                            try
                            {
                                File.Copy(file, destFile, true);
                                copiedCount++;
                                installed = true;
                            }
                            catch
                            {
                                failedFiles.Add(relativePath);
                            }
                        }
                    }
                }

                if (NeedWinHttp && File.Exists(LocalWinHttpPath))
                {
                    try
                    {
                        File.Copy(LocalWinHttpPath, GameWinHttpPath, true);
                        installed = true;
                    }
                    catch
                    {
                        failedFiles.Add("winhttp.dll");
                    }
                }

                if (!installed)
                {
                    MessageBox.Show(Setting.T("Nothing to install. All components already exist."), Setting.T("Info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (failedFiles.Count > 0)
                {
                    MessageBox.Show(
                        Setting.T("Installed files successfully.") + " " + copiedCount + "\n" + Setting.T("Failed to copy:") + " " + string.Join(", ", failedFiles),
                        Setting.T("Install Warning"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(Setting.T("Mod Manager installed successfully!"), Setting.T("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Setting.T("Failed to install Mod Manager:") + " " + ex.Message, Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void installModManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GamePath.Text) || !Directory.Exists(GamePath.Text))
            {
                MessageBox.Show(Setting.T("Please locate the game folder first."), Setting.T("Game not found"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (isOK2InstallModManager())
                InstallModManager();
            else
                MessageBox.Show(Setting.T("Mod Manager is already installed or the ModManager folder is missing."), Setting.T("Install Failed"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void uninstallModManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isOK2InstallModManager())
            {
                if (File.Exists(GameWinHttpPath))
                {
                    try
                    {
                        File.Delete(GameWinHttpPath);
                        MessageBox.Show(Setting.T("Mod Manager uninstalled successfully!"), Setting.T("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch { }
                }
            }
        }
    }
}