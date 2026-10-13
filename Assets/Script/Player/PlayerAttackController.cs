using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float autoTargetRadius = 4f;
    [SerializeField] private LayerMask enemyLayer;

    [Inject] private AnimationPlayer animationPlayer;
    [Inject] private IDialogService dialogService;

    private bool isAttacking;

    private void Update()
    {
        if (dialogService != null && dialogService.IsDialogueActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            PerformBite();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            PerformBite();
        }
    }

    private void PerformBite()
    {
        if (isAttacking || animationPlayer == null) return;

        isAttacking = true;

        Vector2 attackDirection = animationPlayer.LastDirection;
        Transform nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null)
        {
            attackDirection = ((Vector2)nearestEnemy.position - (Vector2)transform.position).normalized;
        }

        animationPlayer.PlayBiteAnimation(attackDirection, () => isAttacking = false);

        Vector2 attackPoint = (Vector2)transform.position + attackDirection * attackRange;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange * 0.5f, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyHealth>(out var health))
            {
                health.TakeDamage(attackDamage);
            }
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, autoTargetRadius, enemyLayer);
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }
}