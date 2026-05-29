using System.Data.SQLite;

namespace CinemaIS.Data
{
    public static class DatabaseHelper
    {
        private static string _dbPath = "cinema.db";
        private static string ConnectionString => $"Data Source={_dbPath};Version=3;";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public static void InitializeDatabase()
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    login TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    role TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Movies (
                    movie_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    genre TEXT NOT NULL,
                    duration INTEGER NOT NULL,
                    age_limit TEXT NOT NULL,
                    description TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS Halls (
                    hall_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    hall_name TEXT NOT NULL,
                    seats_count INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Sessions (
                    session_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    movie_id INTEGER NOT NULL,
                    hall_id INTEGER NOT NULL,
                    session_time TEXT NOT NULL,
                    price REAL NOT NULL,
                    FOREIGN KEY(movie_id) REFERENCES Movies(movie_id),
                    FOREIGN KEY(hall_id) REFERENCES Halls(hall_id)
                );

                CREATE TABLE IF NOT EXISTS Bookings (
                    booking_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    session_id INTEGER NOT NULL,
                    seat_number INTEGER NOT NULL,
                    booking_date TEXT NOT NULL,
                    FOREIGN KEY(user_id) REFERENCES Users(user_id),
                    FOREIGN KEY(session_id) REFERENCES Sessions(session_id)
                );
            ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Создаём администратора по умолчанию если его нет
            string checkAdmin = "SELECT COUNT(*) FROM Users WHERE login='admin'";
            using var checkCmd = new SQLiteCommand(checkAdmin, conn);
            long count = (long)checkCmd.ExecuteScalar()!;

            if (count == 0)
            {
                string insertAdmin = "INSERT INTO Users (login, password, role) VALUES ('admin', 'admin123', 'Администратор')";
                using var insertCmd = new SQLiteCommand(insertAdmin, conn);
                insertCmd.ExecuteNonQuery();
            }

            // Демо-залы
            string checkHalls = "SELECT COUNT(*) FROM Halls";
            using var hallCmd = new SQLiteCommand(checkHalls, conn);
            long hallCount = (long)hallCmd.ExecuteScalar()!;

            if (hallCount == 0)
            {
                string insertHalls = @"
                    INSERT INTO Halls (hall_name, seats_count) VALUES ('Зал №1', 50);
                    INSERT INTO Halls (hall_name, seats_count) VALUES ('Зал №2', 80);
                    INSERT INTO Halls (hall_name, seats_count) VALUES ('VIP-зал', 30);
                ";
                using var insertHallCmd = new SQLiteCommand(insertHalls, conn);
                insertHallCmd.ExecuteNonQuery();
            }

            // Демо-фильмы
            string checkMovies = "SELECT COUNT(*) FROM Movies";
            using var movCmd = new SQLiteCommand(checkMovies, conn);
            long movCount = (long)movCmd.ExecuteScalar()!;

            if (movCount == 0)
            {
                string insertMovies = @"
                    INSERT INTO Movies (title, genre, duration, age_limit, description) VALUES ('Дюна: Часть вторая', 'Фантастика', 166, '12+', 'Продолжение эпической саги о Поле Атрейдесе.');
                    INSERT INTO Movies (title, genre, duration, age_limit, description) VALUES ('Мастер и Маргарита', 'Драма', 157, '18+', 'Экранизация бессмертного романа Булгакова.');
                    INSERT INTO Movies (title, genre, duration, age_limit, description) VALUES ('Холоп 2', 'Комедия', 110, '12+', 'Продолжение популярной комедии.');
                ";
                using var insertMovCmd = new SQLiteCommand(insertMovies, conn);
                insertMovCmd.ExecuteNonQuery();
            }

            // Демо-сеансы
            string checkSessions = "SELECT COUNT(*) FROM Sessions";
            using var sesCmd = new SQLiteCommand(checkSessions, conn);
            long sesCount = (long)sesCmd.ExecuteScalar()!;

            if (sesCount == 0)
            {
                // Получаем ID фильмов и залов (могут быть любыми после вставки)
                string today = DateTime.Now.ToString("dd.MM.yyyy");
                string tomorrow = DateTime.Now.AddDays(1).ToString("dd.MM.yyyy");
                string dayAfter = DateTime.Now.AddDays(2).ToString("dd.MM.yyyy");

                string insertSessions = $@"
                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 10:00', 350
                        FROM Movies m, Halls h WHERE m.title='Дюна: Часть вторая' AND h.hall_name='Зал №1';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 13:30', 350
                        FROM Movies m, Halls h WHERE m.title='Дюна: Часть вторая' AND h.hall_name='Зал №2';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 16:00', 500
                        FROM Movies m, Halls h WHERE m.title='Дюна: Часть вторая' AND h.hall_name='VIP-зал';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 11:00', 400
                        FROM Movies m, Halls h WHERE m.title='Мастер и Маргарита' AND h.hall_name='Зал №1';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 19:00', 400
                        FROM Movies m, Halls h WHERE m.title='Мастер и Маргарита' AND h.hall_name='Зал №2';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 14:00', 300
                        FROM Movies m, Halls h WHERE m.title='Холоп 2' AND h.hall_name='Зал №2';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{today} 20:30', 300
                        FROM Movies m, Halls h WHERE m.title='Холоп 2' AND h.hall_name='Зал №1';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{tomorrow} 10:00', 350
                        FROM Movies m, Halls h WHERE m.title='Дюна: Часть вторая' AND h.hall_name='Зал №1';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{tomorrow} 15:00', 400
                        FROM Movies m, Halls h WHERE m.title='Мастер и Маргарита' AND h.hall_name='VIP-зал';

                    INSERT INTO Sessions (movie_id, hall_id, session_time, price)
                        SELECT m.movie_id, h.hall_id, '{dayAfter} 12:00', 300
                        FROM Movies m, Halls h WHERE m.title='Холоп 2' AND h.hall_name='Зал №1';
                ";
                using var insertSesCmd = new SQLiteCommand(insertSessions, conn);
                insertSesCmd.ExecuteNonQuery();
            }
        }
    }
}
