using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour {

    private PlayfieldObserver _observer;
    private PlayfieldInitialiser _boardInfo;

    public GameObject[] TrapsPrefab_Tiles;                          // Prefabs, die bei Spielertot unter den Tiles gespawned werden --> Aufwaertsbewegung
    public GameObject[] TrapsPrefab_Misc;                           // sonstige Prefabs, die an anderen Positionen gespawned werden

    // Use this for initialization
    void Start () {
        _observer = GetComponent<PlayfieldObserver>();
        _boardInfo = GetComponent<PlayfieldInitialiser>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TriggerRandomTrap()
    {

    }

    /* 
     * ##### TriggerTrapAnimation #####
     * Spielt die Animation fuer das Ausloesen einer Falle ab
     */
    public IEnumerator TriggerTrap_Tiles(Vector3 currentTilePos)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        GameObject trap = Instantiate(TrapsPrefab_Tiles[Random.Range(0, TrapsPrefab_Tiles.Length)], currentTilePos - offset, Quaternion.identity);
        float trapScale = trap.transform.Find("base").localScale.y;
        StartCoroutine(_observer.MoveSmooth(trap, new Vector3(currentTilePos.x, currentTilePos.y + _boardInfo.TilePrefab.transform.localScale.y / 2, currentTilePos.z), 0.2f));
        yield return _observer.WaitForOtherRoutines();
        yield return new WaitForSeconds(0.3f); // Falle bleibt kurz stehen
        StartCoroutine(_observer.MoveSmooth(trap, currentTilePos - offset, 0.2f));
        yield return _observer.WaitForOtherRoutines();
        Destroy(trap);
    }
}
