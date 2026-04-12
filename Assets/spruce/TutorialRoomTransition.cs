using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutorialRoomTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D roomBoundry;
    CinemachineConfiner confiner;

    [SerializeField] RoomDirection direction;

    enum RoomDirection {Up, Down}

    [SerializeField] private PopUpManager popUpManager;



    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (popUpManager.phase != 1)
        {
            if (other.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = roomBoundry;
            UpdatePlayerPosition(other.gameObject);
            popUpManager.phase = 3;
        }
        }

    }


    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPosition = player.transform.position;

        switch (direction)
        {
            case RoomDirection.Up:
                newPosition.y += 3f; 
                break;
            case RoomDirection.Down:
                newPosition.y -= 3f;
                break;
        }

        player.transform.position = newPosition;
    }
}

