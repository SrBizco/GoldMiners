using UnityEngine;

public class MinerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsFleeingHash = Animator.StringToHash("IsFleeing");
    private static readonly int IsChoppingHash = Animator.StringToHash("IsChopping");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void SetSpeed(float speed)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(SpeedHash, speed);
    }

    public void SetFleeing(bool value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsFleeingHash, value);
    }

    public void SetChopping(bool value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsChoppingHash, value);
    }

    public void SetDead(bool value)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsDeadHash, value);
    }
}