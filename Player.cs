using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Transform _boardManager;
    private PlayfieldInitialiser _boardInfo;
    private PlayfieldObserver _boardObserver;

    private Rigidbody rb;

    [HideInInspector]
    public Vector3 ResetPoint_V;

    private float _stepForward;
    private float _stepSide;

    private Vector3 _endPos_V;

    [HideInInspector]
    public bool CanMove = false;
    private bool _isMoving = false;
    private float _resetTimer = 0f;

    // Use this for initialization
    void Start()
    {
        _boardManager = GameObject.Find("boardGameManager").transform;
        _boardInfo = _boardManager.GetComponent<PlayfieldInitialiser>();
        _boardObserver = _boardManager.GetComponent<PlayfieldObserver>();

        rb = GetComponent<Rigidbody>();

        //m_resetPointV wird in initPlayfield initialisiert
        _stepForward = _boardInfo.TileHeight;
        _stepSide = _boardInfo.TileWidth;

        CanMove = false;
    }

    // Update is called once per frame
    void Update()
    {
        // mit velocity ueberpruefen ???????
        if (CanMove && rb.velocity.magnitude <= 0)
        {
            if (Input.GetKeyDown(KeyCode.W) && !_isMoving)
            {
                _endPos_V = new Vector3(transform.position.x, transform.position.y, transform.position.z + _stepForward);
                //transform.position += new Vector3(0, 0, _stepForward);
                StartCoroutine(Move(_endPos_V, 0.25f));
            }

            if (Input.GetKeyDown(KeyCode.A) && transform.position.x > _boardInfo.transform.position.x + 1 && !_isMoving)
            {
                _endPos_V = new Vector3(transform.position.x - _stepSide, transform.position.y, transform.position.z);
                //transform.position += new Vector3(-1 * _stepSide, 0, 0);
                StartCoroutine(Move(_endPos_V, 0.25f));
            }

            if (Input.GetKeyDown(KeyCode.D) && transform.position.x < _boardInfo.transform.position.x - 1 + _boardInfo.TileWidth * (_boardInfo.GetWidthPlayfield() - 1) && !_isMoving)
            {
                _endPos_V = new Vector3(transform.position.x + _stepSide, transform.position.y, transform.position.z);
                //transform.position += new Vector3(_stepSide, 0, 0);
                StartCoroutine(Move(_endPos_V, 0.25f));
            }
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
            StartCoroutine(_boardObserver.ResetBoard(currentTilePos));
        }
        else if (tileCollider.tag == "correctTile")
        {

        }
    }

    // benutzt in PlayfieldObserver
    public void IsDead()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    // benutzt in PlayfieldObserver
    public void IsAlive()
    {
        GetComponent<MeshRenderer>().enabled = true;
        transform.position = ResetPoint_V;
    }

    private IEnumerator Move(Vector3 direction, float smoothTime)
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;

        _isMoving = true;
        while (elapsedTime < smoothTime)
        {
            transform.position = Vector3.Lerp(startingPos, direction, (elapsedTime / smoothTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _isMoving = false;
        _boardObserver.CheckWin();
    }

    private IEnumerator WaitForMovement()
    {
        while (_isMoving)
        {
            yield return null;
        }
    }
}