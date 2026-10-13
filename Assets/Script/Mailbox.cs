using UnityEngine;
using DG.Tweening;

public class Mailbox : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt; 
    [SerializeField] private CanvasGroup letterPanel;     
    [SerializeField] private RectTransform letterRect;   

    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float animDuration = 0.4f;
    [SerializeField] private float slideOffset = 800f;     

    private bool playerInRange = false;
    private bool isOpened = false;
    private Vector2 originalPosition;

    private void Awake()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        if (letterPanel != null)
        {
            if (letterRect == null) letterRect = letterPanel.GetComponent<RectTransform>();
            originalPosition = letterRect.anchoredPosition;
            letterPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(interactKey))
        {
            OpenLetter();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpened) return;

        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    private void OpenLetter()
    {
        isOpened = true;

        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        if (letterPanel != null && letterRect != null)
        {
            letterPanel.gameObject.SetActive(true);
            letterPanel.alpha = 0f;

            letterRect.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y - slideOffset);

            letterPanel.DOFade(1f, animDuration).SetUpdate(true);
            letterRect.DOAnchorPosY(originalPosition.y, animDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

 

    public void CloseLetter()
    {
        if (letterPanel == null || letterRect == null) return;

        letterPanel.DOFade(0f, animDuration).SetUpdate(true);
        letterRect.DOAnchorPosY(originalPosition.y - slideOffset, animDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                letterPanel.gameObject.SetActive(false);
            });
    }
}