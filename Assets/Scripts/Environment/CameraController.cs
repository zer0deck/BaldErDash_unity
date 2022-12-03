using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float offset;
    public float offsetSmoothing;
    private Vector3 playerPosition;
    private SpriteRenderer sprite;

    void Start()
    {
        sprite = player.GetComponentInChildren<SpriteRenderer>();
    }


    void Update()
    {
        playerPosition = new Vector3(player.transform.position.x, player.transform.position.y - 1000f, transform.position.z);

        if(sprite.flipX)
        {
            playerPosition = new Vector3(playerPosition.x - offset, playerPosition.y, playerPosition.z);
        }
        else
        {
            playerPosition = new Vector3(playerPosition.x + offset, playerPosition.y, playerPosition.z);
        }

        transform.position = Vector3.Lerp(transform.position, playerPosition, offsetSmoothing * Time.deltaTime);
    }
}
