using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform windowTransform;

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.5f;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (windowTransform == null) windowTransform = GetComponent<RectTransform>();

        if (retryButton != null) retryButton.onClick.AddListener(OnRetryPressed);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuPressed);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        windowTransform.localScale = Vector3.zero;

        Time.timeScale = 0f;

        canvasGroup.DOFade(1f, animDuration).SetUpdate(true);
        windowTransform.DOScale(Vector3.one, animDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }


    public void OnRetryPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}