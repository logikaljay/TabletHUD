// -----------------------------------------------------------------------
// <copyright file="SignalRStartup.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System;
    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Cors;
    using Owin;

    /// <summary>
    /// SignalR routes
    /// </summary>
    public class SignalRStartup
    {
        /// <summary>
        /// Gets or sets the application
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public static IAppBuilder App { get; set; }

        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            app.Map(
                "/signalr", 
                map => 
                {
                    map.UseCors(CorsOptions.AllowAll);
                    var hubConfiguration = new HubConfiguration()
                    {
                        // EnableDetailedErrors = true
                    };

                    map.RunSignalR(hubConfiguration);
                });

            app.Map(
                "/web", 
                map =>
                {
                    map.UseCors(CorsOptions.AllowAll);
                    map.UseStaticFiles(new Microsoft.Owin.StaticFiles.StaticFileOptions()
                    {
                        FileSystem = new Microsoft.Owin.FileSystems.PhysicalFileSystem(Settings.WebFolder)
                    });
                });

            app.Map(
                "/browserconfig.xml", 
                map =>
                {
                    map.UseStaticFiles(new Microsoft.Owin.StaticFiles.StaticFileOptions()
                    {
                        FileSystem = new Microsoft.Owin.FileSystems.PhysicalFileSystem(Settings.WebFolder)
                    });
                });
        }
    }
}
