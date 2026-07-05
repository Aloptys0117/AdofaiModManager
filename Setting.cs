using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AdofaiModManager
{
    public partial class Setting : Form
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.SettingsFileName);
        private static readonly string LanguagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.LanguageFolder);

        private bool _isLoading = true;

        private readonly Dictionary<string, string> _languageMap = new Dictionary<string, string>
        {
            { AppConstants.LangChineseDisplay, AppConstants.LangChinese },
            { AppConstants.LangEnglishDisplay, AppConstants.LangEnglish }
        };

        private readonly Dictionary<string, string> _reverseLanguageMap = new Dictionary<string, string>
        {
            { AppConstants.LangChinese, AppConstants.LangChineseDisplay },
            { AppConstants.LangEnglish, AppConstants.LangEnglishDisplay }
        };

        public static string CurrentLanguage = AppConstants.LangEnglish;
        public static Dictionary<string, string> LangData = new Dictionary<string, string>();

        public static string CurrentOnlineModsUrl { get; private set; } = AppConstants.DefaultApiUrl;

        public Setting()
        {
            InitializeComponent();
            APIUrl.TextChanged += APIUrl_TextChanged;
            LanguageSelect.SelectedIndexChanged += LanguageSelect_SelectedIndexChanged;
            SettingSelect.SelectedIndexChanged += SettingSelect_SelectedIndexChanged;
            LanguageSelect.Items.Clear();
            LanguageSelect.Items.AddRange(new object[] { AppConstants.LangChineseDisplay, AppConstants.LangEnglishDisplay });
            LoadSettings();
            LoadLanguage(CurrentLanguage);
            ApplyTranslation();
            _isLoading = false;
        }

        public static string GetCurrentOnlineModsUrl() => CurrentOnlineModsUrl;

        public static void LoadLanguage(string langCode)
        {
            string filePath = Path.Combine(LanguagePath, $"{langCode}.json");
            if (!File.Exists(filePath))
            {
                CurrentLanguage = AppConstants.LangEnglish;
                filePath = Path.Combine(LanguagePath, $"{AppConstants.LangEnglish}.json");
            }
            try
            {
                string json = File.ReadAllText(filePath);
                LangData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                CurrentLanguage = langCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Setting] Failed to load language file {langCode}: {ex.Message}");
                LangData = new Dictionary<string, string>();
            }
        }

        public static string T(string key) => LangData.TryGetValue(key, out var value) ? value : key;

        private void ApplyTranslation()
        {
            this.Text = T("Setting");
            SettingSelect.Groups[0].Header = T("General");
            SettingSelect.Items[0].Text = T("API");
            SettingSelect.Items[1].Text = T("Language");
            SettingModAPI.Text = T("Online Mod API");
            DefaultAPI.Text = T("Default");
            SettingLanguage.Text = T("Language");
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsPath)) { SetDefault(); return; }
            try
            {
                var doc = XDocument.Load(SettingsPath);
                var root = doc.Element(AppConstants.XmlSettingsRoot);
                if (root == null) { SetDefault(); return; }
                var useDefaultElement = root.Element(AppConstants.XmlUseDefaultApi);
                var apiUrlElement = root.Element(AppConstants.XmlApiUrl);
                var languageElement = root.Element(AppConstants.XmlLanguage);
                bool useDefault = useDefaultElement != null && useDefaultElement.Value == "true";
                string apiUrl = apiUrlElement?.Value ?? AppConstants.DefaultApiUrl;
                string langCode = languageElement?.Value ?? AppConstants.LangEnglish;
                CurrentLanguage = langCode;
                DefaultAPI.Checked = useDefault;
                APIUrl.Enabled = !useDefault;
                APIUrl.Text = useDefault ? AppConstants.DefaultApiUrl : apiUrl;
                CurrentOnlineModsUrl = useDefault ? AppConstants.DefaultApiUrl : apiUrl;
                if (_reverseLanguageMap.TryGetValue(langCode, out var displayName))
                    LanguageSelect.SelectedItem = displayName;
                else
                    LanguageSelect.SelectedItem = AppConstants.LangEnglishDisplay;
            }
            catch (Exception ex) { Debug.WriteLine($"[Setting] Failed to load settings: {ex.Message}"); SetDefault(); }
        }

        private void SaveSettings()
        {
            try
            {
                string selectedLang = LanguageSelect.SelectedItem?.ToString() ?? AppConstants.LangEnglishDisplay;
                string langCode = _languageMap.TryGetValue(selectedLang, out var code) ? code : AppConstants.LangEnglish;
                var doc = new XDocument(new XElement(AppConstants.XmlSettingsRoot,
                    new XElement(AppConstants.XmlUseDefaultApi, DefaultAPI.Checked),
                    new XElement(AppConstants.XmlApiUrl, DefaultAPI.Checked ? AppConstants.DefaultApiUrl : APIUrl.Text),
                    new XElement(AppConstants.XmlLanguage, langCode)));
                doc.Save(SettingsPath);
            }
            catch (Exception ex) { Debug.WriteLine($"[Setting] Failed to save settings: {ex.Message}"); }
        }

        private void SetDefault()
        {
            DefaultAPI.Checked = true;
            APIUrl.Enabled = false;
            APIUrl.Text = AppConstants.DefaultApiUrl;
            CurrentOnlineModsUrl = AppConstants.DefaultApiUrl;
            string systemLang = System.Globalization.CultureInfo.CurrentUICulture.Name.ToLower();
            LanguageSelect.SelectedItem = systemLang.StartsWith("zh") ? AppConstants.LangChineseDisplay : AppConstants.LangEnglishDisplay;
            SaveSettings();
        }

        private void SettingSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SettingSelect.SelectedItems.Count == 0) return;
            string key = SettingSelect.SelectedItems[0].Text;
            bool showApi = key == "API" || key == T("API");
            SettingModAPI.Visible = showApi; APIUrl.Visible = showApi; DefaultAPI.Visible = showApi;
            bool showLang = key == "Language" || key == T("Language");
            SettingLanguage.Visible = showLang; LanguageSelect.Visible = showLang;
        }

        private void DefaultAPI_CheckedChanged(object sender, EventArgs e)
        {
            if (DefaultAPI.Checked) { APIUrl.Enabled = false; APIUrl.Text = AppConstants.DefaultApiUrl; CurrentOnlineModsUrl = AppConstants.DefaultApiUrl; }
            else { APIUrl.Enabled = true; APIUrl.Text = ""; APIUrl.Focus(); }
            SaveSettings();
        }

        private void APIUrl_TextChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            if (!DefaultAPI.Checked && !string.IsNullOrEmpty(APIUrl.Text)) { CurrentOnlineModsUrl = APIUrl.Text; SaveSettings(); }
        }

        private void LanguageSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            string selectedLang = LanguageSelect.SelectedItem?.ToString() ?? AppConstants.LangEnglishDisplay;
            string langCode = _languageMap.TryGetValue(selectedLang, out var code) ? code : AppConstants.LangEnglish;
            LoadLanguage(langCode);
            ApplyTranslation();
            SaveSettings();
        }
    }
}
