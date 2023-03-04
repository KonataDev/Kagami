using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable StaticMemberInGenericType

namespace Kagami.SandBox;

public class ReplRuntime<T>
{
    private static readonly RegexOptions FilterOption
        = RegexOptions.IgnoreCase | RegexOptions.Singleline;

    private static readonly Regex[] FilterKeywords =
    {
        new(@"^(Type|TypeInfo|Console|Reflection|Activator)$", FilterOption),
        new(@"^(Environment|Process|Directory|Socket)$", FilterOption),
        new(@"^(Domain|Assemblies|Assemply|Modules|AppDomain|Win32|AppContext|Microsoft|OperatingSystem)$", FilterOption),
        new(@"^(InteropServices|Marshal|DllImport)$", FilterOption),
        new(@"^(File|Http|Net)$", FilterOption),
    };

    private static readonly Regex[] FilterNamespaces =
    {
        new(@"(System\.IO)", FilterOption),
        new(@"(System\.Net)", FilterOption),
        new(@"(System\.Web)", FilterOption),
        new(@"(System\.Reflection)", FilterOption),
        new(@"(System\.Diagnostics)", FilterOption),
        new(@"(System\.Runtime)", FilterOption),
        new(@"(System\.Windows)", FilterOption),
        new(@"(System\.Resources)", FilterOption),
        new(@"(System\.CodeDom)", FilterOption),
        new(@"(System\.Activator)", FilterOption),
    };

    private ScriptState<object> _globalState;
    private readonly ScriptOptions _globalOptions;
    private readonly bool _enableChecks;
    private readonly int _execTimeout;

    /// <summary>
    /// Repl runtime ctor
    /// </summary>
    /// <param name="global">global environment object</param>
    /// <param name="additionalReferences">additional references</param>
    /// <param name="additionalUsings">additional usings</param>
    /// <param name="initialScript">initial script</param>
    /// <param name="enableChecks">enable code rule check</param>
    /// <param name="execTimeout">timeout of script execution</param>
    public ReplRuntime(T global, string[]? additionalReferences, string[]? additionalUsings,
        string? initialScript, bool enableChecks, int execTimeout)
    {
        // Create script options
        _globalOptions = ScriptOptions.Default
            .AddReferences("Microsoft.CSharp")
            .AddReferences("System")
            .AddReferences(typeof(System.Text.Json.JsonSerializer).Assembly)
            .AddImports("System")
            .AddImports("System.Linq")
            .AddImports("System.Collections.Generic")
            .AddImports("System.Text")
            .AddImports("System.Text.Json")
            .AddImports("System.Text.Encoding")
            .WithCheckOverflow(true);
        {
            if (additionalReferences != null)
            {
                foreach (var i in additionalReferences)
                    _globalOptions.AddReferences(i);
            }

            if (additionalUsings != null)
            {
                foreach (var i in additionalUsings)
                    _globalOptions.AddImports(i);
            }
        }

        // Create initial script
        var script = CSharpScript.Create(initialScript ?? "",
            _globalOptions, global == null ? null : typeof(T));
        {
            // Create a empty state
            _globalState = script.RunAsync
                (global, _ => true).GetAwaiter().GetResult();
        }

        // Enable code rule checks
        _enableChecks = enableChecks;
        _execTimeout = execTimeout;
    }

