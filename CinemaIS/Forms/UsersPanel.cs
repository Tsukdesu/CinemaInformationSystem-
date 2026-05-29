using CinemaIS.Data;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class UsersPanel : UserControl
    {
        private DataGridView dgv;
        private Button btnRefresh, btnDelete, btnChangeRole;
        private Label lblTitle, lblCount;

        public UsersPanel()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            lblTitle = new Label { Text = "Пользователи системы", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Color.FromArgb(255, 180, 0), Location = new Point(0, 5), AutoSize = true };

            var pnlTop = new Panel { Location = new Point(0, 38), Width = 1000, Height = 40 };

            btnRefresh = new Button { Text = "↻ Обновить", Location = new Point(0, 3), Width = 120, Height = 34, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(60, 100, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadUsers();

            btnChangeRole = new Button { Text = "🔄 Сменить роль", Location = new Point(130, 3), Width = 150, Height = 34, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(100, 60, 160), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnChangeRole.FlatAppearance.BorderSize = 0;
            btnChangeRole.Click += BtnChangeRole_Click;

            btnDelete = new Button { Text = "🗑 Удалить", Location = new Point(295, 3), Width = 120, Height = 34, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            lblCount = new Label { Location = new Point(435, 10), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(160, 160, 200) };
            pnlTop.Controls.AddRange(new Control[] { btnRefresh, btnChangeRole, btnDelete, lblCount });

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

        private void LoadUsers()
        {
            dgv.Columns.Clear(); dgv.Rows.Clear();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "uid", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Логин", Name = "login", FillWeight = 30 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Роль", Name = "role", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Броней", Name = "bookings", FillWeight = 15 });

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT u.user_id, u.login, u.role,
                                  (SELECT COUNT(*) FROM Bookings b WHERE b.user_id=u.user_id)
                           FROM Users u ORDER BY u.role, u.login";
            using var cmd = new SQLiteCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                int idx = dgv.Rows.Add(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3));
                if (r.GetString(2) == "Администратор")
                    dgv.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(255, 180, 0);
            }

            lblCount.Text = $"Пользователей: {dgv.Rows.Count}";
            dgv.Width = this.Width - 10;
            dgv.Height = this.Height - 92;
        }

        private void BtnChangeRole_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Выберите пользователя.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int uid = Convert.ToInt32(dgv.SelectedRows[0].Cells["uid"].Value);
            string login = dgv.SelectedRows[0].Cells["login"].Value?.ToString() ?? "";
            string currentRole = dgv.SelectedRows[0].Cells["role"].Value?.ToString() ?? "";

            if (uid == Program.CurrentUser?.UserId) { MessageBox.Show("Нельзя изменить роль самому себе.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string newRole = currentRole == "Администратор" ? "Пользователь" : "Администратор";
            if (MessageBox.Show($"Изменить роль пользователя «{login}» на «{newRole}»?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SQLiteCommand("UPDATE Users SET role=@r WHERE user_id=@id", conn);
                cmd.Parameters.AddWithValue("@r", newRole);
                cmd.Parameters.AddWithValue("@id", uid);
                cmd.ExecuteNonQuery();
                LoadUsers();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Выберите пользователя.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int uid = Convert.ToInt32(dgv.SelectedRows[0].Cells["uid"].Value);
            string login = dgv.SelectedRows[0].Cells["login"].Value?.ToString() ?? "";

            if (uid == Program.CurrentUser?.UserId) { MessageBox.Show("Нельзя удалить собственную учётную запись.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show($"Удалить пользователя «{login}» и все его бронирования?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd1 = new SQLiteCommand("DELETE FROM Bookings WHERE user_id=@id", conn);
                cmd1.Parameters.AddWithValue("@id", uid); cmd1.ExecuteNonQuery();
                using var cmd2 = new SQLiteCommand("DELETE FROM Users WHERE user_id=@id", conn);
                cmd2.Parameters.AddWithValue("@id", uid); cmd2.ExecuteNonQuery();
                LoadUsers();
            }
        }
    }
}
