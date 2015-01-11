using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_ButtonPanel))]
    public class ControlButtonBlock : MyGameLogicComponent
    {
        private IMyButtonPanel _button;
        private MyObjectBuilder_EntityBase _objectBuilder;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            _objectBuilder = objectBuilder;
            _button = Entity as IMyButtonPanel;
            _button.ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(int button)
        {
            if (_button.CustomName.Equals("race.control", StringComparison.OrdinalIgnoreCase))
            {
                var track = RaceServer.GetTrack();

                switch (button)
                {
                    // set mode
                    case 0:
                        if (track.Mode == Track.Modes.Hotlap)
                            RaceServer.PopulateTrackMode(track, "race");
                        else
                            RaceServer.PopulateTrackMode(track, "hotlap");
                        break;

                    // reset
                    case 1:
                        RaceServer.Reset();
                        break;

                    // incrase race lap
                    case 2:
                        RaceServer.PopulateTrackTotalRaceLaps(track, track.Race.TotalLaps + 1);
                        break;

                    // decrase race lap
                    case 3:
                        RaceServer.PopulateTrackTotalRaceLaps(track, track.Race.TotalLaps - 1);
                        break;
                }
            }
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
        }
    }
}
