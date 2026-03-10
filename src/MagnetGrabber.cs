using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagnetGrabber
{
    public class MainForm : Form
    {
        private TextBox txtInput;
        private TextBox txtOutput;
        private Button btnExtract;
        private Label lblStatus;
        private Panel titleBar;
        private Button btnClose;
        private Label lblTitle;
        private Timer fadeTimer;
        private double fadeOpacity = 1.0;

        // Custom Colors for Dark Theme
        private Color bgColor = Color.FromArgb(30, 30, 30);
        private Color panelColor = Color.FromArgb(45, 45, 48);
        private Color primaryColor = Color.FromArgb(0, 122, 204);
        private Color primaryHover = Color.FromArgb(28, 151, 234);
        private Color textColor = Color.WhiteSmoke;
        private Color textDark = Color.FromArgb(150, 150, 150);
        private Color successColor = Color.FromArgb(40, 167, 69);
        private Color errorColor = Color.FromArgb(220, 53, 69);

        // Native methods for Borderless Form Dragging
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            // Basic Window Settings
            this.Text = "Magnet Grabber";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = bgColor;
            this.ForeColor = textColor;

            try
            {
                // Set the taskbar icon from the executable's embedded icon
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            SetupUI();
            
            // Setup Fade Timer for Animations
            fadeTimer = new Timer();
            fadeTimer.Interval = 50; // 50ms per tick
            fadeTimer.Tick += FadeTimer_Tick;
        }

        private void SetupUI()
        {
            // Custom Title Bar
            titleBar = new Panel();
            titleBar.BackColor = panelColor;
            titleBar.Height = 35;
            titleBar.Dock = DockStyle.Top;
            titleBar.MouseDown += TitleBar_MouseDown;
            this.Controls.Add(titleBar);

            lblTitle = new Label();
            lblTitle.Text = "Stremio Magnet Grabber";
            lblTitle.ForeColor = textColor;
            lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(10, 8);
            lblTitle.MouseDown += TitleBar_MouseDown; // Allow drag on label too
            titleBar.Controls.Add(lblTitle);

            btnClose = new Button();
            btnClose.Text = "X";
            btnClose.Size = new Size(35, 35);
            btnClose.Location = new Point(this.Width - 35, 0);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = panelColor;
            btnClose.ForeColor = textColor;
            btnClose.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = errorColor;
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = panelColor;
            titleBar.Controls.Add(btnClose);

            // Input Area
            Label lblInput = new Label();
            lblInput.Text = "Stremio URL'sini Yapıştırın:";
            lblInput.Location = new Point(25, 60);
            lblInput.AutoSize = true;
            lblInput.Font = new Font("Segoe UI", 9F);
            lblInput.ForeColor = textDark;
            this.Controls.Add(lblInput);

            txtInput = new TextBox();
            txtInput.Location = new Point(25, 85);
            txtInput.Size = new Size(450, 25);
            txtInput.Font = new Font("Segoe UI", 10F);
            txtInput.BackColor = panelColor;
            txtInput.ForeColor = textColor;
            txtInput.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtInput);

            // Action Button
            btnExtract = new Button();
            btnExtract.Text = "Mıknatısla! (Çıkar ve Kopyala)";
            btnExtract.Location = new Point(25, 125);
            btnExtract.Size = new Size(450, 45);
            btnExtract.FlatStyle = FlatStyle.Flat;
            btnExtract.FlatAppearance.BorderSize = 0;
            btnExtract.BackColor = primaryColor;
            btnExtract.ForeColor = Color.White;
            btnExtract.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnExtract.Cursor = Cursors.Hand;
            btnExtract.Click += BtnExtract_Click;
            btnExtract.MouseEnter += (s, e) => btnExtract.BackColor = primaryHover;
            btnExtract.MouseLeave += (s, e) => btnExtract.BackColor = primaryColor;
            this.Controls.Add(btnExtract);

            // Output Area
            Label lblOutput = new Label();
            lblOutput.Text = "Sonuç:";
            lblOutput.Location = new Point(25, 195);
            lblOutput.AutoSize = true;
            lblOutput.Font = new Font("Segoe UI", 9F);
            lblOutput.ForeColor = textDark;
            this.Controls.Add(lblOutput);

            txtOutput = new TextBox();
            txtOutput.Location = new Point(25, 220);
            txtOutput.Size = new Size(450, 25);
            txtOutput.Font = new Font("Segoe UI", 10F);
            txtOutput.BackColor = panelColor;
            txtOutput.ForeColor = textColor;
            txtOutput.BorderStyle = BorderStyle.FixedSingle;
            txtOutput.ReadOnly = true;
            this.Controls.Add(txtOutput);

            // Animated Status Label
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(25, 260);
            lblStatus.Size = new Size(450, 30);
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblStatus.ForeColor = textDark;
            this.Controls.Add(lblStatus);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void BtnExtract_Click(object sender, EventArgs e)
        {
            string url = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                ShowStatus("Lütfen bir URL girin!", errorColor, false);
                return;
            }

            try
            {
                // URL Decode
                string decodedUrl = Uri.UnescapeDataString(url);

                // Check for existing magnet link pattern
                Match magnetMatch = Regex.Match(decodedUrl, @"magnet:\?xt=urn:btih:[a-zA-Z0-9]+", RegexOptions.IgnoreCase);
                if (magnetMatch.Success)
                {
                    SetResult(magnetMatch.Value);
                    return;
                }

                // Check for 40 char hash
                Match hashMatch = Regex.Match(decodedUrl, @"(?<![a-zA-Z0-9])[a-fA-F0-9]{40}(?![a-zA-Z0-9])");
                if (hashMatch.Success)
                {
                    string magnet = "magnet:?xt=urn:btih:" + hashMatch.Value.ToLower();
                    SetResult(magnet);
                    return;
                }

                // Invalid format
                ShowStatus("Hata: Geçerli bir torrent kimliği (info hash) bulunamadı.", errorColor, false);
                txtOutput.Text = "";
            }
            catch (Exception ex)
            {
                ShowStatus("Hata oluştu: " + ex.Message, errorColor, false);
            }
        }

        private void SetResult(string magnet)
        {
            txtOutput.Text = magnet;
            Clipboard.SetText(magnet);
            ShowStatus("Başarılı: Magnet linki panoya kopyalandı! 📋", successColor, true);
        }

        private void ShowStatus(string message, Color color, bool animate)
        {
            fadeTimer.Stop();
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
            lblStatus.Visible = true;

            if (animate)
            {
                fadeOpacity = 1.0;
                
                // Keep the success message visible for 3 seconds before fading out
                System.Threading.Tasks.Task.Delay(3000).ContinueWith(t =>
                {
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.Invoke(new Action(() => fadeTimer.Start()));
                    }
                });
            }
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            fadeOpacity -= 0.05; // speed of fading out
            if (fadeOpacity <= 0)
            {
                fadeTimer.Stop();
                lblStatus.Text = "";
                lblStatus.Visible = false;
                fadeOpacity = 1.0;
            }
            else
            {
                // Simple color interpolation to blend into background text color
                Color target = bgColor;
                Color current = lblStatus.ForeColor;
                
                int r = (int)(target.R + (current.R - target.R) * fadeOpacity);
                int g = (int)(target.G + (current.G - target.G) * fadeOpacity);
                int b = (int)(target.B + (current.B - target.B) * fadeOpacity);
                
                lblStatus.ForeColor = Color.FromArgb(
                    Math.Max(0, Math.Min(255, r)), 
                    Math.Max(0, Math.Min(255, g)), 
                    Math.Max(0, Math.Min(255, b))
                );
            }
        }

        // Draw a slight border since BorderStyle.None makes edges blend in completely
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                         Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid,
                                         Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid,
                                         Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid,
                                         Color.FromArgb(60, 60, 60), 1, ButtonBorderStyle.Solid);
        }
    }
}
