using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void LoadLevelByName(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
