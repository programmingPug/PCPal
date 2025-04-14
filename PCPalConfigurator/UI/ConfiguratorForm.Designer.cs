using PCPalConfigurator.Rendering;
using PCPalConfigurator.UI;

namespace PCPalConfigurator.UI
{
    partial class ConfiguratorForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tab1602;
        private System.Windows.Forms.TabPage tabTFT;
        private System.Windows.Forms.TabPage tabOLED;
        private System.Windows.Forms.TabPage tabHelp;
        private System.Windows.Forms.TabPage tabAbout;

        private System.Windows.Forms.ComboBox cmbLine1;
        private System.Windows.Forms.TextBox txtLine1;
        private System.Windows.Forms.TextBox txtLine1Post;
        private System.Windows.Forms.ComboBox cmbLine2;
        private System.Windows.Forms.TextBox txtLine2;
        private System.Windows.Forms.TextBox txtLine2Post;
        private System.Windows.Forms.Button btnSave;

        private System.Windows.Forms.Label lblLine1;
        private System.Windows.Forms.Label lblLine1Prefix;
        private System.Windows.Forms.Label lblLine1Suffix;
        private System.Windows.Forms.Label lblLine2;
        private System.Windows.Forms.Label lblLine2Prefix;
        private System.Windows.Forms.Label lblLine2Suffix;

        // OLED tab controls
        private System.Windows.Forms.TextBox txtOledMarkup;
        private System.Windows.Forms.Panel panelOledButtons;
        private System.Windows.Forms.Button btnInsertIcon;
        private System.Windows.Forms.Button btnLoadOledExample;
        private System.Windows.Forms.Button btnSaveOled;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Label lblPreviewHeader;
        private OledPreviewPanel oledPreviewPanel;

        // Help tab controls
        private System.Windows.Forms.TextBox txtHelpContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tab1602 = new System.Windows.Forms.TabPage("1602 LCD");
            this.tabTFT = new System.Windows.Forms.TabPage("4.6\" TFT LCD");
            this.tabOLED = new System.Windows.Forms.TabPage("OLED Display");
            this.tabHelp = new System.Windows.Forms.TabPage("Help");
            this.tabAbout = new System.Windows.Forms.TabPage("About");

            this.cmbLine1 = new System.Windows.Forms.ComboBox();
            this.txtLine1 = new System.Windows.Forms.TextBox();
            this.txtLine1Post = new System.Windows.Forms.TextBox();
            this.cmbLine2 = new System.Windows.Forms.ComboBox();
            this.txtLine2 = new System.Windows.Forms.TextBox();
            this.txtLine2Post = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();

            this.lblLine1 = new System.Windows.Forms.Label();
            this.lblLine1Prefix = new System.Windows.Forms.Label();
            this.lblLine1Suffix = new System.Windows.Forms.Label();
            this.lblLine2 = new System.Windows.Forms.Label();
            this.lblLine2Prefix = new System.Windows.Forms.Label();
            this.lblLine2Suffix = new System.Windows.Forms.Label();

            // OLED tab controls
            this.txtOledMarkup = new System.Windows.Forms.TextBox();
            this.panelOledButtons = new System.Windows.Forms.Panel();
            this.btnInsertIcon = new System.Windows.Forms.Button();
            this.btnLoadOledExample = new System.Windows.Forms.Button();
            this.btnSaveOled = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.lblPreviewHeader = new System.Windows.Forms.Label();
            this.oledPreviewPanel = new OledPreviewPanel();

            // Help tab controls
            this.txtHelpContent = new System.Windows.Forms.TextBox();

            // === TabControl ===
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
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

