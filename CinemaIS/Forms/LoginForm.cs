using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class LoginForm : Form
    {
        private Label lblTitle, lblLogin, lblPassword;
        private TextBox txtLogin, txtPassword;
        private Button btnLogin, btnRegister;
        private Panel pnlMain;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Информационная система кинотеатра — Вход";
            this.Size = new Size(420, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 35);

            pnlMain = new Panel
            {
                Size = new Size(340, 280),
                Location = new Point(40, 40),
                BackColor = Color.FromArgb(30, 30, 50),
            };

            lblTitle = new Label
            {
                Text = "🎬 ВХОД В СИСТЕМУ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                AutoSize = false,
                Width = 340,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 20)
            };

            lblLogin = new Label
            {
                Text = "Логин:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(30, 80),
                AutoSize = true
            };

            txtLogin = new TextBox
            {
                Location = new Point(30, 102),
                Width = 280,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(45, 45, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(30, 135),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(30, 157),
                Width = 280,
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●',
                BackColor = Color.FromArgb(45, 45, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(30, 210),
                Width = 130,
                Height = 38,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnRegister = new Button
            {
                Text = "Регистрация",
                Location = new Point(180, 210),
                Width = 130,
                Height = 38,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;

            pnlMain.Controls.AddRange(new Control[] { lblTitle, lblLogin, txtLogin, lblPassword, txtPassword, btnLogin, btnRegister });
            this.Controls.Add(pnlMain);

            // Подсказка
            var lblHint = new Label
            {
                Text = "Администратор по умолчанию: admin / admin123",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(120, 120, 160),
                Location = new Point(40, 330),
                AutoSize = true
            };
            this.Controls.Add(lblHint);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "SELECT user_id, login, password, role FROM Users WHERE login=@login AND password=@password";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Program.CurrentUser = new User
                {
                    UserId = reader.GetInt32(0),
                    Login = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3)
                };

                this.Hide();
                var main = new MainForm();
                main.FormClosed += (s, args) => this.Close();
                main.Show();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear();
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            var reg = new RegisterForm();
            reg.ShowDialog(this);
        }
    }
}
