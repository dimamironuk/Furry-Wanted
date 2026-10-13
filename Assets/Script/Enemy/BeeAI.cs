using System.Collections;
using UnityEngine;
using DG.Tweening;
using Zenject;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class BeeAI : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float windupTime = 0.6f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float cooldownTime = 1.5f;
    [SerializeField] private int damageAmount = 1;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip windupSFX;  
    [SerializeField] private AudioClip flyDashSFX; 

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private HPController playerHP;
    private Rigidbody2D rb;
    private bool isDashing = false;
    private bool isDead = false;

    [Inject]
    private void Construct(HPController playerHP)
    {
        this.playerHP = playerHP;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (!isDead)
        {
            if (playerHP == null) yield break;
            Vector2 targetPos = playerHP.transform.position;
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            spriteRenderer.flipX = direction.x > 0;

            if (windupSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(windupSFX);
            }

            spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
            transform.DOShakePosition(windupTime, 0.2f, 20, 90, false, true);

            yield return new WaitForSeconds(windupTime);

            isDashing = true;
            spriteRenderer.color = Color.white;

            if (flyDashSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(flyDashSFX);
            }

            rb.velocity = direction * dashSpeed;

            yield return new WaitForSeconds(dashDuration);

            rb.velocity = Vector2.zero;
            isDashing = false;

            yield return new WaitForSeconds(cooldownTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDashing && other.CompareTag("Player"))
        {
            if (playerHP != null)
            {
                playerHP.DMG(damageAmount, transform.position);

                rb.velocity = Vector2.zero;
                isDashing = false;

                Vector2 bounceDir = (transform.position - other.transform.position).normalized;
                transform.DOPunchPosition(bounceDir * 1f, 0.3f);
            }
        }
    }

    private void OnDestroy()
    {
        isDead = true;
        transform.DOKill();
    }
}