// -----------------------------------------------------------------------
// <copyright file="Debug.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;
    using Enigma.D3;
    using ServiceStack.Redis;
    using ServiceStack.Redis.Generic;

    /// <summary>
    /// Debug class for running TabletHUD in the console.
    /// </summary>
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

        /// <summary>
        /// Runs TabletHUD in the console.
        /// </summary>
        public void RunConsole()
        {
            if (this.c == null)
            {
                this.c = new Character();
            }

            this.c.Debug = Settings.Debug;

            // Setup the memory watcher
            Program.Instance = Engine.Create();
            Program.CurrentActor = Enigma.D3.Helpers.ActorHelper.GetLocalActor();
            Program.CurrentACD = Enigma.D3.Helpers.ActorCommonDataHelper.GetLocalAcd();
            this.c.Id = Program.CurrentActor.x000_Id;

            // get our character from redis if it exists.
            using (var redisClient = new RedisClient(Settings.RedisHost, Settings.RedisPort))
            {
                IRedisTypedClient<Character> redis = redisClient.As<Character>();
                var currentCharacters = redis.Lists["urn:characters:current"];
                if (currentCharacters.Count > 0)
                {
                    // retrieve our character from redis with the appropriate debug flag.
                    var character = currentCharacters.SingleOrDefault(ch => ch.Id == this.c.Id && ch.Debug == Settings.Debug);
                    if (character != default(Character))
                    {
                        this.c = character;
                        Console.WriteLine(string.Format("Loaded Character {0} with {1} experience", character.Name, character.ExperienceEarned));
                    }
                }
            }

            this.t = new Timer(Settings.Interval);
            this.t.Elapsed += this.TimerElapsedHandler;
            this.t.Enabled = true;

            Console.ReadLine();
        }

        /// <summary>
        /// Timer tick handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            // check for objectManager being null before accessing any memory.
            if (Program.Instance == null || Program.Instance == default(Engine) || Engine.Current.ObjectManager == null || Engine.Current.ObjectManager == default(ObjectManager))
            {
                Program.Instance = Engine.Create();
                Program.CurrentActor = Enigma.D3.Helpers.ActorHelper.GetLocalActor();
                Program.CurrentACD = Enigma.D3.Helpers.ActorCommonDataHelper.GetLocalAcd();
            }

            var actor = Program.CurrentActor;
            var acd = Program.CurrentACD;

            this.c.Name = actor.x004_Name;
            this.c.Id = actor.x000_Id;

            this.c.Level = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Level);
            this.c.Paragon = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Alt_Level);
            this.c.ExperienceRemaining = Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Alt_Experience_Next_Lo);
            this.c.ExperienceNeeded = Paragon.Levels[this.c.Paragon + 1];
            this.c.ExperienceEarned = this.c.ExperienceNeeded - this.c.ExperienceRemaining;

            this.c.Intelligence = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Intelligence_Total);
            this.c.Vitality = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Vitality_Total);
            this.c.Armor = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Armor_Total);
            this.c.Dexterity = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Dexterity_Total);
            this.c.Strength = (int)Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Strength_Total);

            this.c.HealthTotal = Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Hitpoints_Max_Total);
            this.c.HealthCurrent = Enigma.D3.Helpers.AttributeHelper.GetAttributeValue(acd, Enigma.D3.Enums.AttributeId.Hitpoints_Cur);

            int zoneId = Engine.Current.LevelArea.x044_SnoId;

            if (this.oldExperience != 0)
            {
                double experience = this.c.ExperienceEarned - this.oldExperience;
                if (this.c.ExperienceEarnedTotal > 0)
                {
                    this.experienceEarnedTotal = this.c.ExperienceEarnedTotal;
                    this.experienceStartPosition = this.experienceEarnedTotal;
                    Console.WriteLine("Recalculating experience start position for new zone");
                }
                else
                {
                    this.experienceEarnedTotal += experience;
                }

                Console.WriteLine(string.Format("Experience start position: {0}", this.experienceStartPosition));

                this.c.ExperienceEarned += experience;

                // check if we have changed zones since last time we checked
                if (this.currentZone != zoneId)
                {
                    // update the previous zone - if it exists
                    if (this.c.Zones.ContainsKey(this.currentZone))
                    {
                        this.c.Zones[this.currentZone].Leave = DateTime.Now;
                    }

                    // zone has changed - set experienceStartPosition to experienceEarnedTotal
                    this.experienceStartPosition = this.c.ExperienceEarned;
                    Console.WriteLine(string.Format("Zone has changed, old {0} new {1}", this.currentZone, zoneId));
                }

                if (this.c.Zones.ContainsKey(zoneId))
                {
                    // zone exists in list - append experience to this zone.
                    this.c.Zones[zoneId].ExperienceEarned += this.experienceEarnedTotal - this.c.Zones.Sum(entity => entity.Value.ExperienceEarned);
                    this.c.Zones[zoneId].Duration += (double)Settings.Interval / (double)1000;
                    this.c.Zones[zoneId].Enter = DateTime.Now;
                    this.currentZone = zoneId;
                }
                else
                {
                    Zone zone = new Zone();
                    zone.Id = zoneId;
                    zone.Name = Engine.Current.LevelAreaName;
                    zone.Enter = DateTime.Now;
                    zone.ExperienceEarned = this.experienceEarnedTotal - this.c.Zones.Sum(entity => entity.Value.ExperienceEarned);
                    this.c.Zones.Add(zoneId, zone);
                    this.currentZone = zoneId;
                }

                this.c.ExperienceEarnedTotal = this.experienceEarnedTotal;

                using (var redisClient = new RedisClient(Settings.RedisHost, Settings.RedisPort))
                {
                    IRedisTypedClient<Character> redis = redisClient.As<Character>();
                    var currentCharacters = redis.Lists["urn:characters:current"];

                    currentCharacters.RemoveAll();
                    currentCharacters.Add(this.c);
                }
            }
            else
            {
                this.oldExperience = this.c.ExperienceEarned;
                Console.WriteLine("Updating experience old position");
            }
        }
    }
}
