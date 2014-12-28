using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Checkpoint
    {
        public Checkpoint(string fullname)
        {
            FullName = fullname;
            IsStartFinish = fullname.Equals(RaceServer.RaceStartSensorName, StringComparison.OrdinalIgnoreCase);
            Name = (IsStartFinish 
                ? "start/finish"
                : fullname.Substring(RaceServer.RaceCheckpointSensorName.Length));
        }

        public string FullName { get; set; }
        public string Name { get; set; }
        public DateTime? Passed { get; set; }
        public BestTime BestTime { get; set; }
        public bool IsStartFinish { get; set; }
    }
}
