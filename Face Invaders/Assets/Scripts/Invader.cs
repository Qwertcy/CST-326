using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Invader : MonoBehaviour
{
    public Sprite[] animationSprites; //keeps track of array of sprites to cycle between them
    public float animationTime = 1.0f; //how often cycles to the next sprite
    private SpriteRenderer _spriteRenderer; //to change which sprite is being rendered
    private int _animationFrame; //keeps track which sprite is being used

    public AudioClip deathSound;
    private AudioSource _audioSource;

    public System.Action killed;

    public GameObject explosionPrefab;

    public int scoreValue;

    private bool isDying = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(AnimateSprite), this.animationTime, this.animationTime);
    }

    private void AnimateSprite()
    {
        if (isDying) return;

        _animationFrame++;
        if (_animationFrame >= animationSprites.Length - 1)
        {
            _animationFrame = 0;
        }

        _spriteRenderer.sprite = this.animationSprites[_animationFrame];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Laser"))
        {
            ScoreManager.instance.AddPoints(scoreValue);
            this.killed?.Invoke(); // Null-safe invoke

            _audioSource.PlayOneShot(deathSound);

            Destroy(other.gameObject);

            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2.0f);

            isDying = true;
            CancelInvoke(nameof(AnimateSprite));

            _spriteRenderer.sprite = this.animationSprites[animationSprites.Length - 1];
            GetComponent<Collider2D>().enabled = false; // disables collision
            

            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(0.5f);
        _spriteRenderer.enabled = false; //hides sprite
        yield return new WaitForSeconds(deathSound.length - 0.5f);
        this.gameObject.SetActive(false);
    }

}
