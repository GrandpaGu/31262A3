using UnityEngine;
using UnityEngine.Audio;

public class MushroomBounceL3 : MonoBehaviour
{
    public float bounceForce = 12f;
    public float squishAmount = 0.8f;
    public float squishTime = 0.1f;

    private AudioSource audioSource;
    public AudioClip bounce;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (collision.contacts[0].normal.y < -0.5f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                    rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
                    StartCoroutine(SquashEffect());

                    PlayBounceSound();

                }
            }
        }
    }

    void PlayBounceSound()
    {
        if (audioSource != null && bounce != null)
        {
            audioSource.clip = bounce;
            audioSource.time = 0.05f;
            audioSource.volume = 1f;
            audioSource.Play();
        }
    }








    System.Collections.IEnumerator SquashEffect()
    {
        // 压缩并下移蘑菇顶部，但底部不动
        float offsetY = originalScale.y * (1f - squishAmount) * 0.5f;
        transform.localScale = new Vector3(originalScale.x, originalScale.y * squishAmount, originalScale.z);
        transform.position = originalPosition - new Vector3(0f, offsetY, 0f);

        yield return new WaitForSeconds(squishTime);

        transform.localScale = originalScale;
        transform.position = originalPosition;
    }
}
