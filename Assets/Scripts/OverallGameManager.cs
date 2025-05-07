using UnityEngine;
using UnityEngine.SceneManagement;

public class OverallGameManager : MonoBehaviour
{
    public static OverallGameManager Instance;

    private string mainMenuSceneName = "Menu";

    void Awake()
    {
        // 单例模式，确保 GameManager 持久存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 跨场景保持不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 任意非菜单场景按下 Tab 返回主菜单
        if (SceneManager.GetActiveScene().name != mainMenuSceneName &&
            Input.GetKeyDown(KeyCode.Tab))
        {
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
