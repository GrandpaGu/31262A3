using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform slime;

    [Header("Camera Setup")]
    public Camera camStage1;
    public Camera camStage2;
    public Camera camStage3;

    [Header("Slime Spawn Coordinates")]
    public Vector3 spawnPos1 = new Vector3(-6.5f, -1.5f, 0f);
    public Vector3 spawnPos2 = new Vector3(10f, -1.5f, 0f);
    public Vector3 spawnPos3 = new Vector3(24f, -1.5f, 0f);

    [Header("Stage Boundaries")]
    public float stage2ThresholdX = 6f;
    public float stage3ThresholdX = 20f;
    public float endGameThresholdX = 35f;

    private int currentStage = 1;

    void Start()
    {
        // 初始状态，激活第一阶段相机并设置出生位置
        camStage1.enabled = true;
        camStage2.enabled = false;
        camStage3.enabled = false;
        slime.position = spawnPos1;
    }

    void Update()
    {
        float slimeX = slime.position.x;

        if (currentStage == 1 && slimeX > stage2ThresholdX)
        {
            currentStage = 2;
            SwitchStage(camStage2, spawnPos2);
        }
        else if (currentStage == 2 && slimeX > stage3ThresholdX)
        {
            currentStage = 3;
            SwitchStage(camStage3, spawnPos3);
        }
        else if (currentStage == 3 && slimeX > endGameThresholdX)
        {
            EndGame();
        }
    }

    void SwitchStage(Camera targetCamera, Vector3 newSpawnPosition)
    {
        camStage1.enabled = false;
        camStage2.enabled = false;
        camStage3.enabled = false;

        targetCamera.enabled = true;
        slime.position = newSpawnPosition;

        Debug.Log("[GameManager] Switched to Stage " + currentStage);
    }

    public void RespawnSlime()
    {
        switch (currentStage)
        {
            case 1:
                slime.position = spawnPos1;
                break;
            case 2:
                slime.position = spawnPos2;
                break;
            case 3:
                slime.position = spawnPos3;
                break;
        }
        Debug.Log("[GameManager] Slime respawned at Stage " + currentStage);
    }

    void EndGame()
    {
        Debug.Log("[GameManager] Game Complete!");
        // TODO: Add end game UI, transition, etc.
    }
}
