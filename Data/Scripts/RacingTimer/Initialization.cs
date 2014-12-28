using Sandbox.Common;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Initialization : MySessionComponentBase
    {
        private static bool _isInitilized;

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;

            if (!_isInitilized)
                Init();
        }

        private void Init()
        {
            _isInitilized = true;

            Helper.Log = new Logger("RacingTimer.log", true);

            Helper.Log.WriteLine("Initialization.Init", "");
            
            EventServer.Init();
            RaceServer.Init();

            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        private void MessageEntered(string messageText, ref bool sendToOthers)
        {
            Helper.Log.WriteLine("Initialization.MessageEntered", messageText);

            // is race cmd
            if (messageText.StartsWith("/race", StringComparison.OrdinalIgnoreCase))
            {
                var parts = messageText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var cmd = (parts.Length == 1 ? null : parts[1].ToLower());

                switch(cmd)
                {
                    case "reset":
                        RaceServer.Reset();
                        break;

                    case "laptimes":
                        var player = RaceServer.GetCurrentPlayer();
                        RaceServer.ShowLaptimes(player);
                        break;

                    default:
                        Helper.ChatMessage("Invalid command");
                        Helper.ChatMessage("/race reset");
                        Helper.ChatMessage("/race laptimes");
                        break;
                }
            }

            
        }

        protected override void UnloadData()
        {
            Helper.Log.WriteLine("Initialization.Unload", "");

            // save tracktimes
            RaceServer.SaveBestTimes(RaceServer.GetTrack());

            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
            Helper.Log.Close();

            base.UnloadData();
        }
    }
}
