// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;
    using Enigma.D3;
    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Hosting;

    /// <summary>
    /// Main program hook class
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Gets or sets the paragon level requirements
        /// </summary>
        private static long[] paragonLevelRequirements = Paragon.Levels;

        /// <summary>
        /// Gets or sets the web application
        /// </summary>
        /// <value>
        /// The web application.
        /// </value>
        public static IDisposable WebAppInstance { get; set; }

        /// <summary>
        /// Gets or sets the current actor
        /// </summary>
        /// <value>
        /// The current actor.
        /// </value>
        public static Actor CurrentActor { get; set; }

        /// <summary>
        /// Gets or sets the current actor common data
        /// </summary>
        /// <value>
        /// The current actor common data.
        /// </value>
        public static ActorCommonData CurrentACD { get; set; }

        /// <summary>
        /// Gets or sets the instance of EnigmaD3
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static Engine Instance { get; set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            WebAppInstance = WebApp.Start<SignalRStartup>(Settings.Url);

            if (args.Length > 0) 
            {
                RunService();
            } 
            else 
            {
                Debug d = new Debug();
                d.RunConsole();
            }
        }

        /// <summary>
        /// Runs TabletHUD as a service.
        /// </summary>
        private static void RunService()
        {
            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[] 
            { 
                new TabletHUDService() 
            };

            ServiceBase.Run(servicesToRun);
        }
    }
}
