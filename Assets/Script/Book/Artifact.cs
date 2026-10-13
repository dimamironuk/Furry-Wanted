using UnityEngine;

[CreateAssetMenu(fileName = "NewArtifact", menuName = "Game/Artifact")]
public class Artifact : ScriptableObject
{
    public string artifactID;
    public string artifactName;

    [TextArea(3, 5)]
    public string description;

    public string bonusEffect; 
    public Sprite icon;
}