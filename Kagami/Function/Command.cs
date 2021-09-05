using System;
using System.Diagnostics;
using System.Text;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using Konata.Core.Message.Model;

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
        internal static void OnGroupMessage(object sender, GroupMessageEvent group)
        {
            // Increase
            ++_messageCounter;

            var bot = (Bot) sender;
            var textChain = group.Message.GetChain<PlainTextChain>();
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
                        reply = OnCommandEcho(group.Message);
                    else if (textChain.Content.StartsWith("BV"))
                        reply = OnCommandBvParser(textChain);
                    else if (textChain.Content.StartsWith("https://github.com/"))
                        reply = OnCommandGithubParser(textChain);
                }

                // Send reply message
                if (reply != null) bot.SendGroupMessage(group.GroupUin, reply);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                // Send error print
                bot.SendGroupMessage(group.GroupUin,
                    Text($"{e.Message}\n{e.StackTrace}"));
            }
        }

        /// <summary>
        /// On help
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandHelp(PlainTextChain chain)
            => new MessageBuilder()
                .PlainText("[Kagami Help]\n")
                .PlainText("/help\n Print this message\n\n")
                .PlainText("/ping\n Pong!\n\n")
                .PlainText("/status\n Show bot status\n\n")
                .PlainText("/echo\n Send a message");

        /// <summary>
        /// On status
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandStatus(PlainTextChain chain)
            => new MessageBuilder()
                // Core descriptions
                .PlainText($"[Kagami]\n")
                .PlainText($"[branch:{BuildStamp.Branch}]\n")
                .PlainText($"[commit:{BuildStamp.CommitHash[..12]}]\n")
                .PlainText($"[version:{BuildStamp.Version}]\n")
                .PlainText($"[{BuildStamp.BuildTime}]\n\n")

                // System status
                .PlainText($"Processed {_messageCounter} message(s)\n")
                .PlainText($"GC Memory {Util.Bytes2MiB(GC.GetTotalAllocatedBytes(), 2)} MiB " +
                           $"({Math.Round((double) GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n")
                .PlainText($"Total Memory {Util.Bytes2MiB(Process.GetCurrentProcess().WorkingSet64, 2)} MiB\n\n")

                // Copyrights
                .PlainText("Konata Project (C) 2021");

        /// <summary>
        /// On ping me
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandPing(PlainTextChain chain)
            => Text("Hello, I'm Kagami");

        /// <summary>
        /// On message echo
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandEcho(MessageChain chain)
            => MessageBuilder.Eval(chain.ToString()[5..].TrimStart());

        /// <summary>
        /// Bv parser
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        public static MessageBuilder OnCommandBvParser(PlainTextChain chain)
        {
            var avCode = Util.Bv2Av(chain.Content);
            if (avCode == "") return Text("Invalid BV code");
            {
                // Download the page
                var bytes = Util.Download($"https://www.bilibili.com/video/{avCode}").Result;
                var html = Encoding.UTF8.GetString(bytes);
                {
                    // Get meta data
                    var metaData = Util.GetMetaData("itemprop", html);
                    var titleMeta = metaData["description"];
                    var imageMeta = metaData["image"];
                    var keywdMeta = metaData["keywords"];

                    // Download the image
                    var image = Util.Download(imageMeta).Result;

                    // Build message
                    var result = new MessageBuilder();
                    {
                        result.PlainText($"{titleMeta}\n");
                        result.PlainText($"https://www.bilibili.com/video/{avCode}\n\n");
                        result.Image(image);
                        result.PlainText("\n#" + string.Join(" #", keywdMeta.Split(",")[1..^4]));
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
        public static MessageBuilder OnCommandGithubParser(PlainTextChain chain)
        {
            // Download the page
            var bytes = Util.Download(chain.Content).Result;
            var html = Encoding.UTF8.GetString(bytes);
            {
                // Get meta data
                var metaData = Util.GetMetaData("property", html);
                var imageMeta = metaData["og:image"];

                // Download the image
                var image = Util.Download(imageMeta).Result;

                // Build message
                return new MessageBuilder().Image(image);
            }
        }

        private static MessageBuilder Text(string text)
            => new MessageBuilder().PlainText(text);
    }
}
