using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; 

public class PauseMenu : MonoBehaviour
{
    [Header("Panels & UI")]
    [SerializeField] private CanvasGroup bgCanvasGroup; 
    [SerializeField] private RectTransform panelPause;  
    [SerializeField] private RectTransform panelSettings; 

    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.35f;

    private bool isPaused = false;

    private void Start()
    {
        if (bgCanvasGroup != null)
        {
            bgCanvasGroup.alpha = 0f;
            bgCanvasGroup.gameObject.SetActive(false);
        }

        InitWindow(panelPause);
        InitWindow(panelSettings);
    }

    private void InitWindow(RectTransform window)
    {
        if (window != null)
        {
            window.localScale = Vector3.zero;
            window.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelSettings.gameObject.activeSelf)
            {
                CloseSettings();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        bgCanvasGroup.gameObject.SetActive(true);
        panelPause.gameObject.SetActive(true);
        panelSettings.gameObject.SetActive(false);

        bgCanvasGroup.DOKill();
        panelPause.DOKill();

        bgCanvasGroup.DOFade(1f, animDuration).SetUpdate(true);

        // Пружне виринання вікна (Pop-up ефект)
        panelPause.localScale = Vector3.zero;
        panelPause.DOScale(Vector3.one, animDuration)
            .SetEase(Ease.OutBack) 
            .SetUpdate(true);
    }

    public void ResumeGame()
    {
        isPaused = false;

        bgCanvasGroup.DOKill();
        panelPause.DOKill();

        bgCanvasGroup.DOFade(0f, animDuration).SetUpdate(true);

        panelPause.DOScale(Vector3.zero, animDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 1f;
                bgCanvasGroup.gameObject.SetActive(false);
                panelPause.gameObject.SetActive(false);
            });
    }

    public void OpenSettings()
    {
        panelPause.DOKill();
        panelSettings.DOKill();

        panelPause.DOScale(Vector3.zero, animDuration * 0.7f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                panelPause.gameObject.SetActive(false);
                panelSettings.gameObject.SetActive(true);

                panelSettings.localScale = Vector3.zero;
                panelSettings.DOScale(Vector3.one, animDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            });
    }

    public void CloseSettings()
    {
        panelSettings.DOKill();
        panelPause.DOKill();

        panelSettings.DOScale(Vector3.zero, animDuration * 0.7f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                panelSettings.gameObject.SetActive(false);
                panelPause.gameObject.SetActive(true);

                panelPause.localScale = Vector3.zero;
                panelPause.DOScale(Vector3.one, animDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            });
    }

    public void OpenMenu()
    {
        Time.timeScale = 1f;
        DOTween.KillAll(); 
        SceneManager.LoadScene(0);
    }
}