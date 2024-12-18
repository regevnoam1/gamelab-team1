using System.Collections;
using UnityEngine;

namespace GamePlay.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] Transform playerTransform; // Assign the player's transform in the Inspector
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); 
        [SerializeField] private float followSpeed = 5f; // Speed at which the camera follows
        [SerializeField] private bool maintainCameraRotation = true; // Keep the camera's rotation fixed
        [SerializeField] Vector2 cameraVerticalBounds; // Bounds for the camera
        [SerializeField] Vector2 cameraHorizontalBounds; // Bounds for the camera
        
        private float initialScale;
        
        
        private Quaternion fixedRotation;
        private float growthSpeed = 0.1f;

        void Start()
        {
            // Store the initial camera rotation if maintainCameraRotation is true
            if (maintainCameraRotation)
            {
                fixedRotation = transform.rotation;
            }
            initialScale = playerTransform.localScale.magnitude;
            Debug.Log(initialScale);
        }

        void LateUpdate()
        {
            if (playerTransform == null)
                return;

            
            // Compute the desired position
            Vector3 targetPosition = playerTransform.position + offset;
            targetPosition.x = Mathf.Clamp(targetPosition.x, cameraHorizontalBounds.x, cameraHorizontalBounds.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, cameraVerticalBounds.x, cameraVerticalBounds.y);

            // Smoothly move the camera to the desired position
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Maintain the fixed rotation if enabled
            if (maintainCameraRotation)
            {
                transform.rotation = fixedRotation;
            }
            else
            {
                // Optionally, follow player's rotation only on the Y axis
                Quaternion targetRotation = Quaternion.Euler(0, playerTransform.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
            }
        }
    
    }
}
