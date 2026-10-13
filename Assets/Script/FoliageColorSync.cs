using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FoliageColorSync : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Material foliageMaterial;

    private void Update()
    {
        if (globalLight != null && foliageMaterial != null)
        {
            Color currentLightColor = globalLight.color * globalLight.intensity;
            currentLightColor.a = 1f;
            foliageMaterial.SetColor("_NightColor", currentLightColor);
        }
    }
}