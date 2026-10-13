using UnityEngine;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class ChestController : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private Artifact artifact;
    [SerializeField] private GameObject active;
    [SerializeField] private Sprite openChest;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openChestSFX;

    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isPlayerInside = false;
    private bool isOpen = false;
    private BookController bookController;

    [Inject]
    private void Construct(BookController bookController)
    {
        this.bookController = bookController;
    }

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (active != null) active.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && !isOpen && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOpen) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (active != null) active.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (active != null) active.SetActive(false);
        }
    }

    private void OpenChest()
    {
        isOpen = true;

        if (openChestSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(openChestSFX);
        }

        if (spriteRenderer != null && openChest != null)
        {
            spriteRenderer.sprite = openChest;
        }

        if (active != null)
        {
            active.SetActive(false);
        }

        if (bookController != null && artifact != null)
        {
            bookController.CollectArtifact(artifact);
        }
    }
}