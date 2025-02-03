using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleScript : MonoBehaviour {
    private float     yPos;
    public float      paddleSpeed = .05f;

    public KeyCode pUp;
    public KeyCode dDown;

    public float topWall = 3.75f;
    public float botWall = -3.75f;
	
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(dDown) && yPos > botWall) {
                yPos -= paddleSpeed;
        }

        if (Input.GetKey(pUp) && yPos < topWall) {
                yPos += paddleSpeed;
        }

        transform.localPosition = new Vector3(transform.position.x, yPos, 0);
    }
}

