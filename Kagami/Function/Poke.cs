using Konata.Core;
using Konata.Core.Events.Model;

namespace Kagami.Function
{
    public static class Poke
    {
        /// <summary>
        /// On group poke
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="group"></param>
        internal static void OnGroupPoke(object sender, GroupPokeEvent group)
        {
            var bot = (Bot) sender;
            if (group.MemberUin != bot.Uin) return;

            // Convert it to ping
            bot.SendGroupMessage(group.GroupUin, Command.OnCommandPing(null));
        }
    }
}
