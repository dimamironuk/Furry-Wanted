using UnityEngine;
using DG.Tweening;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem deathParticles;

    private float currentHealth;
    private bool isDead;
    private Color originalColor;
    private Tween colorTween;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        AnimateHit();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void AnimateHit()
    {
        transform.DOPunchScale(new Vector3(0.2f, -0.2f, 0), 0.15f);

        if (spriteRenderer != null)
        {
            colorTween?.Kill();
            spriteRenderer.color = Color.red;
            colorTween = spriteRenderer.DOColor(originalColor, 0.2f);
        }
    }

    private void Die()
    {
        isDead = true;

        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        transform.DOKill();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        colorTween?.Kill();
    }
}