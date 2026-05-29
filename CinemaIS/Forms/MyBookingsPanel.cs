using CinemaIS.Data;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class MyBookingsPanel : UserControl
    {
        private DataGridView dgvBookings;
        private Button btnCancel, btnRefresh;
        private Label lblTitle, lblCount;

        public MyBookingsPanel()
        {
            InitializeComponent();
            LoadBookings();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            lblTitle = new Label
            {
                Text = "🎟 Мои бронирования",
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
            btnRefresh.Click += (s, e) => LoadBookings();

            btnCancel = new Button
            {
                Text = "❌ Отменить бронь",
                Location = new Point(140, 4),
                Width = 160,
                Height = 34,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(180, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;

            lblCount = new Label
            {
                Location = new Point(320, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(160, 160, 200)
            };

            pnlTop.Controls.AddRange(new Control[] { btnRefresh, btnCancel, lblCount });

            dgvBookings = new DataGridView
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
            SessionsUserPanel.StyleGrid(dgvBookings);

            this.Controls.AddRange(new Control[] { lblTitle, pnlTop, dgvBookings });
            this.Resize += (s, e) => ResizeControls();
        }

        private void ResizeControls()
        {
            dgvBookings.Width = this.Width - 10;
            dgvBookings.Height = this.Height - 110;
        }

        private void LoadBookings()
        {
            dgvBookings.Columns.Clear();
            dgvBookings.Rows.Clear();

            dgvBookings.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "booking_id", FillWeight = 5 });
            dgvBookings.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фильм", Name = "title", FillWeight = 28 });
            dgvBookings.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата сеанса", Name = "session_time", FillWeight = 18 });
            dgvBookings.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Зал", Name = "hall_name", FillWeight = 12 });
            dgvBookings.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Место №", Name = "seat", FillWeight = 10 });
            dgvBookings.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата бронирования", Name = "booking_date", FillWeight = 18 });

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT b.booking_id, m.title, s.session_time, h.hall_name, b.seat_number, b.booking_date
                           FROM Bookings b
                           JOIN Sessions s ON b.session_id = s.session_id
                           JOIN Movies m ON s.movie_id = m.movie_id
                           JOIN Halls h ON s.hall_id = h.hall_id
                           WHERE b.user_id = @uid
                           ORDER BY b.booking_date DESC";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", Program.CurrentUser!.UserId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                dgvBookings.Rows.Add(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetInt32(4), r.GetString(5));

            lblCount.Text = $"Всего броней: {dgvBookings.Rows.Count}";
            ResizeControls();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0)
            { MessageBox.Show("Выберите бронирование.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int bookingId = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["booking_id"].Value);
            string movie = dgvBookings.SelectedRows[0].Cells["title"].Value?.ToString() ?? "";

            if (MessageBox.Show($"Отменить бронирование на фильм «{movie}»?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SQLiteCommand("DELETE FROM Bookings WHERE booking_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", bookingId);
                cmd.ExecuteNonQuery();
                LoadBookings();
                MessageBox.Show("Бронирование отменено.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
