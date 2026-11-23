using UnityEditor;
using UnityEngine;

public static class MakeFingerMask
{
    [MenuItem("Tools/Create Finger Mask From Selection")]
    static void Create()
    {
        var root = Selection.activeTransform;   // select Player in the Hierarchy first
        if (root == null) { Debug.LogError("Select Player in the Hierarchy first."); return; }

        var all = root.GetComponentsInChildren<Transform>();
        var mask = new AvatarMask { transformCount = all.Length };

        for (int i = 0; i < all.Length; i++)
        {
            string n = all[i].name;
            bool isFinger = n.Contains("HandThumb") || n.Contains("HandIndex") ||
                            n.Contains("HandMiddle") || n.Contains("HandRing") ||
                            n.Contains("HandPinky");

            mask.SetTransformPath(i, AnimationUtility.CalculateTransformPath(all[i], root));
            mask.SetTransformActive(i, isFinger);
        }

        AssetDatabase.CreateAsset(mask, "Assets/FingersOnly.asset");
        Debug.Log("Created Assets/FingersOnly.asset");
    }
}