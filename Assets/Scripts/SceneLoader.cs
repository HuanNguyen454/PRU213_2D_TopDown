using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SenceLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Scene";

    public void LoadGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
