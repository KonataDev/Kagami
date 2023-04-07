using System;
using System.Threading;
using System.Threading.Tasks;
using Kagami.SandBox;
using NUnit.Framework;

namespace Kagami.Test;

public class TestReplRuntime
{
    private static ReplRuntime<ReplEnvironment>? _repl;
    private const string StringExploitOk = "ExploitOK";
    private const string StringProtected = "Protected";

    [SetUp]
    public void Setup()
    {
        _repl = new ReplRuntime<ReplEnvironment>(
            // Additional references
            new[]
            {
                "System.Diagnostics",
                "Konata.Core",
                "Paraparty.JsonChan"
            },

            // Additional usings
            new[]
            {
                "System.Runtime",
                "System.Threading",
                "System.Threading.Tasks",
                "Konata.Core",
                "Konata.Core.Common",
                "Konata.Core.Interfaces",
                "Konata.Core.Interfaces.Api",
                "Konata.Core.Events",
                "Konata.Core.Events.Model",
                "Konata.Core.Message",
                "Konata.Core.Message.Model",
                "Paraparty.JsonChan"
            },

            // Initial script
            null,

            // Enable checks
            true,

            // Execution timeout
            5000
        );
    }

    private static async Task<string?> RunAsync(string code, Action<ReplEnvironment>? cb = null)
    {
        var result = await _repl!.RunAsync(code, cb);
        if (result == null) return null;
        return result.ToString();
    }

    private static async Task<object?> RunAsyncAsObject(string code, Action<ReplEnvironment>? cb = null)
    {
        var result = await _repl!.RunAsync(code, cb);
        if (result == null) return null;
        return result;
    }

    [Test(ExpectedResult = "Hello World!")]
    public async Task<string> HelloWorld()
        => await RunAsync("\"Hello World!\"") ?? string.Empty;

    [Test]
    public void CreateBot()
    {
        var result = RunAsync("BotFather.Create(\"233\",\"testtest\", out _, out _, out _) != null").Result;
        Assert.AreEqual("True", result);
    }

    [Test]
    public void RunInMultiThreaded()
    {
        async Task<bool> Run(uint number)
        {
            var result = await RunAsyncAsObject("CurrentMember",
                env => { env.CurrentMember = number; }) as uint? ?? 0;
            return result == number;
        }

        for (var i = 0U; i < 20; ++i)
        {
            var i1 = i;
            new Thread(() =>
            {
                if (!Run(i1).Result) throw new Exception("Not equal");
            }).Start();
        }

        Assert.Pass();
    }

    [Test(ExpectedResult = StringProtected)]
    public async Task<string> ExpProcessStart()
        => await RunAsync($"System.Diagnostics.Process.Start(\"calc.exe\"); " +
                          $"return \"{StringExploitOk}\";") ?? StringProtected;

    [Test]
    public void ExpUsingRenameProcessStart()
    {
        RunAsync("using Diag = System.Diagnostics;").Wait();
        var result = RunAsync("Diag.Process.Start(\"calc.exe\"); " +
                              "return \"Exploit OK\";").Result ?? StringProtected;
        Assert.AreEqual(StringProtected, result);
    }

    [Test]
    public void ExpThreadDomainPayloadRce()
    {
        RunAsync("using s = System;").Wait();
        RunAsync("var domain = Thread.GetDomain();").Wait();
        RunAsync("var payload = domain.Load(typeof(s.Activator).Assembly.Location);").Wait();
        var result = RunAsync($"return payload == null ? \"{StringExploitOk}\" : \"{StringExploitOk}\";").Result ?? StringProtected;
        Assert.AreEqual(StringProtected, result);
    }

    [Test]
    public void ExpLoadDll()
    {
        RunAsync("using System.Runtime.InteropServices;").Wait();
        RunAsync("[DllImport(\"kernel32.dll\", CharSet = CharSet.Auto, SetLastError = true)]\n" +
                 "static extern IntPtr LoadLibraryA(string libname);").Wait();
        RunAsync("var payload = LoadLibraryA(\"{kernel32.dll}\");").Wait();
        var result = RunAsync($"return payload != null ? \"{StringExploitOk}\" : \"{StringExploitOk}\";").Result ?? StringProtected;
        Assert.AreEqual(StringProtected, result);
    }

    [Test]
    public void ExpGetType()
    {
        var result = RunAsync("var s = 1.GetType().FullName;"
            + $"return \"{StringExploitOk}\";").Result ?? StringProtected;
        Assert.AreEqual(StringProtected, result);
    }

    [Test]
    public void ExpThreadBasedLongLoop()
    {
        RunAsync("Thread.Sleep(int.MaxValue);").Wait();
        RunAsync("Thread.SpinWait(int.MaxValue);").Wait();
        Assert.Pass();
    }

    [Test]
    public void ExpSimpleDeadLoop()
    {
        // Too many deadloop tricks, woaaaa...
        RunAsync("while(true);").Wait();
        RunAsync("while(!false);").Wait();
        RunAsync("var a = 1; while(a == 1);").Wait();
        RunAsync("for(;;);").Wait();
        RunAsync("for(var i = 0; i <= 0; --i);").Wait();
        RunAsync("for(var i = 0; i >= 0; ++i);").Wait();
        RunAsync("for(;true;);").Wait();
        RunAsync("for(;!false;);").Wait();
        RunAsync("Here: goto Here;").Wait();
        Assert.Pass();
    }

    [Test]
    public void AssertPass()
    {
        if (_repl != null) Assert.Pass();
        Assert.Fail();
    }
}
