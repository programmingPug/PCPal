namespace PCPalConfigurator
{
    partial class ConfiguratorForm
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabControl;
        private TabPage tab1602;
        private TabPage tabTFT;
        private TabPage tabOLED;
        private TabPage tabHelp;
        private TabPage tabAbout;

        private ComboBox cmbLine1;
        private TextBox txtLine1;
        private TextBox txtLine1Post;
        private ComboBox cmbLine2;
        private TextBox txtLine2;
        private TextBox txtLine2Post;
        private Button btnSave;

        private Label lblLine1;
        private Label lblLine1Prefix;
        private Label lblLine1Suffix;
        private Label lblLine2;
        private Label lblLine2Prefix;
        private Label lblLine2Suffix;

        // OLED tab controls
        private TextBox txtOledMarkup;
        private Panel panelOledButtons;
        private Button btnInsertIcon;
        private Button btnLoadOledExample;
        private Button btnSaveOled;
        private Panel panelOledPreview;
        private Button btnPreview;
        private Label lblPreviewHeader;

        // Help tab controls
        private TextBox txtHelpContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.tab1602 = new TabPage("1602 LCD");
            this.tabTFT = new TabPage("4.6\" TFT LCD");
            this.tabOLED = new TabPage("OLED Display");
            this.tabHelp = new TabPage("Help");
            this.tabAbout = new TabPage("About");

            this.cmbLine1 = new ComboBox();
            this.txtLine1 = new TextBox();
            this.txtLine1Post = new TextBox();
            this.cmbLine2 = new ComboBox();
            this.txtLine2 = new TextBox();
            this.txtLine2Post = new TextBox();
            this.btnSave = new Button();

            this.lblLine1 = new Label();
            this.lblLine1Prefix = new Label();
            this.lblLine1Suffix = new Label();
            this.lblLine2 = new Label();
            this.lblLine2Prefix = new Label();
            this.lblLine2Suffix = new Label();

            // OLED tab controls
            this.txtOledMarkup = new TextBox();
            this.panelOledButtons = new Panel();
            this.btnInsertIcon = new Button();
            this.btnLoadOledExample = new Button();
            this.btnSaveOled = new Button();
            this.panelOledPreview = new Panel();
            this.btnPreview = new Button();
            this.lblPreviewHeader = new Label();

            // Help tab controls
            this.txtHelpContent = new TextBox();

            // === TabControl ===
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.TabPages.Add(this.tab1602);
            this.tabControl.TabPages.Add(this.tabTFT);
            this.tabControl.TabPages.Add(this.tabOLED);
            this.tabControl.TabPages.Add(this.tabHelp);
            this.tabControl.TabPages.Add(this.tabAbout);
            this.Controls.Add(this.tabControl);

            // === Line 1 UI ===
            this.lblLine1.Text = "Line 1:";
            this.lblLine1.Location = new System.Drawing.Point(20, 20);
            this.lblLine1.AutoSize = true;

            this.cmbLine1.Location = new System.Drawing.Point(100, 18);
            this.cmbLine1.Size = new System.Drawing.Size(200, 21);

            this.lblLine1Prefix.Text = "Prefix:";
            this.lblLine1Prefix.Location = new System.Drawing.Point(20, 50);
            this.lblLine1Prefix.AutoSize = true;

            this.txtLine1.Location = new System.Drawing.Point(100, 48);
            this.txtLine1.Size = new System.Drawing.Size(200, 21);

            this.lblLine1Suffix.Text = "Suffix / Units:";
            this.lblLine1Suffix.Location = new System.Drawing.Point(320, 50);
            this.lblLine1Suffix.AutoSize = true;

            this.txtLine1Post.Location = new System.Drawing.Point(420, 48);
            this.txtLine1Post.Size = new System.Drawing.Size(120, 21);

            // === Line 2 UI ===
            this.lblLine2.Text = "Line 2:";
            this.lblLine2.Location = new System.Drawing.Point(20, 90);
            this.lblLine2.AutoSize = true;

            this.cmbLine2.Location = new System.Drawing.Point(100, 88);
            this.cmbLine2.Size = new System.Drawing.Size(200, 21);

            this.lblLine2Prefix.Text = "Prefix:";
            this.lblLine2Prefix.Location = new System.Drawing.Point(20, 120);
            this.lblLine2Prefix.AutoSize = true;

            this.txtLine2.Location = new System.Drawing.Point(100, 118);
            this.txtLine2.Size = new System.Drawing.Size(200, 21);

            this.lblLine2Suffix.Text = "Suffix / Units:";
            this.lblLine2Suffix.Location = new System.Drawing.Point(320, 120);
            this.lblLine2Suffix.AutoSize = true;

            this.txtLine2Post.Location = new System.Drawing.Point(420, 118);
            this.txtLine2Post.Size = new System.Drawing.Size(120, 21);

            // === Save Button (renamed from Apply) ===
            this.btnSave.Text = "Save";
            this.btnSave.Location = new System.Drawing.Point(20, 170);
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // === Add to 1602 Tab ===
            this.tab1602.Controls.AddRange(new Control[]
            {
                lblLine1, cmbLine1, lblLine1Prefix, txtLine1, lblLine1Suffix, txtLine1Post,
                lblLine2, cmbLine2, lblLine2Prefix, txtLine2, lblLine2Suffix, txtLine2Post,
                btnSave
            });

            // === OLED Tab Setup ===
            // Main panel for layout
            Panel oledMainPanel = new Panel();
            oledMainPanel.Dock = DockStyle.Fill;
            this.tabOLED.Controls.Add(oledMainPanel);

            // Markup editor
            this.txtOledMarkup = new TextBox();
            this.txtOledMarkup.Multiline = true;
            this.txtOledMarkup.ScrollBars = ScrollBars.Vertical;
            this.txtOledMarkup.Font = new Font("Consolas", 9.75F);
            this.txtOledMarkup.TextChanged += new EventHandler(this.txtOledMarkup_TextChanged);
            this.txtOledMarkup.Size = new Size(580, 130);
            this.txtOledMarkup.Location = new Point(10, 10);
            oledMainPanel.Controls.Add(this.txtOledMarkup);

            // Button panel
            this.panelOledButtons = new Panel();
            this.panelOledButtons.Size = new Size(580, 40);
            this.panelOledButtons.Location = new Point(10, 150);
            oledMainPanel.Controls.Add(this.panelOledButtons);

            this.btnInsertIcon = new Button();
            this.btnInsertIcon.Text = "Insert Icon";
            this.btnInsertIcon.Location = new Point(0, 7);
            this.btnInsertIcon.Size = new Size(120, 26);
            this.btnInsertIcon.Click += new EventHandler(this.btnInsertIcon_Click);
            this.panelOledButtons.Controls.Add(this.btnInsertIcon);

            this.btnLoadOledExample = new Button();
            this.btnLoadOledExample.Text = "Load Example";
            this.btnLoadOledExample.Location = new Point(130, 7);
            this.btnLoadOledExample.Size = new Size(120, 26);
            this.btnLoadOledExample.Click += new EventHandler(this.btnLoadOledExample_Click);
            this.panelOledButtons.Controls.Add(this.btnLoadOledExample);

            this.btnPreview = new Button();
            this.btnPreview.Text = "Update Preview";
            this.btnPreview.Location = new Point(260, 7);
            this.btnPreview.Size = new Size(120, 26);
            this.btnPreview.Click += new EventHandler(this.btnPreview_Click);
            this.panelOledButtons.Controls.Add(this.btnPreview);

            this.btnSaveOled = new Button();
            this.btnSaveOled.Text = "Save";
            this.btnSaveOled.Location = new Point(480, 7);
            this.btnSaveOled.Size = new Size(100, 26);
            this.btnSaveOled.Click += new EventHandler(this.btnSave_Click); // Uses same event handler
            this.panelOledButtons.Controls.Add(this.btnSaveOled);

            // Preview header
            this.lblPreviewHeader = new Label();
            this.lblPreviewHeader.Text = "OLED Preview (256x64)";
            this.lblPreviewHeader.Location = new Point(10, 195);
            this.lblPreviewHeader.Size = new Size(580, 20);
            this.lblPreviewHeader.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPreviewHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lblPreviewHeader.BorderStyle = BorderStyle.FixedSingle;
            oledMainPanel.Controls.Add(this.lblPreviewHeader);

            // Preview panel
            this.panelOledPreview = new Panel();
            this.panelOledPreview.Location = new Point(10, 215);
            this.panelOledPreview.Size = new Size(580, 110);
            this.panelOledPreview.BackColor = System.Drawing.Color.Black;
            this.panelOledPreview.BorderStyle = BorderStyle.FixedSingle;
            this.panelOledPreview.Paint += new PaintEventHandler(this.panelOledPreview_Paint);
            oledMainPanel.Controls.Add(this.panelOledPreview);

            // === Help Tab Setup ===
            this.txtHelpContent = new TextBox();
            this.txtHelpContent.Dock = DockStyle.Fill;
            this.txtHelpContent.Multiline = true;
            this.txtHelpContent.ReadOnly = true;
            this.txtHelpContent.ScrollBars = ScrollBars.Vertical;
            this.txtHelpContent.Font = new Font("Segoe UI", 9F);
            this.tabHelp.Controls.Add(this.txtHelpContent);

            // === TFT Tab Placeholder ===
            Label lblTFT = new Label()
            {
                Text = "TFT configuration coming soon...",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.tabTFT.Controls.Add(lblTFT);

            // === About Tab ===
            Label lblAbout = new Label()
            {
                Text = "PCPal Display Configurator\nFor ThermalTake Tower XXX\nVersion 1.0.0\n© 2025 Christopher Koch aka NinjaPug",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.tabAbout.Controls.Add(lblAbout);

            // === Form ===
            this.Text = "Display Configurator";
            this.ClientSize = new System.Drawing.Size(600, 380); // Adjusted size for compact layout
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}