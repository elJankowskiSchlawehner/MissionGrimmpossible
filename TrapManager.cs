﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour {

    private PlayfieldObserver _gameObserver;
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
        _gameObserver = GetComponent<PlayfieldObserver>();
        _boardInfo = GetComponent<PlayfieldInitialiser>();

        //_idleTime = _trapAnimationTime / 2;
        //_moveTime = (_trapAnimationTime - _idleTime) / 2;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator TriggerRandomTrap(Vector3 currentTilePos, Vector3 playerPos)
    {
        // waehle eine der Fallenarten aus
        if (Random.Range(0f, 1f) < 0.5f) StartCoroutine(TriggerTrap_Tiles(currentTilePos)); else StartCoroutine(TriggerTrap_Turret(playerPos));
        while (!_animationFinished)     // solange die Animation der Falle noch nicht zuende ist, bleibe in dieser Funktion
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

        _idleTime = _trapAnimationTime / 2;
        _moveTime = (_trapAnimationTime - _idleTime) / 2;

        Vector3 offset = new Vector3(0, 0.5f, 0);
        GameObject trap = Instantiate(TrapsPrefab_Tiles[Random.Range(0, TrapsPrefab_Tiles.Length)], currentTilePos - offset, Quaternion.identity);
        float trapScale = trap.transform.Find("base").localScale.y;
        // Aufwaertsbewegung, Falle erscheint
        StartCoroutine(_gameObserver.MoveSmooth(trap, new Vector3(currentTilePos.x, currentTilePos.y + _boardInfo.TilePrefab.transform.localScale.y / 2, currentTilePos.z), _moveTime));
        yield return _gameObserver.WaitForOtherRoutines();
        // Falle bleibt kurz stehen
        yield return new WaitForSeconds(_idleTime);
        // Abwaertsbewegung
        StartCoroutine(_gameObserver.MoveSmooth(trap, currentTilePos - offset, _moveTime));
        yield return _gameObserver.WaitForOtherRoutines();
        // Falle wird abschliessend dereferenziert
        Destroy(trap);

        _animationFinished = true;
    }

    private IEnumerator TriggerTrap_Turret(Vector3 playerPos)
    {
        _animationFinished = false;
        _idleTime = _trapAnimationTime / 2;
        _moveTime = (_trapAnimationTime - _idleTime) / 3;

        List<Transform> shootingPos = new List<Transform>();
        RaycastHit hit;

        if (Physics.Raycast(playerPos, Vector3.left, out hit, _gameObserver.GetPlayfieldWidth() * _boardInfo.TileWidth))
        {
            shootingPos.Add(hit.collider.transform.parent.Find("turret").Find("shootingPos").transform);
        }
        if (Physics.Raycast(playerPos, Vector3.right, out hit, _gameObserver.GetPlayfieldWidth() * _boardInfo.TileWidth))
        {
            shootingPos.Add(hit.collider.transform.parent.Find("turret").Find("shootingPos").transform);
        }
        // welche Turret von welcher Seite wird genommen?
        int spawnIndx = Random.Range(0, shootingPos.Count);
        // Berechnung vor der eigentlichen Bewegung
        // Rotation Y-Achse
        GameObject turret = shootingPos[spawnIndx].parent.gameObject;
        Vector3 adjacent_V = turret.transform.TransformDirection(Vector3.forward);
        Vector3 hypotenuse_V = new Vector3(playerPos.x, 0, playerPos.z) - new Vector3(turret.transform.position.x, 0, turret.transform.position.z);
        float angleY = Vector3.Angle(adjacent_V, hypotenuse_V);
        Vector3 cross_V = Vector3.Cross(adjacent_V, hypotenuse_V);
        if (cross_V.y < 0)      // zeigt der Normalenvektor nach unten?
        {
            angleY = -angleY;   // ja, dann ist negativer Winkel
        }

        // Rotation X-Achse
        adjacent_V = turret.transform.TransformDirection(Vector3.forward);
        hypotenuse_V = playerPos - turret.transform.position;
        float angleX = Vector3.Angle(adjacent_V, hypotenuse_V);

        // eigentliche Bewegung der Falle
        yield return _gameObserver.RotateSmooth(turret, turret.transform.localRotation.eulerAngles.x + angleX, turret.transform.localRotation.eulerAngles.y + angleY, 0, _moveTime);
        
        // Turret Animation
        GameObject trap = Instantiate(Particles[Random.Range(0, Particles.Length)], shootingPos[spawnIndx].position, Quaternion.identity);
        StartCoroutine(_gameObserver.MoveSmooth(trap, playerPos, _moveTime));
        yield return _gameObserver.WaitForOtherRoutines();

        yield return new WaitForSeconds(_idleTime);

        // auf Ursprungsposition zuruecksetzen
        yield return _gameObserver.RotateSmooth(turret, 0, 90, 0, _moveTime);
        yield return _gameObserver.WaitForOtherRoutines();
        
        Destroy(trap, 2.0f);

        _animationFinished = true;
    }
}
