using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public void Restart()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.currentScore = 0;
        }
        SceneManager.LoadScene(4);
    }
}
