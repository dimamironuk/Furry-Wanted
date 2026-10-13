using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;

public class GhostBossArenaController : MonoBehaviour
{
    [Header("Arena Setup")]
    [SerializeField] private GameObject[] arenaWalls;
    [SerializeField] private Transform ghostBoss;
    [SerializeField] private float floatAnimHeight = 0.5f;
    [SerializeField] private Collider2D startArenaTrigger;
    [SerializeField] private Collider2D bossHurtCollider;

    [Header("Boss Stats")]
    [SerializeField] private Transform[] movePoints;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Dialogue Setup")]
    [SerializeField] private Dialogue introDialogue;
    [SerializeField] private Dialogue outroDialogue;

    [Header("Attack & Pool Setup")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2.5f;
    [SerializeField] private int poolSize = 100;

    [Header("Reward Setup")]
    [SerializeField] private GameObject rewardChest;

    [Inject] private IDialogService dialogService;

    private bool hasTriggered = false;
    private bool isFightActive = false;
    private List<GameObject> bulletPool = new List<GameObject>();
    private Transform poolContainer;
    private Tween floatTween;

    private void Awake()
    {
        InitializeBulletPool();
    }

    private void Start()
    {
        if (startArenaTrigger != null) startArenaTrigger.enabled = true;
        if (bossHurtCollider != null) bossHurtCollider.enabled = false;

        if (rewardChest != null)
        {
            rewardChest.SetActive(false);
        }

        foreach (var wall in arenaWalls)
        {
            wall.SetActive(false);
        }

        if (ghostBoss != null)
        {
            floatTween = ghostBoss.DOMoveY(ghostBoss.position.y + floatAnimHeight, 1.5f)
                                 .SetLoops(-1, LoopType.Yoyo)
                                 .SetEase(Ease.InOutSine);
        }
    }

    private void InitializeBulletPool()
    {
        if (poolContainer == null)
        {
            GameObject containerObj = new GameObject("GhostBulletPool");
            poolContainer = containerObj.transform;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, poolContainer);
            bullet.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    private GameObject GetPooledBullet()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (bulletPool[i] != null && !bulletPool[i].activeInHierarchy)
            {
                return bulletPool[i];
            }
        }

        Transform parent = poolContainer != null ? poolContainer : transform;
        GameObject newBullet = Instantiate(bulletPrefab, parent);
        newBullet.SetActive(false);
        bulletPool.Add(newBullet);
        return newBullet;
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = GetPooledBullet();
        if (bullet != null)
        {
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(BossFightSequence());
        }
    }

    private IEnumerator BossFightSequence()
    {
        if (startArenaTrigger != null) startArenaTrigger.enabled = false;

        foreach (var wall in arenaWalls)
        {
            wall.SetActive(true);
        }

        if (introDialogue != null)
        {
            dialogService.StartDialogue(introDialogue);
            yield return new WaitWhile(() => dialogService.IsDialogueActive);
        }

        isFightActive = true;
        if (bossHurtCollider != null) bossHurtCollider.enabled = true;

        StartCoroutine(BossMovementRoutine());
        StartCoroutine(BossAttackRoutine());

        yield return new WaitUntil(() => ghostBoss == null);

        isFightActive = false;
        if (bossHurtCollider != null) bossHurtCollider.enabled = false;

        DeactivateAllBullets();

        if (floatTween != null) floatTween.Kill();

        if (outroDialogue != null)
        {
            dialogService.StartDialogue(outroDialogue);
            yield return new WaitWhile(() => dialogService.IsDialogueActive);
        }

        if (rewardChest != null)
        {
            rewardChest.SetActive(true);
            rewardChest.transform.localScale = Vector3.zero;
            rewardChest.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        foreach (var wall in arenaWalls)
        {
            wall.SetActive(false);
        }

        Destroy(gameObject);
    }

    private IEnumerator BossMovementRoutine()
    {
        int currentPoint = 0;
        while (isFightActive && movePoints.Length > 0 && ghostBoss != null)
        {
            Transform target = movePoints[currentPoint];
            while (ghostBoss != null && Vector3.Distance(ghostBoss.position, target.position) > 0.1f)
            {
                ghostBoss.position = Vector3.MoveTowards(ghostBoss.position, target.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
            currentPoint = (currentPoint + 1) % movePoints.Length;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator BossAttackRoutine()
    {
        while (isFightActive)
        {
            yield return new WaitForSeconds(attackInterval);
            if (Random.value > 0.5f)
            {
                yield return StartCoroutine(WhirlpoolAttack());
            }
            else
            {
                RingAttack();
            }
        }
    }

    private IEnumerator WhirlpoolAttack()
    {
        int bulletCount = 16;
        float angleStep = 22.5f;
        float currentAngle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            if (!isFightActive || firePoint == null) yield break;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            SpawnBullet(firePoint.position, rotation);
            currentAngle += angleStep;
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void RingAttack()
    {
        if (firePoint == null) return;
        int bulletCount = 10;
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            SpawnBullet(firePoint.position, rotation);
        }
    }

    private void DeactivateAllBullets()
    {
        foreach (var bullet in bulletPool)
        {
            if (bullet != null) bullet.SetActive(false);
        }
    }
}