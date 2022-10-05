using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float walkSpeed = 6.0f;
    [SerializeField] [Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;
    [SerializeField] float gravity = -13.0f;
    private CharacterController characterController;

    private float velocityY = 0.0f; // the velocity of the gravity

    private Vector2 targetDir;
    private Vector3 velocity;
    private Vector2 currentDir;
    private Vector2 currentDirVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        targetDir = new Vector2();
        velocity = Vector3.zero;
        currentDir = Vector2.zero;
        currentDirVelocity = Vector2.zero;
    }

    private void Start()
    {
        // Let Game Manager know that you have spawned
        GameManager.instance.players[GameManager.instance.numOfPlayers] = transform; // Let Game Manager store my Transform for the monsters
        GameManager.instance.playerCameras[GameManager.instance.numOfPlayers] = transform.GetChild(0).GetComponent<Camera>(); // Game Manager stores cameras for monsters with visibility abilities
        GameManager.instance.numOfPlayers++;
    }


    private void Update()
    {
        if (!base.IsOwner)
            return;

        UpdateMovement(); // The movement of the player
    }

    private void UpdateMovement()
    {
        targetDir.x = Input.GetAxisRaw("Horizontal");
        targetDir.y = Input.GetAxisRaw("Vertical");
        targetDir.Normalize();

        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

        // gravity
        velocityY += gravity * Time.deltaTime;

        if (characterController.isGrounded)
        {
            velocityY = 0.0f;
        }

        velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * walkSpeed + Vector3.up * velocityY;

        characterController.Move(velocity * Time.deltaTime);
    }
}
