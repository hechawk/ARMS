using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ExtractAnimationClips
{
    const string outputFolder = "Assets/Animations/Extracted";

    [MenuItem("Tools/Extract Animation Clips From Selected FBX")]
    static void Run()
    {
        var fbxPaths = Selection.assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        if (fbxPaths.Count == 0)
        {
            Debug.LogWarning("Select one or more FBX files in the Project window first.");
            return;
        }

        EnsureFolder(outputFolder);

        int created = 0;
        foreach (var fbxPath in fbxPaths)
        {
            string fbxName = Path.GetFileNameWithoutExtension(fbxPath);

            var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__"))
                .ToList();

            if (clips.Count == 0)
            {
                Debug.LogWarning($"No animation clips found in {fbxPath}");
                continue;
            }

            foreach (var source in clips)
            {
                // One clip in the file: name it after the FBX. Several: FBX name + clip name.
                string clipName = clips.Count == 1 ? fbxName : $"{fbxName}_{source.name}";
                clipName = Sanitize(clipName);
                string assetPath = $"{outputFolder}/{clipName}.anim";

                var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                if (existing != null)
                {
                    // Overwrite in place so Animator Controllers keep their reference
                    EditorUtility.CopySerialized(source, existing);
                    existing.name = clipName;
                    EditorUtility.SetDirty(existing);
                }
                else
                {
                    var copy = new AnimationClip();
                    EditorUtility.CopySerialized(source, copy);
                    copy.name = clipName;
                    AssetDatabase.CreateAsset(copy, assetPath);
                }
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Extracted {created} clip(s) from {fbxPaths.Count} FBX file(s) into {outputFolder}");
    }

    static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}