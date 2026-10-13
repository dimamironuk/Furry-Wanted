using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class HouseController : MonoBehaviour
{
    [Header("Material & Shader")]
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private string propertyName = "_FadeAmount";

    [Header("Teleport Settings")]
    [SerializeField] private Transform targetSpawnPoint;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Camera Settings")]
    [Inject] private CameraController cameraController;
    [SerializeField] private bool lockCamera = true;
    [SerializeField] private Transform targetCameraPoint;

    private static bool isTransitioning = false;

    private void Start()
    {
        if (dissolveMaterial != null)
            dissolveMaterial.SetFloat(propertyName, 1f);

        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            StartCoroutine(TeleportRoutine(other.transform));
        }
    }

    private IEnumerator TeleportRoutine(Transform player)
    {
        isTransitioning = true;

        var playerMovement = player.GetComponent<PlayerController>();
        var rb = player.GetComponent<Rigidbody2D>();

        if (playerMovement != null) playerMovement.enabled = false;
        if (rb != null) rb.velocity = Vector2.zero;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float amount = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            dissolveMaterial.SetFloat(propertyName, amount);
            yield return null;
        }
        dissolveMaterial.SetFloat(propertyName, 0f);

        player.position = targetSpawnPoint.position;

        if (cameraController != null)
        {
            if (lockCamera)
            {
                cameraController.enabled = false;

                Vector3 newCamPos = targetCameraPoint != null ? targetCameraPoint.position : targetSpawnPoint.position;
                cameraController.transform.position = new Vector3(
                    newCamPos.x,
                    newCamPos.y,
                    cameraController.transform.position.z
                );
            }
            else
            {
                cameraController.transform.position = new Vector3(
                    player.position.x,
                    player.position.y,
                    cameraController.transform.position.z
                );
                cameraController.enabled = true;
            }
        }

        yield return new WaitForSeconds(0.1f);

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float amount = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            dissolveMaterial.SetFloat(propertyName, amount);
            yield return null;
        }
        dissolveMaterial.SetFloat(propertyName, 1f);

        if (playerMovement != null) playerMovement.enabled = true;

        isTransitioning = false;
    }
}