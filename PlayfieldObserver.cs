using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayfieldObserver : MonoBehaviour {

    public GameObject[] TrapsPrefab_Tiles;                          // Prefabs, die bei Spielertot unter den Tiles gespawned werden --> Aufwaertsbewegung
    public GameObject[] TrapsPrefab_Misc;                           // sonstige Prefabs, die an anderen Positionen gespawned werden

    private List<GameObject> _tilesList = new List<GameObject>();   // speichert alle betretenen Bodenplatten, werden bei Fehltritt zurueckgesetzt
    private Player _player;                                         // Referenz auf das Skript des Spielers
    private PlayfieldInitialiser _boardInfo;                        // Referenz auf die Eigenschaften des Spielfelds

    private int _activeCoroutines = 0;                              // zaehlt alle aktiven Routinen in movingSmooth, wichtig fuer ResetPlayer

    public float GameTimer = (int) 30.0f;

    private float _heightDifference = 0.3f;                         // gibt an mit welchem Hoehen-Unterschied sich die Bodenplatten bei Betreten / Fehltritt bewegen sollen
    private float _tileSpeed = 0.5f;                                // die Zeit, die die Bewegung der Bodenplatten benoetigt

    public bool GameStarted = false;
    private bool _gameFinished = false;

	// Use this for initialization
	void Start () {
        TrapsPrefab_Tiles = gameObject.GetComponent<PlayfieldInitialiser>().TrapsPrefab_Tiles;
        TrapsPrefab_Misc = gameObject.GetComponent<PlayfieldInitialiser>().TrapsPrefab_Misc;
        _player = GameObject.Find("Player").GetComponent<Player>();
        _boardInfo = gameObject.GetComponent<PlayfieldInitialiser>();

        GameTimer -= 0.01f;
    }
	
	// Update is called once per frame
	void Update () {
        if (GameStarted && !_gameFinished)
        {
            GameTimer -= Time.deltaTime;
            
            if (GameTimer <= 0f)
            {
                _player.CanMove = false;
                GameTimer = 0f;
                RestartGame();
            }
        }
    }

    /* 
     * ##### SteppedOn #####
     * Erwartet als Parameter die Bodenplatte, die der Spieler betreten hat. Besitzt keinen Rueckgabewert
     * Falls die Bodenplatte in diesem Durchlauf noch nicht betreten wurde, wird sie der _tilesList angefuegt.
     * Der Vektor direction gibt die Verschiebung mit _heightDifference nach unten an und uebergibt
     * diesen Parameter an die Routine MoveTileSmooth.
     */
    public Vector3 SteppedOn(GameObject tile)
    {
        Vector3 direction = new Vector3(tile.transform.position.x, tile.transform.position.y - _heightDifference, tile.transform.position.z);
        if (!_tilesList.Contains(tile))
        {
            _tilesList.Add(tile);
            StartCoroutine(MoveSmooth(tile, direction, _tileSpeed));
        }
        return direction;
    }

    /* 
     * ##### ResetPlayer #####
     * Eine Routine (IEnumerator) ohne Uebergabeparameter.
     * Aufruf, wenn Spieler eine falsche Bodenplatte beruehrt.
     * Wartet auf das Beenden aller Routinen und setzt dann alle Tiles (indem sie eine Schleife durchlaeuft) zurueck.
     * Wartet anschliessend auf das Beenden der Routinen beim Zuruecksetzen in die Ursprungsposition.
     */
    public IEnumerator ResetPlayer(Vector3 currentTilePos)
    {
        // warten auf das Beenden aller laufenden Routinen des Typs MoveTileSmooth
        yield return WaitForOtherRoutines();

        // Animation der Fallen
        yield return TriggerTrapAnimation(currentTilePos);

        // Spieler wird zurueckgesetzt
        yield return ResetPlayer(1.0f);

        // alle Tiles zuruecksetzen
        for (int i = 0; i < _tilesList.Count; i++)
        {
            Vector3 direction = new Vector3(
                                            _tilesList[i].transform.position.x,
                                            _tilesList[i].transform.position.y + _heightDifference, 
                                            _tilesList[i].transform.position.z
                                            );
            StartCoroutine(MoveSmooth(_tilesList[i], direction, _tileSpeed));   // Zuruecksetzen mittels der Routine MoveTileSmooth
        }

        // warten bis wieder alle Bodenplatten verschoben wurden
        yield return WaitForOtherRoutines();

        // alle Referenzen der Bodenplatten aus der Liste entfernen
        _tilesList.Clear();

        // alle Routinen wurden abgearbeitet
        _activeCoroutines = 0;

        // das Feld wurde bereinigt, der Spieler kann sich wieder bewegen
        _player.CanMove = true;
    }

    /* 
     * ##### MoveSmooth #####
     * Eine Routine (IEnumerator) mit Parametern: die vom Spieler betretene Bodenplatte 'tile' und 
     * der Endpunkt der Translation 'direction'. 
     * Die Bodenplatte hat den Endpunkt erreicht, wenn _smoothTime erreicht wurde.
     */
    private IEnumerator MoveSmooth(GameObject go, Vector3 direction, float smoothTime)
    {
        _activeCoroutines++;
        float elapsedTime = 0;                          // zaehlen der bereits vergangenen Zeit
        Vector3 startingPos = go.transform.position;  // der Punkt, ab dem die Translation beginnt
        
        while (elapsedTime < smoothTime)
        {
            go.transform.position = Vector3.Lerp(startingPos, direction, (elapsedTime / smoothTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _activeCoroutines--;
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
    public void RestartGame()
    {
        _gameFinished = true;
        SceneManager.LoadScene(0);
    }

    /* 
     * ##### WaitForOtherRoutines #####
     * Wartet so lange, bis Routinen, die _activeCoroutines beeinflussen, vollstaendig beendet wurden
     */
    private IEnumerator WaitForOtherRoutines ()
    {
        while (_activeCoroutines != 0)
        {
            yield return null;
        }
    }

    /* 
     * ##### TriggerTrapAnimation #####
     * Spielt die Animation fuer das Ausloesen einer Falle ab
     */
    private IEnumerator TriggerTrapAnimation (Vector3 currentTilePos)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        GameObject trap = Instantiate(TrapsPrefab_Tiles[Random.Range(0, TrapsPrefab_Tiles.Length)], currentTilePos - offset, Quaternion.identity);
        float trapScale = trap.transform.Find("base").localScale.y;
        StartCoroutine(MoveSmooth(trap, new Vector3(currentTilePos.x, currentTilePos.y + _boardInfo.TilePrefab.transform.localScale.y / 2, currentTilePos.z), 0.2f));
        yield return WaitForOtherRoutines();
        yield return new WaitForSeconds(0.3f); // Falle bleibt kurz stehen
        StartCoroutine(MoveSmooth(trap, currentTilePos - offset, 0.2f));
        yield return WaitForOtherRoutines();
        Destroy(trap);
    }

    /* 
     * ##### ResetPlayer #####
     * Routine zum Zuruecksetzen des Spielers auf die Startposition innerhalb eines uebergebenen Zeitraums
     */
    private IEnumerator ResetPlayer(float resetTime)
    {
        // Spieler zuruecksetzen
        yield return new WaitForSeconds(resetTime / 2);
        _player.isDead();
        yield return new WaitForSeconds(resetTime / 2);
        _player.isAlive();
    }
}