using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    [Serializable]
    public class BestTime
    {
        public BestTime(string who, TimeSpan time)
        {
            Who = who;
            Time = time;
        }

        public string Who { get; set; }
        public TimeSpan Time { get; set; }
    }

    public class BestTimeEntry
    {
        public BestTimeEntry()
        { }

        public BestTimeEntry(string id, BestTime time)
        {
            Id = id;
            Who = time.Who;
            Time = time.Time.Ticks;
        }

        public string Who { get; set; }
        public string Id { get; set; }
        public long Time { get; set; }
    }
}
