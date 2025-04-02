using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobbingPoster : MonoBehaviour
{
    private float bobbingHeight = 0.1f;  // Height of the bobbing
    private float bobbingSpeed = 0.5f;      // Speed of the bobbing
    private Vector3 startPosition;
     void Start()
    {
        // Store the original position of the coin
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
