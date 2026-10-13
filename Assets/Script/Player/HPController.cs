using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HPController : MonoBehaviour
{
    [SerializeField] private List<Image> hpSlot;
    [SerializeField] private int hp;
    [SerializeField] private int hpMax;
    [SerializeField] private Sprite hpFull;
    [SerializeField] private Sprite hpNotFull;

    [Header("UI Windows")]
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Components")]
    [SerializeField] private AnimationPlayer animationPlayer;

    [Header("Juice / DOTween Settings")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private float punchScaleAmount = 0.4f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDistance = 0.8f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (animationPlayer == null) animationPlayer = GetComponent<AnimationPlayer>();

        if (gameOverUI != null) gameOverUI.gameObject.SetActive(false);

        hpMax = hpSlot.Count;
        hp = hpMax;
        UpdateUIInitial();
    }

    private void UpdateUIInitial()
    {
        for (int i = 0; i < hpSlot.Count; i++)
        {
            if (i < hp) hpSlot[i].sprite = hpFull;
            else hpSlot[i].sprite = hpNotFull;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (animationPlayer != null && animationPlayer.IsJumping) return;

        if (collision.CompareTag("DMG"))
        {
            DMG(1, collision.bounds.center);
        }
    }

    public void DMG(int dmg)
    {
        DMG(dmg, transform.position);
    }

    public void DMG(int dmg, Vector3 damageSourcePos)
    {
        if (hp <= 0) return;

        int oldHp = hp;
        hp = Mathf.Max(hp - dmg, 0);

        PlaySound(damageSound);
        ApplyKnockback(damageSourcePos);

        UpdateUIInitial();

        for (int i = hp; i < oldHp; i++)
        {
            if (i >= 0 && i < hpSlot.Count)
            {
                AnimateDamage(hpSlot[i]);
            }
        }

        if (hp <= 0 && gameOverUI != null)
        {
            gameOverUI.Show();
        }
    }

    public bool Heal(int heal)
    {
        if (hp >= hpMax) return false;

        int oldHp = hp;
        hp = Mathf.Min(hp + heal, hpMax);

        PlaySound(healSound);

        UpdateUIInitial();

        for (int i = oldHp; i < hp; i++)
        {
            if (i >= 0 && i < hpSlot.Count)
            {
                AnimateHeal(hpSlot[i]);
            }
        }

        return true;
    }

    private void ApplyKnockback(Vector3 damageSourcePos)
    {
        Vector3 knockbackDir = (transform.position - damageSourcePos).normalized;

        if (knockbackDir == Vector3.zero) knockbackDir = Vector3.down;

        Vector3 targetPosition = transform.position + knockbackDir * knockbackDistance;

        transform.DOKill();
        transform.DOMove(targetPosition, knockbackDuration).SetEase(Ease.OutQuad);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void AnimateDamage(Image heartImage)
    {
        heartImage.transform.DOKill(true);

        Sequence damageSeq = DOTween.Sequence().SetUpdate(true);

        damageSeq.Append(heartImage.transform.DOShakePosition(0.25f, 8f, 20, 90f));
        damageSeq.Join(heartImage.transform.DOPunchScale(Vector3.one * -0.3f, 0.25f));
        damageSeq.OnComplete(() =>
        {
            heartImage.transform.localScale = Vector3.one;
        });
    }

    private void AnimateHeal(Image heartImage)
    {
        heartImage.transform.DOKill(true);

        heartImage.transform.DOPunchScale(Vector3.one * punchScaleAmount, animDuration, 6, 0.5f)
            .SetUpdate(true);
    }
}