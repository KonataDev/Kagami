using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Paraparty.JsonChan;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Kagami.SandBox;

public class ReplEnvironment
{
    /// <summary>
    /// Bot instance
    /// </summary>
    public Bot? Bot { get; internal set; }

    /// <summary>
    /// Current group
    /// </summary>
    public uint CurrentGroup { get; internal set; }

    /// <summary>
    /// Message source uin
    /// </summary>
    public uint CurrentMember { get; internal set; }

    /// <summary>
    /// Shared Random 
    /// </summary>
    public Random Random => Random.Shared;

    #region Pre-defined classes

    public static class JSON
    {
        public static dynamic Parse(string s) => Json.Parse(s);
    }

    #endregion

    #region Pre-defined functions

    /// <summary>
    /// Print messages
    /// </summary>
    /// <param name="message"></param>
    public void Print(string message)
        => Bot.SendGroupMessage(CurrentGroup, MessageBuilder.Eval(message));

    /// <summary>
    /// Random a selection from an object array
    /// </summary>
    /// <param name="selections"></param>
    /// <returns></returns>
    public object Roll(params object[] selections)
        => selections[Random.Next(0, selections.Length)];

    /// <summary>
    /// Can I do
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool CanIDo(double p = 0.5f)
        => Random.NextDouble() > (1.0 - p);

    /// <summary>
    /// Http download
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public Task<byte[]> Wget(string url)
        => url.UrlDownload();

    /// <summary>
    /// Repeat string
    /// </summary>
    /// <param name="str"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public string Repeat(string str, uint count)
    {
        var arr = new string[count];
        Array.Fill(arr, str);
        return string.Join("", arr);
    }

    #endregion
}
