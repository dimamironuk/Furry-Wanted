using UnityEngine;
using DG.Tweening;
using Zenject;

public class NPC_Controller : MonoBehaviour
{
    [Header("Head Settings")]
    [SerializeField] private Transform npcHead;
    [SerializeField] private float headMotionAmount = 0.06f;
    [SerializeField] private float headMotionDuration = 0.2f;

    [Header("Action Prompt Settings")]
    [SerializeField] private GameObject actionPrompt; 
    [SerializeField] private float promptDuration = 0.25f;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    [Header("Floating Animation")]
    [SerializeField] private float floatDistance = 0.1f;
    [SerializeField] private float floatDuration = 0.6f;

    [Header("Dialogue System Integration")]
    [SerializeField] private Dialogue dialogueToUse; 

    [Inject] private IDialogService dialogueService; 
    [Inject] private IInputReader inputReader;    

    private Tween headTween;
    private Vector3 initialHeadPos;

    private Vector3 promptStartLocalPos;
    private bool isPlayerInside;

    private void Awake()
    {
        if (npcHead != null)
        {
            initialHeadPos = npcHead.localPosition;
        }

        if (actionPrompt != null)
        {
            promptStartLocalPos = actionPrompt.transform.localPosition;
            actionPrompt.transform.localScale = Vector3.zero;
            actionPrompt.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StartHeadMotion();
    }

    private void OnDisable()
    {
        headTween?.Kill();
        if (actionPrompt != null) actionPrompt.transform.DOKill();
    }

    private void Update()
    {
        if (isPlayerInside && inputReader.InteractPressed())
        {
            Interact();
        }
    }

    public virtual void Interact()
    {
        if (dialogueService.IsDialogueActive) return;

        Debug.Log($"NPC: Взаємодія з {gameObject.name}");

        if (dialogueToUse != null)
        {
            HideActionPrompt();
            dialogueService.StartDialogue(dialogueToUse);
        }
        else
        {
            Debug.LogWarning($"NPC: У {gameObject.name} немає діалогу.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (actionPrompt != null && !dialogueService.IsDialogueActive)
            {
                actionPrompt.SetActive(true);
                actionPrompt.transform.DOKill();
                actionPrompt.transform.localPosition = promptStartLocalPos;

                actionPrompt.transform.DOScale(targetScale, promptDuration)
                    .SetEase(Ease.OutBack);

                actionPrompt.transform.DOLocalMoveY(promptStartLocalPos.y + floatDistance, floatDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            HideActionPrompt();

     
        }
    }

    private void HideActionPrompt()
    {
        if (actionPrompt == null) return;

        actionPrompt.transform.DOKill();
        actionPrompt.transform.DOScale(Vector3.zero, promptDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                actionPrompt.SetActive(false);
                actionPrompt.transform.localPosition = promptStartLocalPos;
            });
    }

    private void StartHeadMotion()
    {
        if (npcHead == null) return;

        headTween?.Kill();
        npcHead.localPosition = initialHeadPos;
        headTween = npcHead.DOLocalMoveY(initialHeadPos.y + headMotionAmount, headMotionDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        headTween?.Kill();
        if (actionPrompt != null) actionPrompt.transform.DOKill();
    }
}