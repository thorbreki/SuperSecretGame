using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public static GameManager instance; // this GameManager instance which others can call whenever, wherever

    [HideInInspector] public int numOfPlayers = 0; // the actual number of players currently playing
    [HideInInspector] public Transform[] players; // An array of all the players
    [HideInInspector] public Camera[] playerCameras; // An array of all the player cameras

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;

        // Initialize the players array
        players = new Transform[4];
        playerCameras = new Camera[4];
    }


}
