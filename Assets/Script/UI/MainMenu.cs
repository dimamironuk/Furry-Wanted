using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; 

public class MainMenu : MonoBehaviour
{
    [Header("Panels & UI")]
    [SerializeField] private RectTransform mainPanel;      
    [SerializeField] private RectTransform settingsPanel;  

    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.35f;  

    private void Start()
    {
        if (mainPanel != null)
        {
            mainPanel.gameObject.SetActive(true);
            mainPanel.localScale = Vector3.zero;
            mainPanel.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
        }

        if (settingsPanel != null)
        {
            settingsPanel.localScale = Vector3.zero;
            settingsPanel.gameObject.SetActive(false);
        }
    }

    public void PlayGame()
    {
        if (mainPanel != null)
        {
            mainPanel.DOScale(Vector3.zero, animDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    DOTween.KillAll(); 
                    SceneManager.LoadScene(1); 
                });
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void OpenSettings()
    {
        if (mainPanel == null || settingsPanel == null) return;

        mainPanel.DOKill();
        settingsPanel.DOKill();

        mainPanel.DOScale(Vector3.zero, animDuration * 0.7f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                mainPanel.gameObject.SetActive(false);
                settingsPanel.gameObject.SetActive(true);

                settingsPanel.localScale = Vector3.zero;
                settingsPanel.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
            });
    }

    public void CloseSettings()
    {
        if (mainPanel == null || settingsPanel == null) return;

        settingsPanel.DOKill();
        mainPanel.DOKill();

        settingsPanel.DOScale(Vector3.zero, animDuration * 0.7f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                settingsPanel.gameObject.SetActive(false);
                mainPanel.gameObject.SetActive(true);

                mainPanel.localScale = Vector3.zero;
                mainPanel.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
            });
    }

    public void QuitGame()
    {
        Debug.Log("Вихід з гри!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Закриває збілджену гру (.exe / .apk)
            Application.Quit();
#endif
    }
}