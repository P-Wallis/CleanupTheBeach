using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeachSceneLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("Beach Scene", LoadSceneMode.Additive);
    }
}
