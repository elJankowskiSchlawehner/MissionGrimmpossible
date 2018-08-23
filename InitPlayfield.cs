using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitPlayfield : MonoBehaviour
{
    // Konstanten
    //
    private Vector3 SPAWNER_V;                          // Startpunkt der Generierung, darf nicht veraendert werden
    private const bool IS_CORRECT_TILE = true;
    private const bool IS_WRONG_TILE = false;

    // Variablen
    //
    public int Height = 3;                              // Hoehe des Spielfelds
    public int Width = 5;                               // Breite des Spielfelds
    private bool[,] _tilesField;                        // [hoehe,breite], verwaltet den Bodenplattentyp und gibt Spielfeldgroesse an
    public float TileOffset = 0.0f;                     // Abstand zwischen den einzelnen Tiles
    [HideInInspector]
    public float TileWidth, TileHeight;                 // public, da andere Skripte auf diese Variable zugreifen [HIDE]
    public float PlayerSpawnHeight = 10f;

    // Prefabs - per Inspector einfuegen!
    public GameObject TilePrefab;                       // prefab der Bodenplatten
    public GameObject StartPrefab;                      // prefab der Spawn-Position
    public GameObject Player;                           // Spieler prefab
    public GameObject EnvironmentPrefab;                // prefab der Spielumgebung
    public GameObject Glass;
    public Material [] PaintingsField;                  // alle Gemaelde-Materialien

    // Use this for initialization
    void Start()
    {
        SPAWNER_V = transform.position;                 // Initialiserung des Startpunkts

        // Fehlerfall abdecken, falls Groesse und Hoehe des Spielfelds kleiner als 1
        if (Width < 1)
        {
            Width = 1;
        }
        if (Height < 1)
        {
            Height = 1;
        }

        _tilesField = new bool[Height, Width];          

        int posX = (int)Random.Range(0, Width);         // Startwert auf der X-Achse wird gewuerfelt
        int posY = 0;                                   // zaehlt die "Zeilen" des Arrays tilesField hoch, Start bei 0

        _tilesField[posY, posX] = IS_CORRECT_TILE;      // Startwert als ersten Wert in das Spielfeld eintragen

        int pathDirection = 0;                          // speichert die moeglichen Richtungen des Pfades, initial = 0 (vorwaerts)
        int pathLength;                                 // speichert die Laenge des Pfads in die gewuerfelte Richtung
        int pathEnd;                                    // Abbruchbedingung for-Schleife, ansonsten wird currentPosition in der Bedingung ueberschrieben
        int maxForwardLength = (Height / 3 + 1);        // Laenge des Pfads kann sich je nach Einstellung aendern. DEFAULT: (height - currentPosY)

        TileOffset = Mathf.Abs(TileOffset);             
        TileWidth = TilePrefab.transform.localScale.x + TileOffset;   // speichert die Breite des Tile Prefabs
        TileHeight = TilePrefab.transform.localScale.z + TileOffset;  // speichert die Laenge des Tile Prefabs
        bool isRight = false, isLeft = false;           // geben die Richtung des letzten Schritts an

        int heightCounter = 1;                          // verhindert einen aufeinanderfolgenden links-rechts-Schritt, = 1, da Startwert als ein Schritt gilt, ansonsten = 2

        // legt den richtigen Laufpfad fest
        while (posY < (Height - 1))
        {
            // welche Richtung wurde gewaehlt, Startwert ist immer vorwaerts
            switch (pathDirection)
            {
                // PFADRICHTUNG VORWAERTS
                case 0:
                    pathLength = (int)Random.Range(1, maxForwardLength);
                    heightCounter -= pathLength;            // Hoehenunterschied durch Schritt berechnen

                    pathEnd = posY + pathLength;
                    for (int indx = (posY + 1); indx <= pathEnd && posY < (Height - 1); indx++)
                    {
                        _tilesField[indx, posX] = IS_CORRECT_TILE;
                        posY = indx;
                    }

                    // naechste Pfadrichtung waehlen
                    //
                    // Fall abdecken, falls Breite doch mal 1 entsprechen sollte
                    if (Width == 1)
                    {
                        pathDirection = 0;
                        break;
                    }

                    // Normalfaelle - Hoehenunterschied von 
                    if (heightCounter <= 0)
                    {
                        if (posX == 0)                      // Position ganz links --> naechste Wegrichtung ist rechts
                        {
                            pathDirection = 1;
                        }
                        else if (posX == (Width - 1))     // Position ganz rechts --> neachste Wegrichtung ist links
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
                        if (isLeft && posX != 0)
                        {
                            pathDirection = Random.Range(-1, 1);
                        }
                        else if (heightCounter > 0 && isRight && posX != (Width - 1))
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
                        pathLength = (int)Random.Range(1, (posX + 1));

                        pathEnd = posX - pathLength;
                        for (int indx = (posX - 1); indx >= pathEnd; indx--)
                        {
                            _tilesField[posY, indx] = IS_CORRECT_TILE;
                            posX = indx;
                        }
                        // naechste Pfadrichtung angeben
                        isLeft = true;
                    }
                    // PFADRICHTUNG RECHTS
                    else if (pathDirection > 0)
                    {
                        pathLength = (int)Random.Range(1, (Width - posX));

                        pathEnd = posX + pathLength;
                        for (int indx = (posX + 1); indx <= pathEnd; indx++)
                        {
                            _tilesField[posY, indx] = IS_CORRECT_TILE;
                            posX = indx;
                        }
                        isRight = true;
                    }
                    // naechste Pfadrichtung ist vorwaerts
                    pathDirection = 0;
                    heightCounter = 2;
                    break;
            }
        } // ENDE WHILE

        // Generierung des Spielfelds
        CreateBoard();
    }

    /* 
    * ##### CreateBoard #####
    * 
    * erwartet keine Parameter und liefert void zurueck.
    * Ruft alle folgenden unabhängige Methoden auf:
    * 1. CreatePlayfield    - das eigentliche Spielbrett erstellen
    * 2. CreateSpawn        - der Bereich ausserhalb des Spielfelds, in dem der Spieler spawnt (entweder als Tile oder zusammenhaengendes Mesh)
    * 3. CreateEnvironment  - die Szenerie und das Ambiente
    * 4. Spawn Player       - setzt den Spieler an seine Position
    */
    void CreateBoard()
    {
        //setzt die Bodenplatten ueber den Spawn-Platten
        Vector3 tileSpawn_V = CreateSpawnAsTiles();
        CreatePlayfield(tileSpawn_V);
        CreateEnvironment();

        SpawnPlayer();
    }

    /* 
     * ##### CreateTile #####
     * 
     * Erzeugt eine Bodenplatte und liefert diese zurueck.
     * Je nach Parameter wird eine andere Bodenplatte mit anderen
     * Komponenten erzeugt
     */
    GameObject CreateTile(bool tileType, Vector3 position_V)
    {
        GameObject tile = Instantiate(TilePrefab, position_V, Quaternion.identity);

        if (tileType == IS_CORRECT_TILE)
        {
            tile.name = "correctTile";
        }
        else
        {
            // Tile bearbeiten und ein Kind-Element erstellen
            tile.name = "wrongTile";
            GameObject wrongTile = new GameObject("triggerElem");
            wrongTile.transform.parent = tile.transform;
            wrongTile.transform.localPosition = Vector3.zero;
            wrongTile.tag = "wrongTile";

            // den Collider zur Erkennung des Spielers erstellen
            BoxCollider bc = wrongTile.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            //bc.size = new Vector3(0.5f, 0.5f, 0.5f);
        }
        return tile;
    }

    /* 
     * ##### SpawnPlayer #####
     */
    void SpawnPlayer()
    {
        // Spielerfigur setzen
        Vector3 resetPoint_V = SPAWNER_V;
        resetPoint_V.x += (Width / 2) * TileWidth;
        resetPoint_V.y += (TilePrefab.transform.localScale.y / 2);
        Vector3 playerSpawnV = new Vector3(resetPoint_V.x, resetPoint_V.y + PlayerSpawnHeight, resetPoint_V.z);
        GameObject player = Instantiate(Player, playerSpawnV, Quaternion.identity);

        // Spielfigur - Einstellungen vornehmen
        player.name = "player";
        resetPoint_V.y += player.transform.localScale.y;
        player.GetComponent<Player>().ResetPoint_V = resetPoint_V;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        // Glas unter Spielfigur setzen
        Vector3 glassSpawn_V = new Vector3(playerSpawnV.x, playerSpawnV.y - player.transform.localScale.y, playerSpawnV.z);
        Instantiate(Glass, glassSpawn_V, Quaternion.identity);
    }

    /* 
     * ##### CreateSpawnAsTiles #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    Vector3 CreateSpawnAsTiles()
    {
        Vector3 start_V = SPAWNER_V;
        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
        for (int i = 0; i < Width; i++)
        {
            Instantiate(StartPrefab, start_V, Quaternion.identity);
            start_V.x += TileWidth;
        }
        start_V.x = SPAWNER_V.x;
        start_V.z += TileHeight;

        // Vektor fuer createBoard zurueckgeben
        return start_V;
    }

    /* 
     * ##### CreateSpawn #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    Vector3 CreateSpawnAsCube()
    {
        Vector3 startV = SPAWNER_V;
        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
        for (int i = 0; i < Width; i++)
        {
            Instantiate(StartPrefab, startV, Quaternion.identity);
            startV.x += TileWidth;
        }
        startV.x = SPAWNER_V.x;
        startV.z += TileHeight;

        // Vektor fuer createBoard zurueckgeben
        return startV;
    }

    /* 
     * ##### CreatePlayfield #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    void CreatePlayfield(Vector3 tileSpawnV)
    {

        // platziere die restlichen Bodenplatten
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                // korrekte Bodenplatte legen bei Übereinstimmung
                if (_tilesField[i, j] == IS_CORRECT_TILE)
                {
                    CreateTile(IS_CORRECT_TILE, tileSpawnV);
                }
                // falsche Bodenplatte legen
                else
                {
                    CreateTile(IS_WRONG_TILE, tileSpawnV);
                }
                tileSpawnV.x += TileWidth;
            }
            tileSpawnV.x = SPAWNER_V.x;
            tileSpawnV.z += TileHeight;
        } // ENDE for
    }

    /* 
     * ##### CreateEnvironment #####
     * 
     * Erzeugt die Szenerie, um das Spielbrett herum
     */
    void CreateEnvironment()
    {
        Vector3 envSpawnV = new Vector3  (SPAWNER_V.x - (TilePrefab.transform.localScale.x / 2) - (EnvironmentPrefab.transform.GetChild(0).transform.localScale.x / 2)
                                        , SPAWNER_V.y, 
                                        SPAWNER_V.z - (TilePrefab.transform.localScale.z / 2) + (EnvironmentPrefab.transform.GetChild(0).transform.localScale.z / 2));

        for (int i = 0; i < 5; i++)  // TESTANZAHL
        {
            GameObject environment = EnvironmentPrefab;

            environment.name = "border_left" + i;
            environment.transform.GetChild(4).GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            environment.transform.Find("painting_right").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            Instantiate(environment, envSpawnV, Quaternion.identity);

            environment.name = "border_right" + i;
            environment.transform.Find("painting_left").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            environment.transform.Find("painting_right").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            Instantiate(environment, envSpawnV + new Vector3((TileWidth * Width) - TileOffset + environment.transform.GetChild(0).transform.localScale.x, 0, 0), Quaternion.Euler(0,180,0));

            envSpawnV.z += environment.transform.GetChild(0).transform.localScale.z;
        }
    }

    /* 
     * ##### CreateFinish #####
     */
    void CreateFinish()
    {

    }
}
