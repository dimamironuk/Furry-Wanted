using UnityEngine;

public interface IInputReader
{
    bool InteractPressed();
    bool InteractHeld();  
}

public class KeyboardInputReader : IInputReader
{
    public bool InteractPressed() => Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);
    public bool InteractHeld() => Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space);
}

public interface IDialogService
{
    void StartDialogue(Dialogue dialogue); 
    void HideDialogue();                
    bool IsDialogueActive { get; }       
}