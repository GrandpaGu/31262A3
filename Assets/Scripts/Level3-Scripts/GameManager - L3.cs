using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerL3 : MonoBehaviour
{
    public static GameManagerL3 Instance;

    [Header("玩家对象 (拖拽 Slime)")]
    public SlimeControllerL3 player;          // 在 Inspector 把 Slime 拖进来

    [HideInInspector] public bool isInputLocked = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /* 终点触发时由 GoalTrigger 调用 */
    public void PlayerReachedFinish()
    {
        if (isInputLocked) return;          // 防止重复调用
        isInputLocked = true;

        // 记录玩家 Rigidbody，用于自然减速
        if (player != null) player.BeginNaturalStop();

        StartCoroutine(RestartAfterDelay(5f));
    }

    System.Collections.IEnumerator RestartAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}