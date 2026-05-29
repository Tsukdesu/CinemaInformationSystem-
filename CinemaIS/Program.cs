using CinemaIS.Data;
using CinemaIS.Forms;

namespace CinemaIS
{
    internal static class Program
    {
        public static Models.User? CurrentUser { get; set; }

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            DatabaseHelper.InitializeDatabase();
            Application.Run(new LoginForm());
        }
    }
}
