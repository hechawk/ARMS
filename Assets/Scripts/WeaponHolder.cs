using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponHolder : MonoBehaviour
{
    [SerializeField] Transform weaponMount;
    [SerializeField] GameObject startingWeapon;   // for testing, drag your weapon prefab here

    [SerializeField] Transform rightHandTarget;
    [SerializeField] Transform leftHandTarget;
    [SerializeField] TwoBoneIKConstraint rightArmIK;
    [SerializeField] TwoBoneIKConstraint leftArmIK;

    [SerializeField] Animator animator;

    WeaponGrips current;

    void Start()
    {
        if (startingWeapon != null) Equip(startingWeapon);
    }

    public void Equip(GameObject weaponPrefab)
    {
        if (current != null) Destroy(current.gameObject);

        var instance = Instantiate(weaponPrefab, weaponMount);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        foreach (var t in instance.GetComponentsInChildren<Transform>())
            t.gameObject.layer = weaponMount.gameObject.layer;
        current = instance.GetComponent<WeaponGrips>();
        animator.SetInteger("GripType", current != null ? current.gripType : 0);
    }

    void LateUpdate()
    {
        Follow(current != null ? current.rightGrip : null, rightHandTarget, rightArmIK);
        Follow(current != null ? current.leftGrip : null, leftHandTarget, leftArmIK);
    }

    void Follow(Transform grip, Transform target, TwoBoneIKConstraint ik)
    {
        if (grip == null)
        {
            ik.weight = 0f;   // no grip, so the animation drives the arm
            return;
        }

        target.SetPositionAndRotation(grip.position, grip.rotation);
        ik.weight = 1f;
    }
}