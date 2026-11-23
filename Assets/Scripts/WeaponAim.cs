using UnityEngine;

[DefaultExecutionOrder(-10)]   // runs before WeaponHolder, so the hand targets follow the new rotation
public class WeaponAim : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] LayerMask aimMask = ~0;        // what the ray can hit; exclude the Player's layer
    [SerializeField] float maxDistance = 200f;
    [SerializeField] float turnSpeed = 20f;         // higher = snappier, lower = more weapon sway
    [SerializeField] Movement movement;             // optional: only aim while aiming
    [SerializeField] Transform aimTarget;   // the AimTarget object under Rig

    public Vector3 AimPoint { get; private set; }

    void LateUpdate()
    {
        // Ray through the middle of the screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        AimPoint = Physics.Raycast(ray, out RaycastHit hit, maxDistance, aimMask, QueryTriggerInteraction.Ignore)
            ? hit.point
            : ray.origin + ray.direction * maxDistance;   // nothing hit, so aim at a far point along the ray

        // Rotate from the weapon's position towards that point
        Vector3 toTarget = AimPoint - transform.position;
        if (toTarget.sqrMagnitude < 0.01f) return;   // point is on top of the weapon, avoid a flip

        Quaternion target = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);

        if (aimTarget != null) aimTarget.position = AimPoint;
    }
}