using UnityEngine;
using DG.Tweening;
using Zenject;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class EnemyAI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip stepSFX;
    [SerializeField] private AudioClip attackSFX;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float waitAtPointTime = 2f;

    [Header("Chase & Attack Settings")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseRadius = 5f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damageAmount = 1;

    [Header("Shadow Animation Settings")]
    [SerializeField] private float shadowMinScaleMultiplier = 0.6f;
    [SerializeField] private float walkShadowStepDuration = 0.3f;
    [SerializeField] private float attackShadowDuration = 0.4f;

    [Header("VFX Settings")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem dustParticles;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private HPController playerHP;
    private AnimationPlayer playerAnim;
    private Transform playerTransform;

    private Vector2 startPosition;
    private Vector2 patrolTarget;
    private float waitTimer;
    private float lastAttackTime;
    private bool isWaiting;

    private Vector3 initialShadowScale;
    private Tween shadowTween;
    private bool isAttackingShadow;

    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    [Inject]
    private void Construct(HPController playerHP, AnimationPlayer playerAnim)
    {
        this.playerHP = playerHP;
        this.playerAnim = playerAnim;
        if (playerHP != null) this.playerTransform = playerHP.transform;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (shadowTransform == null)
        {
            Transform foundShadow = transform.Find("Shadow");
            if (foundShadow != null) shadowTransform = foundShadow;
        }

        if (shadowTransform != null)
        {
            initialShadowScale = shadowTransform.localScale;
        }
    }

    private void Start()
    {
        startPosition = transform.position;
        GetNewPatrolTarget();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRadius)
        {
            currentState = State.Attack;
        }
        else if (distanceToPlayer <= chaseRadius)
        {
            currentState = State.Chase;
        }
        else
        {
            currentState = State.Patrol;
        }
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolLogic();
                break;
            case State.Chase:
                ChaseLogic();
                break;
            case State.Attack:
                AttackLogic();
                break;
        }
    }

    private void PatrolLogic()
    {
        if (isWaiting)
        {
            StopMoving();

            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= waitAtPointTime)
            {
                isWaiting = false;
                GetNewPatrolTarget();
            }
            return;
        }

        Vector2 direction = (patrolTarget - rb.position).normalized;
        rb.MovePosition(rb.position + direction * patrolSpeed * Time.fixedDeltaTime);

        StartMoving(direction);

        if (Vector2.Distance(rb.position, patrolTarget) < 0.2f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    private void ChaseLogic()
    {
        if (playerTransform == null) return;

        isWaiting = false;

        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * chaseSpeed * Time.fixedDeltaTime);

        StartMoving(direction);
    }

    private void AttackLogic()
    {
        StopMoving();

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            if (anim != null) anim.SetTrigger("Attack");

            PlaySFX(attackSFX, 1f);

            TriggerAttackShadow();
            DealDamageToPlayer();
        }
    }

    private void DealDamageToPlayer()
    {
        if (playerHP == null) return;

        if (playerAnim != null && playerAnim.IsJumping) return;

        playerHP.DMG(damageAmount, transform.position);
    }

    private void StartMoving(Vector2 direction)
    {
        if (anim != null) anim.SetBool("isWalk", true);
        FlipSprite(direction.x);
        StartWalkShadowAnimation();
        UpdateDustVFX(direction, true);
    }

    private void StopMoving()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool("isWalk", false);
        StopWalkShadowAnimation();
        UpdateDustVFX(Vector2.zero, false);
    }

    private void UpdateDustVFX(Vector2 direction, bool isMoving)
    {
        if (dustParticles == null) return;

        var emission = dustParticles.emission;
        emission.enabled = isMoving;

        if (isMoving)
        {
            float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
            dustParticles.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void StartWalkShadowAnimation()
    {
        if (shadowTransform == null || isAttackingShadow) return;

        if (shadowTween == null || !shadowTween.IsActive())
        {
            Sequence walkSeq = DOTween.Sequence();

            walkSeq.AppendCallback(() =>
            {
                if (jumpParticles != null) jumpParticles.Play();
                PlaySFX(stepSFX, 0.3f);
            });

            walkSeq.Append(shadowTransform.DOScale(initialShadowScale * shadowMinScaleMultiplier, walkShadowStepDuration / 2f).SetEase(Ease.OutQuad));
            walkSeq.Append(shadowTransform.DOScale(initialShadowScale, walkShadowStepDuration / 2f).SetEase(Ease.InQuad));

            walkSeq.SetLoops(-1, LoopType.Restart);
            shadowTween = walkSeq;
        }
    }

    private void StopWalkShadowAnimation()
    {
        if (isAttackingShadow) return;

        if (shadowTween != null)
        {
            shadowTween.Kill();
            shadowTween = null;
        }

        if (shadowTransform != null)
        {
            shadowTransform.DOScale(initialShadowScale, 0.15f);
        }
    }

    private void TriggerAttackShadow()
    {
        if (shadowTransform == null) return;

        isAttackingShadow = true;

        if (shadowTween != null)
        {
            shadowTween.Kill();
            shadowTween = null;
        }

        Sequence attackSeq = DOTween.Sequence();
        attackSeq.Append(shadowTransform.DOScale(initialShadowScale * shadowMinScaleMultiplier, attackShadowDuration / 2f).SetEase(Ease.OutQuad));
        attackSeq.Append(shadowTransform.DOScale(initialShadowScale, attackShadowDuration / 2f).SetEase(Ease.InQuad));
        attackSeq.OnComplete(() =>
        {
            isAttackingShadow = false;
        });
    }

    private void GetNewPatrolTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
        patrolTarget = startPosition + randomPoint;
    }

    private void FlipSprite(float moveX)
    {
        if (spriteRenderer == null) return;

        if (moveX > 0.05f)
            spriteRenderer.flipX = false;
        else if (moveX < -0.05f)
            spriteRenderer.flipX = true;
    }

    private void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || audioSource == null) return;

        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance > chaseRadius * 1.5f) return;
        }

        audioSource.pitch = Random.Range(0.88f, 1.12f);
        audioSource.PlayOneShot(clip, volumeMultiplier);
    }

    private void OnDestroy()
    {
        shadowTween?.Kill();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, patrolRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}