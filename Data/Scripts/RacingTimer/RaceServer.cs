using Sandbox.Common;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public static class RaceServer
    {
        public static readonly string RaceSensorName = "race.";
        public static readonly string RaceStartSensorName = RaceSensorName + "start";
        public static readonly string RaceCheckpointSensorName = RaceSensorName + "checkpoint.";
        public static readonly string RaceControlPanelName = RaceSensorName + "control";

        private static readonly string BestTimeFile = "RacingTimer.BestTimes.xml";
        private static readonly TimeSpan MininumTimeBetweenCheckpoings = new TimeSpan(0, 0, 0, 0, 500);
        private static IDictionary<long, Player> Players = new Dictionary<long, Player>();
        private static Track Track = new Track("Track");

        #region Common

        public static void Init()
        {
            EventServer.Subscribe(ProcessEvent);

            InitTrack(GetTrack());
        }

        public static void Reset()
        {
            Helper.NotificationToAll("Reset Racing Server");

            ResetPlayers();
            InitTrack(GetTrack());
        }

        private static void ProcessEvent(string data)
        {
            var parts = data.Split(':');
            var action = parts[0];
            var argument = parts[1];
            var track = GetTrack();

            switch (action)
            {
                case "Track.Mode":
                    if (argument == "hotlap")
                        SwitchTrackToHotlapMode(track);
                    else
                        SwitchTrackToRaceMode(track);
                    
                    break;

                case "Track.Race.TotalLaps":
                    track.Race.SetTotalLaps(int.Parse(argument));
                    
                    break;
            }
        }

        #endregion

        #region Player

        public static Player GetPlayer(IMyPlayer player)
        {
            if (Players.ContainsKey(player.PlayerID))
                return Players[player.PlayerID];

            Players[player.PlayerID] = new Player(player);

            return Players[player.PlayerID];
        }

        public static Player GetCurrentPlayer()
        {
            return GetPlayer(MyAPIGateway.Session.Player);
        }

        public static void ResetPlayers()
        {
            Players.Clear();
        }

        public static void ShowLaptimes(Player player)
        {
            var buffer = new StringBuilder();

            foreach (var lap in player.Laps.Where(l => l.Valid))
            {
                buffer.AppendLine(string.Format("#{0}: {1}", lap.Number, lap.Time.ToString(true, true)));
            }

            MyAPIGateway.Utilities.ShowMissionScreen(
                screenTitle: string.Format("Laptimes of {0}", player.Name),
                screenDescription: buffer.ToString(),
                okButtonCaption: "OK");
        }

        #endregion

        #region Track

        public static Track GetTrack()
        {
            return Track;
        }

        public static void InitTrack(Track track)
        {
            // reset lap record
            track.BestTime = null;

            // find all checkpoint sensor blocks
            var checkpointSensors = Helper.FindBlocks<IMySensorBlock>(b => b.CustomName != null 
                && (b.CustomName.StartsWith(RaceCheckpointSensorName, StringComparison.OrdinalIgnoreCase)
                    || b.CustomName.Equals(RaceStartSensorName, StringComparison.OrdinalIgnoreCase)));

            // group sensor block names
            var checkpointGroups = checkpointSensors.GroupBy(b => b.CustomName, StringComparer.OrdinalIgnoreCase);

            // clear current checkpoints
            track.Checkpoints.Clear();

            // add checkpoints
            foreach (var checkpoint in checkpointGroups)
            {
                track.Checkpoints.Add(new Checkpoint(checkpoint.Key, track));
            }

            Helper.ChatMessage(m => m("Found checkpoints [{0}] in track", 
                track.Checkpoints.ToFlatString(chk => chk.Name)));

            // load besttimes
            var times = Helper.LoadDataFromFile<List<BestTimeEntry>>(BestTimeFile);

            if (times != null)
            {
                foreach (var entry in times)
                {
                    // track best time
                    if (entry.Id.Equals(track.Name, StringComparison.OrdinalIgnoreCase))
                        track.BestTime = new BestTime(entry.Who, new TimeSpan(entry.Time));

                    // checkpoints best time
                    var chk = track.Checkpoints.FirstOrDefault(c => c.FullName.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));

                    if (chk != null)
                        chk.BestTime = new BestTime(entry.Who, new TimeSpan(entry.Time));
                }
            }
        }

        public static void SwitchTrackToRaceMode(Track track)
        {
            // reset track
            track.Race.Reset();
            track.Mode = RacingTimer.Track.Modes.Race_Running;

            // reset players
            RaceServer.ResetPlayers();

            Helper.NotificationToAll("Track is now in race mode");
        }

        public static void SwitchTrackToHotlapMode(Track track)
        {
            // reset track
            track.Mode = RacingTimer.Track.Modes.Hotlap;

            // reset players
            RaceServer.ResetPlayers();

            Helper.NotificationToAll("Track is now in hotlap mode");
        }

        public static void PopulateTrackTotalRaceLaps(Track track, int laps)
        {
            EventServer.Send("Track.Race.TotalLaps:" + laps.ToString());
        }

        public static void PopulateTrackMode(Track track, string mode)
        {
            EventServer.Send("Track.Mode:" + mode);
        }

        public static void SaveBestTimes(Track track)
        {
            var times = new List<BestTimeEntry>();

            if (track.BestTime != null)
                times.Add(new BestTimeEntry(track.Name, track.BestTime));

            foreach (var checkpoint in track.Checkpoints)
            {
                if (checkpoint.BestTime != null)
                    times.Add(new BestTimeEntry(checkpoint.FullName, checkpoint.BestTime));
            }

            Helper.SaveDataToFile(BestTimeFile, times);

        }

        #endregion

        #region Race

        public static bool IsMinimumTimeBetweenCheckpoints(Player player)
        {
            if (player.CurrentLap == null)
                return true;

            return (DateTime.Now - player.GetPreviousCheckpointTime()) > MininumTimeBetweenCheckpoings;
        }

        public static void HandleRaceLap(Player player, Track track)
        {
            if (track.Mode == RacingTimer.Track.Modes.Race_Running)
            {
                // has player finish the race
                if (player.Laps.Count() >= track.Race.TotalLaps)
                {
                    Helper.NotificationToAll(m => m("## [{0}] won the race! Congratulation! ##", player.Name));

                    track.Mode = RacingTimer.Track.Modes.Race_Finished;

                    return;
                }
                
                // update current lap
                if (player.Laps.Count() >= track.Race.CurrentLap)
                {
                    track.Race.CurrentLap++;

                    Helper.NotificationToAll(m => m("## [{0}] is now in lap {1}", player.Name, track.Race.CurrentLap));
                }
            }
        }

        

        #endregion
    }
}
