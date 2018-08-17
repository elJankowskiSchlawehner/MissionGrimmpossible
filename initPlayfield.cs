using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initPlayfield : MonoBehaviour
{
    // Makros
    private const bool isCorrectTile = true;
    private const bool isWrongTile = false;

    //Variablen
    //
    public int height = 3;                                  // Hoehe des Spielfelds
    public int width = 5;                                   // Breite des Spielfelds
    private bool[,] tilesField;                             // [hoehe,breite], verwaltet den Bodenplattentyp und gibt Spielfeldgroesse an

    public GameObject tilePrefab;                           // prefab der Bodenplatten
    public GameObject startPrefab;                          // prefab der Spawn-Position
    public GameObject player;                               // Spieler prefab

    public float tileOffset = 0.0f;
    [HideInInspector]
    public float tileWidth, tileHeight;

    public Vector3 spawnerV = new Vector3(0, 0, 0);         // Startpunkt, ab dem die Tiles gesetzt werden

    // Use this for initialization
    void Start()
    {
        if (width < 1)
        {
            width = 1;
        }
        if (height < 1)
        {
            height = 1;
        }

        tilesField = new bool[height, width];               // initialisiert das Spielfeld

        int currentPosX = (int)Random.Range(0, width);      // Startwert (X-Achse) wird gewuerfelt
        int currentPosY = 0;                                // zaehlt die "Zeilen" des Arrays tilesField hoch, bei Start 0

        tilesField[currentPosY, currentPosX] = isCorrectTile;   // Startwert in das Spielfeld eintragen

        int pathDirection = 0;                              // speichert die moeglichen Richtungen des Pfades, initial = 0 (vorwaerts)
        int pathLength;                                     // speichert die Laenge des Pfads in die gewuerfelte Richtung
        int pathEnd;                                        // Abbruchbedingung for-Schleife, ansonsten wird currentPosition in der Bedingung ueberschrieben
        int maxForwardLength = (height / 3 + 1);            // Laenge des Pfads kann sich je nach Einstellung aendern. DEFAULT: (height - currentPosY)

        tileWidth = tilePrefab.transform.localScale.x + tileOffset;
        tileHeight = tilePrefab.transform.localScale.z + tileOffset;

        bool isRight = false, isLeft = false;               // geben die Richtung des letzten Schritts an

        int heightCounter = 1;                              // verhindert einen aufeinanderfolgenden links-rechts-Schritt, = 1, da Startwert als ein Schritt gilt, ansonsten = 2

        // Schleife bricht ab, wenn in letzter Zeile des Arrays eine Bodenplatte gesetzt wurde
        while (currentPosY < (height - 1))
        {
            // welche Richtung wurde gewaehlt, Startwert ist immer vorwaerts
            switch (pathDirection)
            {
                // PFADRICHTUNG VORWAERTS
                case 0:
                    pathLength = (int)Random.Range(1, maxForwardLength);
                    heightCounter -= pathLength;            // Hoehenunterschied durch Schritt berechnen

                    pathEnd = currentPosY + pathLength;
                    for (int indx = (currentPosY + 1); indx <= pathEnd && currentPosY < (height - 1); indx++)
                    {
                        tilesField[indx, currentPosX] = isCorrectTile;
                        currentPosY = indx;
                    }

                    // naechste Pfadrichtung waehlen
                    //
                    // Fall abdecken, falls Breite doch mal 1 entsprechen sollte
                    if (width == 1)
                    {
                        pathDirection = 0;
                        break;
                    }

                    // Normalfaelle - Hoehenunterschied von 
                    if (heightCounter <= 0)
                    {
                        if (currentPosX == 0)               // Position ganz links --> naechste Wegrichtung ist rechts
                        {
                            pathDirection = 1;
                        }
                        else if (currentPosX == (width - 1))    // Position ganz rechts --> neachste Wegrichtung ist links
                        {
                            pathDirection = -1;
                        }
                        else                                // ansonsten wuerfel aus, ob links oder rechts
                        {
                            pathDirection = (Random.Range(0f, 1f) < 0.5f) ? pathDirection = -1 : pathDirection = 1;
                        }
                        isLeft = false;
                        isRight = false;
                    }
                    else
                    {
                        if (isLeft && currentPosX != 0)
                        {
                            pathDirection = Random.Range(-1, 1);
                        }
                        else if (heightCounter > 0 && isRight && currentPosX != (width - 1))
                        {
                            pathDirection = Random.Range(0, 2);
                        }
                        // ansonsten bleibt pathDirection unveraendert, muss nicht abgefragt werden
                    }
                    break;

                // PFADRICHTUNG LINKS ODER RECHTS
                default:
                    // PFADRICHTUNG LINKS
                    if (pathDirection < 0)
                    {
                        pathLength = (int)Random.Range(1, (currentPosX + 1));

                        pathEnd = currentPosX - pathLength;
                        for (int indx = (currentPosX - 1); indx >= pathEnd; indx--)
                        {
                            tilesField[currentPosY, indx] = isCorrectTile;
                            currentPosX = indx;
                        }
                        // naechste Pfadrichtung angeben
                        isLeft = true;
                    }
                    // PFADRICHTUNG RECHTS
                    else if (pathDirection > 0)
                    {
                        pathLength = (int)Random.Range(1, (width - currentPosX));

                        pathEnd = currentPosX + pathLength;
                        for (int indx = (currentPosX + 1); indx <= pathEnd; indx++)
                        {
                            tilesField[currentPosY, indx] = isCorrectTile;
                            currentPosX = indx;
                        }
                        isRight = true;
                    }
                    // naechste Pfadrichtung ist vorwaerts
                    pathDirection = 0;
                    heightCounter = 2;
                    break;
            }
        } // ENDE WHILE

        createBoard();
    } // ENDE START - Feld fertig initialisiert und erstellt

    // Update is called once per frame
    void Update()
    {

    }

    /* 
     * ##### placeTile #####
     * 
     * Erzeugt eine Bodenplatte und liefert diese zurueck.
     * Je nach Parameter wird eine andere Bodenplatte mit anderen
     * Komponenten erzeugt
     */
    GameObject placeTile(bool tileType, Vector3 position)
    {
        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);

        if (tileType == isCorrectTile)
        {
            tile.GetComponent<Renderer>().material.color = Color.green;     //zu Testzwecken die richtigen Bodenplatten gruen faerben
        }
        else
        {

        }

        return tile;
    }

    /* 
     * ##### createBoard #####
     * 
     * erwartet keine Parameter und liefert void zurueck.
     * Durchlaeuft das Spielfeld-Array und legt so die Bodenplatten
     */
    void createBoard()
    {
        Vector3 tileSpawnV = spawnerV;
        Vector3 playerSpawnV = spawnerV;

        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
        for (int i = 1; i <= tilesField.GetLength(1); i++)
        {
            Instantiate(startPrefab, tileSpawnV, Quaternion.identity);
            tileSpawnV.x += tileWidth;
        }
        tileSpawnV.x = spawnerV.x;
        tileSpawnV.z += tileHeight;

        // Spielerfigur setzen
        playerSpawnV.x = (width / 2) * tileWidth;
        Instantiate(player, new Vector3(playerSpawnV.x, playerSpawnV.y + 10, playerSpawnV.z), Quaternion.identity);



        // platziere die restlichen Bodenplatten
        for (int i = 0; i < tilesField.GetLength(0); i++)
        {
            for (int j = 0; j < tilesField.GetLength(1); j++)
            {
                // korrekte Bodenplatte legen bei Übereinstimmung
                if (tilesField[i, j] == isCorrectTile)
                {
                    placeTile(isCorrectTile, tileSpawnV);
                }
                // falsche Bodenplatte legen
                else
                {
                    placeTile(isWrongTile, tileSpawnV);
                }

                tileSpawnV.x += tileWidth;
            }

            tileSpawnV.x = spawnerV.x;
            tileSpawnV.z += tileHeight;
        }
    }
}
