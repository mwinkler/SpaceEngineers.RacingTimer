using Sandbox.Common;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public static class EventServer
    {
        private static readonly string FactionName = "Racing Team";
        private static readonly List<Action<string>> Subscriber = new List<Action<string>>();

        public static void Init()
        {
            MyAPIGateway.Session.Factions.FactionEdited += Factions_FactionEdited;
        }

        private static bool CheckFactionState()
        {
            // is player in rts faction
            var playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.PlayerID);
            
            // is player in correct faction
            if (playerFaction != null && playerFaction.Name == FactionName)
                return true;

            var player = RaceServer.GetCurrentPlayer();

            Helper.NotificationToPlayer(player, m => m("Please join/create faction '{0}'", FactionName), 5000, MyFontEnum.Red);
            
            return false;
        }

        public static void Send(string value)
        {
            if (CheckFactionState())
            {
                var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.PlayerID);

                // place value in private faction info
                MyAPIGateway.Session.Factions.EditFaction(faction.FactionId, faction.Tag, faction.Name, faction.Description, value);
            }
        }

        public static void Subscribe(Action<string> action)
        {
            Subscriber.Add(action);
        }

        private static void Factions_FactionEdited(long obj)
        {
            if (CheckFactionState())
            {
                var playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.Player.PlayerID);

                // trigger subscriber
                Subscriber.ForEach(sub => sub(playerFaction.PrivateInfo));
            }
        }

        //public static void Unload()
        //{
        //    MyAPIGateway.Session.Factions.FactionEdited -= Factions_FactionEdited;
        //}
    }
}
