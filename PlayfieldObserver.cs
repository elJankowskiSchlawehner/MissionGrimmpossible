using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayfieldObserver : MonoBehaviour {

    public GameObject[] TrapsPrefab_Tiles;                          // Prefabs, die bei Spielertot unter den Tiles gespawned werden --> Aufwaertsbewegung
    public GameObject[] TrapsPrefab_Misc;                           // sonstige Prefabs, die an anderen Positionen gespawned werden

    private List<GameObject> _tilesList = new List<GameObject>();   // speichert alle betretenen Bodenplatten, werden bei Fehltritt zurueckgesetzt
    private Player _player;                                         // Referenz auf das Skript des Spielers
    private PlayfieldInitialiser _boardInfo;                        // Referenz auf die Eigenschaften des Spielfelds

    private int _coroutinesActive = 0;                              // zaehlt alle aktiven Routinen, wichtig fuer das Zuruecksetzen der Bodenplatten

    //public float GameTimer = 60.0f;
    //public Text timerText;

    private float _heightDifference = 0.3f;                         // gibt an mit welchem Hoehen-Unterschied sich die Bodenplatten bei Betreten / Fehltritt bewegen sollen
    private float _smoothTime = 0.5f;                               // die Zeit, die die Bewegung der Bodenplatten benoetigt

	// Use this for initialization
	void Start () {
        TrapsPrefab_Tiles = gameObject.GetComponent<PlayfieldInitialiser>().TrapsPrefab_Tiles;
        TrapsPrefab_Misc = gameObject.GetComponent<PlayfieldInitialiser>().TrapsPrefab_Misc;
        _player = GameObject.Find("player").GetComponent<Player>();
        _boardInfo = gameObject.GetComponent<PlayfieldInitialiser>();

        //timerText = GameObject.Find("Timer").GetComponent<Text>();
        //timerText.text = (Mathf.Round(GameTimer * 100f) / 100f).ToString();
	}
	
	// Update is called once per frame
	void Update () {
        /*
        if (_playerSkript.CanMove)
        {
            if (GameTimer <= 0f)
            {
                RestartGame();
            }

            timerText.text = (Mathf.Round(GameTimer * 100f) / 100f).ToString();
            GameTimer -= Time.deltaTime;
        }
        */
    }

    /* 
     * ##### SteppedOn #####
     * Erwartet als Parameter die Bodenplatte, die der Spieler betreten hat. Besitzt keinen Rueckgabewert
     * Falls die Bodenplatte in diesem Durchlauf noch nicht betreten wurde, wird sie der _tilesList angefuegt.
     * Der Vektor direction gibt die Verschiebung mit _heightDifference nach unten an und uebergibt
     * diesen Parameter an die Routine MoveTileSmooth.
     */
    public void SteppedOn(GameObject tile)
    {
        if (!_tilesList.Contains(tile))
        {
            Vector3 direction = new Vector3(tile.transform.position.x, tile.transform.position.y - _heightDifference, tile.transform.position.z);
            _tilesList.Add(tile);
            _coroutinesActive++;
            StartCoroutine(MoveTileSmooth(tile, direction));
        }
    }

    /* 
     * ##### ResetTiles #####
     * Eine Routine (IEnumerator) ohne Uebergabeparameter.
     * Aufruf, wenn Spieler eine falsche Bodenplatte beruehrt.
     * Wartet auf das Beenden aller Routinen und setzt dann alle Tiles (indem sie eine Schleife durchlaeuft) zurueck.
     * Wartet anschliessend auf das Beenden der Routinen beim Zuruecksetzen in die Ursprungsposition.
     */
    public IEnumerator ResetTiles()
    {
        // warten auf das Beenden aller laufenden Routinen des Typs MoveTileSmooth
        while (_coroutinesActive != 0)
        {
            yield return null;
        }

        // Spieler zuruecksetzen
        yield return new WaitForSeconds(0.5f);
        _player.isDead();
        yield return new WaitForSeconds(0.5f);
        _player.isAlive();


        // alle Tiles zuruecksetzen
        for (int i = 0; i < _tilesList.Count; i++)
        {
            Vector3 direction = new Vector3(_tilesList[i].transform.position.x, _tilesList[i].transform.position.y + _heightDifference, _tilesList[i].transform.position.z);
            _coroutinesActive++;
            StartCoroutine(MoveTileSmooth(_tilesList[i], direction));  // Zuruecksetzen mittels der Routine MoveTileSmooth
        }

        // warten bis wieder alle Bodenplatten verschoben wurden
        while (_coroutinesActive != 0)
        {
            yield return null;
        }

        // alle Referenzen der Bodenplatten aus der Liste entfernen
        _tilesList.Clear();

        // alle Routinen wurden abgearbeitet
        _coroutinesActive = 0;

        // das Feld wurde bereinigt, der Spieler kann sich wieder bewegen
        _player.CanMove = true;
    }

    /* 
     * ##### MoveTileSmooth #####
     * Eine Routine (IEnumerator) mit Parametern: die vom Spieler betretene Bodenplatte 'tile' und 
     * der Endpunkt der Translation 'direction'. 
     * Die Bodenplatte hat den Endpunkt erreicht, wenn _smoothTime erreicht wurde.
     */
    private IEnumerator MoveTileSmooth(GameObject tile, Vector3 direction)
    {
        float elapsedTime = 0;                          // zaehlen der bereits vergangenen Zeit
        Vector3 startingPos = tile.transform.position;  // der Punkt, ab dem die Translation beginnt
        
        while (elapsedTime < _smoothTime)
        {
            tile.transform.position = Vector3.Lerp(startingPos, direction, (elapsedTime / _smoothTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _coroutinesActive--;
    }

    /* 
     * ##### CheckWin #####
     * Aufruf der Funktion geschieht bei der Vorwaertsbewegung des Spielers.
     * Ueberprueft, ob die Endbedingung erfuellt wurde und startet falls diese der Fall ist ein neues Level
     */
    public void CheckWin ()
    {
        float endConditionY = transform.position.z + _boardInfo.TileHeight * _boardInfo.HeightPlayfield;
        if (_player.transform.position.z >= endConditionY)
        {
            RestartGame();
        }
    }

    /* 
     * ##### RestartGame #####
     * Neuinstanziierung des Levels
     */
    public void RestartGame ()
    {
        SceneManager.LoadScene(0);
    }
}