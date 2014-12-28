using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Lap
    {
        public Lap(int number)
        {
            Time = new TimeDelta(DateTime.Now);
            Number = number;
        }

        public TimeDelta Time { get; set; }
        public int Number { get; set; }

        public void Reset()
        {
            Time.Start = DateTime.Now;
        }
    }
}
