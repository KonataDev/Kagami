using System.Threading.Tasks;
using Kagami.SandBox;
using NUnit.Framework;

namespace Kagami.Test;

public class TestReplRuntime
{
    private static ReplEnvironment? _env;
    private static ReplRuntime<ReplEnvironment>? _repl;
    private const string StringExploitOk = "ExploitOK";
    private const string StringProtected = "Protected";

    [SetUp]
    public void Setup()
    {
        _env = new ReplEnvironment();
        _repl = new ReplRuntime<ReplEnvironment>(_env,

            // Additional references
            new[] {"System.Diagnostics"},

            // Additional usings
            null,

            // Initial script
            null,

            // Enable checks
            true,
            
            // Execution timeout
            5000
        );
    }

    private static async Task<string?> RunAsync(string code)
    {
        var result = await _repl!.RunAsync(code);
        if (result == null) return null;
        return result.ToString();
    }

    [Test(ExpectedResult = "Hello World!")]
    public async Task<string> HelloWorld()
        => await RunAsync("\"Hello World!\"") ?? string.Empty;

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
        Assert.AreEqual(result, StringProtected);
    }

    [Test]
    public void ExpThreadDomainPayloadRce()
    {
        RunAsync("using s = System;").Wait();
        RunAsync("var domain = Thread.GetDomain();").Wait();
        RunAsync("var payload = domain.Load(typeof(s.Activator).Assembly.Location);").Wait();
        var result = RunAsync($"return payload == null ? \"{StringExploitOk}\" : \"{StringExploitOk}\";").Result ?? StringProtected;
        Assert.AreEqual(result, StringProtected);
    }

    [Test]
    public void ExpLoadDll()
    {
        RunAsync("using System.Runtime.InteropServices;").Wait();
        RunAsync("[DllImport(\"kernel32.dll\", CharSet = CharSet.Auto, SetLastError = true)]\n" +
                 "static extern IntPtr LoadLibraryA(string libname);").Wait();
        RunAsync("var payload = LoadLibraryA(\"{kernel32.dll}\");").Wait();
        var result = RunAsync($"return payload != null ? \"{StringExploitOk}\" : \"{StringExploitOk}\";").Result ?? StringProtected;
        Assert.AreEqual(result, StringProtected);
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
