using UnityEngine;

public class LavaProtect : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var slime = other.GetComponent<Level2SlimeController>();
            if (slime != null)
            {
                slime.immuneToLava = true;
                Debug.Log("[FireSuit] 玩家已获得岩浆免疫！");
            }

            Destroy(gameObject);  // 拾取后销毁防火服
        }
    }
}
