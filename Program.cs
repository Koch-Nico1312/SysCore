//Author: Koch Nico 
//Sys Core - an interactive Console Dashboard

using AdminApp;
using CasualUserApp;
using LoginPage;
using StartScreen;
using System.Text;

namespace SysCore
{
    public class Program
    {
        // Einfacher Hauptablauf: Start -> Login -> passendes Portal.
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            StartScreenView start = new StartScreenView();
            start.ShowStartScreen();

            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
                Console.ResetColor();
            }

            LoginPageView loginPage = new LoginPageView();
            bool adminMode = loginPage.ShowLogin();

            if (adminMode)
            {
                AdminPortal admin = new AdminPortal();
                admin.Run();
            }
            else
            {
                CasualPortal user = new CasualPortal();
                user.Run();
            }
        }
    }
}