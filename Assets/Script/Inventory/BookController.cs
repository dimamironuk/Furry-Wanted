using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BookController : MonoBehaviour
{
    [Header("Databases")]
    [SerializeField] private List<Artifact> allArtifacts = new List<Artifact>();
    [SerializeField] private List<Artifact> unlockedArtifacts = new List<Artifact>();

    [Header("UI Animations")]
    [SerializeField] private RectTransform bookPanel;
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Right Page UI")]
    [SerializeField] private Image rightPageIcon;
    [SerializeField] private TextMeshProUGUI rightPageName;
    [SerializeField] private TextMeshProUGUI rightPageInfo;

    [Header("Left Page UI")]
    [SerializeField] private List<ArtifactSlotUI> uiSlots;

    private void Start()
    {
        bookPanel.anchoredPosition = new Vector2(0, -2000);
        ClearRightPage();
        RefreshSlots();
    }

    public void OpenBook()
    {
        RefreshSlots();
        ClearRightPage();

        bookPanel.DOAnchorPosY(0, animationDuration).SetEase(Ease.OutBack);
    }

    public void CloseBook()
    {
        bookPanel.DOAnchorPosY(-2000, animationDuration).SetEase(Ease.InBack);
    }

    public void CollectArtifact(Artifact newArtifact)
    {
        if (newArtifact == null) return;

        if (!unlockedArtifacts.Contains(newArtifact))
        {
            unlockedArtifacts.Add(newArtifact);
            Debug.Log($"Artifact {newArtifact.artifactName} successfully added to the book!");
            RefreshSlots();
        }
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < allArtifacts.Count; i++)
        {
            if (i < uiSlots.Count)
            {
                bool isUnlocked = unlockedArtifacts.Contains(allArtifacts[i]);
                uiSlots[i].Setup(allArtifacts[i], isUnlocked, this);
            }
        }
    }

    public void ShowArtifactInfo(Artifact artifact, bool isUnlocked)
    {
        rightPageIcon.sprite = artifact.icon;
        rightPageIcon.gameObject.SetActive(true);

        if (isUnlocked)
        {
            rightPageIcon.color = Color.white;
            rightPageName.text = artifact.artifactName;

            // Output in English
            rightPageInfo.text = $"{artifact.description}\n\n<color=#FFD700>Bonus: {artifact.bonusEffect}</color>";
        }
        else
        {
            rightPageIcon.color = new Color(0, 0, 0, 0.8f);
            rightPageName.text = "???";
            rightPageInfo.text = "This artifact has not been found yet. Explore the world to unlock its secrets!";
        }
    }

    private void ClearRightPage()
    {
        rightPageIcon.gameObject.SetActive(false);
        rightPageName.text = "";
        rightPageInfo.text = "Select an artifact from the list on the left to learn more about it.";
    }
}