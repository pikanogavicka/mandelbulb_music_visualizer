using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsInGame : MonoBehaviour
{
    public void LoadOptions()
    {
        SceneManager.LoadScene("StartMenu");
    }

}
