using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Core.Managers
{
    public class RumbleManager : MonoBehaviour
    {
        public static RumbleManager instance;
        private Gamepad _pad;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        public void Rumble(float lowFrequency, float highFrequency, float duration)
        {
            _pad = Gamepad.current;
            if (_pad != null)
            {
                _pad.SetMotorSpeeds(lowFrequency, highFrequency);
            }
            StartCoroutine(StopRumble(duration,_pad));
        }
        private IEnumerator StopRumble(float duration, Gamepad pad)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            pad.SetMotorSpeeds(0f,0f);
        }
    }
}