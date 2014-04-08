namespace TabletHUD
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Zone
    {
        public int Id { get; set; }

        public DateTime Enter { get; set; }

        public DateTime Leave { get; set; }

        public double ExperienceEarned { get; set; }

        public double Duration { get; set; }

        public int Legendaries { get; set; }

        public int Rares { get; set; }

        public int Gold { get; set; }
    }
}