            // === Save Button ===
            this.btnSave.Text = "Save";
            this.btnSave.Location = new System.Drawing.Point(20, 170);
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // === Add to 1602 Tab ===
            this.tab1602.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblLine1, cmbLine1, lblLine1Prefix, txtLine1, lblLine1Suffix, txtLine1Post,
                lblLine2, cmbLine2, lblLine2Prefix, txtLine2, lblLine2Suffix, txtLine2Post,
                btnSave
            });

            // === OLED Tab Setup ===
            // Main panel for layout
            System.Windows.Forms.Panel oledMainPanel = new System.Windows.Forms.Panel();
            oledMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOLED.Controls.Add(oledMainPanel);

            // Markup editor
            this.txtOledMarkup = new System.Windows.Forms.TextBox();
            this.txtOledMarkup.Multiline = true;
            this.txtOledMarkup.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOledMarkup.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.txtOledMarkup.TextChanged += new System.EventHandler(this.txtOledMarkup_TextChanged);
            this.txtOledMarkup.Size = new System.Drawing.Size(580, 130);
            this.txtOledMarkup.Location = new System.Drawing.Point(10, 10);
            oledMainPanel.Controls.Add(this.txtOledMarkup);

            // Button panel
            this.panelOledButtons = new System.Windows.Forms.Panel();
            this.panelOledButtons.Size = new System.Drawing.Size(580, 40);
            this.panelOledButtons.Location = new System.Drawing.Point(10, 150);
            oledMainPanel.Controls.Add(this.panelOledButtons);

            this.btnInsertIcon = new System.Windows.Forms.Button();
            this.btnInsertIcon.Text = "Insert Icon";
            this.btnInsertIcon.Location = new System.Drawing.Point(0, 7);
            this.btnInsertIcon.Size = new System.Drawing.Size(120, 26);
            this.btnInsertIcon.Click += new System.EventHandler(this.btnInsertIcon_Click);
            this.panelOledButtons.Controls.Add(this.btnInsertIcon);

            this.btnLoadOledExample = new System.Windows.Forms.Button();
            this.btnLoadOledExample.Text = "Load Example";
            this.btnLoadOledExample.Location = new System.Drawing.Point(130, 7);
            this.btnLoadOledExample.Size = new System.Drawing.Size(120, 26);
            this.btnLoadOledExample.Click += new System.EventHandler(this.btnLoadOledExample_Click);
            this.panelOledButtons.Controls.Add(this.btnLoadOledExample);

            this.btnPreview = new System.Windows.Forms.Button();
            this.btnPreview.Text = "Update Preview";
            this.btnPreview.Location = new System.Drawing.Point(260, 7);
            this.btnPreview.Size = new System.Drawing.Size(120, 26);
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            this.panelOledButtons.Controls.Add(this.btnPreview);

            this.btnSaveOled = new System.Windows.Forms.Button();
            this.btnSaveOled.Text = "Save";
            this.btnSaveOled.Location = new System.Drawing.Point(480, 7);
            this.btnSaveOled.Size = new System.Drawing.Size(100, 26);
            this.btnSaveOled.Click += new System.EventHandler(this.btnSave_Click); // Uses same event handler
            this.panelOledButtons.Controls.Add(this.btnSaveOled);

            // Preview header
            this.lblPreviewHeader = new System.Windows.Forms.Label();
            this.lblPreviewHeader.Text = "OLED Preview (256x64)";
            this.lblPreviewHeader.Location = new System.Drawing.Point(10, 195);
            this.lblPreviewHeader.Size = new System.Drawing.Size(580, 20);
            this.lblPreviewHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPreviewHeader.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lblPreviewHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            oledMainPanel.Controls.Add(this.lblPreviewHeader);

            // Preview panel
            this.oledPreviewPanel.Location = new System.Drawing.Point(10, 215);
            this.oledPreviewPanel.Size = new System.Drawing.Size(580, 110);
            oledMainPanel.Controls.Add(this.oledPreviewPanel);

            // === Help Tab Setup ===
            this.txtHelpContent = new System.Windows.Forms.TextBox();
            this.txtHelpContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHelpContent.Multiline = true;
            this.txtHelpContent.ReadOnly = true;
            this.txtHelpContent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtHelpContent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabHelp.Controls.Add(this.txtHelpContent);

            // === TFT Tab Placeholder ===
            System.Windows.Forms.Label lblTFT = new System.Windows.Forms.Label()
            {
                Text = "TFT configuration coming soon...",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.tabTFT.Controls.Add(lblTFT);

            // === About Tab ===
            System.Windows.Forms.Label lblAbout = new System.Windows.Forms.Label()
            {
                Text = "PCPal Display Configurator\nFor ThermalTake Tower XXX\nVersion 1.0.0\n© 2025 Christopher Koch aka NinjaPug",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            this.tabAbout.Controls.Add(lblAbout);

            // === Form ===
            this.Text = "Display Configurator";
            this.ClientSize = new System.Drawing.Size(600, 380);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}