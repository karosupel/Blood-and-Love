using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldSuccubus : MonoBehaviour
{
    [SerializeField] private PopUpManager popUpManagerScript;
    [SerializeField] private int phaseToActivate;
    void Update()
    {
        gameObject.SetActive(popUpManagerScript.phase < phaseToActivate);
    }
}
