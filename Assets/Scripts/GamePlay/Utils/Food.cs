using Core.Managers;
using UnityEngine;

public class Food : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Node") || other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance._attackLevel++;
            Destroy(gameObject);
        }
    }
}
