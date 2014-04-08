using Enigma.D3;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TabletHUD
{
    static class Program
    {
        public static IDisposable webApp;

        public static Actor CurrentActor;

        public static ActorCommonData CurrentACD;

        public static Engine Instance;

        private static long[] ParagonLevelRequirements = Paragon.paragon;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            webApp = WebApp.Start<SignalRStartup>(Settings.Url);

            if (args.Length > 0) {
                RunService();
            } else {
                RunConsole();
            }
        }


        static void RunService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new TabletHUDService() 
            };

            ServiceBase.Run(ServicesToRun);
        }

        static void RunConsole()
        {
            Console.ReadLine();
        }
    }
}
