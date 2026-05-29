namespace CinemaIS.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsAdmin => Role == "Администратор";
    }

    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public int Duration { get; set; }
        public string AgeLimit { get; set; } = "";
        public string Description { get; set; } = "";

        public override string ToString() => Title;
    }

    public class Hall
    {
        public int HallId { get; set; }
        public string HallName { get; set; } = "";
        public int SeatsCount { get; set; }

        public override string ToString() => HallName;
    }

    public class Session
    {
        public int SessionId { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public string SessionTime { get; set; } = "";
        public decimal Price { get; set; }

        // Для отображения
        public string MovieTitle { get; set; } = "";
        public string HallName { get; set; } = "";

        public override string ToString() => $"{MovieTitle} | {SessionTime} | {HallName} | {Price} руб.";
    }

    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int SessionId { get; set; }
        public int SeatNumber { get; set; }
        public string BookingDate { get; set; } = "";

        // Для отображения
        public string MovieTitle { get; set; } = "";
        public string SessionTime { get; set; } = "";
        public string UserLogin { get; set; } = "";
        public string HallName { get; set; } = "";
    }
}
