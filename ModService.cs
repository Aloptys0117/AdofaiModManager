using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdofaiModManager
{
    public class ModService
    {
        private static readonly HttpClient _httpClient;

        static ModService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", AppConstants.ApiUserAgent);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public string ConfigPath { get; }
        public string SettingsPath { get; }

        public ModService()
        {
            ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);
            SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.SettingsFileName);
        }

        public string OnlineModsUrl { get; set; } = AppConstants.DefaultApiUrl;

        public string TryLoadGamePath()
        {
            if (!File.Exists(ConfigPath)) return null;
            try
            {
                var doc = System.Xml.Linq.XDocument.Load(ConfigPath);
                var gamePathElement = doc.Element(AppConstants.XmlConfigRoot)?.Element(AppConstants.XmlGamePath);
                if (gamePathElement != null && Directory.Exists(gamePathElement.Value))
                    return gamePathElement.Value;
            }
            catch (Exception ex) { Debug.WriteLine($"[ModService] Failed to load config: {ex.Message}"); }
            return null;
        }

        public void SaveGamePath(string gamePath)
        {
            try
            {
                var doc = new System.Xml.Linq.XDocument(
                    new System.Xml.Linq.XElement(AppConstants.XmlConfigRoot,
                        new System.Xml.Linq.XElement(AppConstants.XmlGamePath, gamePath)));
                doc.Save(ConfigPath);
            }
            catch (Exception ex) { Debug.WriteLine($"[ModService] Failed to save config: {ex.Message}"); }
        }

        public void LoadApiSettings()
        {
            if (!File.Exists(SettingsPath)) return;
            try
            {
                var doc = System.Xml.Linq.XDocument.Load(SettingsPath);
                var root = doc.Element(AppConstants.XmlSettingsRoot);
                if (root == null) return;
                var useDefaultElement = root.Element(AppConstants.XmlUseDefaultApi);
                var apiUrlElement = root.Element(AppConstants.XmlApiUrl);
                bool useDefault = useDefaultElement != null && useDefaultElement.Value == "true";
                string apiUrl = apiUrlElement?.Value;
                if (!useDefault && !string.IsNullOrEmpty(apiUrl))
                    OnlineModsUrl = apiUrl;
            }
            catch (Exception ex) { Debug.WriteLine($"[ModService] Failed to load API settings: {ex.Message}"); }
        }

        public List<ModInfo> LoadModsFromFolder(string folderPath, bool skipDisableModsFolder = true)
        {
            var mods = new List<ModInfo>();
            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); return mods; }
            var modsFolder = new DirectoryInfo(folderPath);
            foreach (var modDir in modsFolder.GetDirectories())
            {
                if (skipDisableModsFolder && modDir.Name == AppConstants.DisableModsFolder) continue;
                string infoPath = Path.Combine(modDir.FullName, AppConstants.ModInfoFileName);
                var modInfo = ParseModFolder(modDir, infoPath);
                if (modInfo != null) mods.Add(modInfo);
            }
            return mods;
        }

        private ModInfo ParseModFolder(DirectoryInfo modDir, string infoPath)
        {
            var modInfo = new ModInfo { FolderPath = modDir.FullName, DisplayName = modDir.Name, Version = Setting.T("Unknown") };
            if (!File.Exists(infoPath)) return modInfo;
            try
            {
                string jsonContent = File.ReadAllText(infoPath);
                var parsed = JsonConvert.DeserializeObject<ModInfo>(jsonContent);
                if (parsed != null) { parsed.FolderPath = modDir.FullName; return parsed; }
            }
            catch (Exception ex) { Debug.WriteLine($"[ModService] Failed to parse Info.json in {modDir.Name}: {ex.Message}"); }
            return modInfo;
        }

        public async Task<List<OnlineModInfo>> LoadOnlineModsAsync(IProgress<int> progress = null)
        {
            var result = new List<OnlineModInfo>();
            try
            {
                string jsonContent = await _httpClient.GetStringAsync(OnlineModsUrl);
                var onlineMods = JsonConvert.DeserializeObject<List<OnlineModInfoRaw>>(jsonContent);
                if (onlineMods == null) return result;
                var latestMods = new Dictionary<string, OnlineModInfoRaw>(StringComparer.OrdinalIgnoreCase);
                foreach (var mod in onlineMods)
                {
                    if (string.IsNullOrEmpty(mod.name) || string.IsNullOrEmpty(mod.version)) continue;
                    string key = mod.name;
                    if (!latestMods.ContainsKey(key) || CompareVersions(mod.version, latestMods[key].version) > 0)
                        latestMods[key] = mod;
                }
                foreach (var kvp in latestMods)
                {
                    var mod = kvp.Value;
                    result.Add(new OnlineModInfo
                    {
                        DisplayName = mod.name,
                        Version = mod.version,
                        Author = mod.cachedUsername ?? Setting.T("Unknown"),
                        DownloadUrl = !string.IsNullOrEmpty(mod.parsedDownload) ? mod.parsedDownload : mod.download
                    });
                }
            }
            catch (Exception ex) { throw new Exception($"{Setting.T("Failed to load online mods:")} {ex.Message}", ex); }
            return result;
        }

        public (string tempPath, ModInfo modInfo) PrepareModFromZip(string zipPath)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try { ZipFile.ExtractToDirectory(zipPath, tempPath); }
            catch (Exception ex) { throw new Exception($"{Setting.T("Failed to extract ZIP file:")} {ex.Message}", ex); }
            string infoJsonPath = FindInfoJson(tempPath);
            if (infoJsonPath == null) { SafeDeleteDirectory(tempPath); throw new Exception($"{Setting.T("Invalid Mod File!")}\n{Setting.T("Info.json not found.")}"); }
            string modFolderPath = Path.GetDirectoryName(infoJsonPath);
            string modInfoJson = File.ReadAllText(infoJsonPath);
            ModInfo modInfo = JsonConvert.DeserializeObject<ModInfo>(modInfoJson);
            if (modInfo == null || string.IsNullOrEmpty(modInfo.Id)) { SafeDeleteDirectory(tempPath); throw new Exception($"{Setting.T("Invalid Info.json!")}\n{Setting.T("Missing Mod Id.")}"); }
            return (tempPath, modInfo);
        }

        public void InstallModToTarget(string sourceFolder, string modId, string modsPath)
        {
            string targetPath = Path.Combine(modsPath, modId);
            CopyDirectory(sourceFolder, targetPath);
            SafeDeleteDirectory(sourceFolder);
        }

        public async Task DownloadAndInstallModAsync(string downloadUrl, string modsPath)
        {
            string tempZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
            try
            {
                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    using (var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        await response.Content.CopyToAsync(fileStream);
                }
                var (sourceFolder, modInfo) = PrepareModFromZip(tempZipPath);
                InstallModToTarget(sourceFolder, modInfo.Id, modsPath);
            }
            finally
            {
                if (File.Exists(tempZipPath))
                    try { File.Delete(tempZipPath); } catch (Exception ex) { Debug.WriteLine($"[ModService] Failed to delete temp zip: {ex.Message}"); }
            }
        }

        public void MoveModToDisabled(ModInfo modInfo, string disableModsPath)
        {
            if (modInfo == null || string.IsNullOrEmpty(modInfo.FolderPath)) return;
            string folderName = Path.GetFileName(modInfo.FolderPath);
            string targetPath = Path.Combine(disableModsPath, folderName);
            if (!Directory.Exists(disableModsPath)) Directory.CreateDirectory(disableModsPath);
            if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
            Directory.Move(modInfo.FolderPath, targetPath);
        }

        public void MoveModToEnabled(ModInfo modInfo, string modsPath)
        {
            if (modInfo == null || string.IsNullOrEmpty(modInfo.FolderPath)) return;
            string folderName = Path.GetFileName(modInfo.FolderPath);
            string targetPath = Path.Combine(modsPath, folderName);
            if (Directory.Exists(targetPath)) Directory.Delete(targetPath, true);
            Directory.Move(modInfo.FolderPath, targetPath);
        }

        public static int CompareVersions(string v1, string v2)
        {
            if (Version.TryParse(v1, out var version1) && Version.TryParse(v2, out var version2))
                return version1.CompareTo(version2);
            try
            {
                var parts1 = v1.Split('.'); var parts2 = v2.Split('.');
                int maxLength = Math.Max(parts1.Length, parts2.Length);
                for (int i = 0; i < maxLength; i++)
                {
                    int num1 = i < parts1.Length && int.TryParse(parts1[i], out int p1) ? p1 : 0;
                    int num2 = i < parts2.Length && int.TryParse(parts2[i], out int p2) ? p2 : 0;
                    if (num1 > num2) return 1; if (num1 < num2) return -1;
                }
                return 0;
            }
            catch { return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase); }
        }

        public bool CanInstallModManager(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath)) return false;
            string localModManagerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ModManagerFolder, AppConstants.UnityModManagerFolder);
            string localWinHttpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ModManagerFolder, AppConstants.WinHttpDll);
            bool hasModManagerFolder = Directory.Exists(localModManagerPath) && Directory.GetFiles(localModManagerPath, "*", SearchOption.AllDirectories).Length > 0;
            bool hasWinHttpDll = File.Exists(localWinHttpPath);
            if (!hasModManagerFolder && !hasWinHttpDll) return false;
            string gameManagedPath = Path.Combine(gamePath, AppConstants.GameDataFolder, AppConstants.GameManagedFolder, AppConstants.UnityModManagerFolder);
            string gameWinHttpPath = Path.Combine(gamePath, AppConstants.WinHttpDll);
            bool needModManager = !Directory.Exists(gameManagedPath);
            bool needWinHttp = !File.Exists(gameWinHttpPath);
            return (hasModManagerFolder && needModManager) || (hasWinHttpDll && needWinHttp);
        }

        public (bool installed, int copiedCount, List<string> failedFiles) InstallModManager(string gamePath)
        {
            bool installed = false; int copiedCount = 0;
            var failedFiles = new List<string>();
            string localModManagerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ModManagerFolder, AppConstants.UnityModManagerFolder);
            string localWinHttpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ModManagerFolder, AppConstants.WinHttpDll);
            string gameManagedPath = Path.Combine(gamePath, AppConstants.GameDataFolder, AppConstants.GameManagedFolder, AppConstants.UnityModManagerFolder);
            string gameWinHttpPath = Path.Combine(gamePath, AppConstants.WinHttpDll);
            bool needModManager = !Directory.Exists(gameManagedPath);
            bool needWinHttp = !File.Exists(gameWinHttpPath);
            if (needModManager && Directory.Exists(localModManagerPath))
            {
                var allFiles = Directory.GetFiles(localModManagerPath, "*", SearchOption.AllDirectories);
                if (allFiles.Length > 0)
                {
                    string managedDir = Path.GetDirectoryName(gameManagedPath);
                    if (!Directory.Exists(managedDir)) Directory.CreateDirectory(managedDir);
                    string modsPath = Path.Combine(gamePath, AppConstants.ModsFolder);
                    if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);
                    foreach (var file in allFiles)
                    {
                        string relativePath = file.Substring(localModManagerPath.Length + 1);
                        string destFile = Path.Combine(gameManagedPath, relativePath);
                        string destDir = Path.GetDirectoryName(destFile);
                        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                        try { File.Copy(file, destFile, true); copiedCount++; installed = true; }
                        catch (Exception ex) { failedFiles.Add(relativePath); Debug.WriteLine($"[ModService] Failed to copy {relativePath}: {ex.Message}"); }
                    }
                }
            }
            if (needWinHttp && File.Exists(localWinHttpPath))
            {
                try { File.Copy(localWinHttpPath, gameWinHttpPath, true); installed = true; }
                catch (Exception ex) { failedFiles.Add(AppConstants.WinHttpDll); Debug.WriteLine($"[ModService] Failed to copy winhttp.dll: {ex.Message}"); }
            }
            return (installed, copiedCount, failedFiles);
        }

        private static string FindInfoJson(string directory)
        {
            string directPath = Path.Combine(directory, AppConstants.ModInfoFileName);
            if (File.Exists(directPath)) return directPath;
            foreach (var subDir in Directory.GetDirectories(directory))
            { string result = FindInfoJson(subDir); if (result != null) return result; }
            return null;
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            { string destFile = Path.Combine(targetDir, Path.GetFileName(file)); File.Copy(file, destFile, true); }
            foreach (var dir in Directory.GetDirectories(sourceDir))
            { string destDir = Path.Combine(targetDir, Path.GetFileName(dir)); CopyDirectory(dir, destDir); }
        }

        private static void SafeDeleteDirectory(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); }
            catch (Exception ex) { Debug.WriteLine($"[ModService] Failed to delete directory {path}: {ex.Message}"); }
        }

        public static void OpenFolderInExplorer(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;
            try { Process.Start(folderPath); }
            catch (Exception ex) { MessageBox.Show($"{Setting.T("Failed to open Mods folder:")} {ex.Message}", Setting.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
