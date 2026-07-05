namespace AdofaiModManager
{
    partial class ModManager
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModManager));
            this.FindGame = new System.Windows.Forms.Button();
            this.GamePath = new System.Windows.Forms.TextBox();
            this.BrowersButton = new System.Windows.Forms.Button();
            this.Installed = new System.Windows.Forms.TabPage();
            this.ModList = new System.Windows.Forms.ListView();
            this.ModName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Select = new System.Windows.Forms.TabControl();
            this.Disable = new System.Windows.Forms.TabPage();
            this.DisableModList = new System.Windows.Forms.ListView();
            this.DisableModName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DisableModVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OnlineMods = new System.Windows.Forms.TabPage();
            this.progressBarForOnline = new System.Windows.Forms.ProgressBar();
            this.Loading = new System.Windows.Forms.Label();
            this.OnlineModsList = new System.Windows.Forms.ListView();
            this.OnlineModName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OnlineModVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.InstallButton = new System.Windows.Forms.Button();
            this.DisableButton = new System.Windows.Forms.Button();
            this.ModTitleLabel = new System.Windows.Forms.Label();
            this.ModVersionLabel = new System.Windows.Forms.Label();
            this.ModAuthorLabel = new System.Windows.Forms.Label();
            this.OPENMODSFOLDER = new System.Windows.Forms.Button();
            this.ModHomePage = new System.Windows.Forms.Button();
            this.OnlineInstall = new System.Windows.Forms.Button();
            this.AMMVersion = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.Option = new System.Windows.Forms.ToolStripMenuItem();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.patchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installModManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallModManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Refresh = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ModDescription = new System.Windows.Forms.RichTextBox();
            this.Installed.SuspendLayout();
            this.Select.SuspendLayout();
            this.Disable.SuspendLayout();
            this.OnlineMods.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FindGame
            // 
            resources.ApplyResources(this.FindGame, "FindGame");
            this.FindGame.Name = "FindGame";
            this.FindGame.UseVisualStyleBackColor = true;
            this.FindGame.Click += new System.EventHandler(this.FindGame_Click);
            // 
            // GamePath
            // 
            resources.ApplyResources(this.GamePath, "GamePath");
            this.GamePath.Name = "GamePath";
            // 
            // BrowersButton
            // 
            resources.ApplyResources(this.BrowersButton, "BrowersButton");
            this.BrowersButton.Name = "BrowersButton";
            this.BrowersButton.UseVisualStyleBackColor = true;
            this.BrowersButton.Click += new System.EventHandler(this.BrowersButton_Click);
            // 
            // Installed
            // 
            this.Installed.Controls.Add(this.ModList);
            resources.ApplyResources(this.Installed, "Installed");
            this.Installed.Name = "Installed";
            this.Installed.UseVisualStyleBackColor = true;
            // 
            // ModList
            // 
            resources.ApplyResources(this.ModList, "ModList");
            this.ModList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ModName,
            this.ModVersion});
            this.ModList.HideSelection = false;
            this.ModList.MultiSelect = false;
            this.ModList.Name = "ModList";
            this.ModList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ModList.UseCompatibleStateImageBehavior = false;
            this.ModList.View = System.Windows.Forms.View.Details;
            // 
            // ModName
            // 
            resources.ApplyResources(this.ModName, "ModName");
            // 
            // ModVersion
            // 
            resources.ApplyResources(this.ModVersion, "ModVersion");
            // 
            // Select
            // 
            this.Select.Controls.Add(this.Installed);
            this.Select.Controls.Add(this.Disable);
            this.Select.Controls.Add(this.OnlineMods);
            resources.ApplyResources(this.Select, "Select");
            this.Select.Name = "Select";
            this.Select.SelectedIndex = 0;
            // 
            // Disable
            // 
            this.Disable.Controls.Add(this.DisableModList);
            resources.ApplyResources(this.Disable, "Disable");
            this.Disable.Name = "Disable";
            this.Disable.UseVisualStyleBackColor = true;
            // 
            // DisableModList
            // 
            resources.ApplyResources(this.DisableModList, "DisableModList");
            this.DisableModList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DisableModName,
            this.DisableModVersion});
            this.DisableModList.HideSelection = false;
            this.DisableModList.MultiSelect = false;
            this.DisableModList.Name = "DisableModList";
            this.DisableModList.UseCompatibleStateImageBehavior = false;
            this.DisableModList.View = System.Windows.Forms.View.Details;
            // 
            // DisableModName
            // 
            resources.ApplyResources(this.DisableModName, "DisableModName");
            // 
            // DisableModVersion
            // 
            resources.ApplyResources(this.DisableModVersion, "DisableModVersion");
            // 
            // OnlineMods
            // 
            this.OnlineMods.Controls.Add(this.progressBarForOnline);
            this.OnlineMods.Controls.Add(this.Loading);
            this.OnlineMods.Controls.Add(this.OnlineModsList);
            resources.ApplyResources(this.OnlineMods, "OnlineMods");
            this.OnlineMods.Name = "OnlineMods";
            this.OnlineMods.UseVisualStyleBackColor = true;
            // 
            // progressBarForOnline
            // 
            resources.ApplyResources(this.progressBarForOnline, "progressBarForOnline");
            this.progressBarForOnline.Name = "progressBarForOnline";
            this.progressBarForOnline.Step = 5;
            // 
            // Loading
            // 
            resources.ApplyResources(this.Loading, "Loading");
            this.Loading.Name = "Loading";
            // 
            // OnlineModsList
            // 
            resources.ApplyResources(this.OnlineModsList, "OnlineModsList");
            this.OnlineModsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OnlineModName,
            this.OnlineModVersion});
            this.OnlineModsList.HideSelection = false;
            this.OnlineModsList.Name = "OnlineModsList";
            this.OnlineModsList.UseCompatibleStateImageBehavior = false;
            this.OnlineModsList.View = System.Windows.Forms.View.Details;
            // 
            // OnlineModName
            // 
            resources.ApplyResources(this.OnlineModName, "OnlineModName");
            // 
            // OnlineModVersion
            // 
            resources.ApplyResources(this.OnlineModVersion, "OnlineModVersion");
            // 
            // InstallButton
            // 
            resources.ApplyResources(this.InstallButton, "InstallButton");
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // DisableButton
            // 
            resources.ApplyResources(this.DisableButton, "DisableButton");
            this.DisableButton.Name = "DisableButton";
            this.DisableButton.UseVisualStyleBackColor = true;
            this.DisableButton.Click += new System.EventHandler(this.DisableButton_Click);
            // 
            // ModTitleLabel
            // 
            resources.ApplyResources(this.ModTitleLabel, "ModTitleLabel");
            this.ModTitleLabel.BackColor = System.Drawing.SystemColors.Control;
            this.ModTitleLabel.Name = "ModTitleLabel";
            // 
            // ModVersionLabel
            // 
            resources.ApplyResources(this.ModVersionLabel, "ModVersionLabel");
            this.ModVersionLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ModVersionLabel.Name = "ModVersionLabel";
            // 
            // ModAuthorLabel
            // 
            resources.ApplyResources(this.ModAuthorLabel, "ModAuthorLabel");
            this.ModAuthorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ModAuthorLabel.Name = "ModAuthorLabel";
            // 
            // OPENMODSFOLDER
            // 
            resources.ApplyResources(this.OPENMODSFOLDER, "OPENMODSFOLDER");
            this.OPENMODSFOLDER.Name = "OPENMODSFOLDER";
            this.OPENMODSFOLDER.UseVisualStyleBackColor = true;
            this.OPENMODSFOLDER.Click += new System.EventHandler(this.OPENMODSFOLDER_Click);
            // 
            // ModHomePage
            // 
            resources.ApplyResources(this.ModHomePage, "ModHomePage");
            this.ModHomePage.Name = "ModHomePage";
            this.ModHomePage.UseVisualStyleBackColor = true;
            this.ModHomePage.Click += new System.EventHandler(this.ModHomePage_Click);
            // 
            // OnlineInstall
            // 
            resources.ApplyResources(this.OnlineInstall, "OnlineInstall");
            this.OnlineInstall.Name = "OnlineInstall";
            this.OnlineInstall.UseVisualStyleBackColor = true;
            this.OnlineInstall.Click += new System.EventHandler(this.OnlineInstall_Click);
            // 
            // AMMVersion
            // 
            resources.ApplyResources(this.AMMVersion, "AMMVersion");
            this.AMMVersion.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.AMMVersion.Name = "AMMVersion";
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Option,
            this.patchToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // Option
            // 
            this.Option.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingToolStripMenuItem});
            this.Option.Name = "Option";
            resources.ApplyResources(this.Option, "Option");
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            resources.ApplyResources(this.settingToolStripMenuItem, "settingToolStripMenuItem");
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.SettingToolStripMenuItem_Click);
            // 
            // patchToolStripMenuItem
            // 
            this.patchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installModManagerToolStripMenuItem,
            this.uninstallModManagerToolStripMenuItem});
            this.patchToolStripMenuItem.Name = "patchToolStripMenuItem";
            resources.ApplyResources(this.patchToolStripMenuItem, "patchToolStripMenuItem");
            // 
            // installModManagerToolStripMenuItem
            // 
            this.installModManagerToolStripMenuItem.Name = "installModManagerToolStripMenuItem";
            resources.ApplyResources(this.installModManagerToolStripMenuItem, "installModManagerToolStripMenuItem");
            this.installModManagerToolStripMenuItem.Click += new System.EventHandler(this.installModManagerToolStripMenuItem_Click);
            // 
            // uninstallModManagerToolStripMenuItem
            // 
            this.uninstallModManagerToolStripMenuItem.Name = "uninstallModManagerToolStripMenuItem";
            resources.ApplyResources(this.uninstallModManagerToolStripMenuItem, "uninstallModManagerToolStripMenuItem");
            this.uninstallModManagerToolStripMenuItem.Click += new System.EventHandler(this.uninstallModManagerToolStripMenuItem_Click);
            // 
            // Refresh
            // 
            resources.ApplyResources(this.Refresh, "Refresh");
            this.Refresh.Name = "Refresh";
            this.Refresh.UseVisualStyleBackColor = true;
            this.Refresh.Click += new System.EventHandler(this.Refresh_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ModDescription);
            this.panel1.Controls.Add(this.ModTitleLabel);
            this.panel1.Controls.Add(this.ModVersionLabel);
            this.panel1.Controls.Add(this.ModAuthorLabel);
            this.panel1.Controls.Add(this.OnlineInstall);
            this.panel1.Controls.Add(this.OPENMODSFOLDER);
            this.panel1.Controls.Add(this.ModHomePage);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // ModDescription
            // 
            this.ModDescription.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.ModDescription, "ModDescription");
            this.ModDescription.Name = "ModDescription";
            this.ModDescription.ReadOnly = true;
            // 
            // ModManager
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Refresh);
            this.Controls.Add(this.AMMVersion);
            this.Controls.Add(this.DisableButton);
            this.Controls.Add(this.BrowersButton);
            this.Controls.Add(this.InstallButton);
            this.Controls.Add(this.GamePath);
            this.Controls.Add(this.FindGame);
            this.Controls.Add(this.Select);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "ModManager";
            this.Installed.ResumeLayout(false);
            this.Select.ResumeLayout(false);
            this.Disable.ResumeLayout(false);
            this.OnlineMods.ResumeLayout(false);
            this.OnlineMods.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button FindGame;
        private System.Windows.Forms.TextBox GamePath;
        private System.Windows.Forms.Button BrowersButton;
        private System.Windows.Forms.TabPage Installed;
        private new System.Windows.Forms.TabControl Select;
        private System.Windows.Forms.Button DisableButton;
        private System.Windows.Forms.Button InstallButton;
        private System.Windows.Forms.Label ModTitleLabel;
        private System.Windows.Forms.Label ModVersionLabel;
        private System.Windows.Forms.Label ModAuthorLabel;
        private System.Windows.Forms.ListView ModList;
        private System.Windows.Forms.ColumnHeader ModName;
        private System.Windows.Forms.ColumnHeader ModVersion;
        private System.Windows.Forms.Button OPENMODSFOLDER;
        private System.Windows.Forms.Button ModHomePage;
        private System.Windows.Forms.TabPage Disable;
        private System.Windows.Forms.ListView DisableModList;
        private System.Windows.Forms.ColumnHeader DisableModName;
        private System.Windows.Forms.ColumnHeader DisableModVersion;
        private System.Windows.Forms.TabPage OnlineMods;
        private System.Windows.Forms.Button OnlineInstall;
        private System.Windows.Forms.ListView OnlineModsList;
        private System.Windows.Forms.ColumnHeader OnlineModName;
        private System.Windows.Forms.ColumnHeader OnlineModVersion;
        private System.Windows.Forms.ProgressBar progressBarForOnline;
        private System.Windows.Forms.Label Loading;
        private System.Windows.Forms.Label AMMVersion;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem Option;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem patchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installModManagerToolStripMenuItem;
        private System.Windows.Forms.Button Refresh;
        private System.Windows.Forms.ToolStripMenuItem uninstallModManagerToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox ModDescription;
    }
}