using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DynamicTimeByDistance : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("URP 2D Light")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Gradient timeGradient;
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.2f;

    private float totalDistance;

    private void Start()
    {
        if (startPoint != null && endPoint != null)
        {
            totalDistance = Vector2.Distance(startPoint.position, endPoint.position);
        }
    }

    private void Update()
    {
        if (player == null || startPoint == null || endPoint == null || globalLight == null) return;

        float currentDistanceToEnd = Vector2.Distance(player.position, endPoint.position);
        float progress = 1f - Mathf.Clamp01(currentDistanceToEnd / totalDistance);

        globalLight.color = timeGradient.Evaluate(progress);
        globalLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, progress);
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
}