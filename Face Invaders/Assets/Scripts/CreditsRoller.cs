using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsRoller : MonoBehaviour
{
    public float speed = 150f;
    public RectTransform creditsText;
    public float creditsDuration = 5f;

    void Start()
    {
        Invoke("LoadMainMenu", creditsDuration);
    }

    void Update()
    {
        creditsText.transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
