using Enigma.D3;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TabletHUD.Infrastructure;

namespace TabletHUD
{
    public class Debug
    {
        /// <summary>
        /// The current zone
        /// </summary>
        private int currentZone = 0;

        /// <summary>
        /// The start zone experience
        /// </summary>
        private double experienceEarnedTotal = 0;

        /// <summary>
        /// The experience old
        /// </summary>
        private double experienceStartPosition = 0;

        /// <summary>
        /// The old experience
        /// </summary>
        private double oldExperience = 0;

        /// <summary>
        /// The c
        /// </summary>
        private Character c;

        /// <summary>
        /// The t
        /// </summary>
        private Timer t;

        private Dictionary<int, AreaModel> Areas;

        public void RunConsole()
        {
            if (true)
            {
                Console.ReadLine();
            }
            else
            {
                Area area = new Area();
                this.Areas = area;

                if (c == null)
                {
                    c = new Character();
                }

                c.Debug = Settings.Debug;

                // Setup the memory watcher
                Program.Instance = Engine.Create();
                Program.CurrentActor = Enigma.D3.Helpers.ActorHelper.GetLocalActor();
                Program.CurrentACD = Enigma.D3.Helpers.ActorCommonDataHelper.GetLocalAcd();
                c.Id = Program.CurrentActor.x000_Id;

                // get our character from redis if it exists.
                using (var redisClient = new RedisClient(Settings.RedisHost, Settings.RedisPort))
                {
                    IRedisTypedClient<Character> redis = redisClient.As<Character>();
                    var currentCharacters = redis.Lists["urn:characters:current"];
                    if (currentCharacters.Count > 0)
                    {
                        // retrieve our character from redis with the appropriate debug flag.
                        var character = currentCharacters.SingleOrDefault(ch => ch.Id == c.Id && ch.Debug == Settings.Debug);
                        if (character != default(Character))
                        {
                            c = character;
                            Console.WriteLine(string.Format("Loaded Character {0} with {1} experience", character.Name, character.ExperienceEarned));
                        }
                    }
                }


                t = new Timer(Settings.Interval);
                t.Elapsed += TimerElapsedHandler;
                t.Enabled = true;

                Console.ReadLine();
            }
        }

        void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            var actor = Program.CurrentActor;
            var acd = Program.CurrentACD;

            c.Name = actor.x004_Name;
            c.Id = actor.x000_Id;

            c.Level = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Level);
            c.Paragon = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Alt_Level);
            c.ExperienceRemaining = Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Alt_Experience_Next_Lo);
            c.ExperienceNeeded = Paragon.paragon[c.Paragon + 1];
            c.ExperienceEarned = c.ExperienceNeeded - c.ExperienceRemaining;


            c.Intelligence = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Intelligence_Total);
            c.Vitality = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Vitality_Total);
            c.Armor = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Armor_Total);
            c.Dexterity = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Dexterity_Total);
            c.Strength = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Strength_Total);

            c.HealthTotal = Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Hitpoints_Max_Total);
            c.HealthCurrent = Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Hitpoints_Cur);

            int zoneId = Enigma.D3.Helpers.WorldHelper.GetLocalWorld().x04_SnoId;

            if (oldExperience != 0)
            {
                double experience = c.ExperienceEarned - oldExperience;
                if (c.ExperienceEarnedTotal > 0)
                {
                    experienceEarnedTotal = c.ExperienceEarnedTotal;
                    experienceStartPosition = experienceEarnedTotal;
                    Console.WriteLine("Recalculating experience start position for new zone");
                }
                else
                {
                    experienceEarnedTotal += experience;
                }

                Console.WriteLine(string.Format("Experience start position: {0}", experienceStartPosition));

                c.ExperienceEarned += experience;

                // check if we have changed zones since last time we checked
                if (currentZone != zoneId)
                {
                    // update the previous zone - if it exists
                    if (c.Zones.ContainsKey(currentZone))
                    {
                        c.Zones[currentZone].Leave = DateTime.Now;
                    }

                    // zone has changed - set experienceStartPosition to experienceEarnedTotal
                    experienceStartPosition = c.ExperienceEarned;
                    Console.WriteLine(string.Format("Zone has changed, old {0} new {1}", currentZone, zoneId));
                }

                if (c.Zones.ContainsKey(zoneId))
                {
                    // zone exists in list - append experience to this zone.
                    c.Zones[zoneId].ExperienceEarned += experienceEarnedTotal - c.Zones.Sum(entity => entity.Value.ExperienceEarned);
                    c.Zones[zoneId].Duration += (double)Settings.Interval / (double)1000;
                    c.Zones[zoneId].Enter = DateTime.Now;
                    currentZone = zoneId;
                }
                else
                {
                    Zone zone = new Zone();
                    zone.Id = zoneId;
                    zone.Enter = DateTime.Now;
                    zone.ExperienceEarned = experienceEarnedTotal - c.Zones.Sum(entity => entity.Value.ExperienceEarned);
                    c.Zones.Add(zoneId, zone);
                    currentZone = zoneId;
                }

                c.ExperienceEarnedTotal = experienceEarnedTotal;

                using (var redisClient = new RedisClient(Settings.RedisHost, Settings.RedisPort))
                {
                    IRedisTypedClient<Character> redis = redisClient.As<Character>();
                    var currentCharacters = redis.Lists["urn:characters:current"];

                    currentCharacters.RemoveAll();
                    currentCharacters.Add(c);
                }
            }
            else
            {
                oldExperience = c.ExperienceEarned;
                Console.WriteLine("Updating experience old position");
            }
        }
    }
}
