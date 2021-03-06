﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Erzeugt ein Feld von Sprites, die den richtigen Weg durch das Musuem weisen sollen.
 * Dieses kann uebersprungen werden, um Zeit zu sparen.
 * Nach der Darstellung des Weges soll das Feld fuer eine kurze Zeit
 * zum Einpraegen gezeigt werden, dann wird es zerstoert.
 */
public class ShowPathManager : MonoBehaviour {

    // Referenzen
    private PlayfieldInitialiser _boardInfo;
    public Sprite TileSprite;
    public GameObject PathDisplay;

    private GameObject _displayField;
    private List<Vector2> _pathList = new List<Vector2>();              /* Verwaltung der Koordinaten der richtigen Bodenplatten
                                                                         * wird waehrend der Initialsierung befuellt
                                                                         */
    private const float OFFSET = 1.1f;                                // der Versatz zwischen den Sprites
    private float _animTime = 1.0f;                                     // Alternative, um das Darstellen zu beschleunigen, bei Skip-Taste verringern
    //private float _smoothTime = 0.5f;                                  

    // Flags
    public bool _displayStarted = false;                                       
    public bool _displayFinished = false;
    bool _isSkipped = false;

    private int _listCnt = 0;                                           // zaehlt ddie Indizes der Liste hoch; Membervariable da der Index beim Skippen benoetigt wird
    private IEnumerator coroutineDisplay;                               // die Coroutine, die beim Skippen gestoppt wird

    private float _destroyTimer = 0f;                                   // zaehlt die Zeit hoch, bis die Hilfe nicht mehr angezeigt wird

	// Use this for initialization
	void Start () {
        _boardInfo = gameObject.GetComponent<PlayfieldInitialiser>();   // Breite und Hoehe des Spielfelds wird in GenerateSprites benoetigt

        _displayField = new GameObject("displayField");
        _displayField.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        GenerateSprites(_boardInfo.getHeightField(), _boardInfo.getWidthField());
        coroutineDisplay = DisplayPath();                               // diese Coroutine kann spaeter auf Tastendruck gestoppt werden
    }
	
	// Update is called once per frame
	void Update () {
        //_offset *= _parentTransform.transform.localScale.x;

        // *** Routine spaeter bei Spielstart o.ae. automatisch starten, nicht auf Tastendruck ***
        if (Input.GetKeyDown(KeyCode.X) && !_displayStarted)
        {
            PathDisplay.GetComponent<ParticleSystem>().startSize = _displayField.transform.localScale.x;
            StartCoroutine(coroutineDisplay);
            _displayStarted = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !_displayFinished)
        {
            // ueberspringen der Erstellung
            _isSkipped = true;
            StopCoroutine(coroutineDisplay);

            while (_listCnt < _pathList.Count)
            {
                Instantiate(PathDisplay, DotPosition(), Quaternion.identity, _displayField.transform);
                _listCnt++;
            }
            //_listCnt--;
            //Instantiate(PathDisplay, new Vector3(_pathList[_listCnt].x * _offset, (_pathList[_listCnt].y * _offset) + _offset, -1), Quaternion.identity);
            //Destroy(PathDisplay);
            _displayFinished = true;
        }

        if (_displayFinished)
        {         
            if (_destroyTimer >= 2.0f)
            {
                // Kamera auf Spieler, in diesem Skript instanziierten Sachen zerstoeren (evtl.)
                //Debug.Log("Hilfe fertig");
            }
            _destroyTimer += Time.deltaTime;
        }
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

                start_V.x += OFFSET * _displayField.transform.localScale.x;
            }
            start_V.x = 0;
            start_V.y += OFFSET * _displayField.transform.localScale.y;
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
        Instantiate(PathDisplay, DotPosition(), Quaternion.identity, _displayField.transform);
        _listCnt++;
        while (_listCnt < _pathList.Count && !_isSkipped)
        {
            yield return new WaitForSeconds(_animTime);
            GameObject go = Instantiate(PathDisplay, DotPosition(), Quaternion.identity, _displayField.transform);
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
                            (_pathList[_listCnt].x * OFFSET * _displayField.transform.localScale.x )+ _displayField.transform.position.x,
                            (_pathList[_listCnt].y * OFFSET * _displayField.transform.localScale.y )+ _displayField.transform.position.y,
                            -1 + _displayField.transform.position.z);
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