    /// <summary>
    /// Run code async
    /// </summary>
    /// <param name="code">c# code</param>
    /// <returns></returns>
    public async Task<object?> RunAsync(string code)
    {
        // Run semantic check
        if (_enableChecks)
        {
            var syntax = CSharpSyntaxTree.ParseText(code);
            foreach (var node in (await syntax.GetRootAsync()).DescendantNodes())
            {
                // Not allow typeof expression
                if (node is TypeOfExpressionSyntax) return null;

                // Not allow using directive expression
                else if (node is UsingDirectiveSyntax) return null;

                // Check namespaces
                else if (node is MemberAccessExpressionSyntax)
                {
                    // Match keywords
                    var name = node.ToString();
                    foreach (var rules in FilterNamespaces)
                    {
                        if (rules.IsMatch(name))
                            return null;
                    }
                }

                // Check identifiers
                else if (node is IdentifierNameSyntax || node is QualifiedNameSyntax)
                {
                    // Match keywords
                    var name = node.ToString();
                    foreach (var rules in FilterKeywords)
                    {
                        if (rules.IsMatch(name))
                            return null;
                    }
                }
            }

            // Checks are all passed, here we go~ = w =
        }

        lock (_globalState)
        {
            // Append the code to the last session
            var newScript = _globalState.Script
                .ContinueWith(code, _globalOptions);

            // Analysis context
            (Exception? compileErr, Exception? runtimeErr,
                bool hasCompileErr, Thread thread, bool finish) analysisCtx = new();
            {
                analysisCtx.thread = new Thread(() =>
                {
                    try
                    {
                        // Execute the code in the thread
                        _globalState = newScript.RunFromAsync(_globalState, e =>
                        {
                            analysisCtx.runtimeErr = e;
                            return true;
                        }).Result;
                    }

                    // Compile-time errors
                    catch (Exception e)
                    {
                        // Ignore CS0103
                        analysisCtx.compileErr = e.Message.Contains("error CS0103") ? null : e;
                        analysisCtx.hasCompileErr = true;
                    }
                    finally
                    {
                        analysisCtx.finish = true;
                    }
                });
            }

            // Configure thread an run
            analysisCtx.thread.IsBackground = true;
            analysisCtx.thread.Priority = ThreadPriority.Lowest;
            analysisCtx.thread.Start();
            analysisCtx.thread.Join(_execTimeout);

            // If execution timeout
            if (!analysisCtx.finish)
            {
                analysisCtx.thread.Interrupt();
                analysisCtx.runtimeErr = new TimeoutException("Script execution timeout, exceed 5000ms.");
            }

            // Return results
            if (analysisCtx.hasCompileErr) return analysisCtx.compileErr;
            else if (analysisCtx.runtimeErr != null) return new ReplRuntimeException(analysisCtx.runtimeErr);
            else return _globalState.ReturnValue;
        }
    }

    /// <summary>
    /// Get REPL function
    /// </summary>
    /// <param name="funcName"></param>
    /// <param name="funcDelegate"></param>
    /// <returns></returns>
    public bool GetReplFuncion(string funcName, out Delegate? funcDelegate)
    {
        funcDelegate = null;

        // Check the name
        for (var i = _globalState.Variables.Length - 1; i >= 0; --i)
        {
            if (_globalState.Variables[i].Name != funcName) continue;

            try
            {
                // Create a delegate
                var method = _globalState.Variables[i].Value;
                var invoke = Delegate.CreateDelegate(method.GetType(),
                    method, method.GetType().GetMethod("Invoke")!, true);

                // Ready to use
                if (invoke != null)
                {
                    funcDelegate = invoke;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Call REPL function delegates
    /// </summary>
    /// <param name="funcDelegate"></param>
    /// <param name="funcParameters"></param>
    /// <returns></returns>
    public object? CallReplDelegate(Delegate? funcDelegate, params string[] funcParameters)
    {
        if (funcDelegate == null) return null;

        // Process parameter conventions
        var args = new List<object?>();
        var argindex = 0;

        foreach (var arg in funcDelegate.Method.GetParameters())
        {
            // Only supports c# standard types
            args.Add(arg.ParameterType.Name switch
            {
                nameof(String) => funcParameters[argindex],
                nameof(Boolean) => Convert.ToBoolean(funcParameters[argindex]),

                nameof(Byte) => Convert.ToByte(funcParameters[argindex]),
                nameof(UInt16) => Convert.ToUInt16(funcParameters[argindex]),
                nameof(UInt32) => Convert.ToUInt32(funcParameters[argindex]),
                nameof(UInt64) => Convert.ToUInt64(funcParameters[argindex]),

                nameof(SByte) => Convert.ToSByte(funcParameters[argindex]),
                nameof(Int16) => Convert.ToInt16(funcParameters[argindex]),
                nameof(Int32) => Convert.ToInt32(funcParameters[argindex]),
                nameof(Int64) => Convert.ToInt64(funcParameters[argindex]),

                nameof(Single) => Convert.ToSingle(funcParameters[argindex]),
                nameof(Double) => Convert.ToDouble(funcParameters[argindex]),
                nameof(Decimal) => Convert.ToDecimal(funcParameters[argindex]),
                nameof(DateTime) => Convert.ToDateTime(funcParameters[argindex]),
                nameof(Char) => Convert.ToChar(funcParameters[argindex]),

                // any as object 
                _ => funcParameters[argindex]
            });

            ++argindex;
        }

        return funcDelegate.DynamicInvoke(args.ToArray());
    }

    /// <summary>
    /// Export script
    /// </summary>
    /// <returns></returns>
    public string ExportScript()
    {
        var export = "";
        var script = _globalState.Script;

        // enum till the head of script
        do
        {
            export = $"{script.Code}\n{export};";
            script = script.Previous;
        } while (script.Previous != null);

        return export;
    }
}

public class ReplRuntimeException : Exception
{
    public ReplRuntimeException(Exception e) : base(e.Message, e)
    {
    }
}
