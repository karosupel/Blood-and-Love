using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    public GameObject[] popUps;
    private int popUpIndex;

    [SerializeField] private GameObject player;
    [SerializeField] private SpawnerManagerScript spawnerManagerScript;

    public int phase = 1;
    private bool popupsHiddenByPause;
    private int pausedPopupIndex = -1;

    void Start()
    {
        StartCoroutine(TutorialCorotine());
    }

    void Update()
    {
        if (PauseMenuManager.IsPaused)
        {
            HidePopupsForPause();
        }
        else if (popupsHiddenByPause)
        {
            RestorePopupAfterPause();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Phase:"+ phase);
        }
    }

    private IEnumerator waitForKeyPress(KeyCode[] key)
    {
        bool done = false;
        while(!done)
        {
            for(int i = 0; i < key.Length; i++)
            {
                if(Input.GetKeyDown(key[i]))
                {
                    done = true;
                    break;
                }
            }
            yield return null;
        }
    }

    IEnumerator TutorialCorotine()
    {
        if (player.GetComponent<PlayerHealth>().hearts == 0)
        {
            player.GetComponent<PlayerHealth>().hearts = 1;
        }

        while(phase == 1)
        {
            popUpIndex = 0;
            popUps[popUpIndex].SetActive(true);
            yield return waitForKeyPress(new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D });
            yield return new WaitForSeconds(1);
            popUps[popUpIndex].SetActive(false);
            popUpIndex = 1;
            popUps[popUpIndex].SetActive(true);
            phase = 2;
            yield return new WaitForSeconds(1);
            popUps[popUpIndex].SetActive(false);
            popUpIndex = 2;
        }

        while(phase == 2)
        {
            yield return null;
        }

        while(phase == 3)
        {
            popUpIndex = 2;
            popUps[popUpIndex].SetActive(true);
            yield return new WaitForSeconds(1);
            popUps[popUpIndex].SetActive(false);
            popUpIndex = 3;
            popUps[popUpIndex].SetActive(true);
            yield return new WaitForSeconds(1);
            popUps[popUpIndex].SetActive(false);
            if (!player.GetComponent<PlayerHealth>().IsInAfterlife)
            {
                phase = 4;
                popUpIndex = 4;
            }
        }

        while(phase == 4)
        {
                popUps[popUpIndex].SetActive(true);
                yield return new WaitForSeconds(1);
                popUps[popUpIndex].SetActive(false);
                popUpIndex = 5;
                player.GetComponent<PlayerHealth>().Heal(100f);
                phase = 5;
            
                yield return null;
            }

        while(phase == 5)
        {
            popUps[popUpIndex].SetActive(true);
            yield return new WaitForSeconds(1);
            if (spawnerManagerScript.GetActiveEnemiesInScene().Count > 0)
            {
                yield return new WaitForSeconds(1);
                player.GetComponent<PlayerHealth>().Heal(100f);
            }
            else
            {
                popUps[popUpIndex].SetActive(false);
                phase = 0;
                yield return null;
            }
        }
    }

    private void HidePopupsForPause()
    {
        if (popUps == null || popUps.Length == 0)
        {
            return;
        }

        if (!popupsHiddenByPause)
        {
            pausedPopupIndex = -1;
            for (int i = 0; i < popUps.Length; i++)
            {
                if (popUps[i] != null && popUps[i].activeSelf)
                {
                    pausedPopupIndex = i;
                    break;
                }
            }
        }

        for (int i = 0; i < popUps.Length; i++)
        {
            if (popUps[i] != null && popUps[i].activeSelf)
            {
                popUps[i].SetActive(false);
            }
        }

        popupsHiddenByPause = true;
    }

    private void RestorePopupAfterPause()
    {
        if (popUps == null || popUps.Length == 0)
        {
            popupsHiddenByPause = false;
            pausedPopupIndex = -1;
            return;
        }

        if (pausedPopupIndex >= 0 && pausedPopupIndex < popUps.Length && popUps[pausedPopupIndex] != null)
        {
            popUps[pausedPopupIndex].SetActive(true);
            popUpIndex = pausedPopupIndex;
        }

        popupsHiddenByPause = false;
        pausedPopupIndex = -1;
    }
}

