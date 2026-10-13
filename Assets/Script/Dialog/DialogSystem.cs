using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Zenject;

public class DialogSystem : MonoBehaviour, IDialogService, IInitializable
{
    [Header("Налаштування UI")]
    [SerializeField] private GameObject dialogObject;
    [SerializeField] private Transform dialogBG;
    [SerializeField] private Image speakerIcon;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Кнопки Вибору")]
    [SerializeField] private GameObject choicesButtonsObject;
    [SerializeField] private Button[] choicesButtons;

    [Header("Налаштування Друку")]
    [SerializeField] private float defaultTypingSpeed = 0.05f;
    [SerializeField] private float skipMultiplier = 3.0f;

    [Header("Налаштування Анімації Голови")]
    [SerializeField] private float idleFloatDistance = 8f;
    [SerializeField] private float idleFloatDuration = 0.6f;

    [Header("Звуки")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private int playSoundEveryXChar = 2;

    [Inject] private IInputReader inputReader;

    private DialogueNode currentNode;
    private float currentTypingSpeed;
    private bool isTyping;
    private string fullText;
    private int currentCharacter;
    private bool playerCanInteract = true;
    private bool isDialogueActive = false;

    private Coroutine typingCoroutine;
    private Sequence speechHeadSequence;
    private Tween idleHeadTween;
    private Vector3 initialHeadPos;
    private bool justOpenedDialogue;

    public bool IsDialogueActive => isDialogueActive;

    public void Initialize()
    {
        if (speakerIcon != null)
        {
            initialHeadPos = speakerIcon.transform.localPosition;
        }

        isDialogueActive = false;
        dialogObject.SetActive(false);
        choicesButtonsObject.SetActive(false);
        SetupButtons();
    }

    private void Update()
    {
        if (justOpenedDialogue)
        {
            justOpenedDialogue = false;
            return;
        }

        if (isDialogueActive && playerCanInteract && inputReader.InteractPressed())
        {
            PlaySound(clickSound);
            HandlePlayerInput();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.StartNode == null)
        {
            Debug.LogError("DialogSystem: Діалог або стартовий нод не знайдені.");
            return;
        }

        currentNode = dialogue.StartNode;
        isDialogueActive = true;
        justOpenedDialogue = true;

        ShowDialogue();
        DisplayNode(currentNode);
    }

    public void HideDialogue()
    {
        if (!isDialogueActive) return;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        StopHeadAnimation();

        PlaySound(closeSound);

        dialogBG.DOKill();
        dialogBG.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            dialogObject.SetActive(false);
            isDialogueActive = false;
            isTyping = false;
        });
    }

    private void HandlePlayerInput()
    {
        if (isTyping)
        {
            CompleteTyping();
        }
        else if (!currentNode.HasChoices)
        {
            ShowNextLine();
        }
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        dialogueText.text = fullText;
        isTyping = false;

        StartHeadIdleAnimation();

        if (currentNode.HasChoices)
        {
            ShowChoices(currentNode);
        }
    }

    private void ShowDialogue()
    {
        dialogObject.SetActive(true);
        PlaySound(openSound);

        dialogBG.DOKill();
        dialogBG.localScale = Vector3.zero;
        dialogBG.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    private void DisplayNode(DialogueNode node)
    {
        speakerIcon.sprite = node.SpeakerIcon;
        speakerNameText.text = node.SpeakerName;
        choicesButtonsObject.SetActive(false);

        isTyping = true;
        fullText = node.DialogueText;
        dialogueText.text = "";
        currentCharacter = 0;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        StartHeadSpeechAnimation();
        typingCoroutine = StartCoroutine(TypewriterEffectCoroutine());
    }

    private IEnumerator TypewriterEffectCoroutine()
    {
        while (currentCharacter < fullText.Length && isTyping)
        {
            dialogueText.text += fullText[currentCharacter];

            if (audioSource != null && typeSound != null && currentCharacter % playSoundEveryXChar == 0)
            {
                if (!char.IsWhiteSpace(fullText[currentCharacter]))
                {
                    audioSource.pitch = Random.Range(0.92f, 1.08f);
                    audioSource.PlayOneShot(typeSound);
                }
            }

            currentCharacter++;
            currentTypingSpeed = inputReader.InteractHeld() ? defaultTypingSpeed / skipMultiplier : defaultTypingSpeed;

            yield return new WaitForSeconds(currentTypingSpeed);
        }

        if (audioSource != null) audioSource.pitch = 1f; 

        CompleteTyping();
    }

    private void StartHeadSpeechAnimation()
    {
        StopHeadAnimation();

        speechHeadSequence = DOTween.Sequence();
        speechHeadSequence.Append(speakerIcon.transform.DOScale(new Vector3(1.08f, 0.92f, 1f), 0.12f).SetEase(Ease.InOutSine))
                          .Join(speakerIcon.transform.DOLocalRotate(new Vector3(0, 0, 4f), 0.12f).SetEase(Ease.InOutSine))
                          .Append(speakerIcon.transform.DOScale(new Vector3(0.92f, 1.08f, 1f), 0.12f).SetEase(Ease.InOutSine))
                          .Join(speakerIcon.transform.DOLocalRotate(new Vector3(0, 0, -4f), 0.12f).SetEase(Ease.InOutSine))
                          .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartHeadIdleAnimation()
    {
        StopHeadAnimation();

        idleHeadTween = speakerIcon.transform.DOLocalMoveY(initialHeadPos.y + idleFloatDistance, idleFloatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopHeadAnimation()
    {
        speechHeadSequence?.Kill();
        idleHeadTween?.Kill();
        speakerIcon.transform.DOKill();

        if (initialHeadPos != Vector3.zero)
        {
            speakerIcon.transform.localPosition = initialHeadPos;
        }

        speakerIcon.transform.localScale = Vector3.one;
        speakerIcon.transform.localRotation = Quaternion.identity;
    }

    private void SetupButtons()
    {
        for (int i = 0; i < choicesButtons.Length; i++)
        {
            int index = i;
            choicesButtons[i].onClick.AddListener(() => SelectChoice(index));
        }
    }

    private void ShowChoices(DialogueNode node)
    {
        if (!node.HasChoices) return;

        playerCanInteract = false;
        choicesButtonsObject.SetActive(true);

        for (int i = 0; i < choicesButtons.Length; i++)
        {
            if (i < node.Choices.Length)
            {
                choicesButtons[i].gameObject.SetActive(true);
                choicesButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = node.Choices[i].ChoiceText;

                choicesButtons[i].transform.DOKill();
                choicesButtons[i].transform.localScale = Vector3.zero;
                choicesButtons[i].transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetDelay(i * 0.1f);
            }
            else
            {
                choicesButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectChoice(int index)
    {
        if (currentNode == null || !currentNode.HasChoices || index < 0 || index >= currentNode.Choices.Length) return;

        PlaySound(clickSound);

        currentNode = currentNode.Choices[index].TargetNode;
        choicesButtonsObject.SetActive(false);
        playerCanInteract = true;

        if (currentNode != null)
        {
            DisplayNode(currentNode);
        }
        else
        {
            HideDialogue();
        }
    }

    private void ShowNextLine()
    {
        DialogueNode nextLinearNode = currentNode.NextLinearNode;

        currentNode = nextLinearNode;
        if (currentNode != null)
        {
            DisplayNode(currentNode);
        }
        else
        {
            HideDialogue();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDestroy()
    {
        dialogBG.DOKill();
        StopHeadAnimation();
    }
}