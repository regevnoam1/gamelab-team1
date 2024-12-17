using UnityEngine;

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
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ameba")) // If the other GameObject has the "Player" tag
        {
            Die(); // Call the Die method
        }
    }

    private void Die()
    {
        // Play the death effect at the enemy's position
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        // Destroy the enemy GameObject
        Destroy(gameObject);
    }
}
