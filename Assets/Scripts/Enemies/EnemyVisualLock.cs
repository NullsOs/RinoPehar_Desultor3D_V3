using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyVisualLock : MonoBehaviour
{
    [Header("Main Fix")]
    [SerializeField] private bool disableRootMotion = true;

    [Header("Lock Animator Object")]
    [SerializeField] private bool lockAnimatorTransform = true;

    [Header("Optional: Lock Root Bone / Hips")]
    [SerializeField] private Transform rootBone;
    [SerializeField] private bool lockRootBonePosition = true;

    private Animator animator;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private Vector3 rootBoneStartLocalPosition;
    private Quaternion rootBoneStartLocalRotation;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (disableRootMotion)
            animator.applyRootMotion = false;

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

        if (rootBone != null)
        {
            rootBoneStartLocalPosition = rootBone.localPosition;
            rootBoneStartLocalRotation = rootBone.localRotation;
        }
    }

    private void OnAnimatorMove()
    {
        if (animator != null)
            animator.applyRootMotion = false;

        LockVisual();
    }

    private void LateUpdate()
    {
        LockVisual();
    }

    private void LockVisual()
    {
        if (lockAnimatorTransform)
        {
            transform.localPosition = startLocalPosition;
            transform.localRotation = startLocalRotation;
        }

        if (rootBone != null && lockRootBonePosition)
        {
            rootBone.localPosition = rootBoneStartLocalPosition;
            rootBone.localRotation = rootBoneStartLocalRotation;
        }
    }
}