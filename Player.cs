using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Transform _boardManager;
    private PlayfieldInitialiser _boardInfo;
    private PlayfieldObserver _boardObserver;

    [HideInInspector]
    public Vector3 ResetPoint_V;

    private float _stepForward;
    private float _stepSide;
    public float Speed = 0.5f;
    private float _currentLerpTime = 0f;
    private Vector3 _startPos_V;
    private Vector3 _endPos_V;

    [HideInInspector]
    public bool CanMove = false;
    private float _resetTimer = 0f;

    // Use this for initialization
    void Start()
    {
        Debug.Log("zum Spielen: Space druecken!");
        Debug.Log("R gedrueckt halten: Zuruecksetzen");

        _boardManager = GameObject.Find("boardGameManager").transform;
        _boardInfo = _boardManager.GetComponent<PlayfieldInitialiser>();
        _boardObserver = _boardManager.GetComponent<PlayfieldObserver>();

        //m_resetPointV wird in initPlayfield initialisiert
        _stepForward = _boardInfo.TileHeight;
        _stepSide = _boardInfo.TileWidth;
        _startPos_V = ResetPoint_V;

        CanMove = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_boardObserver.GameTimer <= 0f)
        {
            CanMove = false;
        }

        if (CanMove)
        {
            _boardObserver.GameStarted = true;
            if (Input.GetKeyDown(KeyCode.W))
            {
                _endPos_V = new Vector3(transform.position.x, transform.position.y, transform.position.z + _stepForward);
                transform.position += new Vector3(0, 0, _stepForward);
                _boardObserver.CheckWin();
            }

            if (Input.GetKeyDown(KeyCode.A) && transform.position.x > _boardInfo.transform.position.x)
            {
                _endPos_V = new Vector3(transform.position.x - _stepSide, transform.position.y, transform.position.z);
                transform.position += new Vector3(-1 * _stepSide, 0, 0);
            }

            if (Input.GetKeyDown(KeyCode.D) && transform.position.x < _boardInfo.transform.position.x + _boardInfo.TileWidth * (_boardInfo.GetWidthPlayfield() - 1))
            {
                _endPos_V = new Vector3(transform.position.x + _stepSide, transform.position.y, transform.position.z);
                transform.position += new Vector3(_stepSide, 0, 0);
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

        if (Input.GetKey(KeyCode.R))
        {
            if (_resetTimer >= 1.0f)
            {
                _boardObserver.RestartGame(0);
            }
            _resetTimer += Time.deltaTime;
        }
        else
        {
            _resetTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider tileCollider)
    {
        Vector3 currentTilePos = _boardObserver.SteppedOn(tileCollider.transform.parent.gameObject);

        if (tileCollider.tag == "wrongTile")
        {
            CanMove = false;
            // Falle Routine auch ausfuehren
            StartCoroutine(_boardObserver.ResetBoard(currentTilePos));
        }
        else if (tileCollider.tag == "correctTile")
        {

        }
    }

    // TO DO
    // benutzt in PlayfieldObserver
    public void isDead()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void isAlive()
    {
        GetComponent<MeshRenderer>().enabled = true;
        transform.position = ResetPoint_V;
    }
}