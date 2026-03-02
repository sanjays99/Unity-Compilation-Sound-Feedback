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

        if (hadErrors && DragonPreferences.ErrorEnabled)
            Play(DragonPreferences.ErrorOverride ?? errorClip);
        else if (hadWarnings && DragonPreferences.WarningEnabled)
            Play(DragonPreferences.WarningOverride ?? warningClip);
        else if (!hadErrors && !hadWarnings && DragonPreferences.SuccessEnabled)
            Play(DragonPreferences.SuccessOverride ?? successClip);
    }

    private static void Play(AudioClip clip)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("DragonDevTools_AudioPreview");
        tempGO.hideFlags = HideFlags.HideAndDontSave;

        var source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = DragonPreferences.Volume;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D sound

        source.Play();

        EditorApplication.update += Update;

        void Update()
        {
            if (source == null)
            {
                EditorApplication.update -= Update;
                return;
            }

            if (!source.isPlaying)
            {
                EditorApplication.update -= Update;
                GameObject.DestroyImmediate(tempGO);
            }
        }
    }
}