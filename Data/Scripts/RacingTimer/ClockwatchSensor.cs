using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_SensorBlock))]
    public class ClockwatchSensor : MyGameLogicComponent
    {
        private IMySensorBlock _sensor;
        private MyObjectBuilder_EntityBase _objectBuilder;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;
            _sensor = Entity as IMySensorBlock;
            _sensor.StateChanged += SensorTriggered;
        }

        private void SensorTriggered(bool obj)
        {
            // if object leaves range, ignore
            if (!obj)
                return;

            // is racing sensor
            if (!_sensor.CustomName.StartsWith("race.", StringComparison.OrdinalIgnoreCase))
                return;

            // get nearest player
            var playerEntity = Helper.GetNearestPlayer(Entity.GetPosition());

            // break if no player is found
            if (playerEntity == null)
                return;

            // get player data
            var player = RaceServer.GetPlayer(playerEntity);

            // minimum time between checkpoints reached
            if (!RaceServer.IsMinimumTimeBetweenCheckpoints(player))
                return;

            // get track
            var track = RaceServer.GetTrack();

            // get checkpoint
            var checkpoint = track.GetCheckpoint(_sensor.CustomName);

            // player pass checkpoint
            if (player.CurrentLap != null)
            {
                var time = player.CurrentLap.PassCheckpoint(checkpoint);

                if (time != null)
                {
                    // message
                    Helper.NotificationToPlayer(
                        player,
                        m => m("Checkpoint '{0}' {1}",
                            checkpoint.Name,
                            time.ToString(true, true)),
                        10000,
                        time.GetLaptimeFontColor());
                }
            }

            // player cross start/finish line
            if (_sensor.CustomName.Equals(RaceServer.RaceStartSensorName, StringComparison.OrdinalIgnoreCase))
            {
                // is player starting?
                if (player.CurrentLap == null)
                {
                    player.StartNewLap();

                    Helper.NotificationToPlayer(player, m => m("=== Good race {0} ===", player.Name));
                    
                    return;
                }

                var lap = player.CurrentLap;

                // valid lap
                if (lap.Validate())
                {
                    lap.CalculateTime();

                    RaceServer.HandleRaceLap(player, track);

                    // show message
                    Helper.NotificationToPlayer(
                        player,
                        m => m("== Lap #{0} {1} ==",
                            lap.Number,
                            lap.Time.ToString(true, true, true)),
                        10000,
                        lap.Time.GetLaptimeFontColor());

                    // start new lap
                    player.StartNewLap();

                    return;
                }

                // invalid lap
                else
                {
                    // show invalid map message
                    Helper.NotificationToPlayer(
                        player,
                        m => m("Invalid lap, you miss checkpoints [{0}]",
                            player.CurrentLap.GetMissingCheckpoints().ToFlatString(chk => chk.Name)),
                        5000,
                        MyFontEnum.Red);

                    // reset current lap
                    player.CurrentLap.Reset();

                    return;
                }
                
            }

            // player passes checkpoint marker
            if (_sensor.CustomName.StartsWith(RaceServer.RaceCheckpointSensorName, StringComparison.OrdinalIgnoreCase))
            {
                // is lap started
                if (!player.Laps.Any())
                {
                    Helper.NotificationToPlayer(
                        player,
                        "Please cross start line first",
                        2000,
                        MyFontEnum.Red);

                    return;
                }
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
        }
    }
}
