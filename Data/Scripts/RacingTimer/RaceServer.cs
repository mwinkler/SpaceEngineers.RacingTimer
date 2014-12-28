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
            MyAPIGateway.Utilities.ShowMissionScreen(
                "screenTitle",//string.Format("Laptimes of {0}", player.Name),
                "currentObjectivePrefix",
                "currentObjective",
                "screenDescription",
                result => { },
                "okButton");
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
                track.Checkpoints.Add(new Checkpoint(checkpoint.Key));
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

        public static bool IsValidLap(Player player, Track track, bool showMessage)
        {
            // count passed checkpoints
            var missingCheckpoints = track.Checkpoints
                .Where(tCheck => !player.Checkpoints.Any(pCheck => pCheck.FullName == tCheck.FullName && pCheck.Passed != null))
                .ToArray();

            // are all checkpoints passed
            if (missingCheckpoints.Any())
            {
                if (showMessage)
                {
                    Helper.NotificationToPlayer(
                        player,
                        m => m("Invalid lap, you miss checkpoints [{0}]",
                            missingCheckpoints.ToFlatString(checkpoint => checkpoint.Name)),
                        5000,
                        MyFontEnum.Red);
                }

                return false;
            }

            return true;
        }

        public static bool IsMinimumTimeBetweenCheckpoints(Player player)
        {
            if (player.CurrentLap == null)
                return true;

            return (DateTime.Now - PreviousCheckpointTime(player)) > MininumTimeBetweenCheckpoings;
        }

        public static void CalculateCheckpoint(Track track, Checkpoint checkpoint, Player player)
        {
            // has player lap started
            if (!player.Laps.Any())
                return;

            // has player checkpoint already passed
            if (checkpoint.Passed != null)
                return;

            // mark as passed
            checkpoint.Passed = DateTime.Now;

            // before calculate start/finish checkpoint, check if lap is valid
            if (checkpoint.IsStartFinish && !IsValidLap(player, track, false))
                return;

            // get track checkpoint
            var trackCheckpoint = track.GetCheckpoint(checkpoint.FullName);

            // take time
            var time = new TimeDelta(PreviousCheckpointTime(player), checkpoint.Passed.Value, checkpoint.BestTime, trackCheckpoint.BestTime);

            // is new personal best time
            if (time.IsPersonalBest)
                checkpoint.BestTime = new BestTime(player.Name, time.Delta);

            // is new global best time
            if (time.IsGlobalBest)
                trackCheckpoint.BestTime = new BestTime(player.Name, time.Delta);

            // set current checkpoint
            player.CurrentCheckpoint = checkpoint;

            // message
            Helper.NotificationToPlayer(
                player,
                m => m("Checkpoint '{0}' {1}",
                    checkpoint.Name,
                    time.ToString(true, true)),
                10000,
                time.GetLaptimeFontColor());

            return;
        }

        public static void CalculateLap(Track track, Player player)
        {
            var lap = player.CurrentLap;

            // calculate laptime
            lap.Time.Calculate(DateTime.Now, player.BestLapTime, track.BestTime);

            // is personal record
            if (lap.Time.IsPersonalBest)
                player.BestLapTime = new BestTime(player.Name, lap.Time.Delta);

            // is global record
            if (lap.Time.IsGlobalBest)
                track.BestTime = new BestTime(player.Name, lap.Time.Delta);

            // show message
            Helper.NotificationToPlayer(
                player,
                m => m("== Lap #{0} {1} ==",
                    lap.Number,
                    lap.Time.ToString(true, true, true)),
                10000,
                lap.Time.GetLaptimeFontColor());
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

        public static DateTime PreviousCheckpointTime(Player player)
        {
            // get previous passed checkpoint time
            return (player.CurrentCheckpoint == null
                ? player.CurrentLap.Time.Start
                : player.CurrentCheckpoint.Passed.Value);
        }

        #endregion
    }
}
