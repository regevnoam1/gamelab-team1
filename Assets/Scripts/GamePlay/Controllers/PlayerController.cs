using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using Core.Managers;

namespace GamePlay.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [Header("Aiming Arrow Configuration")]
        [SerializeField] private GameObject trianglePrefab; // Drag and drop a prefab in the inspector
        private GameObject _triangleInstance;
        [SerializeField] private float triangleDistance = 2f; // Distance from player to triangle
        [SerializeField] private float triangleSize = 1f; // Size of the triangle
        
        
        // Movement variables
        [Header("Movement Configuration")]
        [SerializeField]
        private float speed = 10f;
        [SerializeField] 
        private float pullingForce = 3f;
        private Vector2 _movement;
        
        private Rigidbody2D _rb;
        private readonly List<GameObject> _points = new List<GameObject>();
        private Vector2 _pullingDirection;
        private bool _isPullingLever = false; // Flag to track lever pulling

        // LineRenderer configuration
        private LineRenderer _lineRenderer;
    
        [Header("Shooting Configuration")]
        // Shooting parameters
        [SerializeField] private GameObject amebaPrefab;
        [SerializeField] private float shootCooldown = 5f;
        
        private readonly List<(GameObject ameba, float elapsedTime)> _activeAmebaScales = new List<(GameObject, float)>();
        private bool _canShoot = true;
        private float timer = 0f;
        private Gamepad _pad;
        [SerializeField] private AudioSource audioSource;

        [Header("Dash Configuration")]
        // Dashing parameters
        [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private float dashDuration = 0.5f;
        private bool _isDashing = false;
        
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {
            // Initialize Rigidbody2D
            _rb = GetComponent<Rigidbody2D>();
            // Initialize LineRenderer
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            ConfigureTriangle();

            // Add points from the children if their tag is "Point"
            StartCoroutine(InitPoints());
            
        }

        private IEnumerator InitPoints()
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Node"))
                {
                    _points.Add(child.gameObject);
                }
            }
            yield return null;
        }

        private void Update()
        {
            TryRenderLine();
            
            if (_isDashing)
            {
                return;
            }


            if (GameManager.Instance._attackName == "simple")
            {
                TryShoot();
                UpdateScalingAmebas();
            }

            TickTok();

        }
        
        private void FixedUpdate()
        {
            if (_isDashing)
            {
                return;
            }
            
            TryMove();

        }
        

        #endregion

        #region Input Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!context.canceled)
            {
                Debug.Log("Lever Pulled");
                _isPullingLever = true;

                // Set pulling direction based on movement
                _pullingDirection = context.ReadValue<Vector2>();

                Debug.Log($"Pulling Direction: {_pullingDirection}, Magnitude: {pullingForce}");
            }
            else
            {
                Debug.Log("Lever Released");
                _pullingDirection = Vector2.zero;
                _isPullingLever = false;
            }
        }
        
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Debug.Log("Dash!");
                StartCoroutine(Dash());
            }
        }
    
        #endregion

        #region Triangle Indicator
            private void ConfigureTriangle()
            {
                if (trianglePrefab != null)
                {
                    _triangleInstance = Instantiate(trianglePrefab, transform.position, Quaternion.identity);
                    _triangleInstance.SetActive(false); // Initially hide the triangle
                }
            }

            private void UpdateTriangle()
            {
                if (_triangleInstance == null) return;

                _triangleInstance.SetActive(true);
                
                // Set position and rotation
                _triangleInstance.transform.position = transform.position + (Vector3)(_movement.normalized * triangleDistance);
                float angle = Mathf.Atan2(_movement.y, _movement.x) * Mathf.Rad2Deg;
                _triangleInstance.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }

        #endregion

        #region Other Methods
            
            private void SpawnAmebas()
            {
                switch (GameManager.Instance._attackName)
                {
                    case "simple":
                        List<GameObject> amebas = new List<GameObject>();
                        for (int i = 0; i < Mathf.Min(5, GameManager.Instance._attackLevel); i++)
                        {
                            GameObject ameba = Instantiate(amebaPrefab, transform.position, Quaternion.identity);
                            FixedJoint2D joint = ameba.AddComponent<FixedJoint2D>();
                            joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();
                            amebas.Add(ameba);
                        }
                        ShootSimple(amebas);
                        break;
                }
                
            }

            private IEnumerator Dash()
            {
                _isDashing = true;
                _rb.AddForce(_movement * dashSpeed * 5, ForceMode2D.Impulse);
                yield return new WaitForSeconds(dashDuration);
                _isDashing = false;
            }


            private void ShootSimple(List<GameObject> amebas)
            {
                // Generate shooting directions with even angles between them
                int count = amebas.Count;

                Vector2[] directions = new Vector2[count];
                float angleIncrement = 60f / count; // Evenly spaced angles in a circle
                float mainAngle = Mathf.Atan2(_movement.y, _movement.x) * Mathf.Rad2Deg - (amebas.Count - 1) * angleIncrement / 2;

                for (int i = 0; i < count; i++)
                {
                    float angle = mainAngle + i * angleIncrement;
                    float radian = angle * Mathf.Deg2Rad;
                    directions[i] = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
                }

                for (int i = 0; i < amebas.Count; i++)
                {
                    GameObject ameba = amebas[i];

                    // Start animation by adding the ameba to a list of active animations
                    StartScalingAmeba(ameba);

                    // Detach the ameba from the player
                    Destroy(ameba.GetComponent<FixedJoint2D>());

                    // Apply shooting logic (e.g., apply force or set velocity)
                    Rigidbody2D rb = ameba.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        float shootingForce = speed * 2; // Adjust as needed
                        rb.linearVelocity = directions[i] * shootingForce;
                    }
                }
            }
            


            private void StartScalingAmeba(GameObject ameba)
            {
                _activeAmebaScales.Add((ameba, 0f));
            }

            private void UpdateScalingAmebas()
            {
                float duration = 0.5f;

                for (int i = _activeAmebaScales.Count - 1; i >= 0; i--)
                {
                    var (ameba, elapsedTime) = _activeAmebaScales[i];

                    if (ameba == null)
                    {
                        _activeAmebaScales.RemoveAt(i);
                        continue;
                    }

                    elapsedTime += Time.deltaTime;
                    Vector3 originalScale = Vector3.one;
                    Vector3 targetScale = Vector3.one * 1.5f;
                    ameba.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);

                    if (elapsedTime >= duration)
                    {
                        _activeAmebaScales.RemoveAt(i);
                    }
                    else
                    {
                        _activeAmebaScales[i] = (ameba, elapsedTime);
                    }
                }
            }



            private IEnumerator ShootAmeba(GameObject ameba)
            {
                // enlarge the ameba over one second to 1 size
                float elapsedTime = 0;
                float duration = 0.5f;
                Vector3 originalScale = ameba.transform.localScale;
                Vector3 targetScale = Vector3.one * 1.5f;
                while (elapsedTime < duration)
                {
                    ameba.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                // detach the ameba from the player
                Destroy(ameba.GetComponent<FixedJoint2D>());
                // then shoot it in the direction of the lever
                ameba.GetComponent<Rigidbody2D>().AddForce(_pullingDirection * pullingForce * pullingForce, ForceMode2D.Impulse);
                // make sure the ameba will stay with that force without being affected
                ameba.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            }
            
            public void ChangeColorTemporarily(Color color, float duration)
            {
                // Find the child GameObject with the SpriteShapeRenderer
                SpriteShapeRenderer spriteShapeRenderer = GetComponentInChildren<SpriteShapeRenderer>();
                if (spriteShapeRenderer == null)
                {
                    Debug.LogWarning("SpriteShapeRenderer not found on the player's child!");
                    return;
                }

                // Start the coroutine to change the color temporarily
                StartCoroutine(ChangeSpriteShapeColorCoroutine(spriteShapeRenderer, color, duration));
            }

            private IEnumerator ChangeSpriteShapeColorCoroutine(SpriteShapeRenderer spriteShapeRenderer, Color color, float duration)
            { 
                audioSource.Play();
                RumbleManager.instance.Rumble(0.25f,1f,0.25f);
                // Get the fill material from the SpriteShapeRenderer
                Material fillMaterial = spriteShapeRenderer.materials[0];

                if (fillMaterial == null)
                {
                    Debug.LogWarning("Fill material not found on the SpriteShapeRenderer!");
                    yield break;
                }

                // Store the original color of the fill material
                Color originalColor = fillMaterial.color;

                // Calculate the interval for each flash
                int flashCount = 3; // Number of flashes
                float interval = duration / (flashCount * 2); // Time per color change

                for (int i = 0; i < flashCount; i++)
                {
                    // Change to the specified color
                    fillMaterial.color = color;
                    yield return new WaitForSeconds(interval);

                    // Revert to the original color
                    fillMaterial.color = originalColor;
                    yield return new WaitForSeconds(interval);
                }

                // Ensure it ends with the original color
                fillMaterial.color = originalColor;
            }
            
            private void TickTok()
            {
                timer += Time.deltaTime;

                if (timer >= shootCooldown)
                {
                    _canShoot = true;
                }

            }

            private void TryShoot()
            {
                if (_canShoot && _movement.magnitude > 0.3f)
                {
                    Debug.Log("Shoot!");
                    _canShoot = false;
                    timer = 0f;
                    SpawnAmebas();
                }
            }

            private void TryRenderLine()
            {
                if (_movement.magnitude > 0)
                {
                    UpdateTriangle();
                }
                else
                {
                    _triangleInstance.SetActive(false);
                }
                
            }

            private void TryMove()
            {
                _rb.linearVelocity = _movement * speed;
            }
            
        #endregion
    }
}
