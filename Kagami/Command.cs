using Kagami.Utils;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local

namespace Kagami;

public static class Command
{

    /// <summary>
    /// On status
    /// </summary>
    /// <returns></returns>
    public static MessageBuilder OnCommandStatus()
        => new MessageBuilder()
            // Core descriptions
            .Text($"[Kagami]\n")
            .Text($"[branch:{BuildStamp.Branch}]\n")
            .Text($"[commit:{BuildStamp.CommitHash[..12]}]\n")
            .Text($"[version:{BuildStamp.Version}]\n")
            .Text($"[{BuildStamp.BuildTime}]\n\n")

            // System status
            .Text($"GC Memory {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB \n")
            .Text($"Total Memory {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB\n\n")

            // Copyrights
            .Text($"Konata Project (C) {DateTime.Now.Year}");

    /// <summary>
    /// Github repo parser
    /// </summary>
    /// <param name="chain"></param>
    /// <returns></returns>
    public static async Task<MessageBuilder> OnCommandGithubParser(TextChain chain)
    {
        // UrlDownload the page
        try
        {
            var bytes = await $"{chain.Content.TrimEnd('/')}.git".UrlDownload();

            var html = Encoding.UTF8.GetString(bytes);

            // Get meta data
            var metaData = html.GetMetaData("property");
            var imageMeta = metaData["og:image"];

            // Build message
            var image = await imageMeta.UrlDownload();
            return new MessageBuilder().Image(image);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Not a repository link. \n" +
                              $"{e.Message}");
            return Text("Not a repository link.");
        }
    }

    private static MessageBuilder Text(string text)
        => new MessageBuilder().Text(text);
}