using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemID;
    public Sprite sprite;

    [Header("Consumable Settings")]
    public bool isConsumable = true; 
    public int healAmount = 1;  
}