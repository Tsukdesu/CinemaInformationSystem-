using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class MoviesPanel : UserControl
    {
        private DataGridView dgvMovies;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh;
        private TextBox txtSearch;
        private Label lblTitle;
        private bool _adminMode;

        public MoviesPanel(bool adminMode)
        {
            _adminMode = adminMode;
            InitializeComponent();
            LoadMovies();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(25, 25, 42);

            lblTitle = new Label
            {
                Text = "🎥 Список фильмов",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(0, 0),
                AutoSize = true
            };

            var pnlTop = new Panel { Location = new Point(0, 45), Width = 1000, Height = 44 };

            txtSearch = new TextBox
            {
                PlaceholderText = "🔍 Поиск по названию или жанру...",
                Location = new Point(0, 8),
                Width = 320,
                Height = 30,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(40, 40, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadMovies(txtSearch.Text);

            btnRefresh = CreateButton("↻ Обновить", Color.FromArgb(60, 100, 160), 340);
            btnRefresh.Click += (s, e) => LoadMovies();

            pnlTop.Controls.AddRange(new Control[] { txtSearch, btnRefresh });

            if (_adminMode)
            {
                btnAdd = CreateButton("+ Добавить", Color.FromArgb(0, 140, 70), 460);
                btnEdit = CreateButton("✏ Изменить", Color.FromArgb(180, 120, 0), 580);
                btnDelete = CreateButton("🗑 Удалить", Color.FromArgb(180, 40, 40), 700);

                btnAdd.Click += BtnAdd_Click;
                btnEdit.Click += BtnEdit_Click;
                btnDelete.Click += BtnDelete_Click;

                pnlTop.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
            }

            dgvMovies = new DataGridView
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

            dgvMovies.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 50);
            dgvMovies.DefaultCellStyle.ForeColor = Color.White;
            dgvMovies.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 140, 0);
            dgvMovies.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvMovies.DefaultCellStyle.Padding = new Padding(4);

            dgvMovies.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 15, 28);
            dgvMovies.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(255, 180, 0);
            dgvMovies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMovies.EnableHeadersVisualStyles = false;

            dgvMovies.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 58);

            this.Controls.AddRange(new Control[] { lblTitle, pnlTop, dgvMovies });
            this.Resize += (s, e) => ResizeControls();
        }

        private Button CreateButton(string text, Color color, int x)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 4),
                Width = 110,
                Height = 34,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void ResizeControls()
        {
            dgvMovies.Width = this.Width - 10;
            dgvMovies.Height = this.Height - 110;
        }

        public void LoadMovies(string filter = "")
        {
            dgvMovies.Columns.Clear();
            dgvMovies.Rows.Clear();

            dgvMovies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Name = "movie_id", Width = 50, FillWeight = 5 });
            dgvMovies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", Name = "title", FillWeight = 30 });
            dgvMovies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Жанр", Name = "genre", FillWeight = 15 });
            dgvMovies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Длит. (мин)", Name = "duration", FillWeight = 12 });
            dgvMovies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Возраст", Name = "age_limit", FillWeight = 10 });
            dgvMovies.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Описание", Name = "description", FillWeight = 28 });

            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = "SELECT movie_id, title, genre, duration, age_limit, description FROM Movies";
            if (!string.IsNullOrEmpty(filter))
                sql += " WHERE title LIKE @f OR genre LIKE @f";
            sql += " ORDER BY title";

            using var cmd = new SQLiteCommand(sql, conn);
            if (!string.IsNullOrEmpty(filter))
                cmd.Parameters.AddWithValue("@f", $"%{filter}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dgvMovies.Rows.Add(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt32(3),
                    reader.GetString(4),
                    reader.IsDBNull(5) ? "" : reader.GetString(5)
                );
            }

            ResizeControls();
        }

        private Movie? GetSelectedMovie()
        {
            if (dgvMovies.SelectedRows.Count == 0) return null;
            var row = dgvMovies.SelectedRows[0];
            return new Movie
            {
                MovieId = Convert.ToInt32(row.Cells["movie_id"].Value),
                Title = row.Cells["title"].Value?.ToString() ?? "",
                Genre = row.Cells["genre"].Value?.ToString() ?? "",
                Duration = Convert.ToInt32(row.Cells["duration"].Value),
                AgeLimit = row.Cells["age_limit"].Value?.ToString() ?? "",
                Description = row.Cells["description"].Value?.ToString() ?? ""
            };
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var dlg = new MovieEditForm(null);
            if (dlg.ShowDialog() == DialogResult.OK) LoadMovies();
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            var movie = GetSelectedMovie();
            if (movie == null) { MessageBox.Show("Выберите фильм.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var dlg = new MovieEditForm(movie);
            if (dlg.ShowDialog() == DialogResult.OK) LoadMovies();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            var movie = GetSelectedMovie();
            if (movie == null) { MessageBox.Show("Выберите фильм.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show($"Удалить фильм «{movie.Title}»?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();
                using var cmd = new SQLiteCommand("DELETE FROM Movies WHERE movie_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", movie.MovieId);
                cmd.ExecuteNonQuery();
                LoadMovies();
            }
        }
    }
}
