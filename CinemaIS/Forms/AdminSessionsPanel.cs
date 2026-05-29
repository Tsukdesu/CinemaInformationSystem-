using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class AdminSessionsPanel : UserControl
    {
        private DataGridView dgvSessions;
        private Button btnAdd, btnDelete, btnRefresh;
        private Label lblTitle;

        public AdminSessionsPanel()
        {
            InitializeComponent();
            LoadSessions();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            lblTitle = new Label
            {
                Text = "Управление расписанием",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(0, 5),
                AutoSize = true
            };

            var pnlTop = new Panel { Location = new Point(0, 38), Width = 1000, Height = 40 };

            btnRefresh = CreateBtn("↻ Обновить", Color.FromArgb(60, 100, 160), 0);
            btnAdd = CreateBtn("+ Добавить сеанс", Color.FromArgb(0, 140, 70), 130);
            btnDelete = CreateBtn("🗑 Удалить", Color.FromArgb(180, 40, 40), 300);

            btnRefresh.Click += (s, e) => LoadSessions();
            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;

            pnlTop.Controls.AddRange(new Control[] { btnRefresh, btnAdd, btnDelete });

            dgvSessions = new DataGridView
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
            SessionsUserPanel.StyleGrid(dgvSessions);

            this.Controls.AddRange(new Control[] { lblTitle, pnlTop, dgvSessions });
            this.Resize += (s, e) => { dgvSessions.Width = this.Width - 10; dgvSessions.Height = this.Height - 92; };
        }

        private Button CreateBtn(string text, Color color, int x)
        {
            var b = new Button { Text = text, Location = new Point(x, 3), Width = 155, Height = 34, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LoadSessions()
        {
            dgvSessions.Columns.Clear(); dgvSessions.Rows.Clear();
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "session_id", FillWeight = 5 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фильм", Name = "title", FillWeight = 28 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата и время", Name = "session_time", FillWeight = 20 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Зал", Name = "hall_name", FillWeight = 15 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена (руб.)", Name = "price", FillWeight = 12 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Мест всего", Name = "seats", FillWeight = 10 });
            dgvSessions.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Свободно", Name = "free", FillWeight = 10 });

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT s.session_id, m.title, s.session_time, h.hall_name, s.price, h.seats_count,
                                  (h.seats_count - (SELECT COUNT(*) FROM Bookings b WHERE b.session_id = s.session_id))
                           FROM Sessions s JOIN Movies m ON s.movie_id=m.movie_id JOIN Halls h ON s.hall_id=h.hall_id
                           ORDER BY s.session_time";
            using var cmd = new SQLiteCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                dgvSessions.Rows.Add(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), $"{r.GetDecimal(4):F0}", r.GetInt32(5), r.GetInt32(6));

            dgvSessions.Width = this.Width - 10;
            dgvSessions.Height = this.Height - 92;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var dlg = new SessionEditForm();
            if (dlg.ShowDialog() == DialogResult.OK) LoadSessions();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvSessions.SelectedRows.Count == 0) { MessageBox.Show("Выберите сеанс.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int id = Convert.ToInt32(dgvSessions.SelectedRows[0].Cells["session_id"].Value);
            if (MessageBox.Show("Удалить сеанс? Все бронирования на него также будут удалены.", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd1 = new SQLiteCommand("DELETE FROM Bookings WHERE session_id=@id", conn);
                cmd1.Parameters.AddWithValue("@id", id); cmd1.ExecuteNonQuery();
                using var cmd2 = new SQLiteCommand("DELETE FROM Sessions WHERE session_id=@id", conn);
                cmd2.Parameters.AddWithValue("@id", id); cmd2.ExecuteNonQuery();
                LoadSessions();
            }
        }
    }
}
