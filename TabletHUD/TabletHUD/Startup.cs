// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System;
    using Microsoft.AspNet.SignalR;
    using Owin;
    using Microsoft.Owin.Cors;

    public class SignalRStartup
    {
        public static IAppBuilder App = null;

        public void Configuration(IAppBuilder app)
        {
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration()
                {
                    // EnableDetailedErrors = true
                };

                map.RunSignalR(hubConfiguration);
            });

            app.Map("/web", map => 
            {
                map.UseCors(CorsOptions.AllowAll);
                map.UseStaticFiles(new Microsoft.Owin.StaticFiles.StaticFileOptions()
                {
                    FileSystem = new Microsoft.Owin.FileSystems.PhysicalFileSystem(Settings.WebFolder)
                });
            });

            app.Map("/browserconfig.xml", map =>
            {
                map.UseStaticFiles(new Microsoft.Owin.StaticFiles.StaticFileOptions()
                {
                    FileSystem = new Microsoft.Owin.FileSystems.PhysicalFileSystem(Settings.WebFolder)
                });
            });
        }
    }
}
