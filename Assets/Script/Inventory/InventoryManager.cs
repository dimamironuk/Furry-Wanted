using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Zenject; 

[Serializable]
public class SlotInfo
{
    public Button button;
    public Image image;
    public TextMeshProUGUI countText;
    public Item item;
    public int amount;
    public bool isEmpty = true;

    public void SetItem(Item newItem, int qty = 1, float animDuration = 0.3f)
    {
        item = newItem;
        amount = qty;
        image.sprite = newItem.sprite;
        image.enabled = true;
        isEmpty = false;

        UpdateUI();

        image.transform.DOKill();
        image.transform.localScale = Vector3.zero;
        image.transform.DOScale(Vector3.one, animDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void AddAmount(int qty = 1)
    {
        amount += qty;
        UpdateUI();
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;
        image.sprite = null;
        image.enabled = false;
        isEmpty = true;

        if (countText != null)
            countText.text = "";
    }

    public void UpdateUI()
    {
        if (countText == null) return;

        if (isEmpty || amount <= 1)
        {
            countText.text = "";
        }
        else
        {
            countText.text = amount.ToString();
        }
    }
}

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<SlotInfo> slots = new List<SlotInfo>();
    [SerializeField] private float itemAppearDuration = 0.3f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;

    [Inject] private HPController hpController; 

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    private void Start()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;
            if (slots[i].button != null)
            {
                slots[i].button.onClick.AddListener(() => UseItemInSlot(index));
            }
        }
    }

    public void UseItemInSlot(int index)
    {
        Debug.Log("yes");
        SlotInfo slot = slots[index];

        if (slot.isEmpty || slot.item == null) return;

        if (slot.item.isConsumable && hpController != null)
        {
            bool wasHealed = hpController.Heal(slot.item.healAmount);

            if (wasHealed)
            {

                slot.amount--;

                if (slot.amount <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.UpdateUI();
                }
            }
        }
    }

    public bool AddItem(Item newItem)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].isEmpty && slots[i].item.itemID == newItem.itemID)
            {
                slots[i].AddAmount(1);
                PlayPickupSound();
                return true;
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isEmpty)
            {
                slots[i].SetItem(newItem, 1, itemAppearDuration);
                PlayPickupSound();
                return true;
            }
        }

        Debug.Log("Інвентар заповнений!");
        return false;
    }

    private void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
}