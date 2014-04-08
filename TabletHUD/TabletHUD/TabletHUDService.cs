using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
// -----------------------------------------------------------------------
// <copyright file="TabletHUDService.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading.Tasks;
    using Enigma.D3;
    using System.Threading;
    using Microsoft.AspNet.SignalR;

    public partial class TabletHUDService : ServiceBase
    {
        public TabletHUDService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            if (Settings.Debug)
            {
                EventLog.WriteEntry(string.Format("{0} Service running on {1} in debug mode", Settings.ServiceName, Settings.Url));
            }
            else
            {
                EventLog.WriteEntry(string.Format("{0} Service running on {1}", Settings.ServiceName, Settings.Url));
            }
            
            base.OnStart(args);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            EventLog.WriteEntry(string.Format("{0} Service stopping", Settings.ServiceName));

            if (Program.Instance != null)
            {
                Program.Instance.Dispose();
            }

            if (Program.webApp != null)
            {
                Program.webApp.Dispose();
            }


            base.OnStop();
        }
    }
}
