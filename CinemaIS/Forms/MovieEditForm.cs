using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class MovieEditForm : Form
    {
        private Movie? _movie;
        private TextBox txtTitle, txtGenre, txtDuration, txtAgeLimit;
        private RichTextBox rtbDescription;
        private Button btnSave, btnCancel;

        public MovieEditForm(Movie? movie)
        {
            _movie = movie;
            InitializeComponent();
            if (movie != null) FillFields(movie);
        }

        private void InitializeComponent()
        {
            this.Text = _movie == null ? "Добавить фильм" : "Редактировать фильм";
            this.Size = new Size(460, 440);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(25, 25, 42);

            int y = 20;
            AddLabel("Название фильма:", y);
            txtTitle = AddTextBox(y + 22); y += 70;

            AddLabel("Жанр:", y);
            txtGenre = AddTextBox(y + 22); y += 70;

            AddLabel("Длительность (мин):", y);
            txtDuration = AddTextBox(y + 22); y += 70;

            AddLabel("Возрастное ограничение:", y);
            txtAgeLimit = AddTextBox(y + 22); y += 70;

            AddLabel("Описание:", y);
            rtbDescription = new RichTextBox
            {
                Location = new Point(30, y + 22),
                Width = 390,
                Height = 70,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(40, 40, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(rtbDescription);
            y += 100;

            btnSave = new Button
            {
                Text = "💾 Сохранить",
                Location = new Point(30, y),
                Width = 160,
                Height = 38,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(210, y),
                Width = 100,
                Height = 38,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.Height = y + 90;
        }

        private void AddLabel(string text, int y)
        {
            this.Controls.Add(new Label { Text = text, Location = new Point(30, y), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(180, 180, 210) });
        }

        private TextBox AddTextBox(int y)
        {
            var tb = new TextBox
            {
                Location = new Point(30, y),
                Width = 390,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(40, 40, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(tb);
            return tb;
        }

        private void FillFields(Movie m)
        {
            txtTitle.Text = m.Title;
            txtGenre.Text = m.Genre;
            txtDuration.Text = m.Duration.ToString();
            txtAgeLimit.Text = m.AgeLimit;
            rtbDescription.Text = m.Description;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtGenre.Text) ||
                string.IsNullOrWhiteSpace(txtDuration.Text) || string.IsNullOrWhiteSpace(txtAgeLimit.Text))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtDuration.Text, out int dur) || dur <= 0)
            {
                MessageBox.Show("Введите корректную длительность (целое число).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            if (_movie == null)
            {
                string sql = "INSERT INTO Movies (title, genre, duration, age_limit, description) VALUES (@t,@g,@d,@a,@desc)";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@g", txtGenre.Text.Trim());
                cmd.Parameters.AddWithValue("@d", dur);
                cmd.Parameters.AddWithValue("@a", txtAgeLimit.Text.Trim());
                cmd.Parameters.AddWithValue("@desc", rtbDescription.Text.Trim());
                cmd.ExecuteNonQuery();
            }
            else
            {
                string sql = "UPDATE Movies SET title=@t, genre=@g, duration=@d, age_limit=@a, description=@desc WHERE movie_id=@id";
                using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@t", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@g", txtGenre.Text.Trim());
                cmd.Parameters.AddWithValue("@d", dur);
                cmd.Parameters.AddWithValue("@a", txtAgeLimit.Text.Trim());
                cmd.Parameters.AddWithValue("@desc", rtbDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@id", _movie.MovieId);
                cmd.ExecuteNonQuery();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
