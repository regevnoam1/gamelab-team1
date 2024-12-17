using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BaseEnemyController : MonoBehaviour
{
    private GameObject player; // Reference to the player GameObject

    [SerializeField] private float enemySpeed = 2f; // Speed at which the enemy moves
    [SerializeField] private GameObject deathEffect; // Particle effect to play when the enemy dies
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player"); // Find the player GameObject by name
    }

    // Update is called once per frame
    void Update()
    {
        TryMoveTowardsPlayer(); // Attempt to move towards the player
    }

    private void TryMoveTowardsPlayer()
    {
        if (player == null) // If the player GameObject is null
            return; // Exit the method

        // Compute the direction from the enemy to the player
        var direction = player.transform.position - transform.position;

        // Normalize the direction to have a magnitude of 1
        direction.Normalize();

        // Move the enemy towards the player using moveTowards
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, enemySpeed * Time.deltaTime);
        
    }
    
    // OnCollisionEnter is called when this collider/rigidbody has begun touching another collider/rigidbody
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ameba")) // If the other GameObject has the "Player" tag
        {
            // make rigidbody kinematic
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            Die(); // Call the Die method
        }
    }

    private void Die()
    {
        // Play the death effect at the enemy's position
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        // turn off the enemy sprite
        GetComponent<SpriteRenderer>().enabled = false;
        
        // Destroy the enemy GameObject
        StartCoroutine(DelayDestroy());
        StartCoroutine(EnlargeLightRadius());
    }

    private IEnumerator EnlargeLightRadius()
    {
        // Get the Light2D component attached to the enemy
        var light = GetComponent<Light2D>();
        
        // Increase the light's radius over time
        while (light.pointLightOuterRadius < 10)
        {
            light.pointLightOuterRadius += 0.1f;
            yield return null;
        }
    }

    private IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
