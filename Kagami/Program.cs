using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using Kagami.SandBox;

namespace Kagami;

public static class Program
{
    private static Bot _bot = null!;
    private static ReplEnvironment _sandboxEnv = null!;
    private static ReplRuntime<ReplEnvironment> _sandbox = null!;

    private static readonly Regex[] SandBoxFilter =
    {
        new(@"^(true|false)$", RegexOptions.IgnoreCase | RegexOptions.Multiline),
        new(@"^(\+|-)?[0-9.]*(U|L|UL|F|D|M)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline),
    };

    public static async Task Main()
    {
        // Create sandbox
        _sandboxEnv = new ReplEnvironment();
        _sandbox = new ReplRuntime<ReplEnvironment>(_sandboxEnv,

            // Additional references
            new[] {"Konata.Core", "Paraparty.JsonChan"},

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
                "Konata.Core.Events.Model",
                "Paraparty.JsonChan"
            },

            // Initial script
            null,

            // Enable checks
            true,

            // Execution timeout
            5000
        );

        // Compile commands
        await _sandbox.ScanScriptsAndLoad();
        
        // Create bot
        _bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());
        {
            // Print the log
            _bot.OnLog += (_, e) =>
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " +
                                  $"[{e.Level}] [{e.Tag}] {e.EventMessage}");

            // Handle the captcha
            _bot.OnCaptcha += (s, e) =>
            {
                switch (e.Type)
                {
                    case CaptchaEvent.CaptchaType.Sms:
                        Console.WriteLine(e.Phone);
                        s.SubmitSmsCode(Console.ReadLine());
                        break;

                    case CaptchaEvent.CaptchaType.Slider:
                        Console.WriteLine(e.SliderUrl);
                        s.SubmitSliderTicket(Console.ReadLine());
                        break;

                    default:
                    case CaptchaEvent.CaptchaType.Unknown:
                        break;
                }
            };

            // Handle group messages
            _bot.OnGroupMessage += GroupMessageHandler;

            // Update the keystore
            _bot.OnBotOnline += (bot, _) =>
            {
                UpdateKeystore(bot.KeyStore);
                Console.WriteLine("Bot keystore updated.");
            };

            // Login the bot
            if (!await _bot.Login())
            {
                Console.WriteLine("Oops... Login failed.");
                return;
            }

