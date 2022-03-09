using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Exceptions.Model;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using Kagami.Utils;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local

namespace Kagami.Function
{
    public static class Command
    {
        private static uint _messageCounter;

        /// <summary>
        /// On group message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="group"></param>
        internal static async void OnGroupMessage(object sender, GroupMessageEvent group)
        {
            // Increase
            ++_messageCounter;

            var bot = (Bot) sender;
            if (group.MemberUin == bot.Uin) return;

            var textChain = group.Message.GetChain<TextChain>();
            if (textChain == null) return;

            try
            {
                MessageBuilder reply = null;
                {
                    if (textChain.Content.StartsWith("/help"))
                        reply = OnCommandHelp(textChain);
                    else if (textChain.Content.StartsWith("/ping"))
                        reply = OnCommandPing(textChain);
                    else if (textChain.Content.StartsWith("/status"))
                        reply = OnCommandStatus(textChain);
                    else if (textChain.Content.StartsWith("/echo"))
                        reply = OnCommandEcho(textChain, group.Message);
                    else if (textChain.Content.StartsWith("/eval"))
                        reply = OnCommandEval(group.Message);
                    else if (textChain.Content.StartsWith("/member"))
                        reply = await OnCommandMemberInfo(bot, group);
                    else if (textChain.Content.StartsWith("/mute"))
                        reply = await OnCommandMuteMember(bot, group);
                    else if (textChain.Content.StartsWith("/title"))
                        reply = await OnCommandSetTitle(bot, group);
                    else if (textChain.Content.StartsWith("BV"))
                        reply = await OnCommandBvParser(textChain);
                    else if (textChain.Content.StartsWith("https://github.com/"))
                        reply = await OnCommandGithubParser(textChain);
                    else if (Util.CanIDo(0.005))
                        reply = OnRepeat(group.Message);
                }

                // Send reply message
                if (reply != null)
                    await bot.SendGroupMessage(group.GroupUin, reply);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                // Send error print
                await bot.SendGroupMessage(group.GroupUin,
                    Text($"{e.Message}\n{e.StackTrace}"));
            }
        }

        /// <summary>
        /// On help
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandHelp(TextChain chain)
            => new MessageBuilder()
                .Text("[Kagami Help]\n")
                .Text("/help\n Print this message\n\n")
                .Text("/ping\n Pong!\n\n")
                .Text("/status\n Show bot status\n\n")
                .Text("/echo\n Send a message");

        /// <summary>
        /// On status
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandStatus(TextChain chain)
            => new MessageBuilder()
                // Core descriptions
                .Text($"[Kagami]\n")
                .Text($"[branch:{BuildStamp.Branch}]\n")
                .Text($"[commit:{BuildStamp.CommitHash[..12]}]\n")
                .Text($"[version:{BuildStamp.Version}]\n")
                .Text($"[{BuildStamp.BuildTime}]\n\n")

                // System status
                .Text($"Processed {_messageCounter} message(s)\n")
                .Text($"GC Memory {Util.Bytes2MiB(GC.GetTotalAllocatedBytes(), 2)} MiB " +
                      $"({Math.Round((double) GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n")
                .Text($"Total Memory {Util.Bytes2MiB(Process.GetCurrentProcess().WorkingSet64, 2)} MiB\n\n")

                // Copyrights
                .Text("Konata Project (C) 2022");

        /// <summary>
        /// On ping me
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandPing(TextChain chain)
            => Text("Hello, I'm Kagami");

        /// <summary>
        /// On message echo <br/>
        /// <b>Safer than MessageBuilder.Eval()</b>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandEcho(TextChain text, MessageChain chain)
            => new MessageBuilder(text.Content[5..].Trim()).Add(chain[1..]);

        /// <summary>
        /// On message eval
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandEval(MessageChain chain)
            => MessageBuilder.Eval(chain.ToString()[5..].TrimStart());

        /// <summary>
        /// On member info
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static async Task<MessageBuilder> OnCommandMemberInfo(Bot bot, GroupMessageEvent group)
        {
            // Get at
            var at = group.Message.GetChain<AtChain>();
            if (at == null) return Text("Agrument error");

            // Get group info
            var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.AtUin, true);
            if (memberInfo == null) return Text("No such member");

