using UnityEngine;
using DG.Tweening;

public class HeadBobber : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float moveDistance = 0.08f; 
    [SerializeField] private float duration = 0.5f;   
    [SerializeField] private float startDelay = 0f;     

    private Vector3 startPosition;
    private Tween bobTween;

    private void Start()
    {
        startPosition = transform.localPosition;

        bobTween = transform.DOLocalMoveY(startPosition.y + moveDistance, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(startDelay);
    }

    private void OnDestroy()
    {
        if (bobTween != null && bobTween.IsActive())
        {
            bobTween.Kill();
        }
    }
}