using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.TryAgainGameState();
            
        }

    }
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        if (sceneIndex == 4 && ScoreManager.Instance != null)
            ScoreManager.Instance.TryAgainGameState();
    }
}