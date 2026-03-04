using UnityEngine;
using UnityEngine.SceneManagement;

public class Invaders : MonoBehaviour
{
    public Invader[] prefabs;
    public int rows = 5;
    public int cols = 9;

    public float spacing = 0.5f;
    public AnimationCurve speed; //speeds up based on how many killed. uses .Evaluate(num)
    public Projectile missilePrefab;

    public int amountKilled { get; private set; } //public getter, but private setter
    public int amountAlive => this.totalInvaders - amountKilled;
    public int totalInvaders => this.rows * this.cols; //calculated property

    private float percentKilled => (float) this.amountKilled / (float) this.totalInvaders;


    private Vector3 _direction = Vector2.right;

    public float missileAttackRate = 0.5f;

    public AudioClip omgSound;
    private AudioSource _audioSource;

    bool isShooting = true;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
        for (int row = 0; row < this.rows; row++)
        {
            float width = spacing * (this.cols - 1);
            float height = spacing * (this.rows - 1);
            Vector2 centering = new Vector2(-width/2, -height/2);
            Vector3 rowPosition = new Vector3(centering.x, centering.y + (row * spacing), 0.0f);
            for (int col = 0; col < this.cols; col++)
            {
                Invader invader = Instantiate(this.prefabs[row], this.transform);
                invader.killed += InvaderKilled;
                Vector3 position = rowPosition;
                position.x += col * spacing;
                invader.transform.localPosition = position;

            }
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(MissileAttack), this.missileAttackRate, this.missileAttackRate);
    }

    private void Update()
    {
        this.transform.position += _direction * this.speed.Evaluate(this.percentKilled) * Time.deltaTime;

        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        foreach(Transform invader in this.transform)
        {
            if (!invader.gameObject.activeInHierarchy) //skips killed invaders
            {
                continue;
            }
            if(_direction == Vector3.right && invader.position.x >= (rightEdge.x - 0.5f))
            {
                AdvanceRow();
            } else if (_direction == Vector3.left && invader.position.x <= (leftEdge.x + 0.5f))
            {
                AdvanceRow();
            }

        }
    }

    private void AdvanceRow()
    {
        _direction *= -1.0f;

        Vector3 position = this.transform.position;
        position.y -= 0.25f;
        this.transform.position = position;
    }

    private void InvaderKilled()
    {
        this.amountKilled++;
        if (this.amountKilled >= this.totalInvaders)
        {
            SceneManager.LoadScene("Credits");
        }
    }

    private void MissileAttack()
    {
        foreach (Transform invader in this.transform)
        {
            if (!invader.gameObject.activeInHierarchy) //skips killed invaders
            {
                continue;
            }

            if(Random.value < (1.0f/ (float) this.amountAlive)) //chance lower with more enemies
            {
                Instantiate(this.missilePrefab, invader.position, Quaternion.identity); //missile prefab shot from invader
                if (isShooting)
                {
                _audioSource.PlayOneShot(omgSound);
                isShooting = false;
                }
                break; //only 1 missile fired at a time. Fire rate can be adjusted
            }
        }
        isShooting = true;
    }
}
