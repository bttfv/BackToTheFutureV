using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.IO;
//using System.Linq;
using System.Reflection;
//using System.Text;

public static class Potato
{
    /*private static readonly StringBuilder _log = new StringBuilder();
    private static readonly Dictionary<MethodInfo, List<(DateTime, long)>> _times =
        new Dictionary<MethodInfo, List<(DateTime, long)>>();*/
    private static Harmony _harmony;

    private static readonly HashSet<Type> _ignoredTypes = new HashSet<Type>();

    public static void AddIgnoreType(Type type)
    {
        _ignoredTypes.Add(type);
    }

    public static void Start()
    {
        if (_harmony != null)
            throw new Exception($"Profiler already running!");

        _harmony = new Harmony("Profiler");

        //_log.AppendLine("Starting profiler session.");

        var flags = BindingFlags.Static | BindingFlags.NonPublic;

        Assembly assembly = Assembly.GetExecutingAssembly();
        foreach (var module in assembly.GetModules())
        {
            //_log.AppendLine($"Module: {module}");
            foreach (var type in module.GetTypes())
            {
                if (type == typeof(Potato))
                    continue;

                if (_ignoredTypes.Contains(type))
                    continue;

                //_log.AppendLine($"\tType: {type}");

                foreach (var method in type.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly))
                {
                    //_log.AppendLine($"\t\tMethod: {method}");

                    var prefix = typeof(Potato).GetMethod("PatchPre", flags);
                    var postfix = typeof(Potato).GetMethod("PatchPost", flags);

                    try
                    {
                        _harmony.Patch(
                            method, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
                    }
                    catch
                    {
                    }
                }
            }
        }
        //_log.AppendLine("Patching finished.");
    }

    public static void Stop()
    {
        //_log.AppendLine("Stopping profiler session.\n");

        _harmony.UnpatchAll();
        _harmony = null;
    }

    public static void CleanLog()
    {
        /*_log.Clear();
        _times.Clear();*/
    }

    public static void Save()
    {
        /*StringBuilder sb = new StringBuilder();
        foreach (var methodCalls in _times)
        {
            sb.AppendLine($"{methodCalls.Key}");

            var calls = methodCalls.Value
                .OrderByDescending(x => x.Item2);

            foreach (var call in calls)
            {
                string time = call.Item1.ToString("mm:ss:FF");

                sb.AppendLine($"  [{time}] - {call.Item2}ms | {call.Item2 / 1000.0f:0.0}s");
            }
        }

        try
        {
            string fileName = $"PROFILER {DateTime.Now:MM.dd.yyyy HH-mm-ss}.txt";
            File.WriteAllText(fileName, _log.ToString() + "\n" + sb.ToString());
        }
        catch
        {
            // Log...
        }*/
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static void PatchPre(out Stopwatch __state)
    {
        __state = Stopwatch.StartNew();
    }

    private static void PatchPost(MethodInfo __originalMethod, Stopwatch __state)
    {
        __state.Stop();

        /*if (__state.ElapsedMilliseconds < 100)
            return;

        if (!_times.ContainsKey(__originalMethod))
            _times.Add(__originalMethod, new List<(DateTime, long)>()
                {
                    (DateTime.Now, __state.ElapsedMilliseconds)
                });
        else
            _times[__originalMethod].Add((DateTime.Now, __state.ElapsedMilliseconds));*/
    }
#pragma warning restore IDE0051 // Remove unused private members
}
