namespace PCPalConfigurator
{
    partial class ConfiguratorForm
    {
        private System.ComponentModel.IContainer components = null;

        private TabControl tabControl;
        private TabPage tab1602;
        private TabPage tabTFT;
        private TabPage tabAbout;

        private ComboBox cmbLine1;
        private TextBox txtLine1;
        private TextBox txtLine1Post;
        private ComboBox cmbLine2;
        private TextBox txtLine2;
        private TextBox txtLine2Post;
        private Button btnApply;

        private Label lblLine1;
        private Label lblLine1Prefix;
        private Label lblLine1Suffix;
        private Label lblLine2;
        private Label lblLine2Prefix;
        private Label lblLine2Suffix;

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
            this.tabAbout = new TabPage("About");

            this.cmbLine1 = new ComboBox();
            this.txtLine1 = new TextBox();
            this.txtLine1Post = new TextBox();
            this.cmbLine2 = new ComboBox();
            this.txtLine2 = new TextBox();
            this.txtLine2Post = new TextBox();
            this.btnApply = new Button();

            this.lblLine1 = new Label();
            this.lblLine1Prefix = new Label();
            this.lblLine1Suffix = new Label();
            this.lblLine2 = new Label();
            this.lblLine2Prefix = new Label();
            this.lblLine2Suffix = new Label();

            // === TabControl ===
            this.tabControl.Location = new System.Drawing.Point(10, 10);
            this.tabControl.Size = new System.Drawing.Size(620, 280);
            this.tabControl.TabPages.Add(this.tab1602);
            this.tabControl.TabPages.Add(this.tabTFT);
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

            // === Apply Button ===
            this.btnApply.Text = "Apply";
            this.btnApply.Location = new System.Drawing.Point(20, 170);
            this.btnApply.Size = new System.Drawing.Size(100, 30);
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);

            // === Add to 1602 Tab ===
            this.tab1602.Controls.AddRange(new Control[]
            {
                lblLine1, cmbLine1, lblLine1Prefix, txtLine1, lblLine1Suffix, txtLine1Post,
                lblLine2, cmbLine2, lblLine2Prefix, txtLine2, lblLine2Suffix, txtLine2Post,
                btnApply
            });

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
            this.ClientSize = new System.Drawing.Size(640, 300);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
