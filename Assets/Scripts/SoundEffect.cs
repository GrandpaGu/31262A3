using UnityEngine;
using UnityEngine.Audio;

public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip bat;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
           PlayBatSound();
        }
    }

    void PlayBatSound()
    {
        if (audioSource != null && bat != null)
        {
            audioSource.clip = bat;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
    }


}
