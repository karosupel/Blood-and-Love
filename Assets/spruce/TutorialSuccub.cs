using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSuccub : MonoBehaviour
{
    [SerializeField] private Enemy succubus;
    private EnemyStats succubusStats;
    void Start()
    {
        succubusStats = succubus.stats;

        if (succubusStats != null)
        {
            succubusStats.health = 120;
            succubus.currentHealth = succubusStats.health;
        }

    }

    
    }
