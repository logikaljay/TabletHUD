// -----------------------------------------------------------------------
// <copyright file="Zone.cs" company="4o4">
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

    /// <summary>
    /// Zone class for recording data per area
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the enter.
        /// </summary>
        /// <value>
        /// The enter.
        /// </value>
        public DateTime Enter { get; set; }

        /// <summary>
        /// Gets or sets the leave.
        /// </summary>
        /// <value>
        /// The leave.
        /// </value>
        public DateTime Leave { get; set; }

        /// <summary>
        /// Gets or sets the experience earned.
        /// </summary>
        /// <value>
        /// The experience earned.
        /// </value>
        public double ExperienceEarned { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the legendaries.
        /// </summary>
        /// <value>
        /// The legendaries.
        /// </value>
        public int Legendaries { get; set; }

        /// <summary>
        /// Gets or sets the rares.
        /// </summary>
        /// <value>
        /// The rares.
        /// </value>
        public int Rares { get; set; }

        /// <summary>
        /// Gets or sets the gold.
        /// </summary>
        /// <value>
        /// The gold.
        /// </value>
        public int Gold { get; set; }
    }
}
