using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TabletHUD
{
    public static class Settings
    {
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

        public static Character DebugCharacter
        {
            get
            {
                Character c = new Character();
                c.Debug = true;
                c.Id = 1;
                c.Name = "gotdeeps";
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

        public static EventLog EventLog
        {
            get
            {
                EventLog logger = new EventLog();
                logger.Source = ServiceName;
                return logger;
            }
        }

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

        public static string ServiceName
        {
            get
            {
                return string.Format("TabletHUD v{0}", Version);
            }
        }

        public static string Version
        {
            get
            {
                return "1.0";
            }
        }

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
