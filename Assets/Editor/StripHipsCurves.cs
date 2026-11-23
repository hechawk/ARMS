using System.Linq;
using UnityEditor;
using UnityEngine;

public static class StripHipsCurves
{
    // The two axes to REMOVE. Change these if your vertical axis isn't Y.
    const string axisA = "m_LocalPosition.x";
    const string axisB = "m_LocalPosition.z";

    [MenuItem("Tools/Strip Hips XZ Curves")]
    static void Run()
    {
        // Selected .anim files, plus any .anim files inside selected folders
        var paths = Selection.assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
            .SelectMany(p => AssetDatabase.IsValidFolder(p)
                ? AssetDatabase.FindAssets("t:AnimationClip", new[] { p }).Select(AssetDatabase.GUIDToAssetPath)
                : new[] { p })
            .Where(p => p.EndsWith(".anim"))
            .Distinct()
            .ToList();

        if (paths.Count == 0)
        {
            Debug.LogWarning("Select one or more .anim files (or a folder of them) in the Project window. " +
                             "Clips inside an FBX are read-only, so extract them first.");
            return;
        }

        int changed = 0;
        foreach (var path in paths)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            bool touched = false;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (!binding.path.EndsWith("mixamorig:Hips")) continue;
                if (binding.propertyName != axisA && binding.propertyName != axisB) continue;

                AnimationUtility.SetEditorCurve(clip, binding, null);   // null removes the curve
                touched = true;
            }

            if (touched)
            {
                EditorUtility.SetDirty(clip);
                changed++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Stripped Hips XZ curves from {changed} of {paths.Count} clip(s).");
    }
}