using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Utils
{
    public class AmebaNoise : MonoBehaviour
    {
        [SerializeField] private float scale = 1.0f;        // Base scale factor
        [SerializeField] private float noiseIntensity = 0.5f; // Intensity of Perlin Noise
        [SerializeField] private float sinCosSpeed = 2.0f;  // Speed of Sin/Cos distortion
        [SerializeField] private float sinCosAmplitude = 0.2f; // Amplitude of the distortion effect

        private float noiseOffset;                         // Noise offset for variation

        void Start()
        {
            noiseOffset = Random.Range(0f, 100f); // Initialize Perlin noise offset
            StartCoroutine(DelayDestroy());
        }

        void Update()
        {
            float time = Time.time;

            // Generate Perlin Noise for smooth scaling
            float noise = Mathf.PerlinNoise(noiseOffset, time * 0.5f) * noiseIntensity + 1.0f;

            // Apply Sin and Cos distortions to simulate "ink-like" fluctuations
            float sinWave = Mathf.Sin(time * sinCosSpeed) * sinCosAmplitude;
            float cosWave = Mathf.Cos(time * sinCosSpeed * 0.75f) * sinCosAmplitude;

            // Calculate the final scale
            float finalScaleX = scale * noise + sinWave;
            float finalScaleY = scale * noise + cosWave;

            // Update the object's scale
            transform.localScale = new Vector3(finalScaleX, finalScaleY, scale);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                Destroy(gameObject);
            }
        }
        
        private IEnumerator DelayDestroy()
        {
            yield return new WaitForSeconds(0.6f);
            if (gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude < 0.5f)
            {
                Destroy(gameObject);
            }
        }
    }
}