using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{

    [SerializeField] public float maxHealth = 200f;
    public float currentHealth;
    [SerializeField] public int maxHearts = 5;
    [SerializeField] public float afterlifeInvincibilityDuration = 1f;
    int currentHearts;
    bool isInvincible;
    GameObject player;
    BossController bossController;



    bool isInAfterlife = false;
    Vector3 hellOffset = new Vector3(-30f, 0f, 0f);

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentHealth = maxHealth;
        currentHearts = maxHearts;
    }

    public void TakeDamage(float damage, float knockback)
    {
        if (isInvincible)
        {
            return;
        }
        if (!isInAfterlife)
        {
            currentHealth -= damage;
            Debug.Log("Boss HP: " + currentHealth + "/" + maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else
        {
            TakeHeart();
            StartCoroutine(InvincibilityCoroutine(afterlifeInvincibilityDuration));
            bossController.ForceNewBarrier();
        }
    }

    void TakeHeart()
    {
        currentHearts--;
        if (currentHearts < 0)
        {
            Annihilate();
        }
    }

    void Annihilate()
    {
        Debug.Log("Boss has been annihilated! Now, you can live happily, sure that he won't ever come back!");
        Destroy(gameObject);
    }



     IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    public void Die()
    {

        Debug.Log("Boss died! But is he really gone?");
        GoToHell();
    }

    void GoToHell()
    {
        if(isInAfterlife)
        {
            Debug.Log("Boss is already in Hell! He can't go there again!");
            return;
        }

        Vector3 hellPosition = player.transform.position + hellOffset;
        transform.position = hellPosition;
        isInAfterlife = true;
        Debug.Log("Boss has been sent to Hell! But will he stay there?");
    }
}
