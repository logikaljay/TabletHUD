// -----------------------------------------------------------------------
// <copyright file="CharacterHub.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;
    using Enigma.D3;
    using Microsoft.AspNet.SignalR;
    using Newtonsoft.Json;
    using ServiceStack.Redis;
    using ServiceStack.Redis.Generic;

    /// <summary>
    /// Transport Hub for TabletHUD
    /// </summary>
    public class CharacterHub : Hub
    {
        /// <summary>
        /// The timer
        /// </summary>
        private Timer t;

        /// <summary>
        /// The character
        /// </summary>
        private Character c;

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
        /// The connection ids
        /// </summary>
        private HashSet<string> connectionIds = new HashSet<string>();

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override Task OnConnected()
        {
            lock (this.connectionIds)
            {
                this.connectionIds.Add(Context.ConnectionId);
                EventLog e = new EventLog();
                e.Source = "TabletHUD";
                e.WriteEntry(string.Format("Client with id {0} connected.", Context.ConnectionId));
            }

            if (Settings.Debug)
            {
                if (this.c == null)
                {
                    this.c = Settings.DebugCharacter;
                }
            }
            else
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
            }

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
                        Console.WriteLine("Saved Character to Redis");
                    }
                }
            }

            this.t = new Timer(Settings.Interval);
            this.t.Elapsed += this.TimerElapsedHandler;
            this.t.Enabled = true;

            return base.OnConnected();
        }

        /// <summary>
        /// Called when a connection disconnects from this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override Task OnDisconnected()
        {
            // destory our timer
            if (this.t != null)
            {
                this.t.Enabled = false;
                this.t.Dispose();
            }

            // destroy our instance
            if (Program.Instance != null)
            {
                Program.Instance.Dispose();
            }

            lock (this.connectionIds)
            {
                this.connectionIds.RemoveWhere(cid => cid.Equals(Context.ConnectionId));
            }

            return base.OnDisconnected();
        }        
        
        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        private void Send(Character data)
        {
            if (this.c != null)
            {
                Clients.Caller.addMessage(JsonConvert.SerializeObject(this.c));
            }
        }

        /// <summary>
        /// Timers the elapsed handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            // this timer will start to run when a user connects to the web interface
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

            int zoneId = Enigma.D3.Helpers.WorldHelper.GetLocalWorld().x04_SnoId;

            if (this.oldExperience != 0)
            {
                double experience = this.c.ExperienceEarned - this.oldExperience;
                Console.WriteLine(string.Format("Adding {0} experience to earned {1}", experience, this.c.ExperienceEarned));
                this.c.ExperienceEarnedTotal += experience;
                this.experienceEarnedTotal += experience;
                this.oldExperience = this.c.ExperienceEarned;

                Console.WriteLine(string.Format("Experience start position: {0}", this.experienceStartPosition));

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
                    zone.Enter = DateTime.Now;
                    zone.ExperienceEarned = this.experienceEarnedTotal - this.c.Zones.Sum(entity => entity.Value.ExperienceEarned);
                    this.c.Zones.Add(zoneId, zone);
                    this.currentZone = zoneId;
                }

                this.c.ExperienceEarnedTotal = this.experienceEarnedTotal;

                this.Send(this.c);

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
