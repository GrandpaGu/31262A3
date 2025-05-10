using UnityEngine;

public class GameManagerL2 : MonoBehaviour
{
    public static GameManagerL2 Instance;

    [Header("当前有效检查点编号")]
    public int currentCheckpointIndex = -1;  // -1 表示未经过任何检查点

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>更新当前检查点，只接受编号更大的检查点。</summary>
    public void UpdateCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex > currentCheckpointIndex)
        {
            currentCheckpointIndex = checkpointIndex;
            Debug.Log($"[GameManager] 已更新当前检查点为：{currentCheckpointIndex}");
        }
        else
        {
            Debug.Log($"[GameManager] 当前检查点已是 {currentCheckpointIndex}，忽略编号较小的检查点 {checkpointIndex}");
        }
    }
}
