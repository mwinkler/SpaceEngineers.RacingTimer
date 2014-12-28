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
    public static class Helper
    {
        public static Logger Log;

        #region Message

        public delegate void FormatMessageHandler(string format, params object[] args);
        private static string FormattedMessage;

        private static void FormatMessage(string format, params object[] args)
        {
            FormattedMessage = string.Format(format, args);
        }

        public static void NotificationToAll(Action<FormatMessageHandler> messageCallback, int time = 2000, MyFontEnum font = MyFontEnum.White)
        {
            messageCallback(FormatMessage);
            
            NotificationToAll(FormattedMessage, time, font);
        }

        public static void NotificationToAll(string msg, int time = 2000, MyFontEnum font = MyFontEnum.White)
        {
            Helper.Log.WriteLine("Helper.NotificationToAll", msg);

            MyAPIGateway.Utilities.ShowNotification(msg, time, font);
        }

        public static void NotificationToPlayer(Player player, Action<FormatMessageHandler> messageCallback, int time = 2000, MyFontEnum font = MyFontEnum.White)
        {
            messageCallback(FormatMessage);

            NotificationToPlayer(player, FormattedMessage, time, font);
        }

        public static void NotificationToPlayer(Player player, string msg, int time = 2000, MyFontEnum font = MyFontEnum.White)
        {
            Helper.Log.WriteLine("Helper.NotificationToPlayer", string.Format("(to: {0}) {1}", player.Name, msg));

            if (MyAPIGateway.Session.Player.PlayerID == player.PlayerID)
            {
                MyAPIGateway.Utilities.ShowNotification(msg, time, font);
            }
        }

        public static void ChatMessage(Action<FormatMessageHandler> messageCallback)
        {
            ChatMessage("[RacingTimer]", messageCallback);
        }

        public static void ChatMessage(string message)
        {
            ChatMessage("[RacingTimer]", message);
        }

        public static void ChatMessage(string sender, Action<FormatMessageHandler> messageCallback)
        {
            messageCallback(FormatMessage);

            ChatMessage(sender, FormattedMessage);
        }

        public static void ChatMessage(string sender, string message)
        {
            Helper.Log.WriteLine("Helper.ChatMessage", string.Format("(from: {0}) {1}", sender, message));

            MyAPIGateway.Utilities.ShowMessage(sender, message);
        }

        #endregion

        public static IMyPlayer GetNearestPlayer(VRageMath.Vector3D position)
        {
            var players = new List<IMyPlayer>();

            MyAPIGateway.Players.GetPlayers(players);

            return players
                .OrderBy(p => (p.GetPosition() - position).Length())
                .FirstOrDefault();
        }

        public static IEnumerable<T> FindBlocks<T>(Func<T, bool> predicate)
            where T: class
        {
            var grids = new HashSet<IMyEntity>();
            var blocks = new List<Sandbox.ModAPI.IMySlimBlock>();
            MyAPIGateway.Entities.GetEntities(grids, e => e is Sandbox.ModAPI.IMyCubeGrid);

            foreach (var grid in grids)
            {
                ((Sandbox.ModAPI.IMyCubeGrid)grid).GetBlocks(blocks, b => b.FatBlock is T && predicate(b.FatBlock as T));
            }

            return blocks
                .Select(b => b.FatBlock)
                .Cast<T>();
        }

        public static MyFontEnum GetLaptimeFontColor(this TimeDelta time)
        {
            return (time.IsGlobalBest
                    ? MyFontEnum.DarkBlue
                    : time.IsPersonalBest
                        ? MyFontEnum.Green
                        : MyFontEnum.White);
        }

        public static string ToFlatString<T>(this IEnumerable<T> enumeration, Func<T, string> getter, string delimitter = ", ")
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var item in enumeration)
            {
                if (first)
                    first = false;
                else
                    sb.Append(delimitter);

                sb.Append(getter(item));
            }

            return sb.ToString();
        }

        public static T LoadDataFromFile<T>(string file)
        {
            try
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInGlobalStorage(file))
                    return MyAPIGateway.Utilities.SerializeFromXML<T>(reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                Log.WriteLine("Helper.LoadDataFromFile", string.Format("Error while load file '{0}': {1}", file, ex.Message));
            }

            return default(T);
        }

        public static void SaveDataToFile<T>(string file, T data)
        {
            try
            {
                using (var writer = MyAPIGateway.Utilities.WriteBinaryFileInGlobalStorage(file))
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML<T>(data));
            }
            catch (Exception ex)
            {
                Log.WriteLine("Helper.SaveDataToFile", string.Format("Error while save file '{0}': {1}", file, ex.Message));
            }
        }
    }
}
