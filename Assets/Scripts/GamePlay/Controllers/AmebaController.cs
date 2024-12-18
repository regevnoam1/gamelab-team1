using System.Collections;
using Core.Managers;
using UnityEngine;

namespace GamePlay.Controllers
{
    public class AmebaController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(DelayDestroy());
        }
    
        private IEnumerator DelayDestroy()
        {
            yield return new WaitForSeconds(0.6f);
            if (gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude < 3f)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                GameManager.Instance.EnlargeLightRadius();
                Destroy(gameObject);
            }
        }
    }
}
