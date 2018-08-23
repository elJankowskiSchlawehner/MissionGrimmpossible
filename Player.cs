using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private InitPlayfield _board;

    [HideInInspector]
    public Vector3 ResetPoint_V;

    private float _stepForward;
    private float _stepSide;
    public float Speed = 0.5f;
    private float _currentLerpTime = 0f;
    private Vector3 _startPos_V;
    private Vector3 _endPos_V;

    private Rigidbody _rb;
    private CapsuleCollider _cc;

    private bool _isStanding = false;
    private bool _keyHit = false;

    // Use this for initialization
    void Start()
    {
        _board = GameObject.Find("boardGameManager").GetComponent<InitPlayfield>();
        _rb = GetComponent<Rigidbody>();
        _cc = GetComponent<CapsuleCollider>();

        //m_resetPointV wird in initPlayfield initialisiert
        _stepForward = _board.TileHeight;
        _stepSide = _board.TileWidth;
        _startPos_V = ResetPoint_V;
    }

    // Update is called once per frame
    void Update()
    {
        if (_rb.velocity.y == 0 || _rb.velocity.x == 0)
        {
            _isStanding = true;
        }

        if (_isStanding)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                _keyHit = true;
                _endPos_V = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + _stepForward);
                transform.localPosition += new Vector3(0, 0, _stepForward);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                _keyHit = true;
                _endPos_V = new Vector3(transform.localPosition.x - _stepSide, transform.localPosition.y, transform.localPosition.z);
                transform.localPosition += new Vector3(-1 * _stepSide, 0, 0);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                _keyHit = true;
                _endPos_V = new Vector3(transform.localPosition.x + _stepSide, transform.localPosition.y, transform.localPosition.z);
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
            transform.position = ResetPoint_V;
        }
    }
}