            return new MessageBuilder("[Member Info]\n")
                .Text($"Name: {memberInfo.Name}\n")
                .Text($"Join: {memberInfo.JoinTime}\n")
                .Text($"Role: {memberInfo.Role}\n")
                .Text($"Level: {memberInfo.Level}\n")
                .Text($"SpecTitle: {memberInfo.SpecialTitle}\n")
                .Text($"Nickname: {memberInfo.NickName}");
        }

        /// <summary>
        /// On mute
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static async Task<MessageBuilder> OnCommandMuteMember(Bot bot, GroupMessageEvent group)
        {
            // Get at
            var atchain = group.Message.GetChain<AtChain>();
            if (atchain == null) return Text("Argument error");

            var time = 60U;
            var textChains = group.Message
                .FindChain<TextChain>();
            {
                // Parse time
                if (textChains.Count == 2 &&
                    uint.TryParse(textChains[1].Content, out var t))
                {
                    time = t;
                }
            }

            try
            {
                if (await bot.GroupMuteMember(group.GroupUin, atchain.AtUin, time))
                    return Text($"Mute member [{atchain.AtUin}] for {time} sec.");
                return Text("Unknwon error.");
            }
            catch (OperationFailedException e)
            {
                return Text($"{e.Message} ({e.HResult})");
            }
        }

        /// <summary>
        /// Set title
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static async Task<MessageBuilder> OnCommandSetTitle(Bot bot, GroupMessageEvent group)
        {
            // Get at
            var atchain = group.Message.GetChain<AtChain>();
            if (atchain == null) return Text("Argument error");

            var textChains = group.Message
                .FindChain<TextChain>();
            {
                // Check argument
                if (textChains.Count != 2) return Text("Argument error");

                try
                {
                    if (await bot.GroupSetSpecialTitle(group.GroupUin, atchain.AtUin, textChains[1].Content, uint.MaxValue))
                        return Text($"Set special title for member [{atchain.AtUin}].");
                    return Text("Unknwon error.");
                }
                catch (OperationFailedException e)
                {
                    return Text($"{e.Message} ({e.HResult})");
                }
            }
        }

        /// <summary>
        /// Bv parser
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static async Task<MessageBuilder> OnCommandBvParser(TextChain chain)
        {
            var avCode = Util.Bv2Av(chain.Content);
            if (avCode == "") return Text("Invalid BV code");
            {
                // Download the page
                var bytes = await Util.Download($"https://www.bilibili.com/video/{avCode}");
                var html = Encoding.UTF8.GetString(bytes);
                {
                    // Get meta data
                    var metaData = Util.GetMetaData("itemprop", html);
                    var titleMeta = metaData["description"];
                    var imageMeta = metaData["image"];
                    var keywdMeta = metaData["keywords"];

                    // Download the image
                    var image = await Util.Download(imageMeta);

                    // Build message
                    var result = new MessageBuilder();
                    {
                        result.Text($"{titleMeta}\n");
                        result.Text($"https://www.bilibili.com/video/{avCode}\n\n");
                        result.Image(image);
                        result.Text("\n#" + string.Join(" #", keywdMeta.Split(",")[1..^4]));
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Github repo parser
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static async Task<MessageBuilder> OnCommandGithubParser(TextChain chain)
        {
            // Download the page
            try
            {
                var bytes = await Util.Download($"{chain.Content.TrimEnd('/')}.git");
                var html = Encoding.UTF8.GetString(bytes);
                {
                    // Get meta data
                    var metaData = Util.GetMetaData("property", html);
                    var imageMeta = metaData["og:image"];

                    // Build message
                    var image = await Util.Download(imageMeta);
                    return new MessageBuilder().Image(image);
                }
            }
            catch (WebException webException)
            {
                Console.WriteLine($"Not a repository link. \n" +
                                  $"{webException.Message}");
                return null;
            }
        }

        /// <summary>
        /// Repeat
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MessageBuilder OnRepeat(MessageChain message)
            => new(message);

        private static MessageBuilder Text(string text)
            => new MessageBuilder().Text(text);
    }
}
