using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class SessionsUserPanel : UserControl
    {
        private DataGridView dgvSessions;
        private Button btnBook, btnRefresh;
        private Label lblTitle;

        public SessionsUserPanel()
        {
            InitializeComponent();
            LoadSessions();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            lblTitle = new Label
            {
                Text = "🕐 Расписание киносеансов",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(0, 0),
                AutoSize = true
            };

            var pnlTop = new Panel { Location = new Point(0, 45), Width = 1000, Height = 44 };

            btnRefresh = new Button
            {
                Text = "↻ Обновить",
                Location = new Point(0, 4),
                Width = 120,
                Height = 34,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 100, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadSessions();

            btnBook = new Button
            {
                Text = "🎟 Забронировать",
                Location = new Point(140, 4),
                Width = 160,
                Height = 34,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBook.FlatAppearance.BorderSize = 0;
            btnBook.Click += BtnBook_Click;

            pnlTop.Controls.AddRange(new Control[] { btnRefresh, btnBook });

            dgvSessions = new DataGridView
            {
                Location = new Point(0, 100),
                BackgroundColor = Color.FromArgb(30, 30, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 10),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 36 },
                GridColor = Color.FromArgb(50, 50, 80),
                MultiSelect = false
            };
            StyleGrid(dgvSessions);

            this.Controls.AddRange(new Control[] { lblTitle, pnlTop, dgvSessions });
            this.Resize += (s, e) => ResizeControls();
        }

        internal static void StyleGrid(DataGridView grid)
        {
            grid.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 50);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 140, 0);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Padding = new Padding(4);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 15, 28);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(255, 180, 0);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 58);
        }

        private void ResizeControls()
        {
            dgvSessions.Width = this.Width - 10;
            dgvSessions.Height = this.Height - 110;
        }

        public void LoadSessions()
        {
            dgvSessions.Columns.Clear();
            dgvSessions.Rows.Clear();

            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "session_id", FillWeight = 5 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фильм", Name = "title", FillWeight = 28 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Жанр", Name = "genre", FillWeight = 14 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата и время", Name = "session_time", FillWeight = 18 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Зал", Name = "hall_name", FillWeight = 12 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена (руб.)", Name = "price", FillWeight = 10 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Свободных мест", Name = "free", FillWeight = 13 });

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"
                SELECT s.session_id, m.title, m.genre, s.session_time, h.hall_name, s.price,
                       (h.seats_count - (SELECT COUNT(*) FROM Bookings b WHERE b.session_id = s.session_id)) AS free_seats
                FROM Sessions s
                JOIN Movies m ON s.movie_id = m.movie_id
                JOIN Halls h ON s.hall_id = h.hall_id
                ORDER BY s.session_time";

            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int free = reader.GetInt32(6);
                int rowIdx = dgvSessions.Rows.Add(
                    reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                    reader.GetString(3), reader.GetString(4),
                    $"{reader.GetDecimal(5):F0}", free
                );
                if (free == 0)
                    dgvSessions.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(180, 60, 60);
            }
            ResizeControls();
        }

        private void BtnBook_Click(object? sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сеанс.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int sessionId = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["session_id"].Value);
            int free = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["free"].Value);

            if (free == 0)
            {
                MessageBox.Show("На этот сеанс нет свободных мест.", "Нет мест", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dlg = new BookingForm(sessionId);
            if (dlg.ShowDialog() == DialogResult.OK) LoadSessions();
        }
    }
}
