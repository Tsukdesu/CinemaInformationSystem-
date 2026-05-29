using CinemaIS.Data;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class AllBookingsPanel : UserControl
    {
        private DataGridView dgv;
        private Button btnRefresh, btnDelete;
        private Label lblTitle, lblCount;

        public AllBookingsPanel()
        {
            InitializeComponent();
            LoadBookings();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            lblTitle = new Label { Text = "Все бронирования", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(255, 180, 0), Location = new Point(0, 5), AutoSize = true };

            var pnlTop = new Panel { Location = new Point(0, 38), Width = 1000, Height = 40 };

            btnRefresh = new Button { Text = "↻ Обновить", Location = new Point(0, 3), Width = 120, Height = 34, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(60, 100, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadBookings();

            btnDelete = new Button { Text = "🗑 Удалить запись", Location = new Point(130, 3), Width = 155, Height = 34, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            lblCount = new Label { Location = new Point(300, 10), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(160, 160, 200) };
            pnlTop.Controls.AddRange(new Control[] { btnRefresh, btnDelete, lblCount });

            dgv = new DataGridView
            {
                Location = new Point(0, 85),
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
            SessionsUserPanel.StyleGrid(dgv);

            this.Controls.AddRange(new Control[] { lblTitle, pnlTop, dgv });
            this.Resize += (s, e) => { dgv.Width = this.Width - 10; dgv.Height = this.Height - 92; };
        }

        private void LoadBookings()
        {
            dgv.Columns.Clear(); dgv.Rows.Clear();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "bid", FillWeight = 5 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Пользователь", Name = "user", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фильм", Name = "movie", FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата сеанса", Name = "stime", FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Зал", Name = "hall", FillWeight = 12 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Место", Name = "seat", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата брони", Name = "bdate", FillWeight = 17 });

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT b.booking_id, u.login, m.title, s.session_time, h.hall_name, b.seat_number, b.booking_date
                           FROM Bookings b
                           JOIN Users u ON b.user_id=u.user_id
                           JOIN Sessions s ON b.session_id=s.session_id
                           JOIN Movies m ON s.movie_id=m.movie_id
                           JOIN Halls h ON s.hall_id=h.hall_id
                           ORDER BY b.booking_date DESC";
            using var cmd = new SQLiteCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                dgv.Rows.Add(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetInt32(5), r.GetString(6));

            lblCount.Text = $"Всего записей: {dgv.Rows.Count}";
            dgv.Width = this.Width - 10;
            dgv.Height = this.Height - 92;
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Выберите запись.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["bid"].Value);
            if (MessageBox.Show("Удалить запись о бронировании?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SQLiteCommand("DELETE FROM Bookings WHERE booking_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
                LoadBookings();
            }
        }
    }
}
