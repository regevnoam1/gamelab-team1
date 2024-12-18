using System.Collections;
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
        #endregion

        #region Player Controlls

        public void EnlargeLightRadius()
        {
            var light2D = player.GetComponentInChildren<Light2D>();
            StartCoroutine(ChangeRadius(light2D, 0.2f));
        }
        
        public void DecreaseLightRadius()
        {
            var light2D = player.GetComponentInChildren<Light2D>();
            StartCoroutine(ChangeRadius(light2D, -0.2f));
        }
    
        #endregion

        #region Other Methods

        private IEnumerator ChangeRadius(Light2D light2D, float f)
        {
            var initialRadius = light2D.pointLightOuterRadius;
            while (light2D.pointLightOuterRadius < initialRadius + f)
            {
                light2D.pointLightOuterRadius += Time.deltaTime;
                yield return null;
            }
        }

        #endregion
    
    }
}
