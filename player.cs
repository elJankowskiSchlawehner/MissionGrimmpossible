using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{

    public float m_moveTime = 0.1f;           //Time it will take object to move, in seconds.

    private initPlayfield _board;

    private float _stepForward;
    private float _stepSide;

    private Rigidbody _rb;
    private CapsuleCollider _cc;

    private Vector3 _spawnPlayerV;

    private float inverseMoveTime;          //Used to make movement more efficient.

    // Use this for initialization
    void Start()
    {
        _board = GameObject.Find("boardGameManager").GetComponent<initPlayfield>();
        _rb = GetComponent<Rigidbody>();
        _cc = GetComponent<CapsuleCollider>();

        _spawnPlayerV = transform.position;
        _stepForward = _board.m_tileHeight;
        _stepSide = _board.m_tileWidth;

        inverseMoveTime = 1f / m_moveTime;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.W))
        {
            transform.localPosition += new Vector3(0, 0, _stepForward);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.localPosition += new Vector3(-1 *_stepSide, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.localPosition += new Vector3(_stepSide, 0, 0);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "wrongTile")
        {
            Debug.Log("Gotcha!");
            transform.position = _spawnPlayerV;
        }
    }
}
