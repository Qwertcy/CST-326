using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{
    public Projectile laserPrefab;

    public float speed = 5.0f;

    private bool _laserActive;

    public AudioClip laserSound;
    public AudioClip deathSound;
    private AudioSource _audioSource;

    public GameObject explosionPrefab;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }
    void Update()
    {
        float horizontal = 0f; // stores the horizontal movement direction for this frame

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal = 1f;
        }

        Vector3 position = transform.position;

        position += Vector3.right * horizontal * speed * Time.deltaTime;

        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        float halfWidth = cameraWidth / 2f; // half the width are the left and right camera bounds

        position.x = Mathf.Clamp(position.x, -halfWidth + 0.5f, halfWidth - 0.5f); // clamps player x position so it cannot leave the screen 0.5 is padding

        transform.position = position;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (!_laserActive) 
        { 
        Projectile projectile = Instantiate(this.laserPrefab, this.transform.position, Quaternion.identity);
            projectile.destroyed += LaserDestroyed;
            _laserActive = true;
            _audioSource.PlayOneShot(laserSound);
        }

    }

    private void LaserDestroyed()
    {
        _laserActive = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision detected with: " + other.gameObject.name);

        if (other.gameObject.layer == LayerMask.NameToLayer("Invader") ||
            other.gameObject.layer == LayerMask.NameToLayer("Missile"))
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2.0f);

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            StartCoroutine(DelayedSceneLoad());
        }
    }

    private IEnumerator DelayedSceneLoad()
    {
        _audioSource.PlayOneShot(deathSound);
        yield return new WaitForSeconds(5f);
        Debug.Log("Scene change triggered!");
        SceneManager.LoadScene("Credits");
    }

}
