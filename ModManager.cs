using Narod.SteamGameFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdofaiModManager
{
    public partial class ModManager : Form
    {
        private readonly ModService _modService;
        private bool _isPathLoading;
        private List<ModInfo> _modInfos = new List<ModInfo>();
        private List<ModInfo> _disableModInfos = new List<ModInfo>();
        private List<OnlineModInfo> _onlineModInfos = new List<OnlineModInfo>();
        private Panel _dragOverlay;
        private Label _emptyLabelInstalled;
        private Label _emptyLabelDisabled;
        private Label _emptyLabelOnline;
        private ToolTip _gamePathTooltip;
        private int _sortColumn = -1;
        private bool _sortAscending = true;

        private string ModsPath => string.IsNullOrEmpty(GamePath.Text) ? "" : Path.Combine(GamePath.Text, AppConstants.ModsFolder);
        private string DisableModsPath => string.IsNullOrEmpty(ModsPath) ? "" : Path.Combine(ModsPath, AppConstants.DisableModsFolder);

        public ModManager()
        {
            InitializeComponent();
            _modService = new ModService();
            SetInitialLanguage();
            LoadLanguageFromSettings();
            Setting.LoadLanguage(Setting.CurrentLanguage);
            CreateDynamicControls();
            HookEvents();
            InitializeState();
            LoadConfiguration();
            ApplyTranslation();
        }

        private void CreateDynamicControls()
        {
            _dragOverlay = new Panel { BackColor = Color.FromArgb(140, 64, 128, 255), Visible = false, Dock = DockStyle.Fill };
            var dragLabel = new Label { Text = "\ud83d\udce6  Drop Mod ZIP files here", ForeColor = Color.White, Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold), AutoSize = true };
            dragLabel.Location = new Point((this.ClientSize.Width - dragLabel.PreferredWidth) / 2, (this.ClientSize.Height - dragLabel.PreferredHeight) / 2);
            _dragOverlay.Controls.Add(dragLabel);
            _dragOverlay.Resize += (s, e) => { dragLabel.Location = new Point((_dragOverlay.Width - dragLabel.PreferredWidth) / 2, (_dragOverlay.Height - dragLabel.PreferredHeight) / 2); };
            this.Controls.Add(_dragOverlay);
            _dragOverlay.BringToFront();
            _emptyLabelInstalled = CreateEmptyLabel(ModList);
            _emptyLabelDisabled = CreateEmptyLabel(DisableModList);
            _emptyLabelOnline = CreateEmptyLabel(OnlineModsList);
            _gamePathTooltip = new ToolTip { AutoPopDelay = 10000, InitialDelay = 500 };
            GamePath.MouseHover += (s, e) => { if (!string.IsNullOrEmpty(GamePath.Text) && GamePath.Text.Length > 50) _gamePathTooltip.SetToolTip(GamePath, GamePath.Text); };
            ModList.ColumnClick += ListView_ColumnClick;
            DisableModList.ColumnClick += ListView_ColumnClick;
        }

        private static Label CreateEmptyLabel(Control parent)
        {
            var label = new Label { TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Gray, Font = new Font("Microsoft YaHei UI", 10F), Visible = false, AutoSize = false };
            parent.Controls.Add(label);
            parent.Resize += (s, e) => { label.Size = parent.ClientSize; label.Location = Point.Empty; };
            label.Size = parent.ClientSize; label.Location = Point.Empty; label.BringToFront();
            return label;
        }

        private void HookEvents()
        {
            ModList.SelectedIndexChanged += ModList_SelectedIndexChanged;
            DisableModList.SelectedIndexChanged += DisableModList_SelectedIndexChanged;
            OnlineModsList.SelectedIndexChanged += OnlineModsList_SelectedIndexChanged;
            Select.SelectedIndexChanged += Select_SelectedIndexChanged;
            GamePath.TextChanged += GamePath_TextChanged;
            AllowDrop = true;
            DragEnter += ModManager_DragEnter;
            DragDrop += ModManager_DragDrop;
            DragLeave += ModManager_DragLeave;
        }

        private void InitializeState()
        {
            HideModInfo();
            InstallButton.Enabled = false;
            DisableButton.Enabled = false;
            OnlineInstall.Visible = false;
            OnlineInstall.Enabled = false;
            Loading.Visible = false;
            progressBarForOnline.Visible = false;
            Refresh.Visible = false;
            UpdateAllEmptyStates();
        }

        private void LoadConfiguration()
        {
            string savedPath = _modService.TryLoadGamePath();
            if (!string.IsNullOrEmpty(savedPath)) { _isPathLoading = true; GamePath.Text = savedPath; _isPathLoading = false; InstallButton.Enabled = true; DisableButton.Enabled = true; RefreshModLists(); }
            _modService.LoadApiSettings();
        }

        private void SetInitialLanguage()
        {
            try
            {
                if (File.Exists(_modService.SettingsPath))
                {
                    var doc = System.Xml.Linq.XDocument.Load(_modService.SettingsPath);
                    var e = doc.Element(AppConstants.XmlSettingsRoot)?.Element(AppConstants.XmlLanguage);
                    if (e != null) { Setting.CurrentLanguage = e.Value; return; }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[ModManager] Failed to load initial language: {ex.Message}"); }
            string sl = System.Globalization.CultureInfo.CurrentUICulture.Name.ToLower();
            Setting.CurrentLanguage = sl.StartsWith("zh") ? AppConstants.LangChinese : AppConstants.LangEnglish;
        }

        private void LoadLanguageFromSettings()
        {
            if (!File.Exists(_modService.SettingsPath)) return;
            try
            {
                var doc = System.Xml.Linq.XDocument.Load(_modService.SettingsPath);
                var e = doc.Element(AppConstants.XmlSettingsRoot)?.Element(AppConstants.XmlLanguage);
                if (e != null) Setting.CurrentLanguage = e.Value;
            }
            catch (Exception ex) { Debug.WriteLine($"[ModManager] Failed to load language: {ex.Message}"); }
        }

        private void ApplyTranslation()
        {
            this.Text = $"{Setting.T("ModManager")} \u2014 v0.0.3";
            FindGame.Text = Setting.T("Find Game Folder");
            BrowersButton.Text = Setting.T("Browser");
            InstallButton.Text = Setting.T("Install");
            DisableButton.Text = Setting.T("Disable");
            Installed.Text = Setting.T("Installed");
            Disable.Text = Setting.T("Disabled");
            OnlineMods.Text = Setting.T("Online");
            ModName.Text = Setting.T("Name"); ModVersion.Text = Setting.T("Version");
            DisableModName.Text = Setting.T("Name"); DisableModVersion.Text = Setting.T("Version");
            OnlineModName.Text = Setting.T("Name"); OnlineModVersion.Text = Setting.T("Version");
            OPENMODSFOLDER.Text = Setting.T("Open Mod Folder");
            ModHomePage.Text = Setting.T("HomePage");
            Refresh.Text = Setting.T("Refresh");
            Loading.Text = Setting.T("Loading");
            Option.Text = Setting.T("Option");
            settingToolStripMenuItem.Text = Setting.T("Setting");
            patchToolStripMenuItem.Text = Setting.T("Tools");
            installModManagerToolStripMenuItem.Text = Setting.T("Install Mod Manager");
            uninstallModManagerToolStripMenuItem.Text = Setting.T("Uninstall Mod Manager");
            AMMVersion.Text = AppConstants.AppVersion;
            UpdateAllEmptyStates();
        }

        private void GamePath_TextChanged(object sender, EventArgs e)
        {
            if (_isPathLoading) return;
            string p = GamePath.Text.Trim();
            if (string.IsNullOrEmpty(p)) { DisableModManagement(); return; }
            if (Directory.Exists(p)) { InstallButton.Enabled = true; DisableButton.Enabled = true; _modService.SaveGamePath(p); RefreshModLists(); }
            else { InstallButton.Enabled = false; DisableButton.Enabled = false; }
        }

        private void DisableModManagement()
        {
            InstallButton.Enabled = false; DisableButton.Enabled = false;
            ModList.Items.Clear(); _modInfos.Clear();
            DisableModList.Items.Clear(); _disableModInfos.Clear();
            HideModInfo(); UpdateAllEmptyStates(); UpdateTabBadges();
        }

        private void FindGame_Click(object sender, EventArgs e)
        {
            var locator = new SteamGameLocator();
            try
            {
                string dir = locator.getGameInfoByFolder(AppConstants.GameFolderName).steamGameLocation;
                _isPathLoading = true; GamePath.Text = dir; _isPathLoading = false;
                InstallButton.Enabled = true; DisableButton.Enabled = true;
                _modService.SaveGamePath(dir); RefreshModLists();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ModManager] Steam locate failed: {ex.Message}");
                MessageBox.Show($"{Setting.T("Steam is not installed on this computer.")}\n{Setting.T("Please install Steam first.")}", Setting.T("Steam not found"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BrowersButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _isPathLoading = true; GamePath.Text = dlg.SelectedPath; _isPathLoading = false;
                    InstallButton.Enabled = true; DisableButton.Enabled = true;
                    _modService.SaveGamePath(dlg.SelectedPath); RefreshModLists();
                }
            }
        }

        private void RefreshModLists() { LoadModsList(); LoadDisableModsList(); HideModInfo(); UpdateAllEmptyStates(); UpdateTabBadges(); }

        private void LoadModsList()
        {
            ModList.Items.Clear(); _modInfos.Clear();
            _modInfos = _modService.LoadModsFromFolder(ModsPath);
            PopulateListView(ModList, _modInfos);
            ApplySort(ModList, _sortColumn, _sortAscending);
            UpdateTabBadges();
            _emptyLabelInstalled.Visible = _modInfos.Count == 0;
        }

        private void LoadDisableModsList()
        {
            DisableModList.Items.Clear(); _disableModInfos.Clear();
            if (!Directory.Exists(DisableModsPath)) { UpdateTabBadges(); _emptyLabelDisabled.Visible = true; return; }
            _disableModInfos = _modService.LoadModsFromFolder(DisableModsPath, skipDisableModsFolder: false);
            PopulateListView(DisableModList, _disableModInfos);
            ApplySort(DisableModList, _sortColumn, _sortAscending);
            UpdateTabBadges();
            _emptyLabelDisabled.Visible = _disableModInfos.Count == 0;
        }

        private static void PopulateListView(ListView lv, List<ModInfo> mods)
        {
            foreach (var m in mods) { var item = new ListViewItem(m.DisplayName); item.SubItems.Add(m.Version); item.Tag = m.FolderPath; lv.Items.Add(item); }
        }

        private void UpdateTabBadges()
        {
            Installed.Text = _modInfos.Count > 0 ? $"{Setting.T("Installed")} ({_modInfos.Count})" : Setting.T("Installed");
            Disable.Text = _disableModInfos.Count > 0 ? $"{Setting.T("Disabled")} ({_disableModInfos.Count})" : Setting.T("Disabled");
        }

        private void UpdateAllEmptyStates()
        {
            if (_emptyLabelInstalled == null) return;
            _emptyLabelInstalled.Text = Setting.T("No Mods installed.\nDrag a ZIP file here or click \"Install\" to get started.");
            _emptyLabelInstalled.Visible = ModList.Items.Count == 0;
            _emptyLabelDisabled.Text = Setting.T("No disabled Mods.");
            _emptyLabelDisabled.Visible = DisableModList.Items.Count == 0 && Directory.Exists(DisableModsPath);
            _emptyLabelOnline.Text = Setting.T("No online Mods loaded.\nClick Refresh to fetch the list.");
            _emptyLabelOnline.Visible = OnlineModsList.Items.Count == 0;
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var lv = sender as ListView; if (lv == null) return;
            if (_sortColumn == e.Column) _sortAscending = !_sortAscending; else { _sortColumn = e.Column; _sortAscending = true; }
            ApplySort(lv, _sortColumn, _sortAscending);
        }

        private static void ApplySort(ListView lv, int col, bool asc)
        {
            if (col < 0) return;
            lv.ListViewItemSorter = new ListViewItemComparer(col, asc); lv.Sort();
        }

        private class ListViewItemComparer : System.Collections.IComparer
        {
            private readonly int _col; private readonly bool _asc;
            public ListViewItemComparer(int col, bool asc) { _col = col; _asc = asc; }
            public int Compare(object x, object y)
            {
                var a = (ListViewItem)x; var b = (ListViewItem)y;
                string ta = _col < a.SubItems.Count ? a.SubItems[_col].Text : "";
                string tb = _col < b.SubItems.Count ? b.SubItems[_col].Text : "";
                if (decimal.TryParse(ta, out var na) && decimal.TryParse(tb, out var nb)) { int r = na.CompareTo(nb); return _asc ? r : -r; }
                int sr = string.Compare(ta, tb, StringComparison.CurrentCultureIgnoreCase); return _asc ? sr : -sr;
            }
        }

        private async void LoadOnlineModsList()
        {
            OnlineModsList.Items.Clear(); _onlineModInfos.Clear();
            Loading.Text = Setting.T("Fetching online Mod list\u2026"); Loading.Visible = true; progressBarForOnline.Visible = true; progressBarForOnline.Style = ProgressBarStyle.Marquee;
            try
            {
                _onlineModInfos = await _modService.LoadOnlineModsAsync();
                foreach (var mod in _onlineModInfos) { var item = new ListViewItem(mod.DisplayName); item.SubItems.Add(mod.Version); item.Tag = mod; OnlineModsList.Items.Add(item); }
                _emptyLabelOnline.Visible = OnlineModsList.Items.Count == 0;
            }
            catch (Exception ex) { _emptyLabelOnline.Text = $"\u26a0 {Setting.T("Failed to load online mods:")}\n{ex.Message}"; _emptyLabelOnline.Visible = true; }
            finally { Loading.Visible = false; progressBarForOnline.Visible = false; progressBarForOnline.Style = ProgressBarStyle.Blocks; }
        }

        private void HideModInfo() { ModTitleLabel.Visible = false; ModVersionLabel.Visible = false; ModAuthorLabel.Visible = false; ModHomePage.Visible = false; ModDescription.Visible = false; OPENMODSFOLDER.Visible = false; OnlineInstall.Visible = false; }

        private void ShowModInfo(ModInfo modInfo, bool isOnline = false)
        {
            ModTitleLabel.Visible = true; ModTitleLabel.ForeColor = SystemColors.ControlText; ModVersionLabel.Visible = true; ModAuthorLabel.Visible = true; ModDescription.Visible = !isOnline;
            ModTitleLabel.Text = modInfo.DisplayName; ModVersionLabel.Text = $"{Setting.T("Version")} {modInfo.Version}"; ModAuthorLabel.Text = $"{Setting.T("by")} {modInfo.Author}";
            if (!isOnline) { ModHomePage.Visible = !string.IsNullOrEmpty(modInfo.HomePage); OPENMODSFOLDER.Visible = true; OnlineInstall.Visible = false; ModDescription.Text = modInfo.Description ?? ""; ModDescription.Visible = !string.IsNullOrEmpty(modInfo.Description); }
        }

        private void ShowBasicModInfo(string name, string version)
        {
            ModTitleLabel.Visible = true; ModTitleLabel.ForeColor = SystemColors.ControlText; ModVersionLabel.Visible = true; ModAuthorLabel.Visible = false; ModHomePage.Visible = false; OPENMODSFOLDER.Visible = true; OnlineInstall.Visible = false; ModDescription.Visible = false;
            ModTitleLabel.Text = name; ModVersionLabel.Text = version;
        }

        private void Select_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideModInfo(); DisableButton.Enabled = false; OnlineInstall.Visible = false; OnlineInstall.Enabled = false;
            if (Select.SelectedTab == Installed)
            {
                DisableButton.Text = ModList.SelectedItems.Count > 1 ? string.Format(Setting.T("Disable ({0})"), ModList.SelectedItems.Count) : Setting.T("Disable");
                DisableButton.Enabled = ModList.SelectedItems.Count > 0; DisableButton.Visible = true; InstallButton.Visible = true; Refresh.Visible = false;
            }
            else if (Select.SelectedTab == Disable)
            {
                DisableButton.Text = DisableModList.SelectedItems.Count > 1 ? string.Format(Setting.T("Enable ({0})"), DisableModList.SelectedItems.Count) : Setting.T("Enable");
                DisableButton.Enabled = DisableModList.SelectedItems.Count > 0; DisableButton.Visible = true; InstallButton.Visible = true; Refresh.Visible = false;
            }
            else if (Select.SelectedTab == OnlineMods)
            {
                DisableButton.Visible = false; InstallButton.Visible = false; Refresh.Visible = true;
                if (OnlineModsList.Items.Count == 0) LoadOnlineModsList();
                UpdateOnlineInstallButtonState();
            }
        }

        private void ModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Select.SelectedTab == Installed) { int c = ModList.SelectedItems.Count; DisableButton.Enabled = c > 0; DisableButton.Text = c > 1 ? string.Format(Setting.T("Disable ({0})"), c) : Setting.T("Disable"); }
            if (ModList.SelectedItems.Count == 0) { if (Select.SelectedTab == Installed) HideModInfo(); return; }
            if (ModList.SelectedItems.Count > 1) { ModTitleLabel.Visible = true; ModTitleLabel.ForeColor = SystemColors.ControlText; ModTitleLabel.Text = string.Format(Setting.T("{0} Mods selected"), ModList.SelectedItems.Count); ModVersionLabel.Visible = false; ModAuthorLabel.Visible = false; ModHomePage.Visible = false; ModDescription.Visible = false; OPENMODSFOLDER.Visible = false; OnlineInstall.Visible = false; return; }
            var si = ModList.SelectedItems[0]; var mi = _modInfos.FirstOrDefault(m => m.FolderPath == si.Tag as string);
            if (mi != null) ShowModInfo(mi); else ShowBasicModInfo(si.Text, si.SubItems.Count > 1 ? si.SubItems[1].Text : "");
        }

        private void DisableModList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Select.SelectedTab == Disable) { int c = DisableModList.SelectedItems.Count; DisableButton.Enabled = c > 0; DisableButton.Text = c > 1 ? string.Format(Setting.T("Enable ({0})"), c) : Setting.T("Enable"); }
            if (DisableModList.SelectedItems.Count == 0) { if (Select.SelectedTab == Disable) HideModInfo(); return; }
            if (DisableModList.SelectedItems.Count > 1) { ModTitleLabel.Visible = true; ModTitleLabel.ForeColor = SystemColors.ControlText; ModTitleLabel.Text = string.Format(Setting.T("{0} Mods selected"), DisableModList.SelectedItems.Count); ModVersionLabel.Visible = false; ModAuthorLabel.Visible = false; ModHomePage.Visible = false; ModDescription.Visible = false; OPENMODSFOLDER.Visible = false; OnlineInstall.Visible = false; return; }
            var si = DisableModList.SelectedItems[0]; var mi = _disableModInfos.FirstOrDefault(m => m.FolderPath == si.Tag as string);
            if (mi != null) ShowModInfo(mi); else ShowBasicModInfo(si.Text, si.SubItems.Count > 1 ? si.SubItems[1].Text : "");
        }

        private void OnlineModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnlineModsList.SelectedItems.Count == 0) { if (Select.SelectedTab == OnlineMods) HideModInfo(); UpdateOnlineInstallButtonState(); return; }
            var si = OnlineModsList.SelectedItems[0];
            if (si.Tag is OnlineModInfo omi) { ShowModInfo(new ModInfo { DisplayName = omi.DisplayName, Version = omi.Version, Author = omi.Author, HomePage = "", Description = "" }, isOnline: true); UpdateOnlineInstallButtonState(); }
        }

        private void UpdateOnlineInstallButtonState()
        {
            if (Select.SelectedTab != OnlineMods || OnlineModsList.SelectedItems.Count == 0) { OnlineInstall.Visible = false; OnlineInstall.Enabled = false; return; }
            if (!(OnlineModsList.SelectedItems[0].Tag is OnlineModInfo omi)) { OnlineInstall.Visible = false; OnlineInstall.Enabled = false; return; }
            var im = _modInfos.FirstOrDefault(m => m.DisplayName.Equals(omi.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (im != null)
            {
                if (ModService.CompareVersions(im.Version, omi.Version) >= 0) { OnlineInstall.Text = Setting.T("Installed"); OnlineInstall.Enabled = false; }
                else { OnlineInstall.Text = Setting.T("Update"); OnlineInstall.Enabled = true; }
                OnlineInstall.Visible = true;
            }
            else { OnlineInstall.Text = Setting.T("Install"); OnlineInstall.Enabled = true; OnlineInstall.Visible = true; }
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog()) { dlg.Filter = $"{Setting.T("ZIP files")} (*.zip)|*.zip"; dlg.Title = Setting.T("Select Mod ZIP File"); dlg.Multiselect = true; if (dlg.ShowDialog() != DialogResult.OK) return; foreach (string f in dlg.FileNames) DoInstallModFromZip(f, false); RefreshModLists(); if (Select.SelectedTab == OnlineMods) UpdateOnlineInstallButtonState(); }
        }

        private void ModManager_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); if (files.Length >= 1 && files.All(f => Path.GetExtension(f).Equals(".zip", StringComparison.OrdinalIgnoreCase))) { e.Effect = DragDropEffects.Copy; _dragOverlay.Visible = true; _dragOverlay.BringToFront(); return; } }
            e.Effect = DragDropEffects.None;
        }
        private void ModManager_DragLeave(object sender, EventArgs e) { _dragOverlay.Visible = false; }

        private void ModManager_DragDrop(object sender, DragEventArgs e)
        {
            _dragOverlay.Visible = false;
            if (!InstallButton.Enabled) { MessageBox.Show(Setting.T("Please locate the game folder first."), Setting.T("Game not found"), MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); if (files == null || files.Length == 0) return;
            foreach (string f in files) { if (!Path.GetExtension(f).Equals(".zip", StringComparison.OrdinalIgnoreCase)) continue; DoInstallModFromZip(f, false); }
            RefreshModLists(); if (Select.SelectedTab == OnlineMods) UpdateOnlineInstallButtonState();
        }

        private void DoInstallModFromZip(string zipPath, bool showSuccessMessage = true)
        {
            string tp = null;
            try
            {
                var (t, mi) = _modService.PrepareModFromZip(zipPath); tp = t;
                var em = _modInfos.FirstOrDefault(m => string.Equals(Path.GetFileName(m.FolderPath), mi.Id, StringComparison.OrdinalIgnoreCase));
                DialogResult cr;
                if (em != null)
                {
                    int cmp = ModService.CompareVersions(mi.Version, em.Version); string msg;
                    if (cmp > 0) msg = $"{Setting.T("Update Mod?")}\n\n{mi.DisplayName}\n{em.Version} \u2192 {mi.Version}";
                    else if (cmp == 0) msg = $"{Setting.T("Same version already installed. Overwrite?")}\n\n{mi.DisplayName} v{mi.Version}";
                    else msg = $"{Setting.T("Older version detected. Downgrade?")}\n\n{mi.DisplayName}\n{em.Version} \u2192 {mi.Version}";
                    cr = MessageBox.Show(msg, Setting.T("Confirm"), MessageBoxButtons.YesNo, cmp <= 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
                }
                else cr = MessageBox.Show($"{Setting.T("Install this Mod?")}\n\n{mi.DisplayName} v{mi.Version}\n{Setting.T("by")} {mi.Author}", Setting.T("Install Mod"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (cr != DialogResult.Yes) { SafeCleanupTemp(tp); return; }
                _modService.InstallModToTarget(tp, mi.Id, ModsPath); tp = null;
                RefreshModLists(); SelectAndScrollToMod(ModList, mi.Id);
                if (Select.SelectedTab == OnlineMods) UpdateOnlineInstallButtonState();
                else if (showSuccessMessage) MessageBox.Show(Setting.T("Install complete!"), Setting.T("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { SafeCleanupTemp(tp); MessageBox.Show(ex.Message, Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static void SelectAndScrollToMod(ListView lv, string folderName)
        { foreach (ListViewItem item in lv.Items) { if (string.Equals(Path.GetFileName(item.Tag as string), folderName, StringComparison.OrdinalIgnoreCase)) { item.Selected = true; item.EnsureVisible(); break; } } }

        private static void SafeCleanupTemp(string tp) { if (string.IsNullOrEmpty(tp) || !Directory.Exists(tp)) return; try { Directory.Delete(tp, true); } catch (Exception ex) { Debug.WriteLine($"[ModManager] Temp cleanup failed: {ex.Message}"); } }

        private async void OnlineInstall_Click(object sender, EventArgs e)
        {
            if (OnlineModsList.SelectedItems.Count == 0) return;
            if (!(OnlineModsList.SelectedItems[0].Tag is OnlineModInfo omi)) return;
            if (string.IsNullOrEmpty(omi.DownloadUrl)) { MessageBox.Show(Setting.T("No download URL available."), Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!InstallButton.Enabled) { MessageBox.Show(Setting.T("Please locate the game folder first."), Setting.T("Game not found"), MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            Loading.Visible = true; progressBarForOnline.Visible = true; progressBarForOnline.Style = ProgressBarStyle.Marquee; OnlineInstall.Enabled = false;
            try { await _modService.DownloadAndInstallModAsync(omi.DownloadUrl, ModsPath); RefreshModLists(); SelectAndScrollToMod(ModList, omi.DisplayName); UpdateOnlineInstallButtonState(); }
            catch (Exception ex) { MessageBox.Show($"{Setting.T("Failed to download Mod:")} {ex.Message}", Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { Loading.Visible = false; progressBarForOnline.Visible = false; progressBarForOnline.Style = ProgressBarStyle.Blocks; }
        }

        private void DisableButton_Click(object sender, EventArgs e) { if (Select.SelectedTab == Installed) MoveModsToDisabled(); else if (Select.SelectedTab == Disable) MoveModsToEnabled(); }

        private void MoveModsToDisabled()
        {
            var sp = ModList.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag as string).Where(p => !string.IsNullOrEmpty(p)).ToList(); if (sp.Count == 0) return;
            var failed = new List<string>();
            foreach (string fp in sp) { var mi = _modInfos.FirstOrDefault(m => m.FolderPath == fp); if (mi == null) continue; try { _modService.MoveModToDisabled(mi, DisableModsPath); } catch (Exception ex) { Debug.WriteLine($"[ModManager] Failed to disable {mi.DisplayName}: {ex.Message}"); failed.Add(mi.DisplayName); } }
            RefreshModLists();
            if (failed.Count > 0) MessageBox.Show(string.Format(Setting.T("Failed to disable: {0}"), string.Join(", ", failed)), Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void MoveModsToEnabled()
        {
            var sp = DisableModList.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag as string).Where(p => !string.IsNullOrEmpty(p)).ToList(); if (sp.Count == 0) return;
            var failed = new List<string>();
            foreach (string fp in sp) { var mi = _disableModInfos.FirstOrDefault(m => m.FolderPath == fp); if (mi == null) continue; try { _modService.MoveModToEnabled(mi, ModsPath); } catch (Exception ex) { Debug.WriteLine($"[ModManager] Failed to enable {mi.DisplayName}: {ex.Message}"); failed.Add(mi.DisplayName); } }
            RefreshModLists();
            if (failed.Count > 0) MessageBox.Show(string.Format(Setting.T("Failed to enable: {0}"), string.Join(", ", failed)), Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OPENMODSFOLDER_Click(object sender, EventArgs e)
        {
            string fp = null;
            if (Select.SelectedTab == Installed && ModList.SelectedItems.Count > 0) fp = ModList.SelectedItems[0].Tag as string;
            else if (Select.SelectedTab == Disable && DisableModList.SelectedItems.Count > 0) fp = DisableModList.SelectedItems[0].Tag as string;
            if (!string.IsNullOrEmpty(fp)) { ModService.OpenFolderInExplorer(fp); try { this.Activate(); } catch (Exception ex) { Debug.WriteLine($"[ModManager] Activate failed: {ex.Message}"); } }
        }

        private void ModHomePage_Click(object sender, EventArgs e)
        {
            string fp = null; List<ModInfo> cl = null;
            if (Select.SelectedTab == Installed && ModList.SelectedItems.Count > 0) { fp = ModList.SelectedItems[0].Tag as string; cl = _modInfos; }
            else if (Select.SelectedTab == Disable && DisableModList.SelectedItems.Count > 0) { fp = DisableModList.SelectedItems[0].Tag as string; cl = _disableModInfos; }
            if (!string.IsNullOrEmpty(fp) && cl != null) { var mi = cl.FirstOrDefault(m => m.FolderPath == fp); if (mi != null && !string.IsNullOrEmpty(mi.HomePage)) try { Process.Start(mi.HomePage); } catch (Exception ex) { Debug.WriteLine($"[ModManager] Failed to open homepage: {ex.Message}"); } }
        }

        private void Refresh_Click(object sender, EventArgs e) { LoadOnlineModsList(); }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sf = new Setting())
            {
                sf.ShowDialog(); _modService.OnlineModsUrl = Setting.GetCurrentOnlineModsUrl(); ApplyTranslation();
                if (!string.IsNullOrEmpty(GamePath.Text) && Directory.Exists(GamePath.Text))
                {
                    RefreshModLists();
                    if (_onlineModInfos.Count > 0) { _onlineModInfos.Clear(); OnlineModsList.Items.Clear(); if (Select.SelectedTab == OnlineMods) LoadOnlineModsList(); }
                }
            }
        }

        private void installModManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GamePath.Text) || !Directory.Exists(GamePath.Text)) { MessageBox.Show(Setting.T("Please locate the game folder first."), Setting.T("Game not found"), MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (_modService.CanInstallModManager(GamePath.Text))
            {
                var (installed, cc, ff) = _modService.InstallModManager(GamePath.Text);
                if (!installed) { MessageBox.Show(Setting.T("Nothing to install. All components already exist."), Setting.T("Info"), MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                if (ff.Count > 0) MessageBox.Show($"{Setting.T("Installed files successfully.")} {cc}\n{Setting.T("Failed to copy:")} {string.Join(", ", ff)}", Setting.T("Install Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else MessageBox.Show(Setting.T("Mod Manager installed successfully!"), Setting.T("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show(Setting.T("Mod Manager is already installed or the ModManager folder is missing."), Setting.T("Install Failed"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void uninstallModManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_modService.CanInstallModManager(GamePath.Text))
            {
                string wp = Path.Combine(GamePath.Text, AppConstants.WinHttpDll);
                if (File.Exists(wp))
                    try { File.Delete(wp); MessageBox.Show(Setting.T("Mod Manager uninstalled successfully!"), Setting.T("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    catch (Exception ex) { Debug.WriteLine($"[ModManager] Failed to uninstall: {ex.Message}"); MessageBox.Show($"{Setting.T("Failed to uninstall Mod Manager:")} {ex.Message}", Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }
    }
}
