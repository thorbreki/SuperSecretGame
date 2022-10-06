using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WatchfulEnemy : MonoBehaviour
{
    [SerializeField] private Transform playersParent;
    [SerializeField] private float maxSightDistance = 100f; // This is the maximum distance I can see to

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    // patroling
    private Vector3 walkPoint;
    private Vector3 targetVector; // So I can efficiently be sure that the enemy never goes to a wrong y level 
    private bool walkPointSet;
    private float walkPointRange;

    private int targetTransformIndex;
    private bool lookedAtByPlayer; // Becomes false when the player stops looking at the enemy, used in the target phase

    // FOR THE VISIBILITY CHECKING METHOD
    private Renderer m_renderer;
    private Vector3 screenPos;
    private bool onScreen;
    [SerializeField] private float padding;

    private enum Phase
    {
        searchPhase,
        targetPhase
    }

    private Phase currPhase = Phase.searchPhase; // the current phase I am in

    void Start()
    {
        targetVector = Vector3.zero;
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

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.x + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }
    
    /// <summary>
    /// The code that runs when I am looking if I can spot any players
    /// </summary>
    private void SearchPhase()
    {
        // The part of the code that allows me to walk around the map in search of flesh
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

            // Walkpoint reached
        if (distanceToWalkPoint.sqrMagnitude < 2f)
        {
            walkPointSet = false;
        }

        // Where I am checking if I can actually see a player
        //RaycastHit hit;
        //for (int i = 0; i < GameManager.instance.numOfPlayers; i++)
        //{
        //    if (Physics.Raycast(transform.position, GameManager.instance.players[i].position - transform.position, out hit, maxDistance: maxSightDistance))
        //    {
        //        if (hit.transform.CompareTag("Player"))
        //        {
        //            currPhase = Phase.targetPhase; // Time to look menacingly at the player and try to eat him :)
        //            targetTransformIndex = i; // Set the target to be the player I can see
        //            print("I'M GONNA EAT YOU! :)");
        //            break;
        //        }
        //    }
        //}
    }

    /// <summary>
    /// In the target phase, I will get closer to the player if none of the players are looking at me, I'm shy
    /// </summary>
    private void TargetPhase()
    {
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
            // This is where I should be moving
            targetVector = GameManager.instance.players[targetTransformIndex].position;
            targetVector.y = transform.position.y;
            agent.SetDestination(targetVector);
        } else
        {
            // This is when the player is looking at me so I should be still
            agent.SetDestination(transform.position);
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
