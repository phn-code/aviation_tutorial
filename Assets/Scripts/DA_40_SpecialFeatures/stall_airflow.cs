using System.Collections;
using UnityEngine;

public class StallVisualTrigger : MonoBehaviour
{
    [SerializeField] private DA_40 aircraft;
    [SerializeField] private ParticleSystem ghostTrail;
    [SerializeField] private float sinkRate = 4f;

    private ParticleSystem.Particle[] particles;

    public void OnStallBegin()
    {
        aircraft.EnableGhostTrail(true);
        StartCoroutine(ApplyStallMotion());
    }

    public void OnStallEnd()
    {
        StopAllCoroutines();
        aircraft.EnableGhostTrail(false);
    }

    private IEnumerator ApplyStallMotion()
    {
        particles = new ParticleSystem.Particle[ghostTrail.main.maxParticles];

        while (true)
        {
            int count = ghostTrail.GetParticles(particles);

            for (int i = 0; i < count; i++)
            {
                float age = 1f - (particles[i].remainingLifetime / particles[i].startLifetime);

                if (age < 0.5f)
                    particles[i].velocity += Vector3.up * 1.5f * Time.deltaTime;
                else
                    particles[i].velocity += Vector3.down * sinkRate * Time.deltaTime;
            }

            ghostTrail.SetParticles(particles, count);
            yield return null;
        }
    }
}