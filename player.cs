using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{

    private initPlayfield board;

    private float stepForward;
    private float stepSide;

    // Use this for initialization
    void Start()
    {

        board = GameObject.Find("boardGameManager").GetComponent<initPlayfield>();

        stepForward = board.tileHeight;
        stepSide = board.tileWidth;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.localPosition += new Vector3(0, 0, stepForward);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.localPosition += new Vector3(-stepSide, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.localPosition += new Vector3(stepSide, 0, 0);
        }

    }
}
