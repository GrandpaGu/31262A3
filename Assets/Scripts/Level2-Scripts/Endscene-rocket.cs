using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RocketLaunch : MonoBehaviour
{
    [Header("Timing")]
    public float startDuration = 2f;
    public float launchSpeed = 6f;

    [Header("References")]
    public GameObject rocket;               // 拖 Slime 子物体 Rocket
    public Animator rocketAnimator;         // 拖 Rocket 的 Animator
    public CanvasGroup whiteOverlay;
    public TextMeshProUGUI endingText;

    [Header("Slime Control")]
    public Transform slime;  // ← 新增这个！
    public MonoBehaviour[] controlScripts;
    public BackgroundVerticalMapping backgroundScript;


    [Header("Scene")]
    public string nextSceneName;

    bool _ascending = false;

    public void StartLaunch()
    {
        foreach (var s in controlScripts) if (s) s.enabled = false;
        if (backgroundScript) backgroundScript.enabled = false;

        if (rocket) rocket.SetActive(true);
        if (rocketAnimator) rocketAnimator.Play("Start");

        // ✅ 锁定摄像机
        CameraFollowYOnlyL2 camControl = Camera.main.GetComponent<CameraFollowYOnlyL2>();
        if (camControl) camControl.lockCamera = true;

        // ✅ 禁用 Slime 的物理组件防止卡住
        Collider2D slimeCol = slime.GetComponent<Collider2D>();
        if (slimeCol) slimeCol.enabled = false;

        Rigidbody2D slimeRb = slime.GetComponent<Rigidbody2D>();
        if (slimeRb) slimeRb.simulated = false;

        StartCoroutine(LaunchRoutine());
    }

    void Update()
    {
        if (_ascending)
        {
            transform.position += Vector3.up * launchSpeed * Time.deltaTime;
        }
    }

    System.Collections.IEnumerator LaunchRoutine()
    {
        yield return new WaitForSeconds(startDuration);

        if (rocketAnimator) rocketAnimator.Play("Loop");
        _ascending = true;

        while (transform.position.y < Camera.main.transform.position.y + 5f)
            yield return null;

        Debug.Log("[Fade] 开始渐变...");

        while (whiteOverlay.alpha < 1f)
        {
            whiteOverlay.alpha += Time.deltaTime * 0.5f;
            yield return null;
        }

        Debug.Log("[Fade] 渐变完成！");

        endingText.text = "I wandered the skies in search of gods and saviors, but found only the Earth and its people.";
        endingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(nextSceneName);
    }
}
