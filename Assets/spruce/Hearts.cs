using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hearts : MonoBehaviour
{
    [SerializeField] private Color heartFull;
    [SerializeField] private Color heartEmpty;

    [SerializeField] private int heartOrderIndex;

    [SerializeField] private BossHealth bossHealth;

    [SerializeField] private HeartBeat heartBeat;

    void Update()
    {
        int currentHearts = bossHealth.CurrentHearts;

        if (heartOrderIndex < currentHearts)
        {
            GetComponent<Image>().color = heartFull;
        }
        else
        {
            GetComponent<Image>().color = heartEmpty;
            heartBeat.beatsPerMinute = 0;
        }
        
    }
}
