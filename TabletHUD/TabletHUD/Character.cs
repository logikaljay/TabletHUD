// -----------------------------------------------------------------------
// <copyright file="Character.cs" company="4o4">
// Copyright 2014 Efinity Group Limited. All Rights Reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace TabletHUD
{
    using Microsoft.AspNet.SignalR;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;

    /// <summary>
    /// Character class for TabletHUD
    /// </summary>
    public class Character
    {
        public Character()
        {
            this.Zones = new Dictionary<int, Zone>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Character"/> is debug.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug; otherwise, <c>false</c>.
        /// </value>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the paragon.
        /// </summary>
        /// <value>
        /// The paragon.
        /// </value>
        public int Paragon { get; set; }

        /// <summary>
        /// Gets or sets the experience remaining.
        /// </summary>
        /// <value>
        /// The experience remaining.
        /// </value>
        public double ExperienceRemaining { get; set; }

        /// <summary>
        /// Gets or sets the experience needed.
        /// </summary>
        /// <value>
        /// The experience needed.
        /// </value>
        public double ExperienceNeeded { get; set; }

        /// <summary>
        /// Gets or sets the experience earned.
        /// </summary>
        /// <value>
        /// The experience earned.
        /// </value>
        public double ExperienceEarned { get; set; }

        /// <summary>
        /// Gets or sets the experience earned total.
        /// </summary>
        /// <value>
        /// The experience earned total.
        /// </value>
        public double ExperienceEarnedTotal { get; set; }

        /// <summary>
        /// Gets the experience percent.
        /// </summary>
        /// <value>
        /// The experience percent.
        /// </value>
        public int ExperiencePercent
        {
            get
            {
                return (int)(this.ExperienceEarned / this.ExperienceNeeded * 100);
            }
        }

        /// <summary>
        /// Gets or sets the dexterity.
        /// </summary>
        /// <value>
        /// The dexterity.
        /// </value>
        public int Dexterity { get; set; }

        /// <summary>
        /// Gets or sets the strength.
        /// </summary>
        /// <value>
        /// The strength.
        /// </value>
        public int Strength { get; set; }

        /// <summary>
        /// Gets or sets the int total.
        /// </summary>
        /// <value>
        /// The int total.
        /// </value>
        public int Intelligence { get; set; }

        /// <summary>
        /// Gets or sets the vit total.
        /// </summary>
        /// <value>
        /// The vit total.
        /// </value>
        public int Vitality { get; set; }

        /// <summary>
        /// Gets or sets the armor total.
        /// </summary>
        /// <value>
        /// The armor total.
        /// </value>
        public int Armor { get; set; }

        /// <summary>
        /// Gets or sets the health total.
        /// </summary>
        /// <value>
        /// The health total.
        /// </value>
        public double HealthTotal { get; set; }

        /// <summary>
        /// Gets or sets the health current.
        /// </summary>
        /// <value>
        /// The health current.
        /// </value>
        public double HealthCurrent { get; set; }

        /// <summary>
        /// Gets the health percent.
        /// </summary>
        /// <value>
        /// The health percent.
        /// </value>
        public int HealthPercent
        {
            get
            {
                return (int)(this.HealthCurrent / this.HealthTotal * 100);
            }
        }

        /// <summary>
        /// Gets or sets the zones.
        /// </summary>
        /// <value>
        /// The zones.
        /// </value>
        public Dictionary<int, Zone> Zones { get; set; }
    }
}
