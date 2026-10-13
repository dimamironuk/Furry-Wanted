using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    [SerializeField] private DialogueNode startNode; 

    public DialogueNode StartNode => startNode;
}