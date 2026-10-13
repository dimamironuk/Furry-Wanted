using UnityEngine;
using DG.Tweening;

public class AnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform spriteZaza;
    [SerializeField] private Transform zazaHead;

    [Header("Shadow Settings")]
    [SerializeField] private Transform shadowTransform;
    [SerializeField] private float shadowMinScaleMultiplier = 0.5f;

    [Header("Body Sprites")]
    [SerializeField] private SpriteRenderer bodySpriteRenderer;

    [Header("Jump Body Sprites")]
    [SerializeField] private Sprite jumpDown;
    [SerializeField] private Sprite jumpUp;
    [SerializeField] private Sprite jumpLeft;
    [SerializeField] private Sprite jumpRight;

    [Header("Head Sprites")]
    [SerializeField] private SpriteRenderer headSpriteRenderer;
    [SerializeField] private Sprite headDown;
    [SerializeField] private Sprite headRight;
    [SerializeField] private Sprite headLeft;
    [SerializeField] private Sprite headUp;
    [SerializeField] private Sprite headBite;

    [Header("Jump Settings")]
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private string lowObstacleLayerName = "LowObstacle";

    [Header("VFX Settings")]
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private ParticleSystem jumpParticles;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip attackSound;

    [Header("DOTween Head Motion Settings")]
    [SerializeField] private float headMotionAmount = 0.08f;
    [SerializeField] private float headMotionDuration = 0.2f;

    [Header("Animation Speed Settings")]
    [SerializeField] private float walkAnimSpeed = 1f;
    [SerializeField] private float runAnimSpeed = 1.6f;

    private Tween headTween;
    private Vector3 initialHeadPos;
    private Vector3 initialSpritePos;
    private Vector3 initialShadowScale;
    private bool isJumping;
    private bool isAttacking;
    private Vector2 lastDirection = Vector2.down;
    private int lowObstacleLayer;

    public bool IsJumping => isJumping;
    public Vector2 LastDirection => lastDirection;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int SpeedXHash = Animator.StringToHash("SpeedX");
    private static readonly int SpeedYHash = Animator.StringToHash("SpeedY");

    private void Awake()
    {
        lowObstacleLayer = LayerMask.NameToLayer(lowObstacleLayerName);

        if (spriteZaza != null)
        {
            initialSpritePos = spriteZaza.localPosition;

            if (bodySpriteRenderer == null)
            {
                bodySpriteRenderer = spriteZaza.GetComponent<SpriteRenderer>();
            }
        }

        if (shadowTransform != null)
        {
            initialShadowScale = shadowTransform.localScale;
        }

        if (zazaHead != null)
        {
            initialHeadPos = zazaHead.localPosition;
            StartInfiniteHeadMotion();

            if (headSpriteRenderer == null)
            {
                headSpriteRenderer = zazaHead.GetComponent<SpriteRenderer>();
            }
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void PlayBiteAnimation(Vector2 attackDirection, System.Action onComplete)
    {
        if (zazaHead == null)
        {
            onComplete?.Invoke();
            return;
        }

        isAttacking = true;
        UpdateHeadDirection(attackDirection);

        // Звук атаки/укусу
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Sprite previousSprite = headSpriteRenderer != null ? headSpriteRenderer.sprite : null;
        if (headSpriteRenderer != null && headBite != null)
        {
            headSpriteRenderer.sprite = headBite;
        }

        zazaHead.DOPunchPosition(attackDirection * 0.4f, 0.18f, 1, 0.5f);
        zazaHead.DOPunchScale(new Vector3(0.2f, -0.15f, 0), 0.18f)
            .OnComplete(() =>
            {
                if (headSpriteRenderer != null && previousSprite != null)
                {
                    headSpriteRenderer.sprite = previousSprite;
                }
                isAttacking = false;
                onComplete?.Invoke();
            });
    }

    public void PerformJump()
    {
        if (isJumping) return;

        isJumping = true;

        if (lowObstacleLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, lowObstacleLayer, true);
        }

        SetJumpBodySprite(lastDirection);

        if (dustParticles != null)
        {
            var emission = dustParticles.emission;
            emission.enabled = false;
        }

        if (jumpParticles != null) jumpParticles.Play();
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);

        Sequence jumpSequence = DOTween.Sequence();

        jumpSequence.Append(spriteZaza.DOLocalMoveY(initialSpritePos.y + jumpHeight, jumpDuration / 2f)
            .SetEase(Ease.OutQuad));

        if (shadowTransform != null)
        {
            jumpSequence.Join(shadowTransform.DOScale(initialShadowScale * shadowMinScaleMultiplier, jumpDuration / 2f)
                .SetEase(Ease.OutQuad));
        }

        jumpSequence.Append(spriteZaza.DOLocalMoveY(initialSpritePos.y, jumpDuration / 2f)
            .SetEase(Ease.InQuad));

        if (shadowTransform != null)
        {
            jumpSequence.Join(shadowTransform.DOScale(initialShadowScale, jumpDuration / 2f)
                .SetEase(Ease.InQuad));
        }

        jumpSequence.OnComplete(() =>
        {
            if (lowObstacleLayer != -1)
            {
                Physics2D.IgnoreLayerCollision(gameObject.layer, lowObstacleLayer, false);
            }

            if (jumpParticles != null) jumpParticles.Play();

            isJumping = false;
        });
    }

    public void SetMovement(Vector2 movement, bool isRunning)
    {
        bool movingNow = movement.sqrMagnitude > 0.01f;

        if (movingNow)
        {
            lastDirection = movement;
        }

        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, movingNow);
            animator.SetFloat(SpeedXHash, movement.x);
            animator.SetFloat(SpeedYHash, movement.y);

            animator.speed = movingNow ? (isRunning ? runAnimSpeed : walkAnimSpeed) : 1f;
        }

        if (headTween != null)
        {
            headTween.timeScale = (movingNow && isRunning) ? 1.6f : 1f;
        }

        if (!isJumping && !isAttacking)
        {
            if (movingNow) UpdateHeadDirection(movement);
            UpdateDustVFX(movement, movingNow);
        }
    }

    private void SetJumpBodySprite(Vector2 direction)
    {
        if (bodySpriteRenderer == null) return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0 && jumpRight != null) bodySpriteRenderer.sprite = jumpRight;
            else if (direction.x < 0 && jumpLeft != null) bodySpriteRenderer.sprite = jumpLeft;
        }
        else
        {
            if (direction.y > 0 && jumpUp != null) bodySpriteRenderer.sprite = jumpUp;
            else if (jumpDown != null) bodySpriteRenderer.sprite = jumpDown;
        }
    }

    private void UpdateDustVFX(Vector2 movement, bool movingNow)
    {
        if (dustParticles == null) return;

        var emission = dustParticles.emission;
        emission.enabled = movingNow;

        if (movingNow)
        {
            float angle = Mathf.Atan2(-movement.y, -movement.x) * Mathf.Rad2Deg;
            dustParticles.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void UpdateHeadDirection(Vector2 movement)
    {
        if (headSpriteRenderer == null) return;

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            if (movement.x > 0 && headRight != null) headSpriteRenderer.sprite = headRight;
            else if (movement.x < 0 && headLeft != null) headSpriteRenderer.sprite = headLeft;
        }
        else
        {
            if (movement.y < 0 && headDown != null) headSpriteRenderer.sprite = headDown;
            else if (movement.y > 0) headSpriteRenderer.sprite = headUp != null ? headUp : headDown;
        }
    }

    private void StartInfiniteHeadMotion()
    {
        if (zazaHead == null) return;

        headTween?.Kill();
        headTween = zazaHead.DOLocalMoveY(initialHeadPos.y + headMotionAmount, headMotionDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        headTween?.Kill();
    }
}