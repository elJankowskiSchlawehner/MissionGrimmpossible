using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private InitPlayfield _board;

    [HideInInspector]
    public Vector3 m_resetPointV;

    private float _stepForward;
    private float _stepSide;
    public float m_speed = 0.5f;
    private float _currentLerpTime = 0f;
    private Vector3 _startPos;
    private Vector3 _endPos;

    private Rigidbody _rb;
    private CapsuleCollider _cc;

    private bool isStanding = false;
    private bool keyHit = false;

    // Use this for initialization
    void Start()
    {
        _board = GameObject.Find("boardGameManager").GetComponent<InitPlayfield>();
        _rb = GetComponent<Rigidbody>();
        _cc = GetComponent<CapsuleCollider>();

        //m_resetPointV wird in initPlayfield initialisiert
        _stepForward = _board.m_tileHeight;
        _stepSide = _board.m_tileWidth;
        _startPos = m_resetPointV;
    }

    // Update is called once per frame
    void Update()
    {
        if (_rb.velocity.y == 0 || _rb.velocity.x == 0)
        {
            isStanding = true;
        }

        if (isStanding)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                keyHit = true;
                _endPos = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + _stepForward);
                transform.localPosition += new Vector3(0, 0, _stepForward);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                keyHit = true;
                _endPos = new Vector3(transform.localPosition.x - _stepSide, transform.localPosition.y, transform.localPosition.z);
                transform.localPosition += new Vector3(-1 * _stepSide, 0, 0);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                keyHit = true;
                _endPos = new Vector3(transform.localPosition.x + _stepSide, transform.localPosition.y, transform.localPosition.z);
                transform.localPosition += new Vector3(_stepSide, 0, 0);
            }

            /*if (keyHit)
            {
                isStanding = false;
                _currentLerpTime += Time.deltaTime;
                if (_currentLerpTime >= m_speed)
                {
                    _currentLerpTime = m_speed;
                }

                float Perc = _currentLerpTime / m_speed;
                transform.localPosition = Vector3.Lerp(_startPos, _endPos, Perc);
                if (_currentLerpTime >= m_speed)
                {
                    keyHit = false;
                    _currentLerpTime = 0f;
                    isStanding = true;
                }
            }*/
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "wrongTile")
        {
            transform.position = m_resetPointV;
        }
    }
}
