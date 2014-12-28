using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Player
    {
        public Player(IMyPlayer player)
        {
            Name = player.DisplayName;
            PlayerID = player.PlayerID;
        }

        public List<Checkpoint> Checkpoints = new List<Checkpoint>();
        public List<Lap> Laps = new List<Lap>();

        public string Name { get; set; }
        public long PlayerID { get; set; }
        public Checkpoint CurrentCheckpoint { get; set; }
        public Lap CurrentLap { get; set; }
        public BestTime BestLapTime { get; set; }

        public Checkpoint GetCheckpoint(string checkpointName)
        {
            // get checkpoint by name
            var checkpoint = Checkpoints.FirstOrDefault(sm => sm.FullName.Equals(checkpointName, StringComparison.OrdinalIgnoreCase));

            // create checkpoint
            if (checkpoint == null)
            {
                checkpoint = new Checkpoint(checkpointName);
                Checkpoints.Add(checkpoint);
            }

            return checkpoint;
        }

        public void ResetCheckpoints()
        {
            // reset current checkpoint
            CurrentCheckpoint = null;

            // reset passed checkpoints
            Checkpoints.ForEach(s => s.Passed = null);
        }

        public void StartNewLap()
        {
            // create new lap
            CurrentLap = new Lap(Laps.Count + 1);

            Laps.Add(CurrentLap);

            ResetCheckpoints();
        }

        public void RestartCurrentLap()
        {
            CurrentLap.Reset();

            ResetCheckpoints();
        }
    }
}
