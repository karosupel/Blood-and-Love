using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneTransition : MonoBehaviour
{
    [SerializeField] private PopUpManager popUpManagerScript;

    [SerializeField] private int mainSceneIndex = 1;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && popUpManagerScript.phase == 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainSceneIndex);
        }
    }

}
