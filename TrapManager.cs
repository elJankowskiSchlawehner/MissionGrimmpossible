using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour {

    private PlayfieldObserver _observer;
    private PlayfieldInitialiser _boardInfo;

    public GameObject[] TrapsPrefab_Tiles;                          // Prefabs, die bei Spielertot unter den Tiles gespawned werden --> Aufwaertsbewegung
    //public GameObject[] TrapsPrefab_Misc;                           // sonstige Prefabs, die an anderen Positionen gespawned werden
    public GameObject[] Particles;

    private bool _animationFinished = true;

    public float _trapAnimationTime = 1.0f;
    private float _idleTime;                                        // Falle bleibt diese Zeit kurz stehen und sichtbar und macht etwas
    private float _moveTime;                                        // wird zweimal ausgefuehrt, bei Auf- und Abwaertsbewegung

    // Use this for initialization
    void Start () {
        _observer = GetComponent<PlayfieldObserver>();
        _boardInfo = GetComponent<PlayfieldInitialiser>();

        _idleTime = _trapAnimationTime / 2;
        _moveTime = (_trapAnimationTime - _idleTime) / 2;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator TriggerRandomTrap(Vector3 currentTilePos, Vector3 playerPos)
    {
        if (Random.Range(0f, 1f) < 0.5f) StartCoroutine(TriggerTrap_Tiles(currentTilePos)); else StartCoroutine(TriggerTrap_Misc(playerPos));
        while (!_animationFinished)
        {
            yield return null;
        }
    }

    /* SPEATER PRIVATE
     * ##### TriggerTrapAnimation #####
     * Spielt die Animation fuer das Ausloesen einer Falle ab
     */
    private IEnumerator TriggerTrap_Tiles(Vector3 currentTilePos)
    {
        _animationFinished = false;

        Vector3 offset = new Vector3(0, 0.5f, 0);
        GameObject trap = Instantiate(TrapsPrefab_Tiles[Random.Range(0, TrapsPrefab_Tiles.Length)], currentTilePos - offset, Quaternion.identity);
        float trapScale = trap.transform.Find("base").localScale.y;
        // Aufwaertsbewegung, Falle erscheint
        StartCoroutine(_observer.MoveSmooth(trap, new Vector3(currentTilePos.x, currentTilePos.y + _boardInfo.TilePrefab.transform.localScale.y / 2, currentTilePos.z), _moveTime));
        yield return _observer.WaitForOtherRoutines();
        // Falle bleibt kurz stehen
        yield return new WaitForSeconds(_idleTime);
        // Abwaertsbewegung
        StartCoroutine(_observer.MoveSmooth(trap, currentTilePos - offset, _moveTime));
        yield return _observer.WaitForOtherRoutines();
        // Falle wird abschliessend dereferenziert
        Destroy(trap);

        _animationFinished = true;
    }

    private IEnumerator TriggerTrap_Misc(Vector3 playerPos)
    {
        _animationFinished = false;

        List<Vector3> triggerSpawns = new List<Vector3>();
        RaycastHit hit;
        if (Physics.Raycast(playerPos, Vector3.left, out hit, _boardInfo.WidthPlayfield * _boardInfo.TileWidth))
        {
            triggerSpawns.Add(hit.collider.transform.position);
        }
        if (Physics.Raycast(playerPos, Vector3.right, out hit, _boardInfo.WidthPlayfield * _boardInfo.TileWidth))
        {
            triggerSpawns.Add(hit.collider.transform.position);
        }
        //triggerSpawns.Add(playerPos + new Vector3(0, 30, 0));

        //GameObject trap = Instantiate(TrapsPrefab_Misc[Random.Range(0, TrapsPrefab_Misc.Length)], new Vector3(6, 0, 15), Quaternion.identity);
        //Vector3 start = trap.transform.Find("shootingPos").position;
        GameObject ps = Instantiate(Particles[Random.Range(0, Particles.Length)], triggerSpawns[Random.Range(0, triggerSpawns.Count)], Quaternion.identity);
        StartCoroutine(_observer.MoveSmooth(ps, playerPos, 0.3f));
        yield return _observer.WaitForOtherRoutines();

        yield return new WaitForSeconds(_idleTime);

        //Destroy(trap);
        Destroy(ps, 2.0f);

        _animationFinished = true;
    }
}
