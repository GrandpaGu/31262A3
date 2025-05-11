using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RocketLaunch : MonoBehaviour
{
    [Header("Timing")]
    public float startDuration = 2f;
    public float launchSpeed = 6f;

    [Header("References")]
    public Animator fireAnimator;
    public CanvasGroup whiteOverlay;
    public TextMeshPro endingText;

    [Header("Slime Sync")]
    public Transform slime;
    public float slimeYOffset = -0.5f;
    public MonoBehaviour[] controlScripts;

    [Header("Scene")]
    public string nextSceneName;

    bool _ascending = false;

    void Start()
    {
        whiteOverlay.alpha = 0f;
        endingText.gameObject.SetActive(false);
        // 移除自动发射，等待触发调用 StartLaunch()
    }

    void Update()
    {
        if (_ascending)
        {
            transform.position += Vector3.up * launchSpeed * Time.deltaTime;

            if (slime)
            {
                Vector3 s = slime.position;
                s.y = transform.position.y + slimeYOffset;
                slime.position = s;
            }
        }
    }

    public void StartLaunch()
    {
        StartCoroutine(LaunchRoutine());
    }

    System.Collections.IEnumerator LaunchRoutine()
    {
        foreach (var s in controlScripts) if (s) s.enabled = false;

        // 锁定摄像机
        var camControl = Camera.main.GetComponent<CameraFollowYOnlyL2>();
        if (camControl) camControl.lockCamera = true;

        fireAnimator.Play("Start");
        yield return new WaitForSeconds(startDuration);

        fireAnimator.Play("Loop");
        _ascending = true;

        while (transform.position.y < Camera.main.transform.position.y + 5f)
            yield return null;

        while (whiteOverlay.alpha < 1f)
        {
            whiteOverlay.alpha += Time.deltaTime * 0.5f;
            yield return null;
        }

        endingText.text = "我在天空四处张望，这里没有什么上帝和救世主，只有地球和他的人民";
        endingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);

        SceneManager.LoadScene(nextSceneName);
    }
}
