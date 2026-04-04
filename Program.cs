//Author: Koch Nico 
//Sys Core - an interactive Console Dashboard

using AdminApp;
using LoginPage;
using StartScreen;

namespace SysCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var startScreen = new startScreen();
            startScreen.ShowStartScreen();

            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
                Console.ResetColor();
            }

            var login = new loginPage();
            bool istAdmin = login.ShowLogin();

            if (istAdmin)
            {
                var admin = new AdminPortal();
                admin.Run();
            }
        }
    }
}