using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public static class DragonCompileAudio
{
    private static AudioClip errorClip;
    private static AudioClip successClip;
    private static AudioClip warningClip;

    private static bool hadErrors;
    private static bool hadWarnings;

    static DragonCompileAudio()
    {
        Debug.Log("Dragon Dev Tools Loaded");
        CompilationPipeline.compilationStarted += OnCompilationStarted; 
        CompilationPipeline.assemblyCompilationFinished += OnAssemblyFinished;
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        LoadClips();
    }

    private static void LoadClips()
    {
        errorClip = LoadPackageClip("error");
        successClip = LoadPackageClip("success");
        warningClip = LoadPackageClip("warning");
    }

    private static AudioClip LoadPackageClip(string clipName)
    {
        string packagePath = "Packages/com.dragon.devtools/Runtime/Audio/";

        string[] guids = AssetDatabase.FindAssets(
            $"{clipName} t:AudioClip",
            new[] { packagePath });

        if (guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    private static void OnCompilationStarted(object obj)
    {
        hadErrors = false;
        hadWarnings = false;
    }

    private static void OnAssemblyFinished(string assemblyPath, CompilerMessage[] messages)
    {
        if (messages.Any(m => m.type == CompilerMessageType.Error))
            hadErrors = true;

        if (messages.Any(m => m.type == CompilerMessageType.Warning))
            hadWarnings = true;
    }

    private static void OnCompilationFinished(object obj)
    {
        if (!DragonPreferences.Enabled)
            return;

        if (hadErrors)
            Play(DragonPreferences.ErrorOverride ?? errorClip);
        else if (hadWarnings)
            Play(DragonPreferences.WarningOverride ?? warningClip);
        else
            Play(DragonPreferences.SuccessOverride ?? successClip);
    }

    private static void Play(AudioClip clip)
    {
        if (clip == null) return;

        var audioUtil = typeof(Editor).Assembly.GetType("UnityEditor.AudioUtil");
        var method = audioUtil.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null);

        method?.Invoke(null, new object[] { clip, 0, false });
    }
}