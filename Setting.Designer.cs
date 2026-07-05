namespace AdofaiModManager
{
    partial class Setting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("General", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "API"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "Language"}, -1, System.Drawing.Color.Empty, System.Drawing.Color.Empty, new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))));
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setting));
            this.SettingModAPI = new System.Windows.Forms.Label();
            this.SettingSelect = new System.Windows.Forms.ListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.LanguageSelect = new System.Windows.Forms.ComboBox();
            this.SettingLanguage = new System.Windows.Forms.Label();
            this.DefaultAPI = new System.Windows.Forms.CheckBox();
            this.APIUrl = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingModAPI
            // 
            this.SettingModAPI.AutoSize = true;
            this.SettingModAPI.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SettingModAPI.Location = new System.Drawing.Point(3, 14);
            this.SettingModAPI.Name = "SettingModAPI";
            this.SettingModAPI.Size = new System.Drawing.Size(156, 25);
            this.SettingModAPI.TabIndex = 0;
            this.SettingModAPI.Text = "Online Mod API";
            // 
            // SettingSelect
            // 
            this.SettingSelect.Font = new System.Drawing.Font("微软雅黑", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            listViewGroup1.Header = "General";
            listViewGroup1.Name = "SettingGeneral";
            this.SettingSelect.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1});
            this.SettingSelect.HideSelection = false;
            this.SettingSelect.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
            this.SettingSelect.Location = new System.Drawing.Point(12, 12);
            this.SettingSelect.MultiSelect = false;
            this.SettingSelect.Name = "SettingSelect";
            this.SettingSelect.Size = new System.Drawing.Size(167, 426);
            this.SettingSelect.TabIndex = 1;
            this.SettingSelect.UseCompatibleStateImageBehavior = false;
            this.SettingSelect.View = System.Windows.Forms.View.SmallIcon;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.LanguageSelect);
            this.panel1.Controls.Add(this.SettingLanguage);
            this.panel1.Controls.Add(this.DefaultAPI);
            this.panel1.Controls.Add(this.APIUrl);
            this.panel1.Controls.Add(this.SettingModAPI);
            this.panel1.Location = new System.Drawing.Point(185, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(603, 426);
            this.panel1.TabIndex = 2;
            // 
            // LanguageSelect
            // 
            this.LanguageSelect.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LanguageSelect.FormattingEnabled = true;
            this.LanguageSelect.Items.AddRange(new object[] {
            "Chinese",
            "English"});
            this.LanguageSelect.Location = new System.Drawing.Point(8, 147);
            this.LanguageSelect.Name = "LanguageSelect";
            this.LanguageSelect.Size = new System.Drawing.Size(151, 32);
            this.LanguageSelect.TabIndex = 4;
            // 
            // SettingLanguage
            // 
            this.SettingLanguage.AutoSize = true;
            this.SettingLanguage.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SettingLanguage.Location = new System.Drawing.Point(3, 119);
            this.SettingLanguage.Name = "SettingLanguage";
            this.SettingLanguage.Size = new System.Drawing.Size(100, 25);
            this.SettingLanguage.TabIndex = 3;
            this.SettingLanguage.Text = "Language";
            // 
            // DefaultAPI
            // 
            this.DefaultAPI.AutoSize = true;
            this.DefaultAPI.Checked = true;
            this.DefaultAPI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DefaultAPI.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.DefaultAPI.Location = new System.Drawing.Point(8, 76);
            this.DefaultAPI.Name = "DefaultAPI";
            this.DefaultAPI.Size = new System.Drawing.Size(99, 28);
            this.DefaultAPI.TabIndex = 2;
            this.DefaultAPI.Text = "Default";
            this.DefaultAPI.UseVisualStyleBackColor = true;
            this.DefaultAPI.CheckedChanged += new System.EventHandler(this.DefaultAPI_CheckedChanged);
            // 
            // APIUrl
            // 
            this.APIUrl.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.APIUrl.Location = new System.Drawing.Point(8, 42);
            this.APIUrl.Name = "APIUrl";
            this.APIUrl.Size = new System.Drawing.Size(576, 31);
            this.APIUrl.TabIndex = 1;
            // 
            // Setting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.SettingSelect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Setting";
            this.Text = "Setting";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label SettingModAPI;
        private System.Windows.Forms.ListView SettingSelect;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox DefaultAPI;
        private System.Windows.Forms.TextBox APIUrl;
        private System.Windows.Forms.Label SettingLanguage;
        private System.Windows.Forms.ComboBox LanguageSelect;
    }
}