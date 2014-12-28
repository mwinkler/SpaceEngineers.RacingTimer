using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Race
    {
        public int TotalLaps { get; set; }
        public int CurrentLap { get; set; }

        public void Reset()
        {
            CurrentLap = 1;
        }

        public void SetTotalLaps(int laps)
        {
            TotalLaps = Math.Max(1, laps);

            Helper.NotificationToAll(m => m("Set race laps to {0}", TotalLaps));
        }

    }
}
