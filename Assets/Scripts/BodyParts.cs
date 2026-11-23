using System.Collections.Generic;
using UnityEngine;

public class BodyParts : MonoBehaviour
{
    [SerializeField] Transform skeletonRoot;   // the root bone of THIS object's armature

    [SerializeField] GameObject head;
    [SerializeField] GameObject torso;
    [SerializeField] GameObject leftArm;
    [SerializeField] GameObject rightArm;
    [SerializeField] GameObject leftLeg;
    [SerializeField] GameObject rightLeg;

    Dictionary<string, Transform> bones;

    void Start()
    {
        // Build the bone lookup once, from the base rig only, before any parts spawn
        bones = new Dictionary<string, Transform>();
        foreach (var t in skeletonRoot.GetComponentsInChildren<Transform>())
            bones[t.name] = t;

        Apply(head);
        Apply(torso);
        Apply(leftArm);
        Apply(rightArm);
        Apply(leftLeg);
        Apply(rightLeg);
    }

    void Apply(GameObject partPrefab)
    {
        if (partPrefab == null) return;

        // Spawn the part and point its mesh at the base skeleton's bones
        var instance = Instantiate(partPrefab, transform);
        foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var newBones = new Transform[smr.bones.Length];
            for (int i = 0; i < newBones.Length; i++)
                newBones[i] = bones[smr.bones[i].name];

            smr.bones = newBones;
            smr.rootBone = bones[smr.rootBone.name];
            smr.updateWhenOffscreen = true;   // stops the mesh vanishing at screen edges
        }

        // Delete the part's own armature copy; the mesh now uses the base one
        foreach (var t in instance.GetComponentsInChildren<Transform>())
            if (t.name == skeletonRoot.name && t != skeletonRoot)
            {
                Destroy(t.gameObject);
                break;
            }
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        foreach (var t in go.GetComponentsInChildren<Transform>())
            t.gameObject.layer = layer;
    }
}