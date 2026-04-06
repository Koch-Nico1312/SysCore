//Author: Koch Nico 
//Sys Core - an interactive Console Dashboard

using AdminApp;
using CasualUserApp;
using LoginPage;
using StartScreen;

namespace SysCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var startScreen = new StartScreenView();
            startScreen.ShowStartScreen();

            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
                Console.ResetColor();
            }

            var login = new LoginPageView();
            bool isAdmin = login.ShowLogin();

            if (isAdmin)
            {
                var admin = new AdminPortal();
                admin.Run();
            }
            else
            {
                var casual = new CasualPortal();
                casual.Run();
            }
        }
    }
}