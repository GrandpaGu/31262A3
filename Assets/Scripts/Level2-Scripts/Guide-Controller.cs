using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuideController : MonoBehaviour
{
    [Header("References")]
    public GameObject guidePanel;         // 拖入 GuidePanel
    public TextMeshProUGUI buttonLabel;   // 拖入 GuideButton 上的 TextMeshPro

    bool isGuideOpen = false;

    public void ToggleGuide()
    {
        isGuideOpen = !isGuideOpen;
        guidePanel.SetActive(isGuideOpen);

        // 切换按钮上的文本内容
        buttonLabel.text = isGuideOpen ? "Close" : "GuideBook";
    }
}