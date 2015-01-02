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
            CurrentTrack = RaceServer.GetTrack();
        }

        public List<Lap> Laps = new List<Lap>();

        public string Name { get; set; }
        public long PlayerID { get; set; }
        public Checkpoint CurrentCheckpoint { get; set; }
        public Lap CurrentLap { get; set; }
        public BestTime BestLapTime { get; set; }
        public Track CurrentTrack { get; set; }

        public void StartNewLap()
        {
            // create new lap
            CurrentLap = new Lap(Laps.Count + 1, this, CurrentTrack);

            Laps.Add(CurrentLap);
        }

        public DateTime GetPreviousCheckpointTime()
        {
            // get previous passed checkpoint time
            return (CurrentCheckpoint == null
                ? CurrentLap.Time.Start
                : CurrentCheckpoint.Passed.Value);
        }
    }
}
