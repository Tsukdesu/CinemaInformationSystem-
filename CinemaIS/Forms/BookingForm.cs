using CinemaIS.Data;
using CinemaIS.Models;
using System.Data.SQLite;

namespace CinemaIS.Forms
{
    public class BookingForm : Form
    {
        private int _sessionId;
        private int _seatsCount;
        private int _hallId;
        private HashSet<int> _bookedSeats = new();
        private int _selectedSeat = -1;
        private Panel pnlSeats;
        private Label lblInfo, lblSelected, lblTitle;
        private Button btnConfirm, btnCancel;

        public BookingForm(int sessionId)
        {
            _sessionId = sessionId;
            InitializeComponent();
            LoadSessionInfo();
            DrawSeats();
        }

        private void InitializeComponent()
        {
            this.Text = "Выбор места";
            this.Size = new Size(700, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 35);

            lblTitle = new Label
            {
                Text = "🎬 Выбор места в зале",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblInfo = new Label
            {
                Location = new Point(20, 50),
                Width = 650,
                Height = 40,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 200, 220),
                AutoSize = false
            };

            // Легенда
            var pnlLegend = new Panel { Location = new Point(20, 95), Width = 650, Height = 28 };
            AddLegend(pnlLegend, "Свободно", Color.FromArgb(0, 140, 70), 0);
            AddLegend(pnlLegend, "Занято", Color.FromArgb(180, 40, 40), 130);
            AddLegend(pnlLegend, "Выбрано", Color.FromArgb(255, 140, 0), 260);

            // Экран
            var lblScreen = new Label
            {
                Text = "▬▬▬▬▬▬▬▬  ЭКРАН  ▬▬▬▬▬▬▬▬",
                Location = new Point(20, 128),
                Width = 650,
                Height = 24,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 150, 200),
                TextAlign = ContentAlignment.MiddleCenter
            };

            pnlSeats = new Panel
            {
                Location = new Point(20, 158),
                Width = 650,
                Height = 320,
                BackColor = Color.FromArgb(25, 25, 42),
                AutoScroll = true
            };

            lblSelected = new Label
            {
                Text = "Выберите место",
                Location = new Point(20, 488),
                Width = 400,
                Height = 24,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(200, 200, 220)
            };

            btnConfirm = new Button
            {
                Text = "✅ Забронировать",
                Location = new Point(430, 483),
                Width = 150,
                Height = 36,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(590, 483),
                Width = 90,
                Height = 36,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(60, 60, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblTitle, lblInfo, pnlLegend, lblScreen, pnlSeats, lblSelected, btnConfirm, btnCancel });
        }

        private void AddLegend(Panel parent, string text, Color color, int x)
        {
            var box = new Panel { Location = new Point(x, 6), Width = 16, Height = 16, BackColor = color };
            var lbl = new Label { Text = text, Location = new Point(x + 20, 4), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(200, 200, 220) };
            parent.Controls.AddRange(new Control[] { box, lbl });
        }

        private void LoadSessionInfo()
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT m.title, s.session_time, h.hall_name, h.hall_id, h.seats_count, s.price
                           FROM Sessions s JOIN Movies m ON s.movie_id=m.movie_id JOIN Halls h ON s.hall_id=h.hall_id
                           WHERE s.session_id=@id";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", _sessionId);
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                lblInfo.Text = $"Фильм: {r.GetString(0)}  |  Время: {r.GetString(1)}  |  Зал: {r.GetString(2)}  |  Цена: {r.GetDecimal(5):F0} руб.";
                _hallId = r.GetInt32(3);
                _seatsCount = r.GetInt32(4);
            }

            // Занятые места
            string bookSql = "SELECT seat_number FROM Bookings WHERE session_id=@id";
            using var bookCmd = new SQLiteCommand(bookSql, conn);
            bookCmd.Parameters.AddWithValue("@id", _sessionId);
            using var bookReader = bookCmd.ExecuteReader();
            while (bookReader.Read()) _bookedSeats.Add(bookReader.GetInt32(0));
        }

        private void DrawSeats()
        {
            pnlSeats.Controls.Clear();
            int cols = 10;
            int seatW = 48, seatH = 36, gap = 6;
            int panelWidth = cols * (seatW + gap) + gap;

            for (int i = 1; i <= _seatsCount; i++)
            {
                int col = (i - 1) % cols;
                int row = (i - 1) / cols;
                int x = gap + col * (seatW + gap);
                int y = gap + row * (seatH + gap);

                bool booked = _bookedSeats.Contains(i);
                int seatNum = i;

                var btn = new Button
                {
                    Text = i.ToString(),
                    Location = new Point(x, y),
                    Size = new Size(seatW, seatH),
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    BackColor = booked ? Color.FromArgb(180, 40, 40) : Color.FromArgb(0, 140, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Enabled = !booked,
                    Cursor = booked ? Cursors.Default : Cursors.Hand,
                    Tag = seatNum
                };
                btn.FlatAppearance.BorderSize = 0;

                if (!booked)
                {
                    btn.Click += (s, e) =>
                    {
                        // Сбросить предыдущий
                        foreach (Control c in pnlSeats.Controls)
                            if (c is Button b && !_bookedSeats.Contains((int)b.Tag!))
                                b.BackColor = Color.FromArgb(0, 140, 70);

                        btn.BackColor = Color.FromArgb(255, 140, 0);
                        _selectedSeat = seatNum;
                        lblSelected.Text = $"Выбрано: место №{seatNum}";
                        btnConfirm.Enabled = true;
                    };
                }

                pnlSeats.Controls.Add(btn);
            }

            // Высота панели
            int rows = (int)Math.Ceiling(_seatsCount / (double)cols);
            int neededH = rows * (seatH + gap) + gap;
            pnlSeats.AutoScrollMinSize = new Size(panelWidth, neededH);
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (_selectedSeat == -1) return;

            // Двойная проверка
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string checkSql = "SELECT COUNT(*) FROM Bookings WHERE session_id=@sid AND seat_number=@seat";
            using var checkCmd = new SQLiteCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@sid", _sessionId);
            checkCmd.Parameters.AddWithValue("@seat", _selectedSeat);
            long exists = (long)checkCmd.ExecuteScalar()!;

            if (exists > 0)
            {
                MessageBox.Show("Это место уже занято. Пожалуйста, выберите другое.", "Место занято", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadSessionInfo();
                DrawSeats();
                return;
            }

            string insertSql = "INSERT INTO Bookings (user_id, session_id, seat_number, booking_date) VALUES (@uid, @sid, @seat, @date)";
            using var cmd = new SQLiteCommand(insertSql, conn);
            cmd.Parameters.AddWithValue("@uid", Program.CurrentUser!.UserId);
            cmd.Parameters.AddWithValue("@sid", _sessionId);
            cmd.Parameters.AddWithValue("@seat", _selectedSeat);
            cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            cmd.ExecuteNonQuery();

            MessageBox.Show($"✅ Место №{_selectedSeat} успешно забронировано!", "Бронирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
