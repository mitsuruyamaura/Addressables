using System;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CommandLineArgs {
    public static string GetValue(string name) {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == $"-{name}" && args.Length > i + 1) {
                return args[i + 1];
            }
        }
        return null;
    }
}