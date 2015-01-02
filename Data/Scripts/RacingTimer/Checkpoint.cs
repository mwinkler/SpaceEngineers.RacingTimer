using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Checkpoint
    {
        public Checkpoint(string fullname, Track track, Lap lap = null)
        {
            Lap = lap;
            Track = track;
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
        public Track Track { get; set; }
        public Lap Lap { get; set; }

        public TimeDelta CalculateTime()
        {
            // if there is no attached lap, do nothing
            if (Lap == null)
                return null;

            // mark as passed
            Passed = DateTime.Now;

            // before calculate start/finish checkpoint, check if lap is valid
            if (IsStartFinish && !Lap.Player.CurrentLap.Validate())
                return null;

            // get track checkpoint
            var trackCheckpoint = Track.GetCheckpoint(FullName);

            // take time
            var time = new TimeDelta(Lap.Player.GetPreviousCheckpointTime(), Passed.Value, BestTime, trackCheckpoint.BestTime);

            // is new personal best time
            if (time.IsPersonalBest)
                BestTime = new BestTime(Lap.Player.Name, time.Delta);

            // is new global best time
            if (time.IsGlobalBest)
                trackCheckpoint.BestTime = new BestTime(Lap.Player.Name, time.Delta);

            return time;
        }
    }
}
