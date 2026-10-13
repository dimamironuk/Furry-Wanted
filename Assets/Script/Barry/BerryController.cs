using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Zenject;

public class BerryController : Item
{
    [SerializeField] private Sprite noBerry;
    [SerializeField] private SpriteRenderer current;
    [SerializeField] private GameObject _action;
    [SerializeField] private float _duration = 0.25f;
    [SerializeField] private Vector3 _targetScale = Vector3.one;

    [Header("Floating Animation")]
    [SerializeField] private float _floatDistance = 0.1f;
    [SerializeField] private float _floatDuration = 0.6f;

    [Inject] private InventoryManager _inventoryManager;

    private Vector3 _startLocalPos;
    private bool _isPlayerInside;
    private bool _isPickedUp;

    private void Awake()
    {
        _startLocalPos = _action.transform.localPosition;
        _action.transform.localScale = Vector3.zero;
        _action.SetActive(false);
    }

    private void Update()
    {
        if (_isPlayerInside && !_isPickedUp && Input.GetKeyDown(KeyCode.E))
        {
            PickUpBerry();
        }
    }

    public void PickUpBerry()
    {
        if (_inventoryManager.AddItem(this))
        {
            _isPickedUp = true;
            current.sprite = noBerry; 
            HideActionPrompt();
            GetComponent<Collider2D>().enabled = false;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !_isPickedUp)
        {
            _isPlayerInside = true;

            _action.SetActive(true);
            _action.transform.DOKill();
            _action.transform.localPosition = _startLocalPos;

            _action.transform.DOScale(_targetScale, _duration)
                .SetEase(Ease.OutBack);

            _action.transform.DOLocalMoveY(_startLocalPos.y + _floatDistance, _floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInside = false;
            HideActionPrompt();
        }
    }

    private void HideActionPrompt()
    {
        _action.transform.DOKill();
        _action.transform.DOScale(Vector3.zero, _duration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _action.SetActive(false);
                _action.transform.localPosition = _startLocalPos;
            });
    }
}