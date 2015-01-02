using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Lap
    {
        public List<Checkpoint> Checkpoints = new List<Checkpoint>();

        public Lap(int number, Player player, Track track)
        {
            Time = new TimeDelta(DateTime.Now);
            Number = number;
            Player = player;
            Track = track;
        }

        public TimeDelta Time { get; set; }
        public int Number { get; set; }
        public bool Valid { get; set; }
        public Player Player { get; set; }
        public Track Track { get; set; }

        public void Reset()
        {
            Time.Start = DateTime.Now;
            
            // reset passed checkpoints
            Checkpoints.ForEach(s => s.Passed = null);
        }

        public Checkpoint GetCheckpoint(string checkpointName)
        {
            // get checkpoint by name
            var checkpoint = Checkpoints.FirstOrDefault(sm => sm.FullName.Equals(checkpointName, StringComparison.OrdinalIgnoreCase));

            // create checkpoint
            if (checkpoint == null)
            {
                checkpoint = new Checkpoint(checkpointName, Track, this);
                Checkpoints.Add(checkpoint);
            }

            return checkpoint;
        }

        public bool Validate()
        {
            Valid = !GetMissingCheckpoints().Any();

            return Valid;
        }

        public IEnumerable<Checkpoint> GetMissingCheckpoints()
        {
            return Track.Checkpoints
                .Where(tCheck => !Checkpoints.Any(pCheck => pCheck.FullName == tCheck.FullName && pCheck.Passed != null))
                .ToArray();
        }

        public void CalculateTime()
        {
            // calculate laptime
            Time.Calculate(DateTime.Now, Player.BestLapTime, Track.BestTime);

            // is personal record
            if (Time.IsPersonalBest)
                Player.BestLapTime = new BestTime(Player.Name, Time.Delta);

            // is global record
            if (Time.IsGlobalBest)
                Track.BestTime = new BestTime(Player.Name, Time.Delta);
        }

        public TimeDelta PassCheckpoint(Checkpoint trackCheckpoint)
        {
            // get players checkpoint
            var playerCheckpoint = GetCheckpoint(trackCheckpoint.FullName);

            // has player checkpoint already passed
            if (playerCheckpoint.Passed != null)
                return null;

            // calculate checkpoint time
            var time = playerCheckpoint.CalculateTime();

            if (time != null)
            {
                // set current checkpoint
                Player.CurrentCheckpoint = playerCheckpoint;
            }

            return time;
        }
    }
}
