using UnityEditor;
using UnityEngine;

public static class DragonPreferences
{
    private const string EnabledKey = "DragonDevTools_Enabled";
    private const string ErrorEnabledKey = "DragonDevTools_ErrorEnabled";
    private const string WarningEnabledKey = "DragonDevTools_WarningEnabled";
    private const string SuccessEnabledKey = "DragonDevTools_SuccessEnabled";
    private const string VolumeKey = "DragonDevTools_Volume";

    private const string ErrorGUIDKey = "DragonDevTools_ErrorClipGUID";
    private const string WarningGUIDKey = "DragonDevTools_WarningClipGUID";
    private const string SuccessGUIDKey = "DragonDevTools_SuccessClipGUID";

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(EnabledKey, true);
        set => EditorPrefs.SetBool(EnabledKey, value);
    }

    public static bool ErrorEnabled
    {
        get => EditorPrefs.GetBool(ErrorEnabledKey, true);
        set => EditorPrefs.SetBool(ErrorEnabledKey, value);
    }

    public static bool WarningEnabled
    {
        get => EditorPrefs.GetBool(WarningEnabledKey, true);
        set => EditorPrefs.SetBool(WarningEnabledKey, value);
    }

    public static bool SuccessEnabled
    {
        get => EditorPrefs.GetBool(SuccessEnabledKey, true);
        set => EditorPrefs.SetBool(SuccessEnabledKey, value);
    }

    public static float Volume
    {
        get => EditorPrefs.GetFloat(VolumeKey, 1f);
        set => EditorPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
    }

    public static AudioClip ErrorOverride
    {
        get => LoadClip(ErrorGUIDKey);
        set => SaveClip(ErrorGUIDKey, value);
    }

    public static AudioClip WarningOverride
    {
        get => LoadClip(WarningGUIDKey);
        set => SaveClip(WarningGUIDKey, value);
    }

    public static AudioClip SuccessOverride
    {
        get => LoadClip(SuccessGUIDKey);
        set => SaveClip(SuccessGUIDKey, value);
    }

    private static void SaveClip(string key, AudioClip clip)
    {
        if (clip == null)
        {
            EditorPrefs.DeleteKey(key);
            return;
        }

        string path = AssetDatabase.GetAssetPath(clip);
        if (string.IsNullOrEmpty(path)) return;

        string guid = AssetDatabase.AssetPathToGUID(path);
        EditorPrefs.SetString(key, guid);
    }

    private static AudioClip LoadClip(string key)
    {
        string guid = EditorPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(guid)) return null;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    [SettingsProvider]
    public static UnityEditor.SettingsProvider CreateProvider()
    {
        return new UnityEditor.SettingsProvider("Preferences/Dragon Dev Tools", SettingsScope.User)
        {
            label = "Dragon Dev Tools",
            guiHandler = (searchContext) =>
            {
                Enabled = EditorGUILayout.ToggleLeft("Enable Compile Sounds", Enabled);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Per-Type Control", EditorStyles.boldLabel);

                EditorGUI.BeginDisabledGroup(!Enabled);

                ErrorEnabled = EditorGUILayout.Toggle("Error Sound", ErrorEnabled);
                WarningEnabled = EditorGUILayout.Toggle("Warning Sound", WarningEnabled);
                SuccessEnabled = EditorGUILayout.Toggle("Success Sound", SuccessEnabled);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Volume", EditorStyles.boldLabel);
                Volume = EditorGUILayout.Slider(Volume, 0f, 1f);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Override Clips (Optional)", EditorStyles.boldLabel);

                ErrorOverride = (AudioClip)EditorGUILayout.ObjectField("Error Clip", ErrorOverride, typeof(AudioClip), false);
                WarningOverride = (AudioClip)EditorGUILayout.ObjectField("Warning Clip", WarningOverride, typeof(AudioClip), false);
                SuccessOverride = (AudioClip)EditorGUILayout.ObjectField("Success Clip", SuccessOverride, typeof(AudioClip), false);

                EditorGUI.EndDisabledGroup();
            }
        };
    }
}