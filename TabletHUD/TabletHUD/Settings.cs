// -----------------------------------------------------------------------
// <copyright file="Settings.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Settings class that contains app specific settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="Settings"/> is debug.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug; otherwise, <c>false</c>.
        /// </value>
        public static bool Debug
        {
            get
            {
                bool value = false;
                if (!bool.TryParse(ConfigurationManager.AppSettings["debug"].ToString(), out value))
                {
                    value = true;
                }

                return value;
            }
        }

        /// <summary>
        /// Gets the debug character.
        /// </summary>
        /// <value>
        /// The debug character.
        /// </value>
        public static Character DebugCharacter
        {
            get
            {
                Character c = new Character();
                c.Debug = true;
                c.Id = 1;
                c.Name = "fdsafdsa";
                c.Level = 70;
                c.Paragon = 84;
                c.ExperienceNeeded = 127000000;
                c.ExperienceEarned = 100000000;
                c.HealthCurrent = 100000;
                c.HealthTotal = 100000;
                c.Intelligence = 1234;
                c.Dexterity = 1234;
                c.Strength = 1234;
                c.Vitality = 4321;
                c.Armor = 3214;

                return c;
            }
        }

        /// <summary>
        /// Gets the event log.
        /// </summary>
        /// <value>
        /// The event log.
        /// </value>
        public static EventLog EventLog
        {
            get
            {
                EventLog logger = new EventLog();
                logger.Source = ServiceName;
                return logger;
            }
        }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <value>
        /// The interval.
        /// </value>
        public static int Interval
        {
            get
            {
                int interval = 1000;
                if (!int.TryParse(ConfigurationManager.AppSettings["interval"].ToString(), out interval))
                {
                    interval = 1000;
                }

                return interval;
            }
        }

        /// <summary>
        /// Gets the Redis host.
        /// </summary>
        /// <value>
        /// The Redis host.
        /// </value>
        public static string RedisHost
        {
            get
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["redisHost"]))
                {
                    return ConfigurationManager.AppSettings["redisHost"];
                }
                else
                {
                    return "localhost";
                }
            }
        }

        /// <summary>
        /// Gets the redis port.
        /// </summary>
        /// <value>
        /// The redis port.
        /// </value>
        public static int RedisPort
        {
            get
            {
                int port = 6379;
                if (!int.TryParse(ConfigurationManager.AppSettings["redisPort"].ToString(), out port))
                {
                    port = 6379;
                }
                
                return port;
            }
        }

        /// <summary>
        /// Gets the URL for SignalR.
        /// </summary>
        /// <value>
        /// The URL for SignalR.
        /// </value>
        public static string Url
        {
            get
            {
                string value = "http://localhost:9001";
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["url"]))
                {
                    value = ConfigurationManager.AppSettings["url"];
                }

                return value;
            }
        }

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public static string ServiceName
        {
            get
            {
                return string.Format("TabletHUD v{0}", Version);
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public static string Version
        {
            get
            {
                return "1.0";
            }
        }

        /// <summary>
        /// Gets the web folder.
        /// </summary>
        /// <value>
        /// The web folder.
        /// </value>
        public static string WebFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["webFolder"]))
                {
                    return ConfigurationManager.AppSettings["webFolder"];
                }
                else
                {
                    string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string webFolder = Path.Combine(exeFolder, "Web");
                    return webFolder;
                }
            }
        }
    }
}
