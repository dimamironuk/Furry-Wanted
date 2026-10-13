using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Zenject;

public class FinaleSequenceController : MonoBehaviour
{
    [Header("Dialogue Setup")]
    [SerializeField] private Dialogue finaleDialogue;

    [Header("Milli Animation Setup")]
    [SerializeField] private Transform milliHeadTransform;
    [SerializeField] private float headBobDistance = 0.08f;
    [SerializeField] private float headBobSpeed = 0.4f;

    [Header("Cutscene Art Setup")]
    [SerializeField] private CanvasGroup cutsceneCanvasGroup;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float displayDuration = 3f;

    [Header("Scene Transition")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Inject] private IDialogService dialogService;

    private bool hasTriggered = false;
    private Tween headBobTween;
    private Vector3 initialHeadPosition;

    private void Start()
    {
        if (cutsceneCanvasGroup != null)
        {
            cutsceneCanvasGroup.alpha = 0f;
            cutsceneCanvasGroup.gameObject.SetActive(false);
        }

        if (milliHeadTransform != null)
        {
            initialHeadPosition = milliHeadTransform.localPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(StartFinaleSequence(other.gameObject));
        }
    }

    private IEnumerator StartFinaleSequence(GameObject player)
    {
        MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            if (script != null && !(script is HPController))
            {
                script.enabled = false;
            }
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        AnimationPlayer animPlayer = player.GetComponent<AnimationPlayer>();
        if (animPlayer != null)
        {
            animPlayer.SetMovement(Vector2.zero, false);
        }

        if (milliHeadTransform != null)
        {
            headBobTween = milliHeadTransform
                .DOLocalMoveY(initialHeadPosition.y + headBobDistance, headBobSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        if (finaleDialogue != null)
        {
            dialogService.StartDialogue(finaleDialogue);
            yield return new WaitWhile(() => dialogService.IsDialogueActive);
        }

        if (headBobTween != null && headBobTween.IsActive())
        {
            headBobTween.Kill();
            milliHeadTransform.localPosition = initialHeadPosition;
        }

        if (cutsceneCanvasGroup != null)
        {
            cutsceneCanvasGroup.gameObject.SetActive(true);
            yield return cutsceneCanvasGroup.DOFade(1f, fadeInDuration).WaitForCompletion();
        }

        yield return new WaitForSeconds(displayDuration);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}