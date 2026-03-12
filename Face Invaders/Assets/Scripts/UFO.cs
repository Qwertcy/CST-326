using UnityEngine;
using System.Collections;

public class UFO : MonoBehaviour
{
    public float speed = 5.0f;
    private bool movingRight = true; //direction tracker

    private int scoreValue => Random.Range(20, 51) * 10;

    public AudioClip runningSound;
    public AudioClip deathSound;
    private AudioSource _audioSource;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }


    private void Start()
    {
        StartCoroutine(UFOMovementLoop()); //continously manages ufo spawning and movement
    }

    private IEnumerator UFOMovementLoop() //allows pausing and resuming execution over time
    {
        while (true)
        {
            yield return new WaitForSeconds(15.0f);
            // determine spawn position
            _audioSource.PlayOneShot(runningSound);
            float spawnX = movingRight ? Camera.main.ViewportToWorldPoint(new Vector3(-0.1f, 0, 0)).x //gets position slightly outside the screen to the left
                                       : Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0, 0)).x; //gets position slightly outside the screen to the right

            // starting position
            transform.position = new Vector3(spawnX, Camera.main.ViewportToWorldPoint(new Vector3(0, 0.8f, 0)).y, 0); //spawning near the top of the screen

            // moves ufo across the screen
            float targetX = movingRight ? Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0, 0)).x
                                        : Camera.main.ViewportToWorldPoint(new Vector3(-0.1f, 0, 0)).x;

            float direction = movingRight ? 1 : -1;

            while ((movingRight && transform.position.x < targetX) || (!movingRight && transform.position.x > targetX))
            {
                transform.position += Vector3.right * direction * speed * Time.deltaTime;
                yield return null;
            }

            // waits 15 secs before spawning another
            yield return new WaitForSeconds(15.0f);

            // chenges direction for next appearance
            movingRight = !movingRight;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Laser"))
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.4f);
            ScoreManager.instance.AddPoints(scoreValue);
            this.gameObject.SetActive(false);

        }
    }
}
