using CinemaIS.Data;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class RegisterForm : Form
    {
        private Label lblTitle, lblLogin, lblPassword, lblConfirm;
        private TextBox txtLogin, txtPassword, txtConfirm;
        private Button btnRegister, btnCancel;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Регистрация нового пользователя";
            this.Size = new Size(400, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 35);

            lblTitle = new Label
            {
                Text = "РЕГИСТРАЦИЯ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                AutoSize = false,
                Width = 360,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20)
            };

            lblLogin = new Label { Text = "Логин:", Font = new Font("Segoe UI", 10), ForeColor = Color.White, Location = new Point(40, 75), AutoSize = true };
            txtLogin = new TextBox { Location = new Point(40, 97), Width = 300, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(45, 45, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            lblPassword = new Label { Text = "Пароль:", Font = new Font("Segoe UI", 10), ForeColor = Color.White, Location = new Point(40, 130), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(40, 152), Width = 300, Font = new Font("Segoe UI", 10), PasswordChar = '●', BackColor = Color.FromArgb(45, 45, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            lblConfirm = new Label { Text = "Подтвердите пароль:", Font = new Font("Segoe UI", 10), ForeColor = Color.White, Location = new Point(40, 185), AutoSize = true };
            txtConfirm = new TextBox { Location = new Point(40, 207), Width = 300, Font = new Font("Segoe UI", 10), PasswordChar = '●', BackColor = Color.FromArgb(45, 45, 70), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            btnRegister = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(40, 260),
                Width = 180,
                Height = 38,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(240, 260),
                Width = 100,
                Height = 38,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, lblLogin, txtLogin, lblPassword, txtPassword, lblConfirm, txtConfirm, btnRegister, btnCancel });
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirm.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            { MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (password != confirm)
            { MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (password.Length < 4)
            { MessageBox.Show("Пароль должен содержать минимум 4 символа.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                string sql = "INSERT INTO Users (login, password, role) VALUES (@login, @password, 'Пользователь')";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (SQLiteException)
            {
                MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
