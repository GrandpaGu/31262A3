using UnityEngine;

public class WaterproofItem : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        var slime = col.GetComponent<Level2SlimeController>();
        if (slime != null)
        {
            slime.hasWaterproofItem = true;
            Debug.Log("【奖励】获得防水道具，从此不再溺水！");
        }

        // 道具被拾取后销毁
        Destroy(gameObject);
    }
}
