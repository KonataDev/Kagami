using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;

namespace Kagami.Function;

public static class Poke
{
    /// <summary>
    /// On group poke
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    internal static void OnGroupPoke(Bot bot, GroupPokeEvent group)
    {
        if (group.MemberUin != bot.Uin) return;

        // Convert it to ping
        bot.SendGroupMessage(group.GroupUin, Command.OnCommandPing(null));
    }
}