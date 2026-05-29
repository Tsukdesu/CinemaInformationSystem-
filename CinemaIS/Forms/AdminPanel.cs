namespace CinemaIS.Forms
{
    public class AdminPanel : UserControl
    {
        private TabControl tabControl;

        public AdminPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            var lblTitle = new Label
            {
                Text = "⚙ Панель администратора",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(0, 0),
                AutoSize = true
            };

            tabControl = new TabControl
            {
                Location = new Point(0, 45),
                Font = new Font("Segoe UI", 10),
                Appearance = TabAppearance.FlatButtons
            };

            var tabMovies = new TabPage("🎥 Фильмы") { BackColor = Color.FromArgb(25, 25, 42), Padding = new Padding(0) };
            var moviesPanel = new MoviesPanel(true);
            moviesPanel.Dock = DockStyle.Fill;
            tabMovies.Controls.Add(moviesPanel);

            var tabSessions = new TabPage("🕐 Сеансы") { BackColor = Color.FromArgb(25, 25, 42) };
            var sessPanel = new AdminSessionsPanel();
            sessPanel.Dock = DockStyle.Fill;
            tabSessions.Controls.Add(sessPanel);

            var tabBookings = new TabPage("🎟 Все бронирования") { BackColor = Color.FromArgb(25, 25, 42) };
            var bookPanel = new AllBookingsPanel();
            bookPanel.Dock = DockStyle.Fill;
            tabBookings.Controls.Add(bookPanel);

            var tabUsers = new TabPage("👥 Пользователи") { BackColor = Color.FromArgb(25, 25, 42) };
            var usersPanel = new UsersPanel();
            usersPanel.Dock = DockStyle.Fill;
            tabUsers.Controls.Add(usersPanel);

            tabControl.TabPages.AddRange(new[] { tabMovies, tabSessions, tabBookings, tabUsers });

            this.Controls.AddRange(new Control[] { lblTitle, tabControl });
            this.Resize += (s, e) =>
            {
                tabControl.Width = this.Width - 10;
                tabControl.Height = this.Height - 55;
            };
        }
    }
}
