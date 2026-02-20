using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI coins;
    public int coinCount = 0;

    void Start()
    {
        //checking camera
        if (Camera.main == null)
        {
            Debug.LogError("No Main Camera found! Make sure your camera is tagged as 'MainCamera'.");
        }
    }

    void Update()
    {
        int timeLeft = 300 - (int)(Time.time);
        timerText.text = timeLeft.ToString();

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Camera cam = Camera.main;
        //    if (cam == null)
        //    {
        //        Debug.LogError("Camera.main is null. Check your camera tag.");
        //        return;
        //    }

        //    //ray from camera to mouse position
        //    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //    Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f); //visualising ray in scene view

        //    if (Physics.Raycast(ray, out RaycastHit hit))
        //    {
        //        GameObject hitObject = hit.collider.transform.root.gameObject;
        //        Debug.Log("Hit object tag: " + hitObject.tag);
        //        Debug.Log("Ray hit: " + hit.collider.gameObject.name);
        //        if (hit.collider.CompareTag("Rock"))
        //        {
        //            Destroy(hitObject);
        //        }
        //        else if (hit.collider.CompareTag("Question"))
        //        {
        //            coinCount += 1;
        //            coins.text = ": " + coinCount.ToString();
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("Raycast did not hit any object.");
        //    }
        //}
    }

    public void AddCoin()
    {
                    coinCount += 1;
                    coins.text = ": " + coinCount.ToString();
    }
    public void EndGame()
    {
        Debug.Log("Game Over - Player fell into a pit!");

//stops play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

}
