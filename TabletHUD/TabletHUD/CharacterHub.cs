// -----------------------------------------------------------------------
// <copyright file="TransportHub.cs" company="4o4">
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
    using System.Diagnostics;
    using System.Configuration;
    using Microsoft.AspNet.SignalR;
    using Newtonsoft.Json;
    using Enigma.D3;
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
        /// The debug iter
        /// </summary>
        private int debugIter = 0;

        /// <summary>
        /// The connection ids
        /// </summary>
        private HashSet<string> ConnectionIds = new HashSet<string>();

        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        private void Send(Character data)
        {
            if (c != null)
            {
                Clients.Caller.addMessage(JsonConvert.SerializeObject(c));
            }
        }

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override Task OnConnected()
        {
            lock (ConnectionIds)
            {
                ConnectionIds.Add(Context.ConnectionId);
                EventLog e = new EventLog();
                e.Source = "TabletHUD";
                e.WriteEntry(string.Format("Client with id {0} connected.", Context.ConnectionId));
            }

            if (Settings.Debug)
            {
                if (c == null)
                {
                    c = Settings.DebugCharacter;
                }
            }
            else
            {
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
            }

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
                        Console.WriteLine("Saved Character to Redis");
                    }
                }
            }

            t = new Timer(Settings.Interval);
            t.Elapsed += TimerElapsedHandler;
            t.Enabled = true;

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
            if (t != null)
            {
                t.Enabled = false;
                t.Dispose();
            }

            // destroy our instance
            if (Program.Instance != null)
            {
                Program.Instance.Dispose();
            }

            lock (ConnectionIds)
            {
                ConnectionIds.RemoveWhere(cid => cid.Equals(Context.ConnectionId));
                //Settings.EventLog.WriteEntry(string.Format("Client with id {0} disconnected.", Context.ConnectionId));
            }

            return base.OnDisconnected();
        }

        /// <summary>
        /// Timers the elapsed handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            if (Settings.Debug)
            {
                Zone z = null;

                if (c.ExperienceEarnedTotal > 0)
                {
                    experienceEarnedTotal = c.ExperienceEarnedTotal;
                    experienceStartPosition = experienceEarnedTotal;
                }

                // add a random number to experience earned
                Random r = new Random(DateTime.Now.Millisecond);
                int experience = r.Next(10000, 50000);
                experienceEarnedTotal += experience;
                c.ExperienceEarned += experience;

                // set random number to zoneId
                Random r2 = new Random(DateTime.Now.Millisecond);
                int zoneId = r2.Next(0, 4);

                if (c.Zones.ContainsKey(zoneId))
                {
                    // zone exists in list - append experience to this zone.
                    c.Zones[zoneId].ExperienceEarned += experienceEarnedTotal - c.Zones.Sum(entity => entity.Value.ExperienceEarned);
                    c.Zones[zoneId].Duration += (double)Settings.Interval / (double)1000;
                    c.Zones[zoneId].Enter = DateTime.Now;
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

                if (currentZone != zoneId)
                {
                    // update the previous zone - if it exists
                    if (c.Zones.ContainsKey(currentZone))
                    {
                        c.Zones[currentZone].Leave = DateTime.Now;
                    }

                    // zone has changed - set experienceStartPosition to experienceEarnedTotal
                    experienceStartPosition = experienceEarnedTotal;
                }

                Send(c);
            }
            else
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
                    Console.WriteLine(string.Format("Adding {0} experience to earned {1}", experience, c.ExperienceEarned));
                    c.ExperienceEarnedTotal += experience;
                    experienceEarnedTotal += experience;
                    oldExperience = c.ExperienceEarned;

                    Console.WriteLine(string.Format("Experience start position: {0}", experienceStartPosition));

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

                    Send(c);

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
}
