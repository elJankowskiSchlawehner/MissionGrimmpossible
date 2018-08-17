using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initPlayfield : MonoBehaviour
{
    // Makros
    private const bool IS_CORRECT_TILE = true;
    private const bool IS_WRONG_TILE = false;

    //Variablen
    //
    public int m_height = 3;                                  // Hoehe des Spielfelds
    public int m_width = 5;                                   // Breite des Spielfelds
    private bool[,] _tilesField;                             // [hoehe,breite], verwaltet den Bodenplattentyp und gibt Spielfeldgroesse an

    public GameObject m_tilePrefab;                           // prefab der Bodenplatten
    public GameObject m_startPrefab;                          // prefab der Spawn-Position
    public GameObject m_player;                               // Spieler prefab

    public float m_tileOffset = 0.0f;
    [HideInInspector]
    public float m_tileWidth, m_tileHeight;

    public Vector3 m_spawnerV = new Vector3(0, 0, 0);         // Startpunkt, ab dem alle Dinge gespawned werden

    // Use this for initialization
    void Start()
    {
        if (m_width < 1)
        {
            m_width = 1;
        }
        if (m_height < 1)
        {
            m_height = 1;
        }

        _tilesField = new bool[m_height, m_width];               // initialisiert das Spielfeld

        int posX = (int)Random.Range(0, m_width);      // Startwert (X-Achse) wird gewuerfelt
        int posY = 0;                                // zaehlt die "Zeilen" des Arrays tilesField hoch, bei Start 0

        _tilesField[posY, posX] = IS_CORRECT_TILE;   // Startwert in das Spielfeld eintragen

        int pathDirection = 0;                              // speichert die moeglichen Richtungen des Pfades, initial = 0 (vorwaerts)
        int pathLength;                                     // speichert die Laenge des Pfads in die gewuerfelte Richtung
        int pathEnd;                                        // Abbruchbedingung for-Schleife, ansonsten wird currentPosition in der Bedingung ueberschrieben
        int maxForwardLength = (m_height / 3 + 1);            // Laenge des Pfads kann sich je nach Einstellung aendern. DEFAULT: (height - currentPosY)

        m_tileWidth = m_tilePrefab.transform.localScale.x + m_tileOffset;
        m_tileHeight = m_tilePrefab.transform.localScale.z + m_tileOffset;

        bool isRight = false, isLeft = false;               // geben die Richtung des letzten Schritts an

        int heightCounter = 1;                              // verhindert einen aufeinanderfolgenden links-rechts-Schritt, = 1, da Startwert als ein Schritt gilt, ansonsten = 2

        // Schleife bricht ab, wenn in letzter Zeile des Arrays eine Bodenplatte gesetzt wurde
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
                        if (posX == 0)               // Position ganz links --> naechste Wegrichtung ist rechts
                        {
                            pathDirection = 1;
                        }
                        else if (posX == (m_width - 1))    // Position ganz rechts --> neachste Wegrichtung ist links
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

        createBoard();
    } // ENDE START - Feld fertig initialisiert und erstellt

    // Update is called once per frame
    void Update()
    {

    }

    /* 
     * ##### createTile #####
     * 
     * Erzeugt eine Bodenplatte und liefert diese zurueck.
     * Je nach Parameter wird eine andere Bodenplatte mit anderen
     * Komponenten erzeugt
     */
    GameObject createTile(bool tileType, Vector3 position)
    {
        GameObject tile = Instantiate(m_tilePrefab, position, Quaternion.identity);

        if (tileType == IS_CORRECT_TILE)
        {
            tile.name = "correctTile";
            //tile.GetComponent<Renderer>().material.color = Color.green;     //zu Testzwecken die richtigen Bodenplatten gruen faerben
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
            bc.size = new Vector3(0.5f, 0.5f, 0.5f);
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
        Vector3 tileSpawnV = m_spawnerV;
        Vector3 playerSpawnV = m_spawnerV;

        //platziere die Startplatten - andere Moeglichkeit mittels einer grossen ganzen Flaeche???
        for (int i = 1; i <= _tilesField.GetLength(1); i++)
        {
            Instantiate(m_startPrefab, tileSpawnV, Quaternion.identity);
            tileSpawnV.x += m_tileWidth;
        }
        tileSpawnV.x = m_spawnerV.x;
        tileSpawnV.z += m_tileHeight;

        // Spielerfigur setzen
        playerSpawnV.x = (m_width / 2) * m_tileWidth;
        Instantiate(m_player, new Vector3(playerSpawnV.x, playerSpawnV.y + 10, playerSpawnV.z), Quaternion.identity);



        // platziere die restlichen Bodenplatten
        for (int i = 0; i < _tilesField.GetLength(0); i++)
        {
            for (int j = 0; j < _tilesField.GetLength(1); j++)
            {
                // korrekte Bodenplatte legen bei Übereinstimmung
                if (_tilesField[i, j] == IS_CORRECT_TILE)
                {
                    createTile(IS_CORRECT_TILE, tileSpawnV);
                }
                // falsche Bodenplatte legen
                else
                {
                    createTile(IS_WRONG_TILE, tileSpawnV);
                }

                tileSpawnV.x += m_tileWidth;
            }

            tileSpawnV.x = m_spawnerV.x;
            tileSpawnV.z += m_tileHeight;
        }
    }
}
