using UnityEngine;

public class GameManagerL2 : MonoBehaviour
{
    public static GameManagerL2 Instance;

    [Header("检查点列表 (按顺序排列)")]
    public Transform[] checkpoints;

    [Header("当前激活的检查点索引")]
    public int currentCheckpointIndex = -1;  // -1 表示未激活任何检查点

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 更新当前检查点索引，只允许向前推进。
    /// </summary>
    public void UpdateCheckpoint(Transform checkpoint)
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == checkpoint)
            {
                if (i > currentCheckpointIndex)
                {
                    currentCheckpointIndex = i;
                    Debug.Log($"[GameManagerL2] 当前检查点更新为 Index: {currentCheckpointIndex}");
                }
                else
                {
                    Debug.Log($"[GameManagerL2] 已到达过更远的检查点，忽略 {checkpoint.name}");
                }
                return;
            }
        }
        Debug.LogWarning("[GameManagerL2] 未在列表中找到此检查点！");
    }

    /// <summary>
    /// 获取当前有效检查点的 Transform。
    /// </summary>
    public Transform GetCurrentCheckpoint()
    {
        if (checkpoints.Length == 0 || currentCheckpointIndex < 0)
        {
            Debug.LogWarning("[GameManagerL2] 没有有效的检查点！");
            return null;
        }
        return checkpoints[Mathf.Clamp(currentCheckpointIndex, 0, checkpoints.Length - 1)];
    }
}
