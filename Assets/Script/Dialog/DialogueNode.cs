using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/DialogueNode")]
public class DialogueNode : ScriptableObject
{
    [SerializeField] private string speakerName = "Спікер";
    [SerializeField] private Sprite speakerIcon; 

    [TextArea(3, 10)] 
    [SerializeField] private string dialogueText;

    [SerializeField] private Choice[] choices;

    private DialogueNode nextLinearNode; 

    public string SpeakerName => speakerName;
    public Sprite SpeakerIcon => speakerIcon;
    public string DialogueText => dialogueText;
    public Choice[] Choices => choices;
    public bool HasChoices => choices != null && choices.Length > 0;
    public DialogueNode NextLinearNode => nextLinearNode;

    public void SetNextLinearNode(DialogueNode node)
    {
        nextLinearNode = node;
    }
}

[System.Serializable]
public class Choice
{
    [SerializeField] private string choiceText; 
    [SerializeField] private DialogueNode targetNode; 

    public string ChoiceText => choiceText;
    public DialogueNode TargetNode => targetNode;
}