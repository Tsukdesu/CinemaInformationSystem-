namespace CinemaIS.Forms
{
    public class MainForm : Form
    {
        private Panel pnlSidebar, pnlContent;
        private Label lblTitle, lblUser;
        private Button btnMovies, btnSessions, btnBooking, btnAdmin, btnLogout;

        public MainForm()
        {
            InitializeComponent();
            SetupUserInfo();
            ShowMoviesPanel();
        }

        private void InitializeComponent()
        {
            this.Text = "🎬 Информационная система кинотеатра";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 600);
            this.BackColor = Color.FromArgb(20, 20, 35);

            // Сайдбар
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(15, 15, 28)
            };

            lblTitle = new Label
            {
                Text = "🎬 КИНОТЕАТР",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 10, 0, 0)
            };

            lblUser = new Label
            {
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(160, 160, 200),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnMovies = CreateNavButton("🎥  Фильмы", 0);
            btnSessions = CreateNavButton("🕐  Расписание", 1);
            btnBooking = CreateNavButton("🎟  Мои билеты", 2);
            btnAdmin = CreateNavButton("⚙  Управление", 3);
            btnLogout = CreateNavButton("🚪  Выход", 10);

            btnMovies.Click += (s, e) => { SetActiveBtn(btnMovies); ShowMoviesPanel(); };
            btnSessions.Click += (s, e) => { SetActiveBtn(btnSessions); ShowSessionsPanel(); };
            btnBooking.Click += (s, e) => { SetActiveBtn(btnBooking); ShowMyBookingsPanel(); };
            btnAdmin.Click += (s, e) => { SetActiveBtn(btnAdmin); ShowAdminPanel(); };
            btnLogout.Click += (s, e) => Logout();

            btnAdmin.Visible = Program.CurrentUser?.IsAdmin ?? false;

            var pnlButtons = new Panel { Dock = DockStyle.Fill };
            pnlButtons.Controls.Add(btnLogout);
            pnlButtons.Controls.Add(btnAdmin);
            pnlButtons.Controls.Add(btnBooking);
            pnlButtons.Controls.Add(btnSessions);
            pnlButtons.Controls.Add(btnMovies);

            pnlSidebar.Controls.Add(pnlButtons);
            pnlSidebar.Controls.Add(lblUser);
            pnlSidebar.Controls.Add(lblTitle);

            // Контент
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 42),
                Padding = new Padding(20)
            };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);

            SetActiveBtn(btnMovies);
        }

        private Button CreateNavButton(string text, int index)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(200, 200, 220),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Height = 52,
                Dock = DockStyle.Top,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 65);
            return btn;
        }

        private void SetActiveBtn(Button active)
        {
            foreach (Control c in pnlSidebar.Controls[0].Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(200, 200, 220);
                }
            }
            active.BackColor = Color.FromArgb(255, 140, 0);
            active.ForeColor = Color.White;
        }

        private void SetupUserInfo()
        {
            var user = Program.CurrentUser;
            if (user != null)
                lblUser.Text = $"👤 {user.Login}\n{user.Role}";
        }

        private void ClearContent()
        {
            pnlContent.Controls.Clear();
        }

        private void ShowMoviesPanel()
        {
            ClearContent();
            var panel = new MoviesPanel(false);
            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
        }

        private void ShowSessionsPanel()
        {
            ClearContent();
            var panel = new SessionsUserPanel();
            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
        }

        private void ShowMyBookingsPanel()
        {
            ClearContent();
            var panel = new MyBookingsPanel();
            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
        }

        private void ShowAdminPanel()
        {
            ClearContent();
            var panel = new AdminPanel();
            panel.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(panel);
        }

        private void Logout()
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Program.CurrentUser = null;
                this.Close();
            }
        }
    }
}
