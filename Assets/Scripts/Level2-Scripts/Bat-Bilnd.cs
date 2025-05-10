#region BlindnessEffect.cs
using System.Collections;
using UnityEngine;
/// <summary>
/// 致盲效果：将主摄像机 CullingMask 临时切换为仅渲染 SlimeLayer。
/// </summary>
public class BatBlind : MonoBehaviour
{
    [Tooltip("Slime 所在层 (仅保留此层)")]
    public string slimeLayerName = "Player";

    Camera mainCam;
    int slimeLayerMask;
    int originalMask;
    bool active = false;

    void Awake()
    {
        mainCam = Camera.main;
        slimeLayerMask = 1 << LayerMask.NameToLayer(slimeLayerName);
    }

    public void Blind(float duration)
    {
        if (active) return;
        StartCoroutine(CoBlind(duration));
    }

    IEnumerator CoBlind(float duration)
    {
        active = true;
        originalMask = mainCam.cullingMask;
        mainCam.cullingMask = slimeLayerMask;

        yield return new WaitForSeconds(duration);

        mainCam.cullingMask = originalMask;
        active = false;
    }
}

#endregion