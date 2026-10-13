using UnityEngine;
using UnityEngine.UI;

public class ArtifactSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private Artifact artifactData;
    private bool isUnlocked;
    private BookController bookController;

    public void Setup(Artifact artifact, bool unlocked, BookController controller)
    {
        artifactData = artifact;
        isUnlocked = unlocked;
        bookController = controller;

        iconImage.sprite = artifact.icon;
        iconImage.gameObject.SetActive(true);

        if (isUnlocked)
        {
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.color = new Color(0, 0, 0, 0.8f);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        bookController.ShowArtifactInfo(artifactData, isUnlocked);
    }
}