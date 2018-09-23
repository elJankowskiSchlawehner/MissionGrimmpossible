using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private Transform _boardManager;
    private PlayfieldInitialiser _boardInfo;
    private PlayfieldObserver _gameObserver;
    private Rigidbody rb;

    private Camera _mainCamera;

    [HideInInspector]
    public Vector3 ResetPoint_V;

    private float _stepForward;
    private float _stepSide;

    private Vector3 _endPos_V;

    [HideInInspector]
    public bool CanMove = false;
    private bool _isMoving = false;
    private float _resetTimer = 0f;

    private Animator anim;

    private bool isPaused = false;


    // Use this for initialization
    void Start()
    {
        _boardManager = GameObject.Find("boardGameManager").transform;
        _boardInfo = _boardManager.GetComponent<PlayfieldInitialiser>();
        _gameObserver = _boardManager.GetComponent<PlayfieldObserver>();

        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        anim = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();

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
            if ((Input.GetKeyDown(KeyCode.W) ||Input.GetKeyDown(KeyCode.UpArrow)) && !_isMoving)
            {
                _endPos_V = new Vector3(transform.position.x, transform.position.y, transform.position.z + _stepForward);
                _mainCamera.transform.parent = null;
                StartCoroutine(Move(_endPos_V));
                _mainCamera.transform.parent = transform;
            }

            if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && transform.position.x > _boardInfo.transform.position.x + 1 && !_isMoving)
            {
                _endPos_V = new Vector3(transform.position.x - _stepSide, transform.position.y, transform.position.z);
                _mainCamera.transform.parent = null;
                StartCoroutine(Move(_endPos_V));
                _mainCamera.transform.parent = transform;
            }

            if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && transform.position.x < _boardInfo.transform.position.x - 1 + _boardInfo.TileWidth * (_gameObserver.GetPlayfieldWidth() - 1) && !_isMoving)
            {
                _endPos_V = new Vector3(transform.position.x + _stepSide, transform.position.y, transform.position.z);
                _mainCamera.transform.parent = null;
                StartCoroutine(Move(_endPos_V));
                _mainCamera.transform.parent = transform;
            }

            
        }

        if (Input.GetKeyDown(KeyCode.P) && !isPaused)
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
                CanMove = false;
                isPaused = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.P) && isPaused)
        {
            Time.timeScale = 1;
            CanMove = true;
            isPaused = false;
        }

        if (Input.GetKey(KeyCode.R))
        {
            if (_resetTimer >= 1.0f)
            {
                _gameObserver.RestartGame(0);
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
        Vector3 currentTilePos = _gameObserver.SteppedOn(tileCollider.transform.parent.gameObject);

        if (tileCollider.tag == "wrongTile")
        {
            CanMove = false;
            StartCoroutine(_gameObserver.ResetBoard(currentTilePos));
        }
        else if (tileCollider.tag == "correctTile")
        {

        }
    }

    // benutzt in PlayfieldObserver
    public void IsDead()
    {

    }

    // benutzt in PlayfieldObserver
    public void IsAlive()
    {
        transform.position = ResetPoint_V;
    }

    private IEnumerator Move(Vector3 direction)
    {
        float smoothTime = 0.25f;
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;

        _isMoving = true;
        anim.SetBool("IsWalking", true);
        while (elapsedTime < smoothTime)
        {
            transform.position = Vector3.Lerp(startingPos, direction, (elapsedTime / smoothTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _isMoving = false;
        anim.SetBool("IsWalking", false);
        _gameObserver.CheckWin();
    }

    private IEnumerator WaitForMovement()
    {
        while (_isMoving)
        {
            yield return null;
        }
    }
}