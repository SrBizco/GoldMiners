using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
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

    public void TriggerAttack()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger(AttackHash);
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