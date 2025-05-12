using UnityEngine;
using UnityEngine.SceneManagement;

public class Level4Finishline : MonoBehaviour
{
    public Animator transitionAnimator;
    public string nextSceneName;  // ← 改成字符串类型

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            transitionAnimator.SetTrigger("FadeOut");
            Invoke(nameof(LoadNextScene), 1f);  // ← 用 nameof 保持代码安全
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
