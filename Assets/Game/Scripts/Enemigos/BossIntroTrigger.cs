using UnityEngine;

public class BossIntroTrigger : MonoBehaviour
{
    public Mother boss;              // Arrastra aquí el GameObject que tiene el script Mother
    public bool destroyAfterUse = true;

    private bool alreadyTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger detecta a: " + other.name);

        if (alreadyTriggered) return;
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = true;
        Debug.Log("El PLAYER entró al trigger del boss");

        if (boss != null)
        {
            boss.ShowBossIntro();
        }
        else
        {
            Debug.LogWarning("No se asignó el boss en BossIntroTrigger");
        }

        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
    }
}
