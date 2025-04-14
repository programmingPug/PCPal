using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PCPalConfigurator.UI
{
    /// <summary>
    /// Dialog for browsing and selecting icons
    /// </summary>
    public partial class IconBrowser : Form
    {
        private string _selectedIconPath = "";
        private int _selectedIconWidth = 0;
        private int _selectedIconHeight = 0;
        public string SelectedIconName { get; private set; } = "";

        public IconBrowser()
        {
            InitializeComponent();
        }

        private void IconBrowser_Load(object? sender, EventArgs e)
        {
            // Load last used directory from settings if available
            string lastDir = Properties.Settings.Default.LastIconDirectory;
            if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
            {
                txtIconDirectory.Text = lastDir;
                LoadIconsFromDirectory(lastDir);
            }
        }

        private void btnBrowse_Click(object? sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            if (!string.IsNullOrEmpty(txtIconDirectory.Text) && Directory.Exists(txtIconDirectory.Text))
            {
                dialog.InitialDirectory = txtIconDirectory.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtIconDirectory.Text = dialog.SelectedPath;
                LoadIconsFromDirectory(dialog.SelectedPath);

                // Save the directory for next time
                Properties.Settings.Default.LastIconDirectory = dialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private void LoadIconsFromDirectory(string directory)
        {
            try
            {
                lstIcons.Items.Clear();
                flowLayoutPanel.Controls.Clear();

                string[] xbmFiles = Directory.GetFiles(directory, "*.bin");
                if (xbmFiles.Length == 0)
                {
                    MessageBox.Show("No XBM icon files found in the selected directory.",
                        "No Icons Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (string file in xbmFiles)
                {
                    // Create a button for each icon
                    Button iconButton = new();

                    // Parse the XBM file to get dimensions
                    (int width, int height) = ParseXbmDimensions(file);

                    string fileName = Path.GetFileNameWithoutExtension(file);
                    iconButton.Text = $"{fileName} ({width}x{height})";
                    iconButton.Tag = new IconInfo
                    {
                        Path = file,
                        Name = fileName,
                        Width = width,
                        Height = height
                    };

                    iconButton.Size = new Size(150, 40);
                    iconButton.Click += IconButton_Click;
                    flowLayoutPanel.Controls.Add(iconButton);
                }

                lblStatus.Text = $"Found {xbmFiles.Length} XBM files";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icons: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (int width, int height) ParseXbmDimensions(string filePath)
        {
            int width = 0;
            int height = 0;

            try
            {
                // Read the first few lines of the XBM file
                using StreamReader reader = new(filePath);
                string line;
                while ((line = reader.ReadLine()) != null && (width == 0 || height == 0))
                {
                    // Look for width and height definitions in XBM format
                    if (line.Contains("_width"))
                    {
                        width = ExtractNumberFromLine(line);
                    }
                    else if (line.Contains("_height"))
                    {
                        height = ExtractNumberFromLine(line);
                    }
                }
            }
            catch
            {
                // If we can't parse, default to unknown dimensions
                width = 0;
                height = 0;
            }

            return (width, height);
        }

        private int ExtractNumberFromLine(string line)
        {
            // Extract the numeric value from a line like "#define icon_width 16"
            try
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[2], out int result))
                    {
                        return result;
                    }
                }
            }
            catch { }

            return 0;
        }

        private void IconButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is IconInfo info)
            {
                // Highlight the selected button
                foreach (Control control in flowLayoutPanel.Controls)
                {
                    if (control is Button btn)
                    {
                        btn.BackColor = SystemColors.Control;
                    }
                }
                button.BackColor = Color.LightBlue;

                // Store the selected icon info
                SelectedIconName = info.Name;
                _selectedIconPath = info.Path;
                _selectedIconWidth = info.Width;
                _selectedIconHeight = info.Height;

                // Update UI
                txtIconName.Text = info.Name;
                txtIconDimensions.Text = $"{info.Width} x {info.Height}";
                numX.Value = 0;
                numY.Value = 0;
            }
        }

        private void btnInsert_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedIconName))
            {
                MessageBox.Show("Please select an icon first.",
                    "No Icon Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Mark as success and close
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public string GetIconMarkup()
        {
            if (string.IsNullOrEmpty(SelectedIconName))
                return string.Empty;

            return $"<icon x={numX.Value} y={numY.Value} name={SelectedIconName} />";
        }

        // Helper class for icon information
        private class IconInfo
        {
            public string Path { get; set; } = "";
            public string Name { get; set; } = "";
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}