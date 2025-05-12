using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Level2SlimeController))]
public class KnockbackReceiver : MonoBehaviour
{
    Level2SlimeController ctrl;
    Rigidbody2D rb;

    void Awake()
    {
        ctrl = GetComponent<Level2SlimeController>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Knock(Vector2 impulse, float lockTime = 0.3f)
    {
        StartCoroutine(DoKnock(impulse, lockTime));
    }

    IEnumerator DoKnock(Vector2 impulse, float lockTime)
    {
        ctrl.StartKnockback(lockTime);  // Æô¶¯»÷ÍË×´Ì¬

        ctrl.enabled = false;
        rb.velocity = Vector2.zero;
        rb.AddForce(impulse, ForceMode2D.Impulse);

        yield return new WaitForSeconds(lockTime);

        ctrl.enabled = true;
    }
}
