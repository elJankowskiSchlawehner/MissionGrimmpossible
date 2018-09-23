using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Erzeugt ein Feld von Sprites, die den richtigen Weg durch das Musuem weisen sollen.
 * Dieses kann uebersprungen werden, um Zeit zu sparen.
 * Nach der Darstellung des Weges soll das Feld fuer eine kurze Zeit
 * zum Einpraegen gezeigt werden, dann wird es zerstoert.
 */
public class ShowPath : MonoBehaviour {

    // Referenzen
    private Camera _mainCamera;
    private PlayfieldInitialiser _boardInfo;
    private PlayfieldObserver _gameObserver;
    private ShowPathUI _pathUI;
    public Sprite TileSprite;
    public GameObject PathDot;

    private GameObject _displayField;
    private float _displayFieldWidth;
    private List<Vector2> _pathList = new List<Vector2>();              /* Verwaltung der Koordinaten der richtigen Bodenplatten
                                                                         * wird waehrend der Initialsierung befuellt
                                                                         */
    private const float OFFSET = 0.1f;                                  // der Versatz zwischen den Sprites
    private float _tileSize;                                            // Groesse der Tiles + OFFSET

    //private float _smoothTime = 0.5f;                                  

    // Flags
    [HideInInspector]
    public bool _displayStarted = false, _displayFinished = false;
    bool _isSkipped = false;

    private int _listCnt = 0;                                           // zaehlt ddie Indizes der Liste hoch; Membervariable da der Index beim Skippen benoetigt wird
    private IEnumerator coroutineDisplay;                               // die Coroutine, die beim Skippen gestoppt wird

    private float _animTime = 0.5f;                                     // Zeit, zum Darstellen der naechsten Leuchte
    private float _destroyTimer = 0f;                                   // zaehlt die Zeit hoch, bis die Hilfe nicht mehr angezeigt wird

	// Use this for initialization
	void Start () {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _mainCamera.orthographic = true;                                // _displayField ist besser erkennbar
        _boardInfo = gameObject.GetComponent<PlayfieldInitialiser>();   // Breite und Hoehe des Spielfelds wird in GenerateSprites benoetigt
        _gameObserver = gameObject.GetComponent<PlayfieldObserver>();

        _pathUI = GameObject.Find("Canvas").GetComponent<ShowPathUI>();
        _displayField = new GameObject("displayField");

        _tileSize = TileSprite.bounds.size.x + OFFSET;
        _displayFieldWidth = (_boardInfo.GetWidthPlayfield() * _tileSize) - OFFSET;  // Breite der Anzeige - letztes OFFSET nach dem letzten Tile wird abgezogen
        //float displayScale = (_pathUI.PhoneImage.rectTransform.rect.width / 100 - 0.3f) / _displayFieldWidth; // Pixel des Smartphones in Units umrechnen
        float displayScale = (_pathUI.PhoneImage.transform.Find("BackgroundImage").GetComponent<RectTransform>().rect.width / 100 - 0.5f) / _displayFieldWidth;
        _displayField.transform.localScale = new Vector3(displayScale, displayScale, 1f);

        GenerateSprites(_boardInfo.GetHeightPlayfield(), _boardInfo.GetWidthPlayfield());
        coroutineDisplay = DisplayPath();                               // diese Coroutine kann spaeter auf Tastendruck gestoppt werden

        // zur Kamera verschieben
        Vector3 pos = new Vector3   (
                                        (0 + TileSprite.bounds.size.x * displayScale / 2) - (_displayFieldWidth * displayScale / 2), 
                                        _mainCamera.transform.position.y - 3, 
                                        1
                                    );
        _displayField.transform.position = pos;

        StartCoroutine(StartDisplay());
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_displayFinished && _destroyTimer <= 0f)
        {
            // ueberspringen der Erstellung
            _isSkipped = true;
            StopCoroutine(coroutineDisplay);

            while (_listCnt < _pathList.Count)
            {
                Instantiate(PathDot, DotPosition(), Quaternion.identity, _displayField.transform);
                _listCnt++;
            }
            //_listCnt--;
            //Instantiate(PathDisplay, new Vector3(_pathList[_listCnt].x * _offset, (_pathList[_listCnt].y * _offset) + _offset, -1), Quaternion.identity);
            //Destroy(PathDisplay);
            _displayFinished = true;
        }
        

