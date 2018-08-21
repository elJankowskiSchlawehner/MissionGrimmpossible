using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitPlayfield : MonoBehaviour
{
    // Makros
    private const bool IS_CORRECT_TILE = true;
    private const bool IS_WRONG_TILE = false;

    //Variablen
    //
    public int m_height = 3;                                // Hoehe des Spielfelds
    public int m_width = 5;                                 // Breite des Spielfelds
    private bool[,] _tilesField;                            // [hoehe,breite], verwaltet den Bodenplattentyp und gibt Spielfeldgroesse an

    public GameObject m_tilePrefab;                         // prefab der Bodenplatten
    public GameObject m_startPrefab;                        // prefab der Spawn-Position
    public GameObject m_player;                             // Spieler prefab
    public GameObject m_environmentPrefab;                  // prefab der Spielumgebung
    public GameObject m_glass;
    public Material [] m_paintingsField;                    // alle Gemaelde-Materialien

    public float m_tileOffset = 0.0f;
    [HideInInspector]
    public float m_tileWidth, m_tileHeight;

    private Vector3 m_spawnerV;                             // Startpunkt, ab dem alles generiert wird
    public float m_playerSpawnHeight = 10f;

    // Use this for initialization
    void Start()
    {
        m_spawnerV = transform.position;

        if (m_width < 1)
        {
            m_width = 1;
        }
        if (m_height < 1)
        {
            m_height = 1;
        }

        _tilesField = new bool[m_height, m_width];          // initialisiert das Spielfeld

        int posX = (int)Random.Range(0, m_width);           // Startwert (X-Achse) wird gewuerfelt
        int posY = 0;                                       // zaehlt die "Zeilen" des Arrays tilesField hoch, bei Start 0

        _tilesField[posY, posX] = IS_CORRECT_TILE;          // Startwert als ersten Wert in das Spielfeld eintragen

        int pathDirection = 0;                              // speichert die moeglichen Richtungen des Pfades, initial = 0 (vorwaerts)
        int pathLength;                                     // speichert die Laenge des Pfads in die gewuerfelte Richtung
        int pathEnd;                                        // Abbruchbedingung for-Schleife, ansonsten wird currentPosition in der Bedingung ueberschrieben
        int maxForwardLength = (m_height / 3 + 1);          // Laenge des Pfads kann sich je nach Einstellung aendern. DEFAULT: (height - currentPosY)

        m_tileWidth = m_tilePrefab.transform.localScale.x + m_tileOffset;   // speichert die Breite des Tile Prefabs
        m_tileHeight = m_tilePrefab.transform.localScale.z + m_tileOffset;  // speichert die Laenge des Tile Prefabs

        bool isRight = false, isLeft = false;               // geben die Richtung des letzten Schritts an

        int heightCounter = 1;                              // verhindert einen aufeinanderfolgenden links-rechts-Schritt, = 1, da Startwert als ein Schritt gilt, ansonsten = 2

        // befuellen der Spielfeldposition - bis 
        while (posY < (m_height - 1))
        {
            // welche Richtung wurde gewaehlt, Startwert ist immer vorwaerts
            switch (pathDirection)
            {
                // PFADRICHTUNG VORWAERTS
                case 0:
                    pathLength = (int)Random.Range(1, maxForwardLength);
                    heightCounter -= pathLength;            // Hoehenunterschied durch Schritt berechnen

                    pathEnd = posY + pathLength;
                    for (int indx = (posY + 1); indx <= pathEnd && posY < (m_height - 1); indx++)
                    {
                        _tilesField[indx, posX] = IS_CORRECT_TILE;
                        posY = indx;
                    }

                    // naechste Pfadrichtung waehlen
                    //
                    // Fall abdecken, falls Breite doch mal 1 entsprechen sollte
                    if (m_width == 1)
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
                        else if (posX == (m_width - 1))     // Position ganz rechts --> neachste Wegrichtung ist links
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
                        else if (heightCounter > 0 && isRight && posX != (m_width - 1))
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
                        pathLength = (int)Random.Range(1, (m_width - posX));

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
    } // ENDE START - Feld fertig initialisiert und erstellt

    // Update is called once per frame
    void Update()
    {
        // not used
    }

    /* 
     * ##### CreateTile #####
     * 
     * Erzeugt eine Bodenplatte und liefert diese zurueck.
     * Je nach Parameter wird eine andere Bodenplatte mit anderen
     * Komponenten erzeugt
     */
    GameObject CreateTile(bool tileType, Vector3 position)
    {
        GameObject tile = Instantiate(m_tilePrefab, position, Quaternion.identity);

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
    } // ENDE creatTile

    /* 
     * ##### CreateSpawn #####
     * 
     * Erzeugt den Spawn-Bereich samt Spieler und gibt eine Vektor zurueck,
     * ab dem spaeter in der Funktion createBoard alle weiteren Platten
     * erzeugt werden
     */
    Vector3 CreateSpawn()
    {
        Vector3 startV = m_spawnerV;
        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
        for (int i = 1; i <= m_width; i++)
        {
            Instantiate(m_startPrefab, startV, Quaternion.identity);
            startV.x += m_tileWidth;
        }
        startV.x = m_spawnerV.x;
        startV.z += m_tileHeight;

        // Spielerfigur setzen
        Vector3 resetPointV = m_spawnerV;
        resetPointV.x += (m_width / 2) * m_tileWidth;
        resetPointV.y += (m_tilePrefab.transform.localScale.y / 2);
        Vector3 playerSpawnV = new Vector3(resetPointV.x, resetPointV.y + m_playerSpawnHeight, resetPointV.z);
        GameObject player = Instantiate(m_player, playerSpawnV, Quaternion.identity);

        // Spielfigur - Einstellungen vornehmen
        player.name = "player";
        resetPointV.y += player.transform.localScale.y;
        player.GetComponent<Player>().m_resetPointV = resetPointV;
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        // Glas unter Spielfigur setzen
        Vector3 glassSpawnV = new Vector3 (playerSpawnV.x, playerSpawnV.y - player.transform.localScale.y, playerSpawnV.z);
        Instantiate(m_glass, glassSpawnV, Quaternion.identity);

        // Vektor fuer createBoard zurueckgeben
        return startV;
    } // ENDE createSpawn


    /* 
     * ##### CreateEnvironment #####
     * 
     * Erzeugt die Szenerie, um das Spielbrett herum
     */
    void CreateEnvironment()
    {
        Vector3 envSpawnV = new Vector3  (m_spawnerV.x - (m_tilePrefab.transform.localScale.x / 2) - (m_environmentPrefab.transform.GetChild(0).transform.localScale.x / 2)
                                        , m_spawnerV.y, 
                                        m_spawnerV.z - (m_tilePrefab.transform.localScale.z / 2) + (m_environmentPrefab.transform.GetChild(0).transform.localScale.z / 2));

        for (int i = 0; i < 5; i++)  // TESTANZAHL
        {
            GameObject environment = m_environmentPrefab;

            environment.name = "border_left" + i;
            environment.transform.GetChild(4).GetComponent<Renderer>().material = m_paintingsField[Random.Range(0, m_paintingsField.Length)];
            environment.transform.Find("painting_right").GetComponent<Renderer>().material = m_paintingsField[Random.Range(0, m_paintingsField.Length)];
            Instantiate(environment, envSpawnV, Quaternion.identity);

            environment.name = "border_right" + i;
            environment.transform.Find("painting_left").GetComponent<Renderer>().material = m_paintingsField[Random.Range(0, m_paintingsField.Length)];
            environment.transform.Find("painting_right").GetComponent<Renderer>().material = m_paintingsField[Random.Range(0, m_paintingsField.Length)];
            Instantiate(environment, envSpawnV + new Vector3((m_tileWidth * m_width) - m_tileOffset + environment.transform.GetChild(0).transform.localScale.x, 0, 0), Quaternion.Euler(0,180,0));

            envSpawnV.z += environment.transform.GetChild(0).transform.localScale.z;
        }
    } // ENDE createEnvironment

    /* 
    * ##### createBoard #####
    * 
    * erwartet keine Parameter und liefert void zurueck.
    * Durchlaeuft das Spielfeld-Array und legt so die Bodenplatten
    * tileSpawnV ist von der Methode createSpawn() abhaengig, da ab dieser
    * Position die weiteren Bodenplatten gesetzt werden
    */
    void CreateBoard()
    {
        //setzt die Bodenplatten ueber den Spawn-Platten
        Vector3 tileSpawnV = CreateSpawn();

        // platziere die restlichen Bodenplatten
        for (int i = 0; i < m_height; i++)
        {
            for (int j = 0; j < m_width; j++)
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
                tileSpawnV.x += m_tileWidth;
            }
            tileSpawnV.x = m_spawnerV.x;
            tileSpawnV.z += m_tileHeight;
        } // ENDE for
        CreateEnvironment();
    } // ENDE createBoard
}
