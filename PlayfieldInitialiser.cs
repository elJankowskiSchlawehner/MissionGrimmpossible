using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfieldInitialiser : MonoBehaviour
{
    // Konstanten
    //
    private Vector3 SPAWNER_V = Vector3.zero;                          // Startpunkt der Generierung, darf nicht veraendert werden
    private const bool IS_CORRECT_TILE = true;
    private const bool IS_WRONG_TILE = false;
    
    // Variablen
    //
    public int HeightPlayfield = 3;                     
    public int WidthPlayfield = 5;                      
    private bool[,] _tilesField;                        // [hoehe,breite], verwaltet den Bodenplattentyp und gibt Spielfeldgroesse an
    public float TileOffset = 0.0f;                     // Abstand zwischen den einzelnen Tiles
    [HideInInspector]
    public float TileWidth, TileHeight;                 // public, da andere Skripte auf diese Variable zugreifen [HIDE]
    public float PlayerSpawnHeight = 10f;

    // Prefabs - per Inspector einfuegen!
    //
    public GameObject TilePrefab;                       // prefab der Bodenplatten
    public GameObject Player;                           // Spieler prefab
    public GameObject BorderPrefab;                     // prefab der Spielumgebung
    public GameObject Glass;
    public GameObject Showcase;
    // Materialien der verschiedenen 3D Objekte
    public Material PlayfieldTexture;
    public Material SpawnTexture;
    public Material ObjectiveTexture;
    public Material [] PaintingsField;                  // alle Gemaelde-Materialien

    // Use this for initialization
    void Awake()
    {
        // Fehlerfall abdecken, falls Groesse und Hoehe des Spielfelds kleiner als 1
        if (WidthPlayfield < 1)
        {
            WidthPlayfield = 1;
        }
        if (HeightPlayfield < 1)
        {
            HeightPlayfield = 1;
        }

        _tilesField = new bool[HeightPlayfield, WidthPlayfield];          

        int posX = (int)Random.Range(0, WidthPlayfield);    // Startwert auf der X-Achse wird gewuerfelt
        int posY = 0;                                   // zaehlt die "Zeilen" des Arrays tilesField hoch, Start bei 0

        _tilesField[posY, posX] = IS_CORRECT_TILE;      // Startwert als ersten Wert in das Spielfeld eintragen

        int pathDirection = 0;                          // speichert die moeglichen Richtungen des Pfades, initial = 0 (vorwaerts)
        int pathLength;                                 // speichert die Laenge des Pfads in die gewuerfelte Richtung
        int pathEnd;                                    // Abbruchbedingung for-Schleife, ansonsten wird currentPosition in der Bedingung ueberschrieben
        int maxForwardLength = (HeightPlayfield / 3 + 1);   // Laenge des Pfads kann sich je nach Einstellung aendern. DEFAULT: (height - currentPosY)

        TileOffset = Mathf.Abs(TileOffset);             
        TileWidth = TilePrefab.transform.localScale.x + TileOffset;   // speichert die Breite des Tile Prefabs
        TileHeight = TilePrefab.transform.localScale.z + TileOffset;  // speichert die Laenge des Tile Prefabs
        bool isRight = false, isLeft = false;           // geben die Richtung des letzten Schritts an

        int heightCounter = 1;                          // verhindert einen aufeinanderfolgenden links-rechts-Schritt, = 1, da Startwert als ein Schritt gilt, ansonsten = 2

        // legt den richtigen Laufpfad fest
        while (posY < (HeightPlayfield - 1))
        {
            // welche Richtung wurde gewaehlt, Startwert ist immer vorwaerts
            switch (pathDirection)
            {
                // PFADRICHTUNG VORWAERTS
                case 0:
                    pathLength = (int)Random.Range(1, maxForwardLength);
                    heightCounter -= pathLength;            // Hoehenunterschied durch Schritt berechnen

                    pathEnd = posY + pathLength;
                    for (int indx = (posY + 1); indx <= pathEnd && posY < (HeightPlayfield - 1); indx++)
                    {
                        _tilesField[indx, posX] = IS_CORRECT_TILE;
                        posY = indx;
                    }

                    // naechste Pfadrichtung waehlen
                    //
                    // Fall abdecken, falls Breite doch mal 1 entsprechen sollte
                    if (WidthPlayfield == 1)
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
                        else if (posX == (WidthPlayfield - 1))     // Position ganz rechts --> neachste Wegrichtung ist links
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
                        else if (heightCounter > 0 && isRight && posX != (WidthPlayfield - 1))
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
                        pathLength = (int)Random.Range(1, (WidthPlayfield - posX));

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
    * 2. CreateFinish_*     - der Endbereich samt Ziel
    * 3. CreateSpawn_*      - der Bereich ausserhalb des Spielfelds, in dem der Spieler spawnt (entweder als Tile oder zusammenhaengendes Mesh)
    * 4. CreateEnvironment  - die Szenerie und das Ambiente
    * 5. Spawn Player       - setzt den Spieler an seine Position
    */
    private void CreateBoard()
    {
        Vector3 endField = CreatePlayfield();
        Vector3 endFinish = CreateFinish_Tiles(endField);
        CreateAmbience(endFinish);

        CreateSpawn_Tiles();
        SpawnPlayer();

        //gameObject.AddComponent<PlayfieldObserver>();
    }

    /* 
     * ##### CreatePlayfield #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    private Vector3 CreatePlayfield()
    {
        Vector3 tileSpawnV = SPAWNER_V;
        // platziere die restlichen Bodenplatten
        for (int i = 0; i < HeightPlayfield; i++)
        {
            for (int j = 0; j < WidthPlayfield; j++)
            {
                GameObject tile = CreateTile(PlayfieldTexture, tileSpawnV);
                // an die Bodenplatte wird ein Kind-Element angehaengt, das im Spiel die Kollision mit dem Spieler abfragen soll
                GameObject triggerContainer = new GameObject("triggerElem");
                triggerContainer.transform.parent = tile.transform;
                triggerContainer.transform.localPosition = Vector3.zero;
                BoxCollider bc = triggerContainer.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.size = new Vector3(1f, 2f, 1f);

                // korrekte Bodenplatte legen bei Übereinstimmung
                if (_tilesField[i, j] == IS_CORRECT_TILE)
                {
                    tile.name = "correctTile";
                    triggerContainer.tag = "correctTile";
                }
                // falsche Bodenplatte legen
                else
                {
                    tile.name = "wrongTile";
                    triggerContainer.tag = "wrongTile";
                }

                tileSpawnV.x += TileWidth;
            }
            tileSpawnV.x = SPAWNER_V.x;
            tileSpawnV.z += TileHeight;
        } // ENDE for



        return tileSpawnV;
    }

    /* 
     * ##### CreateTile #####
     * 
     * Erzeugt eine Bodenplatte und liefert diese zurueck.
     * Je nach Material entsteht ein anderer Bodenplattentyp
     */
    private GameObject CreateTile(Material mat, Vector3 position_V)
    {
        GameObject tile = Instantiate(TilePrefab, position_V, Quaternion.identity);
        tile.GetComponent<Renderer>().material = mat;

        return tile;
    }

    /* 
     * ##### SpawnPlayer #####
     */
    private void SpawnPlayer()
    {
        // Spielerfigur setzen
        Vector3 plaver_V = SPAWNER_V;
        plaver_V.x += (WidthPlayfield / 2) * TileWidth;
        plaver_V.y += (TilePrefab.transform.localScale.y / 2);
        plaver_V.z -= TileHeight;

        Vector3 playerSpawnV = new Vector3(plaver_V.x, plaver_V.y + PlayerSpawnHeight, plaver_V.z);
        GameObject player = Instantiate(Player, playerSpawnV, Quaternion.identity);

        // Spielfigur - Einstellungen vornehmen
        player.name = "Player";
        plaver_V.y += player.transform.localScale.y;
        player.GetComponent<Player>().ResetPoint_V = plaver_V;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        // Glas unter Spielfigur setzen
        Vector3 glassSpawn_V = new Vector3(playerSpawnV.x, playerSpawnV.y - player.transform.localScale.y, playerSpawnV.z);
        Instantiate(Glass, glassSpawn_V, Quaternion.identity);
    }

    /* 
     * ##### CreateSpawn_Tiles #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    private void CreateSpawn_Tiles()
    {
        Vector3 position_V = new Vector3 (SPAWNER_V.x, SPAWNER_V.y, SPAWNER_V.z - TileHeight);
        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < WidthPlayfield; i++)
            {
                CreateTile(SpawnTexture, position_V);
                position_V.x += TileWidth;
            }
            position_V.x = SPAWNER_V.x;
            position_V.z -= TileHeight;
        }

    }

    /* 
     * ##### CreateEnvironment #####
     * 
     * Erzeugt die Szenerie, um das Spielbrett herum
     */
    private void CreateAmbience(Vector3 position_V)
    {
        Vector3 borderBase = BorderPrefab.transform.Find("base").transform.localScale;  // die eigentliche Breite des Objekts (da Elternobjekt andere Breite besitzt)

        // Generiere Wand am Ende des Flurs
        GameObject rearwall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rearwall.name = "rearwall";
        float rearwallScaleX = TileWidth * WidthPlayfield + borderBase.x * 2;
        float rearwallScaleY = 10f;
        float rearwallScaleZ = 1f;
        rearwall.transform.localScale = new Vector3(rearwallScaleX, rearwallScaleY, rearwallScaleZ);
        position_V.x = position_V.x + (TileWidth * WidthPlayfield) / 2 - (TilePrefab.transform.localScale.x / 2);
        position_V.y = position_V.y + TilePrefab.transform.localScale.y / 2;
        position_V.z = position_V.z - (TileHeight / 2) + (rearwall.transform.localScale.z / 2) - TileOffset / 2;
        rearwall.transform.position = new Vector3(position_V.x, position_V.y + (rearwallScaleY / 2), position_V.z);

        Vector3 ambienceSpawn_V = new Vector3 (
                                                position_V.x - (rearwallScaleX / 2) + (borderBase.x / 2), 
                                                position_V.y, 
                                                position_V.z - (rearwallScaleZ / 2) - (borderBase.z / 2)
                                                );

        // Generiere Seitenbegrenzungen
        int nameCnt = 0;
        do
        {
            BorderPrefab.name = "border_left" + nameCnt;
            BorderPrefab.transform.Find("painting_left").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            BorderPrefab.transform.Find("painting_right").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            Instantiate(BorderPrefab, ambienceSpawn_V, Quaternion.identity);

            BorderPrefab.name = "border_right" + nameCnt;
            BorderPrefab.transform.Find("painting_left").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            BorderPrefab.transform.Find("painting_right").GetComponent<Renderer>().material = PaintingsField[Random.Range(0, PaintingsField.Length)];
            Instantiate(BorderPrefab, ambienceSpawn_V + new Vector3((TileWidth * WidthPlayfield) - TileOffset + borderBase.x, 0, 0), Quaternion.Euler(0, 180, 0));

            ambienceSpawn_V.z -= borderBase.z;
            nameCnt++;
        } while (ambienceSpawn_V.z >= SPAWNER_V.z - borderBase.z * 2);
    }

    /* 
     * ##### CreateFinish_Tiles #####
     */
    private Vector3 CreateFinish_Tiles(Vector3 position_V)
    {
        int length = 2;

        Instantiate(Showcase, new Vector3(position_V.x + (WidthPlayfield - 1) * TileWidth / 2, position_V.y + TilePrefab.transform.localScale.y / 2, position_V.z + TileHeight * (length - 1)), Quaternion.identity);

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < WidthPlayfield; j++)
            {
                CreateTile(ObjectiveTexture, position_V);
                position_V.x += TileWidth;
            }
            position_V.x = SPAWNER_V.x;
            position_V.z += TileHeight;
        }

        return position_V;
    }

    /* 
     * ##### CreateSpawn_Mesh #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    private void CreateSpawn_Mesh()
    {
        Vector3 spawn_V = SPAWNER_V;
        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
    }

    /* 
     * ##### CreateFinish_Mesh #####
     */
    private void CreateFinish_Mesh()
    {

    }
}