        if (_displayFinished)
        {
            if (_destroyTimer >= 1.0f)
            {
                StartCoroutine(DisplayBoard());
                _displayFinished = false;
            }
            _destroyTimer += Time.deltaTime;
        }
	}

    private IEnumerator StartDisplay()
    {
        yield return new WaitForSeconds(1.0f);
        PathDot.GetComponent<ParticleSystem>().startSize = _displayField.transform.localScale.x;
        StartCoroutine(coroutineDisplay);
        _displayStarted = true;
    }

    /* 
    * ##### GenerateSprites #####
    *
    * Bekommt die Hoehe und die Breite des Spielfelds uebergeben
    * und erzeugt dann auf deren Grundlage ein vereinfachtes
    * Abbild des Spielfelds.
    * Enthaelt noch nicht die falschen und korrekten Platten
    */
    private void GenerateSprites(int height, int width)
    {
        Vector3 start_V = Vector3.zero;

        for (int i = 0; i < height; i++)
        {
            
            GameObject row = new GameObject("row" + i);
            row.transform.SetParent(_displayField.transform, false);
            
            for (int j = 0; j < width; j++)
            {
                GameObject sprite = new GameObject("pathSprite");
                sprite.transform.SetParent(row.transform, false);
                sprite.transform.position = start_V;
                SpriteRenderer sr = sprite.AddComponent<SpriteRenderer>();
                sr.sprite = TileSprite;

                start_V.x += _tileSize * _displayField.transform.localScale.x;
            }
            start_V.x = 0;
            start_V.y += _tileSize * _displayField.transform.localScale.y;
        }
    }

    /* 
    * ##### DisplayPath #####
    *
    * Routine, die in einer Animation den richtigen Pfad durch
    * das Spielfeld weist
    */
    private IEnumerator DisplayPath()
    {
        Instantiate(PathDot, DotPosition(), Quaternion.identity, _displayField.transform);
        _listCnt++;
        while (_listCnt < _pathList.Count && !_isSkipped)
        {
            yield return new WaitForSeconds(_animTime);
            Instantiate(PathDot, DotPosition(), Quaternion.identity, _displayField.transform);
            _listCnt++;
        }
        //_listCnt--;
        //position = new Vector3(_pathList[_listCnt].x * _offset, _pathList[_listCnt].y * _offset + _offset, PathDisplay.transform.position.z);
        //Instantiate(PathDisplay, position, Quaternion.identity);
        yield return new WaitForSeconds(_animTime);
        _displayFinished = true;
    }

    /* 
    * ##### DotPosition #####
    *
    * Gibt die Position des zu setzenden Wegweisers als Vektor3 
    * aus der Liste mit dem aktuellen Listenzaehler zurueck
    */
    private Vector3 DotPosition()
    {
        return new Vector3(
                            (_pathList[_listCnt].x * _tileSize * _displayField.transform.localScale.x )+ _displayField.transform.position.x,
                            (_pathList[_listCnt].y * _tileSize * _displayField.transform.localScale.y )+ _displayField.transform.position.y,
                            -1 + _displayField.transform.position.z);
    }

    /* 
    * ##### DisplayBoard #####
    *
    * Zeigt nun das eigentliche Spiel an.
    * Die Kamera wird zurueckgesetzt und die Hilfe wird zerstoert
    */
    private IEnumerator DisplayBoard()
    {
        StartCoroutine(_pathUI.FadeOut());
        while (_pathUI.isFading)
        {
            yield return null;
        }
        Destroy(_displayField);
        _pathUI.transform.Find("Phone").gameObject.SetActive(false);

        // Kamera umstellen
        // TO DO
        _mainCamera.orthographic = false;
        _mainCamera.transform.position = new Vector3(9.7f, 21.88f, -13.73f);
        _mainCamera.transform.rotation = Quaternion.Euler(new Vector3(42.7f, 0f, 0f));

        // Canvas Overlay umstellen
        _pathUI.SetCanvasOverlay();
        _pathUI.EnableTimer();

        StartCoroutine(_pathUI.FadeIn());
        while (_pathUI.isFading)
        {
            yield return null;
        }

        Destroy(_pathUI.transform.Find("Phone").gameObject);
        Destroy(_pathUI);
        _gameObserver.GameStarted = true;
        Destroy(this);
    }

    /* 
    * ##### AddToPathList #####
    *
    * Wird innerhalb der Initialisierung aufgerufen, um eine Liste
    * mit Vector2 Koordinaten zu befuellen, die den Weg des richtigen Pfades in
    * korrekter Reihenfolge enthaelt.
    */
    public void AddToPathList(float x, float y)
    {
        _pathList.Add(new Vector2(x, y));
    }

    public int GetCorrectTilesCount()
    {
        return _pathList.Count;
    }

    public float GetDisplayAnimTime()
    {
        return _animTime;
    }
}
