using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTransition : MonoBehaviour
{
    [SerializeField] private int bossSceneIndex = 3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                BossFightRestartState.SavePreFightHearts(playerHealth.hearts);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(bossSceneIndex);
        }
    }

}
