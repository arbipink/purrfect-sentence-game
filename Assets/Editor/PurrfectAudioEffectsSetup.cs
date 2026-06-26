using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class PurrfectAudioEffectsSetup
{
    private static readonly string[] TargetSceneNames =
    {
        "MainMenu",
        "Credit",
        "LevelEasy",
        "LevelMedium",
        "LevelHard",
        "Scene_Easy",
        "Scene_Medium",
        "Scene_Hard"
    };

    [MenuItem("Tools/Purrfect Sentence/Setup Audio And Effects")]
    public static void SetupAudioAndEffects()
    {
        Debug.Log("[Purrfect Audio/Effects] Setup started.");

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("[Purrfect Audio/Effects] Setup canceled because current scene changes were not saved.");
            return;
        }

        string originalScenePath = SceneManager.GetActiveScene().path;
        List<string> scenePaths = FindTargetScenePaths();

        if (scenePaths.Count == 0)
        {
            Debug.LogWarning("[Purrfect Audio/Effects] No target scenes found.");
            return;
        }

        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);
            SetupScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Purrfect Audio/Effects] Scene setup saved: " + scenePath);
        }

        AssetDatabase.SaveAssets();

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        Debug.Log("[Purrfect Audio/Effects] Setup finished. UI objects were not deleted or redesigned.");
    }

    private static List<string> FindTargetScenePaths()
    {
        HashSet<string> paths = new HashSet<string>();

        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (buildScene.enabled && IsTargetScenePath(buildScene.path))
            {
                paths.Add(buildScene.path);
            }
        }

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (IsTargetScenePath(path))
            {
                paths.Add(path);
            }
        }

        return new List<string>(paths);
    }

    private static bool IsTargetScenePath(string path)
    {
        string sceneName = Path.GetFileNameWithoutExtension(path);
        foreach (string targetSceneName in TargetSceneNames)
        {
            if (sceneName == targetSceneName)
            {
                return true;
            }
        }

        return false;
    }

    private static void SetupScene(Scene scene)
    {
        AudioManager audioManager = FindSceneComponent<AudioManager>(scene);
        if (audioManager == null)
        {
            GameObject audioObject = new GameObject("AudioManager");
            audioManager = audioObject.AddComponent<AudioManager>();
            Debug.Log("[Purrfect Audio/Effects] Created AudioManager in " + scene.name + ".");
        }

        ConfigureAudioManager(audioManager);

        SimpleVFXManager vfxManager = FindSceneComponent<SimpleVFXManager>(scene);
        if (vfxManager == null)
        {
            GameObject vfxObject = new GameObject("SimpleVFXManager");
            vfxManager = vfxObject.AddComponent<SimpleVFXManager>();
            Debug.Log("[Purrfect Audio/Effects] Created SimpleVFXManager in " + scene.name + ".");
        }

        ConfigureVFXManager(vfxManager);

        SceneAudioController sceneAudioController = FindSceneComponent<SceneAudioController>(scene);
        if (sceneAudioController == null)
        {
            GameObject controllerObject = new GameObject("SceneAudioController");
            sceneAudioController = controllerObject.AddComponent<SceneAudioController>();
            Debug.Log("[Purrfect Audio/Effects] Added SceneAudioController in " + scene.name + ".");
        }

        EditorUtility.SetDirty(sceneAudioController);
        WireButtonClickSFX(scene);
    }

    private static T FindSceneComponent<T>(Scene scene) where T : Component
    {
        T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (T component in components)
        {
            if (component != null && component.gameObject.scene == scene)
            {
                return component;
            }
        }

        return null;
    }

    private static void ConfigureAudioManager(AudioManager audioManager)
    {
        AudioSource[] sources = audioManager.GetComponents<AudioSource>();
        while (sources.Length < 2)
        {
            audioManager.gameObject.AddComponent<AudioSource>();
            sources = audioManager.GetComponents<AudioSource>();
        }

        sources[0].loop = true;
        sources[0].playOnAwake = false;
        sources[0].spatialBlend = 0f;
        sources[1].loop = false;
        sources[1].playOnAwake = false;
        sources[1].spatialBlend = 0f;

        SerializedObject serializedManager = new SerializedObject(audioManager);
        AssignObjectIfEmpty(serializedManager, "bgmSource", sources[0]);
        AssignObjectIfEmpty(serializedManager, "sfxSource", sources[1]);

        AssignObjectIfEmpty(serializedManager, "mainMenuBGM", FindAudioClip("bgm_main_menu", "main_menu", "mainmenu", "menu"));
        AssignObjectIfEmpty(serializedManager, "easyBGM", FindAudioClip("bgm_easy", "easy"));
        AssignObjectIfEmpty(serializedManager, "mediumBGM", FindAudioClip("bgm_medium", "medium"));
        AssignObjectIfEmpty(serializedManager, "hardBGM", FindAudioClip("bgm_hard", "hard"));
        AssignObjectIfEmpty(serializedManager, "buttonClickSFX", FindAudioClip("sfx_button_click", "button_click", "buttonclick", "click"));
        AssignObjectIfEmpty(serializedManager, "slashSFX", FindAudioClip("sfx_slash", "slash"));
        AssignObjectIfEmpty(serializedManager, "correctAnswerSFX", FindAudioClip("sfx_correct", "correct_answer", "correct"));
        AssignObjectIfEmpty(serializedManager, "wrongAnswerSFX", FindAudioClip("sfx_wrong", "wrong_answer", "wrong"));
        AssignObjectIfEmpty(serializedManager, "enemyDefeatedSFX", FindAudioClip("sfx_enemy_defeated", "enemy_defeated", "defeated"));
        AssignObjectIfEmpty(serializedManager, "damageSFX", FindAudioClip("sfx_damage", "damage"));
        AssignObjectIfEmpty(serializedManager, "gameOverSFX", FindAudioClip("sfx_game_over", "game_over", "gameover"));
        AssignObjectIfEmpty(serializedManager, "levelCompleteSFX", FindAudioClip("sfx_level_complete", "level_complete", "complete"));
        AssignObjectIfEmpty(serializedManager, "pauseSFX", FindAudioClip("sfx_pause", "pause"));

        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(audioManager);
        EditorUtility.SetDirty(audioManager.gameObject);
    }

    private static void ConfigureVFXManager(SimpleVFXManager vfxManager)
    {
        SerializedObject serializedManager = new SerializedObject(vfxManager);
        AssignObjectIfEmpty(serializedManager, "slashEffectPrefab", FindPrefab("electro_slash", "electroslash", "aoe_slash", "slash"));
        AssignObjectIfEmpty(serializedManager, "correctAnswerEffectPrefab", FindPrefab("sparks_flashing_yellow", "sparks_yellow", "sparkle", "sparks"));
        AssignObjectIfEmpty(serializedManager, "enemyDefeatedEffectPrefab", FindPrefab("enemy_explosion", "pow", "boing", "explosion"));
        AssignObjectIfEmpty(serializedManager, "damageEffectPrefab", FindPrefab("red_energy_explosion", "hit_misc", "sparks_red", "damage"));
        AssignObjectIfEmpty(serializedManager, "wrongAnswerEffectPrefab", FindPrefab("boing", "pow", "smoke_puff", "sparks_red"));
        AssignObjectIfEmpty(serializedManager, "levelCompleteEffectPrefab", FindPrefab("sparks_explode_yellow", "sparkle", "fountain", "rainbow"));
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(vfxManager);
        EditorUtility.SetDirty(vfxManager.gameObject);
    }

    private static void WireButtonClickSFX(Scene scene)
    {
        int addedCount = 0;
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button == null || button.gameObject.scene != scene)
            {
                continue;
            }

            if (button.GetComponent<ButtonClickSFX>() == null)
            {
                button.gameObject.AddComponent<ButtonClickSFX>();
                addedCount++;
                EditorUtility.SetDirty(button.gameObject);
            }
        }

        Debug.Log("[Purrfect Audio/Effects] Button click SFX components added in " + scene.name + ": " + addedCount);
    }

    private static void AssignObjectIfEmpty(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
        {
            Debug.LogWarning("[Purrfect Audio/Effects] Serialized property not found: " + propertyName);
            return;
        }

        if (property.objectReferenceValue == null && value != null)
        {
            property.objectReferenceValue = value;
            Debug.Log("[Purrfect Audio/Effects] Assigned " + propertyName + " -> " + AssetDatabase.GetAssetPath(value));
        }
    }

    private static AudioClip FindAudioClip(params string[] candidates)
    {
        return FindBestAsset<AudioClip>("t:AudioClip", candidates);
    }

    private static GameObject FindPrefab(params string[] candidates)
    {
        return FindBestAsset<GameObject>("t:Prefab", candidates);
    }

    private static T FindBestAsset<T>(string filter, string[] candidates) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets(filter, new[] { "Assets" });
        T bestAsset = null;
        int bestScore = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string normalizedName = Normalize(Path.GetFileNameWithoutExtension(path));
            int score = ScoreAssetName(normalizedName, candidates);

            if (score > bestScore)
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    bestAsset = asset;
                    bestScore = score;
                }
            }
        }

        return bestAsset;
    }

    private static int ScoreAssetName(string normalizedName, string[] candidates)
    {
        int score = 0;

        foreach (string candidate in candidates)
        {
            string normalizedCandidate = Normalize(candidate);
            if (string.IsNullOrEmpty(normalizedCandidate))
            {
                continue;
            }

            if (normalizedName == normalizedCandidate)
            {
                score = Mathf.Max(score, 1000 + normalizedCandidate.Length);
            }
            else if (normalizedName.Contains(normalizedCandidate))
            {
                score = Mathf.Max(score, 500 + normalizedCandidate.Length);
            }
        }

        return score;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        value = value.ToLowerInvariant();
        char[] buffer = new char[value.Length];
        int index = 0;

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[index] = character;
                index++;
            }
        }

        return new string(buffer, 0, index);
    }
}
