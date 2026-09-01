using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraShake : MonoBehaviour
{

    // We need:

    // CameraShake struct - public struct containing magnitude and duration

    // ActiveShake struct - containing Magnitude, Duration and TimeRemaining

    // _activeShakes list - a list of ActiveShakes

    // AddShake method - this is public, gets called from the player, and uses the inputted CameraShake to make a new ActiveShake, and add that ActiveShake to the _activeShakes list

    // UpdateShake method - updated by the player, this goes through each ActiveShake in the _activeShakes list. At the start of the update it will generate a shake offset, either via random number or with some kinda noise like Perlin.
    // It goes through each of them and gets their strength - shake.magnitude * (shake.timeRemaining /shake.duration). Then it gets their contribution to the camera shake - strength * randomOffset.
    // Then, it minuses deltaTime from their time remaining, and if time remaining is less than 0, it removes them from the list of activeShakes.  

    [System.Serializable]
    public struct CamShake
    {
        public float Magnitude;
        public float Duration;

        public CamShake(float magnitude, float duration)
        {
            Magnitude = magnitude;
            Duration = duration;
        }
    }

    private class ActiveShake
    {
        public float Magnitude;
        public float Duration;
        public float TimeRemaining;
    }

    private readonly List<ActiveShake> _activeShakes = new();

    public void AddShake(CamShake shake)
    {
        _activeShakes.Add(new ActiveShake
        {
            Magnitude = shake.Magnitude,
            Duration = shake.Duration,
            TimeRemaining = shake.Duration
        });
    }

    public void UpdateShake(float deltaTime)
    {
        Vector3 finalShake = Vector3.zero;
        var shakeOffset = Random.insideUnitCircle;
        for (int i = _activeShakes.Count - 1; i >= 0; i--)
        {
            var shake = _activeShakes[i];

            shake.TimeRemaining -= deltaTime;
            if (shake.TimeRemaining <= 0 )
            {
                _activeShakes.RemoveAt(i);
                continue;
            }

            float fade = shake.TimeRemaining / shake.Duration;
            float strength = shake.Magnitude * fade;
            Vector3 thisShake = strength * shakeOffset;
            finalShake += thisShake;
        }
        transform.localPosition = finalShake;
    }
}
