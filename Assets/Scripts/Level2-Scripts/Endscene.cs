using UnityEngine;

public class EndSceneTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"[Trigger DEBUG] Entered by {col.name}, Tag: {col.tag}");
    }
}