            // cli
            while (true)
            {
                try
                {
                    switch (Console.ReadLine())
                    {
                        case "/stop":
                            await _bot.Logout();
                            _bot.Dispose();
                            return;

                        case "/login":
                            await _bot.Login();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
    }

    /// <summary>
    /// Scan scripts and load
    /// </summary>
    /// <param name="sandbox"></param>
    private static async Task ScanScriptsAndLoad(this ReplRuntime<ReplEnvironment> sandbox)
    {
        // If directory does not exist
        if (!Directory.Exists("scripts"))
        {
            Console.WriteLine($"[ !!! ] The 'scripts' directory does not exist, return.");
            return;
        }

        // Compile scripts and load
        foreach (var i in Directory.EnumerateFiles
                     ("scripts/", "*.cs", SearchOption.AllDirectories))
        {
            try
            {
                Console.WriteLine($"[ *** ] REPL compiling script => {i}");
                await sandbox.AddScript(await File.ReadAllTextAsync(i));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ !!! ] Failed to add script {i}, ignoring.");
                Console.WriteLine($"[     ] {e.Message}");
            }
        }

        Console.WriteLine("[ *** ] REPL scripts load finished.");
    }

    /// <summary>
    /// Load or create device 
    /// </summary>
    /// <returns></returns>
    private static BotDevice? GetDevice()
    {
        // Read the device from config
        if (File.Exists("device.json"))
        {
            return JsonSerializer.Deserialize
                <BotDevice>(File.ReadAllText("device.json"));
        }

        // Create new one
        var device = BotDevice.Default();
        {
            var deviceJson = JsonSerializer.Serialize(device,
                new JsonSerializerOptions {WriteIndented = true});
            File.WriteAllText("device.json", deviceJson);
        }

        return device;
    }

    /// <summary>
    /// Load or create configuration
    /// </summary>
    /// <returns></returns>
    private static BotConfig? GetConfig()
    {
        // Read the device from config
        if (File.Exists("config.json"))
        {
            return JsonSerializer.Deserialize
                <BotConfig>(File.ReadAllText("config.json"));
        }

        // Create new one
        var config = new BotConfig
        {
            EnableAudio = true,
            TryReconnect = true,
            HighwayChunkSize = 8192,
            DefaultTimeout = 6000,
            Protocol = OicqProtocol.AndroidPhone
        };

        // Write to file
        var configJson = JsonSerializer.Serialize(config,
            new JsonSerializerOptions {WriteIndented = true});
        File.WriteAllText("config.json", configJson);

        return config;
    }

    /// <summary>
    /// Load or create keystore
    /// </summary>
    /// <returns></returns>
    private static BotKeyStore? GetKeyStore()
    {
        // Read the device from config
        if (File.Exists("keystore.json"))
        {
            return JsonSerializer.Deserialize
                <BotKeyStore>(File.ReadAllText("keystore.json"));
        }

        Console.WriteLine("For first running, please " +
                          "type your account and password.");

        Console.Write("Account: ");
        var account = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        // Create new one
        Console.WriteLine("Bot created.");
        return UpdateKeystore(new BotKeyStore(account, password));
    }

    /// <summary>
    /// Update keystore
    /// </summary>
    /// <param name="keystore"></param>
    /// <returns></returns>
    private static BotKeyStore UpdateKeystore(BotKeyStore keystore)
    {
        var keystoreJson = JsonSerializer.Serialize(keystore,
            new JsonSerializerOptions {WriteIndented = true});
        File.WriteAllText("keystore.json", keystoreJson);
        return keystore;
    }

    private static async void GroupMessageHandler(Bot bot, GroupMessageEvent group)
    {
        // Ignore messages from bot itself
        if (group.MemberUin == bot.Uin) return;

        // Takeout text chain for below processing
        var textChain = group.Chain.GetChain<TextChain>();
        if (textChain is null) return;

        MessageBuilder? reply = null;

        try
        {
            var debugging = false;
            var content = textChain.Content;

            if (content.StartsWith("/status"))
                reply = Command.OnCommandStatus();
            else if (content.StartsWith("https://github.com/"))
                reply = await Command.OnCommandGithubParser(textChain);

            // User-defined function in REPL
            else if (content.StartsWith("/") && !content.StartsWith("/dbg"))
            {
                // Teardown command into an array
                var cmdArray = content[1..].Split(" ");
                if (cmdArray.Length == 0) return;

                // if user-defined function exist
                object? funcResult = null;
                if (_sandbox.GetReplFuncion(cmdArray[0], out var func))
                    funcResult = _sandbox.CallReplDelegate(func, cmdArray[1..]);

                // Call and convert to result
                if (funcResult != null)
                    reply = MessageBuilder.Eval(funcResult.ToString());
            }
            else
            {
                // Enable repl debug echo
                if (content.StartsWith("/dbg"))
                {
                    content = content[4..];
                    debugging = true;
                }

                // Ignore immediate expressions
                foreach (var filter in SandBoxFilter)
                {
                    if (filter.IsMatch(content)) return;
                }

                // Setup context
                _sandboxEnv.Bot = bot;
                _sandboxEnv.CurrentGroup = group.GroupUin;
                _sandboxEnv.CurrentMember = group.MemberUin;

                try
                {
                    // Run REPL code
                    var result = await _sandbox.RunAsync(content);
                    if (result == null) return;

                    // Then error check
                    if (result is Exception e)
                    {
                        // Normal runtime error
                        if (e is ReplRuntimeException rre)
                            result = $"{rre.InnerException!.Message}\n{rre.InnerException!.StackTrace}";

                        // Silent compilation errors if not debugging
                        else result = debugging ? e.Message : null;
                    }

                    // No errors return
                    if (result != null)
                        reply = MessageBuilder.Eval(result.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            // Send reply message
            if (reply is not null) await bot.SendGroupMessage(group.GroupUin, reply);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
