using UnityEditor;
using UnityEngine;

public static class DragonPreferences
{
    private const string EnabledKey = "DragonDevTools_Enabled";
    private const string ErrorKey = "DragonDevTools_ErrorGUID";
    private const string WarningKey = "DragonDevTools_WarningGUID";
    private const string SuccessKey = "DragonDevTools_SuccessGUID";

    public static bool Enabled
    {
        get => EditorPrefs.GetBool(EnabledKey, true);
        set => EditorPrefs.SetBool(EnabledKey, value);
    }

    public static AudioClip ErrorOverride
    {
        get => LoadClip(ErrorKey);
        set => SaveClip(ErrorKey, value);
    }

    public static AudioClip WarningOverride
    {
        get => LoadClip(WarningKey);
        set => SaveClip(WarningKey, value);
    }

    public static AudioClip SuccessOverride
    {
        get => LoadClip(SuccessKey);
        set => SaveClip(SuccessKey, value);
    }

    private static void SaveClip(string key, AudioClip clip)
    {
        if (clip == null)
        {
            EditorPrefs.DeleteKey(key);
            return;
        }

        string path = AssetDatabase.GetAssetPath(clip);
        string guid = AssetDatabase.AssetPathToGUID(path);
        EditorPrefs.SetString(key, guid);
    }

    private static AudioClip LoadClip(string key)
    {
        string guid = EditorPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(guid))
            return null;

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
                Enabled = EditorGUILayout.Toggle("Enable Compile Sounds", Enabled);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Override Clips (Optional)", EditorStyles.boldLabel);

                ErrorOverride = (AudioClip)EditorGUILayout.ObjectField("Error", ErrorOverride, typeof(AudioClip), false);
                WarningOverride = (AudioClip)EditorGUILayout.ObjectField("Warning", WarningOverride, typeof(AudioClip), false);
                SuccessOverride = (AudioClip)EditorGUILayout.ObjectField("Success", SuccessOverride, typeof(AudioClip), false);
            }
        };
    }
}