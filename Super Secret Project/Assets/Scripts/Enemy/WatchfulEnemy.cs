using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchfulEnemy : MonoBehaviour
{
    [SerializeField] private Transform playersParent;
    [SerializeField] private float maxSightDistance = 100f; // This is the maximum distance I can see to
    [SerializeField] private float movementSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 2.0f;
    private int targetTransformIndex;
    private bool lookedAtByPlayer; // Becomes false when the player stops looking at the enemy, used in the target phase

    // USED FOR ROTATING ME TOWARDS MY TARGET :)
    private Quaternion lookRotation;
    private Vector3 direction;

    // FOR THE VISIBILITY CHECKING METHOD
    private Renderer m_renderer;
    private Vector3 screenPos;
    private bool onScreen;
    [SerializeField] private float padding;

    private Vector3 movementVector;

    private enum Phase
    {
        searchPhase,
        targetPhase
    }

    private Phase currPhase = Phase.searchPhase; // the current phase I am in

    void Start()
    {

        // FOR THE VISIBILITY CHECKING METHOD
        m_renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (currPhase == Phase.searchPhase)
        {
            SearchPhase();
        }
        else
        {
            TargetPhase();
        }

    }


    
    /// <summary>
    /// The code that runs when I am looking if I can spot any players
    /// </summary>
    private void SearchPhase()
    {
        RaycastHit hit;
        for (int i = 0; i < GameManager.instance.numOfPlayers; i++)
        {
            if (Physics.Raycast(transform.position, GameManager.instance.players[i].position - transform.position, out hit, maxDistance: maxSightDistance))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    currPhase = Phase.targetPhase; // Time to look menacingly at the player and try to eat him :)
                    targetTransformIndex = i; // Set the target to be the player I can see
                    print("I'M GONNA EAT YOU! :)");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// In the target phase, I will get closer to the player if none of the players are looking at me, I'm shy
    /// </summary>
    private void TargetPhase()
    {
        // First of all rotate, to see the player

        //find the vector pointing from our position to the target
        direction = GameManager.instance.players[targetTransformIndex].position - transform.position;
        direction.y = 0;
        direction = direction.normalized;

        //create the rotation we need to be in to look at the target
        lookRotation = Quaternion.LookRotation(direction);

        //rotate us over time according to speed until we are in the required rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);


        // Now to move towards the target when nobody is looking!
        lookedAtByPlayer = false;

        RaycastHit hit;

        // This loop both figures out if there are any players that are looking at me, and also if I have lost sight of my target
        for (int i = 0; i < GameManager.instance.numOfPlayers; i++)
        {
            if (Physics.Raycast(transform.position, GameManager.instance.players[i].position - transform.position, out hit, maxDistance: maxSightDistance))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    if (CheckVisibility(GameManager.instance.playerCameras[i]))
                    {
                        lookedAtByPlayer = true;
                        break;
                    }
                }

                else // If the current player is behind a wall and it is my target
                {
                    if (i == targetTransformIndex)
                    {
                        currPhase = Phase.searchPhase;
                        return; // Since I can no longer see my target I should start searching again :(
                    }
                }
            }

            else // If the current player is too far away and it is my target
            {
                currPhase = Phase.searchPhase;
                return; // Since I can no longer see my target I should start searching again :(
            }
        }

        if (!lookedAtByPlayer)
        {
            movementVector = GameManager.instance.players[targetTransformIndex].position - transform.position;
            movementVector.y = 0;
            transform.Translate(movementVector.normalized * movementSpeed * Time.deltaTime, Space.World);
        }


    }

    // TODO: FIX THIS METHOD TO HAVE A CERTAIN PADDING, SO THE PLAYER NEVER SEES THE MONSTER MOVING
    private bool CheckVisibility(Camera m_camera)
    {
        //Check Visibility

        screenPos = m_camera.WorldToScreenPoint(transform.position);
        onScreen = screenPos.x > 0f - padding && screenPos.x < Screen.width + padding && screenPos.y > 0f - padding && screenPos.y < Screen.height + padding;

        return onScreen;
    }
}
