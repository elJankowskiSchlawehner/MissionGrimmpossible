using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathHelp : MonoBehaviour {

    private PlayfieldInitialiser _boardInfo;
    public Sprite TileSprite;
    public GameObject PathDisplay;

    private List<Vector2> _pathList = new List<Vector2>();

    private static float _offset = 1.1f;

    private float _smoothTime = 0.5f;

    bool _isSkipped = false;
    bool _displayFinished = false;

    private IEnumerator coroutine;
    private int _listCnt = 0;

    private float _destroyTimer = 0f;

	// Use this for initialization
	void Start () {
        _boardInfo = gameObject.GetComponent<PlayfieldInitialiser>();

        GenerateSprites();

        PathDisplay = Instantiate(PathDisplay, new Vector3(_pathList[0].x * _offset, _pathList[0].y * _offset, -1), Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.X))
        {
            // wird spaeter automatisch beim Spielstart gestartet
            StartCoroutine(LetItRip());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !_displayFinished)
        {
            // ueberspringen der Erstellung
            _isSkipped = true;
            StopCoroutine(coroutine);

            while (_listCnt < _pathList.Count)
            {
                Instantiate(PathDisplay, new Vector3(_pathList[_listCnt].x * _offset, _pathList[_listCnt].y * _offset, -1), Quaternion.identity);
                _listCnt++;
            }
            _listCnt--;
            Debug.Log((float) _pathList[_listCnt].y);
            Instantiate(PathDisplay, new Vector3(_pathList[_listCnt].x * _offset, (_pathList[_listCnt].y * _offset) + _offset, -1), Quaternion.identity);
            Destroy(PathDisplay);

            _displayFinished = true;
        }

        if (_displayFinished)
        {
            
            if (_destroyTimer >= 2.0f)
            {
                // Kamera auf Spieler, in diesem Skript instanziierten Sachen zerstoeren (evtl.)
            }
            _destroyTimer += Time.deltaTime;
        }
	}

    private void GenerateSprites()
    {
        Vector3 start_V = Vector3.zero;

        for (int i = 0; i < _boardInfo.getHeightField(); i++)
        {
            for (int j = 0; j < _boardInfo.getWidthField(); j++)
            {
                GameObject sprite = new GameObject("pathSprite");
                sprite.transform.position = start_V;
                SpriteRenderer sr = sprite.AddComponent<SpriteRenderer>();
                sr.sprite = TileSprite;

                start_V.x += _offset;
            }
            start_V.x = 0;
            start_V.y += _offset;
        }
    }

    private IEnumerator LetItRip()
    {
        while(_listCnt < _pathList.Count && !_isSkipped)
        {
            Vector3 direction = new Vector3(_pathList[_listCnt].x * _offset, _pathList[_listCnt].y * _offset, -1);
            coroutine = MoveSmooth(PathDisplay, direction, _smoothTime);
            //new Vector3(_pathList[cnt].x * _offset, _pathList[cnt].y * _offset, -1)
            yield return coroutine;
            Instantiate(PathDisplay, direction, Quaternion.identity);
            //yield return new WaitForSeconds(1.0f);
            _listCnt++;
        }
        coroutine = MoveSmooth(PathDisplay, new Vector3(PathDisplay.transform.position.x, PathDisplay.transform.position.y + _offset, PathDisplay.transform.position.z), _smoothTime);
        yield return coroutine;
		_displayFinished = true;
    }

    public void AddToPathList(float x, float y)
    {
        _pathList.Add(new Vector2(x, y));
    }

    private IEnumerator MoveSmooth(GameObject go, Vector3 direction, float smoothTime)
    {
        float elapsedTime = 0;                          // zaehlen der bereits vergangenen Zeit
        Vector3 startingPos = go.transform.position;    // der Punkt, ab dem die Translation beginnt

        while (elapsedTime < smoothTime)
        {
            go.transform.position = Vector3.Lerp(startingPos, direction, (elapsedTime / smoothTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
