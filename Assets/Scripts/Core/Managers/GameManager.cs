using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Fields

        private static GameManager _instance;
        public static GameManager Instance => _instance;
        
        private GameObject player; // Reference to the player GameObject
        
        [SerializeField] private Light2D playerLight; // Reference to the player's light
        [SerializeField] private AudioSource heartbeat; // Reference to the heartbeat sound
        private float _beatingTime = 10f; // Time between heartbeats
        private bool _beating = false;

        #endregion
    
        #region MonoBehaviour Callbacks
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            player = GameObject.Find("Player"); // Find the player GameObject by name
        }
        
        private void Update()
        {
            if (playerLight.pointLightOuterRadius >= 35)
            {
                _beating = true;
            }

            if (_beating)
            {
                if (_beatingTime >= 8)
                {
                    StartCoroutine(HeartBeetSound());
                    _beatingTime = 0;
                }
                _beatingTime += Time.deltaTime;
            }
        }

        private IEnumerator HeartBeetSound()
        {
            StartCoroutine(StartHeartbeatSound());
            yield return new WaitForSeconds(3f);
            StartCoroutine(StopHeartbeatSound());
        }

        private IEnumerator StartHeartbeatSound()
        {
            while (heartbeat.volume <= 1)
            {
                heartbeat.volume += 0.1f;
                yield return null;
            }
            heartbeat.Play();
        }
        
        private IEnumerator StopHeartbeatSound()
        {
            while (heartbeat.volume > 0)
            {
                heartbeat.volume -= 0.1f;
                yield return null;
            }
            heartbeat.Stop();
        }

        #endregion

        #region Player Controlls

        public void EnlargeLightRadius()
        {
            StartCoroutine(ChangeRadius(0.5f));
        }
        
        public void DecreaseLightRadius()
        {
            StartCoroutine(ChangeRadius(-0.5f));
        }
    
        #endregion

        #region Other Methods

        private IEnumerator ChangeRadius(float f)
        {
            var initialRadius = playerLight.pointLightOuterRadius;
            while (Mathf.Abs(playerLight.pointLightOuterRadius - (initialRadius + f)) >= 0.01f)
            {
                playerLight.pointLightOuterRadius += Time.deltaTime * f;
                yield return null;
            }
        }

        #endregion
    
    }
}
