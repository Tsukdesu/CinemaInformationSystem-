using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class SessionEditForm : Form
    {
        private ComboBox cmbMovie, cmbHall;
        private DateTimePicker dtpDate, dtpTime;
        private TextBox txtPrice;
        private Button btnSave, btnCancel;

        public SessionEditForm()
        {
            InitializeComponent();
            LoadMovies();
            LoadHalls();
        }

        private void InitializeComponent()
        {
            this.Text = "Добавить сеанс";
            this.Size = new Size(420, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(25, 25, 42);

            int y = 20;
            AddLabel("Фильм:", y);
            cmbMovie = new ComboBox { Location = new Point(30, y + 22), Width = 350, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(40, 40, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbMovie); y += 68;

            AddLabel("Зал:", y);
            cmbHall = new ComboBox { Location = new Point(30, y + 22), Width = 350, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(40, 40, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbHall); y += 68;

            AddLabel("Дата сеанса:", y);
            dtpDate = new DateTimePicker { Location = new Point(30, y + 22), Width = 160, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(dtpDate);

            AddLabel2("Время:", y, 210);
            dtpTime = new DateTimePicker { Location = new Point(240, y + 22), Width = 140, Format = DateTimePickerFormat.Time, ShowUpDown = true, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(dtpTime); y += 68;

            AddLabel("Цена билета (руб.):", y);
            txtPrice = new TextBox { Location = new Point(30, y + 22), Width = 180, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(40, 40, 65), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "350" };
            this.Controls.Add(txtPrice); y += 68;

            btnSave = new Button { Text = "💾 Сохранить", Location = new Point(30, y), Width = 150, Height = 38, Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(255, 140, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button { Text = "Отмена", Location = new Point(200, y), Width = 100, Height = 38, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(60, 60, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.Height = y + 90;
        }

        private void AddLabel(string text, int y) =>
            this.Controls.Add(new Label { Text = text, Location = new Point(30, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(180, 180, 210) });

        private void AddLabel2(string text, int y, int x) =>
            this.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(180, 180, 210) });

        private void LoadMovies()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT movie_id, title FROM Movies ORDER BY title", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                cmbMovie.Items.Add(new Movie { MovieId = r.GetInt32(0), Title = r.GetString(1) });
            if (cmbMovie.Items.Count > 0) cmbMovie.SelectedIndex = 0;
        }

        private void LoadHalls()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT hall_id, hall_name FROM Halls ORDER BY hall_name", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                cmbHall.Items.Add(new Hall { HallId = r.GetInt32(0), HallName = r.GetString(1) });
            if (cmbHall.Items.Count > 0) cmbHall.SelectedIndex = 0;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbMovie.SelectedItem == null || cmbHall.SelectedItem == null)
            { MessageBox.Show("Выберите фильм и зал.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (!decimal.TryParse(txtPrice.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            { MessageBox.Show("Введите корректную цену.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var movie = (Movie)cmbMovie.SelectedItem;
            var hall = (Hall)cmbHall.SelectedItem;
            string sessionTime = dtpDate.Value.ToString("dd.MM.yyyy") + " " + dtpTime.Value.ToString("HH:mm");

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "INSERT INTO Sessions (movie_id, hall_id, session_time, price) VALUES (@m, @h, @t, @p)";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@m", movie.MovieId);
            cmd.Parameters.AddWithValue("@h", hall.HallId);
            cmd.Parameters.AddWithValue("@t", sessionTime);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Сеанс добавлен!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
