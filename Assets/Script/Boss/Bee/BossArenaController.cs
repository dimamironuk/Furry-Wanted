using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;

public class BossArenaController : MonoBehaviour
{
    [Header("Arena Setup")]
    [SerializeField] private GameObject[] arenaWalls;
    [SerializeField] private Transform queenBoss;
    [SerializeField] private Transform hive;
    [SerializeField] private float floatAnimHeight = 0.5f;

    [Header("Dialogue Setup")]
    [SerializeField] private Dialogue introDialogue;
    [SerializeField] private Dialogue outroDialogue;

    [Header("Spawning Setup")]
    [SerializeField] private GameObject beeEnemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int totalBeesToDefeat = 5;
    [SerializeField] private float timeBetweenSpawns = 2f;

    [Header("Reward Setup")]
    [SerializeField] private GameObject rewardChest;

    [Inject] private IDialogService dialogService;
    [Inject] private DiContainer diContainer;

    private bool hasTriggered = false;
    private int spawnedBeesCount = 0;
    private List<GameObject> activeBees = new List<GameObject>();

    private void Start()
    {
        if (rewardChest != null)
        {
            rewardChest.SetActive(false);
        }

        if (queenBoss != null)
        {
            queenBoss.DOMoveY(queenBoss.position.y + floatAnimHeight, 1.5f)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);
        }

        foreach (var wall in arenaWalls) wall.SetActive(false);
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
        foreach (var wall in arenaWalls) wall.SetActive(true);

        if (introDialogue != null)
        {
            dialogService.StartDialogue(introDialogue);
            yield return new WaitWhile(() => dialogService.IsDialogueActive);
        }

        while (spawnedBeesCount < totalBeesToDefeat)
        {
            SpawnBee();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        yield return new WaitUntil(() => AreAllBeesDead());

        if (outroDialogue != null)
        {
            dialogService.StartDialogue(outroDialogue);
            yield return new WaitWhile(() => dialogService.IsDialogueActive);
        }

        queenBoss.DOKill();
        queenBoss.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack);

        hive.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(queenBoss.gameObject);
            Destroy(hive.gameObject);

            if (rewardChest != null)
            {
                rewardChest.SetActive(true);
                rewardChest.transform.localScale = Vector3.zero;
                rewardChest.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }

            foreach (var wall in arenaWalls) wall.SetActive(false);

            Destroy(gameObject);
        });
    }

    private void SpawnBee()
    {
        GameObject newBee = diContainer.InstantiatePrefab(beeEnemyPrefab, spawnPoint.position, Quaternion.identity, null);
        activeBees.Add(newBee);
        spawnedBeesCount++;

        newBee.transform.localScale = Vector3.zero;
        newBee.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
    }

    private bool AreAllBeesDead()
    {
        activeBees.RemoveAll(bee => bee == null);
        return activeBees.Count == 0;
    }
}