using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelParser : MonoBehaviour
{
    public string filename; // file name inside assets/resources without .txt
    public Transform environmentRoot; // parent transform for spawned tiles

    [Header("Block Prefabs")]
    public GameObject rockPrefab; // spawned for 'b'
    public GameObject brickPrefab; // spawned for 'x'
    public GameObject questionBoxPrefab; // spawned for '?'
    public GameObject stonePrefab; // spawned for 's'
    public GameObject waterPrefab; // spawned for 'w'
    public GameObject goalPrefab; // spawned for 'g'

    [Header("Parsing Settings")]
    public int tabWidth = 4; // how many spaces a tab should count as when converting '\t' to spaces

    void Start()
    {
        LoadLevel(); // builds the level from the text file
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) //r to reload level
        {
            ReloadLevel();
        }
    }

    void LoadLevel()
    {
        string fileToParse = $"{Application.dataPath}/Resources/{filename}.txt"; // absolute path
        Debug.Log($"Loading level file: {fileToParse}");

        Stack<string> levelRows = new Stack<string>(); // stack reverses order so last file line becomes top row

        using (StreamReader sr = new StreamReader(fileToParse))
        {
            while (sr.ReadLine() is { } rawLine)
            {
                string normalized = NormalizeLinePreserveLeadingSpaces(rawLine); // keeps leading spaces but fixes tabs/trailing junk
                levelRows.Push(normalized); // pushing so we build from bottom row upward
            }
        }

        int row = 0; // world y row index starting from bottom

        while (levelRows.Count > 0)
        {
            string currentLine = levelRows.Pop();
            char[] letters = currentLine.ToCharArray();

            for (int col = 0; col < letters.Length; col++)
            {
                char tile = letters[col];

                if (tile == ' ') continue;

                Vector3 pos = new Vector3(col + 0.5f, row + 0.5f, 0f); // centers tile within 1x1 grid cell

                if (tile == 'x') // brick
                {
                    Spawn(brickPrefab, pos);
                }
                else if (tile == 's') // stone
                {
                    Spawn(stonePrefab, pos);
                }
                else if (tile == 'b') // rock
                {
                    Spawn(rockPrefab, pos);
                }
                else if (tile == '?') // question
                {
                    Spawn(questionBoxPrefab, pos);
                }
                else if (tile == 'w') // water
                {
                    Spawn(waterPrefab, pos);
                }
                else if (tile == 'g') // goal
                {
                    Spawn(goalPrefab, pos);
                }

                else
                {
                }
            }

            row++;
        }
    }

    string NormalizeLinePreserveLeadingSpaces(string rawLine) // normalizes input while preserving indentation-based level layout
    {
        if (rawLine == null) return string.Empty; // safety for unexpected null
        string noTabs = rawLine.Replace("\t", new string(' ', tabWidth)); // converts tabs into fixed-width spaces to prevent misalignment
        return noTabs.TrimEnd('\r', ' ', '\t'); // removes trailing whitespace that only adds invisible extra columns
    }

    void Spawn(GameObject prefab, Vector3 position) // helper for consistent instantiation
    {
        if (prefab == null) return; // prevents null reference if prefab not assigned
        GameObject obj = Instantiate(prefab, environmentRoot); // instantiates and parents under environmentRoot
        obj.transform.position = position; // places at grid-derived world position
    }

    void ReloadLevel() // clears existing tiles then rebuilds
    {
        foreach (Transform child in environmentRoot) // iterate all spawned children
        {
            Destroy(child.gameObject);
        }

        LoadLevel();
    }
}