using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GamePlay.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        #region Fields
        [SerializeField] private GameObject trianglePrefab; // Drag and drop a prefab in the inspector
        private GameObject _triangleInstance;
        [SerializeField] private float triangleDistance = 0.5f; // Distance from player to triangle
        [SerializeField] private float triangleSize = 1f; // Size of the triangle
        
        // Movement variables
        private Vector2 _movement;
        [SerializeField]
        private float speed = 10f;
        private Rigidbody2D _rb;
        private readonly List<GameObject> _points = new List<GameObject>();
        [SerializeField] private float pullingForce = 3f;
        private Vector2 _pullingDirection;
        private bool _isPullingLever = false; // Flag to track lever pulling

        // LineRenderer configuration
        private LineRenderer _lineRenderer;
        [Header("Line Renderer Settings")]
        [SerializeField] private float lineStartWidth = 0.4f;
        [SerializeField] private float lineEndWidth = 0.2f;
        [SerializeField] private Color lineStartColor = Color.cyan;
        [SerializeField] private Color lineEndColor = Color.blue;
        [SerializeField] private GameObject amebaPrefab;
    
        // Shooting parameters
        private bool _canShoot = true;
        private float timer = 0f;
        [SerializeField] private float shootCooldown = 5f;

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
            TryMove();

            TryRenderLine();
        
            TryShoot();

            TickTok();
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
            if (_canShoot && _pullingDirection.magnitude > 0.5f)
            {
                Debug.Log("Shoot!");
                _canShoot = false;
                timer = 0f;
                SpawnAmeba();
            }
        }

        private void TryRenderLine()
        {
            // Update LineRenderer to show the ray
            if (_isPullingLever)
            {
                UpdateTriangle();
            }
            else
            {
                _lineRenderer.enabled = false;
            }
        }

        private void TryMove()
        {
            // Handle player movement
            _rb.linearVelocity = _movement * speed;
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
            _triangleInstance.transform.position = transform.position + (Vector3)(_pullingDirection.normalized * triangleSize);
            float angle = Mathf.Atan2(_pullingDirection.y, _pullingDirection.x) * Mathf.Rad2Deg;
            _triangleInstance.transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        private void SpawnAmeba()
        {
            GameObject ameba = Instantiate(amebaPrefab, transform.position, Quaternion.identity);
            // attach the ameba to the player using fixed joint
            FixedJoint2D joint = ameba.AddComponent<FixedJoint2D>();
            joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();
            StartCoroutine(ShootAmeba(ameba));
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

        #endregion
    }
}
