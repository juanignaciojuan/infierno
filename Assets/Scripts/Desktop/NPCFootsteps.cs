using UnityEngine;

public class NPCFootsteps : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource stepSource;           // El AudioSource asignado al personaje
    public AudioClip[] stepClips;            // Lista de clips de pasos (diferentes sonidos)

    [Range(0.8f, 1.2f)]
    public float pitchVariation = 0.1f;      // Pequeña variación de pitch para naturalidad

    // Este método lo llamás desde el Animation Event en el frame donde el pie toca el suelo
    public void PlayStepSound()
    {
        if (stepClips.Length == 0 || stepSource == null)
        {
            Debug.LogWarning("[NPCFootsteps] No hay clips o AudioSource asignado.");
            return;
        }

        // Elegir un clip aleatorio de la lista
        int index = Random.Range(0, stepClips.Length);

        // Variar un poco el pitch para que no suene repetitivo
        stepSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        // Reproducir el sonido
        stepSource.PlayOneShot(stepClips[index]);
    }
}
