using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace AdofaiModManager
{
    public partial class Setting : Form
    {
        private static readonly string DefaultApiUrl = "https://api.aloptys.top/AdofaiMods/ModList.json";
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml");
        private static readonly string LanguagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language");

        private bool isLoading = true;

        private Dictionary<string, string> languageMap = new Dictionary<string, string>
        {
            { "中文", "zh-cn" },
            { "English", "en-us" }
        };

        private Dictionary<string, string> reverseLanguageMap = new Dictionary<string, string>
        {
            { "zh-cn", "中文" },
            { "en-us", "English" }
        };

        public static string CurrentLanguage = "en-us";
        public static Dictionary<string, string> LangData = new Dictionary<string, string>();

        public Setting()
        {
            InitializeComponent();
            APIUrl.TextChanged += APIUrl_TextChanged;
            LanguageSelect.SelectedIndexChanged += LanguageSelect_SelectedIndexChanged;
            SettingSelect.SelectedIndexChanged += SettingSelect_SelectedIndexChanged;
            LanguageSelect.Items.Clear();
            LanguageSelect.Items.AddRange(new object[] { "中文", "English" });
            LoadSettings();
            LoadLanguage(CurrentLanguage);
            ApplyTranslation();
            isLoading = false;
        }

        public static void LoadLanguage(string langCode)
        {
            string filePath = Path.Combine(LanguagePath, $"{langCode}.json");
            if (!File.Exists(filePath))
            {
                CurrentLanguage = "en-us";
                filePath = Path.Combine(LanguagePath, "en-us.json");
            }

            try
            {
                string json = File.ReadAllText(filePath);
                LangData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                CurrentLanguage = langCode;
            }
            catch
            {
                LangData = new Dictionary<string, string>();
            }
        }

        public static string T(string key)
        {
            return LangData.ContainsKey(key) ? LangData[key] : key;
        }

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
            if (!File.Exists(SettingsPath))
            {
                SetDefault();
                return;
            }

            try
            {
                var doc = XDocument.Load(SettingsPath);
                var root = doc.Element("Settings");
                if (root == null)
                {
                    SetDefault();
                    return;
                }

                var useDefaultElement = root.Element("UseDefaultApi");
                var apiUrlElement = root.Element("ApiUrl");
                var languageElement = root.Element("Language");

                bool useDefault = useDefaultElement != null && useDefaultElement.Value == "true";
                string apiUrl = apiUrlElement?.Value ?? DefaultApiUrl;
                string langCode = languageElement?.Value ?? "en-us";

                CurrentLanguage = langCode;

                DefaultAPI.Checked = useDefault;
                APIUrl.Enabled = !useDefault;
                APIUrl.Text = useDefault ? DefaultApiUrl : apiUrl;
                ModManager.OnlineModsUrl = useDefault ? DefaultApiUrl : apiUrl;

                if (reverseLanguageMap.ContainsKey(langCode))
                    LanguageSelect.SelectedItem = reverseLanguageMap[langCode];
                else
                    LanguageSelect.SelectedItem = "English";
            }
            catch
            {
                SetDefault();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string selectedLang = LanguageSelect.SelectedItem?.ToString() ?? "English";
                string langCode = languageMap.ContainsKey(selectedLang) ? languageMap[selectedLang] : "en-us";

                var doc = new XDocument(
                    new XElement("Settings",
                        new XElement("UseDefaultApi", DefaultAPI.Checked),
                        new XElement("ApiUrl", DefaultAPI.Checked ? DefaultApiUrl : APIUrl.Text),
                        new XElement("Language", langCode)
                    )
                );
                doc.Save(SettingsPath);
            }
            catch { }
        }

        private void SetDefault()
        {
            DefaultAPI.Checked = true;
            APIUrl.Enabled = false;
            APIUrl.Text = DefaultApiUrl;
            ModManager.OnlineModsUrl = DefaultApiUrl;

            string systemLang = System.Globalization.CultureInfo.CurrentUICulture.Name.ToLower();
            if (systemLang.StartsWith("zh"))
                LanguageSelect.SelectedItem = "中文";
            else
                LanguageSelect.SelectedItem = "English";

            SaveSettings();
        }

        private void SettingSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SettingSelect.SelectedItems.Count == 0) return;

            string key = SettingSelect.SelectedItems[0].Text;

            SettingModAPI.Visible = key == "API" || key == T("API");
            APIUrl.Visible = key == "API" || key == T("API");
            DefaultAPI.Visible = key == "API" || key == T("API");
            SettingLanguage.Visible = key == "Language" || key == T("Language");
            LanguageSelect.Visible = key == "Language" || key == T("Language");
        }

        private void DefaultAPI_CheckedChanged(object sender, EventArgs e)
        {
            if (DefaultAPI.Checked)
            {
                APIUrl.Enabled = false;
                APIUrl.Text = DefaultApiUrl;
                ModManager.OnlineModsUrl = DefaultApiUrl;
            }
            else
            {
                APIUrl.Enabled = true;
                APIUrl.Text = "";
                APIUrl.Focus();
            }

            SaveSettings();
        }

        private void APIUrl_TextChanged(object sender, EventArgs e)
        {
            if (isLoading) return;

            if (!DefaultAPI.Checked && !string.IsNullOrEmpty(APIUrl.Text))
            {
                ModManager.OnlineModsUrl = APIUrl.Text;
                SaveSettings();
            }
        }

        private void LanguageSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading) return;

            string selectedLang = LanguageSelect.SelectedItem?.ToString() ?? "English";
            string langCode = languageMap.ContainsKey(selectedLang) ? languageMap[selectedLang] : "en-us";

            LoadLanguage(langCode);
            ApplyTranslation();
            SaveSettings();
        }
    }
}