using UnityEditor;
using UnityEngine;

public static class MakeArmMask
{
    [MenuItem("Tools/Create Arm Mask From Selection")]
    static void Create()
    {
        var root = Selection.activeTransform;   // select Player in the Hierarchy first
        if (root == null) { Debug.LogError("Select Player in the Hierarchy first."); return; }

        var all = root.GetComponentsInChildren<Transform>();
        var mask = new AvatarMask { transformCount = all.Length };

        for (int i = 0; i < all.Length; i++)
        {
            string n = all[i].name;
            bool isArm = n.Contains("Shoulder") || n.Contains("Arm") || n.Contains("Hand");

            mask.SetTransformPath(i, AnimationUtility.CalculateTransformPath(all[i], root));
            mask.SetTransformActive(i, isArm);
        }

        AssetDatabase.CreateAsset(mask, "Assets/ArmsOnly.asset");
        Debug.Log("Created Assets/ArmsOnly.asset");
    }
}