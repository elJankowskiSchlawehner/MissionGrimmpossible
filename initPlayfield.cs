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

    bool[,] tilesField;                                     /* verwaltet den Bodenplattentyp und gibt Spielfeldgroesse an
                                                               [y-Achse, x-Achse]
                                                             */
    public GameObject tilePrefab;                           // prefab der Bodenplatten

    private bool isLeft, isMid, isRight;                    // keine doppelten Schritte in die gleiche Richtung

    //public int maxPathLength = 0;                           // verhindert, dass die Pfadlaengen nicht zu gross werden

    // Use this for initialization
    void Start()
    {
        tilesField = new bool[height, width];               // initialisiert das Spielfeld

        int currentPosX = (int)Random.Range(0, width);      // Startwert (X-Achse) wird gewuerfelt
        int currentPosY = 0;                                // zaehlt die "Zeilen" des Arrays tilesField hoch, bei Start 0

        tilesField[currentPosY, currentPosX] = isCorrectTile;   // Startwert in das Spielfeld eintragen

        // Schleife bricht ab, wenn in letzter Zeile des Arrays eine Bodenplatte gesetzt wurde
        while (currentPosY < (height - 1))
        {
            Debug.Log(currentPosX + ", " + currentPosY);
            int pathDirection;                              // speichert die moeglichen Richtungen des Pfades
            int pathLength;                                 // speichert die Laenge des Pfads in die gewuerfelte Richtung
            int pathEnd;                                    // Abbruchbedingung for-Schleife

            /* es werden die drei Moeglichkeiten abgedeckt (-1,0,1): links, vorwaerts, rechts
               kann auch spaeter mit Chancen / Prozentwerten realisiert werden.
               Achtungen Random.Range: obere Grenze ist exklusiv
               Abfangen von Array out of Bounds
             */
            if (currentPosX == 0)                           // Position ganz links --> nur Richtungen Mitte oder Rechts erlaubt
            {
                Debug.Log("mr");
                pathDirection = (int)Random.Range(0, 2);
            }
            else if (currentPosX == (width - 1))            // Position ganz rechts --> nur Richtungen Mitte oder Links erlaubt
            {
                Debug.Log("lm");
                pathDirection = (int)Random.Range(-1, 1);
            }
            else                                            // alle Richtungen sind erlaubt
            {
                Debug.Log("lmr");
                pathDirection = (int)Random.Range(-1, 2);
            }

            switch (pathDirection)
            {
                // PFADRICHTUNG LINKS
                case -1:
                    pathLength = (int)Random.Range(1, (currentPosX + 1));
                    Debug.Log("LINKS um " + pathLength);

                    pathEnd = currentPosX - pathLength;
                    for (int indx = (currentPosX - 1); indx >= pathEnd; indx--)
                    {
                        Debug.Log("Indx: " + indx + ", Bedingung: indx >= " + "currPosX(" + currentPosX + ") - pathLength(" + pathLength + ")");
                        tilesField[currentPosY, indx] = isCorrectTile;
                        currentPosX = indx;
                    }
                    break;

                // PFADRICHTUNG VORWAERTS
                case 0:
                    pathLength = (int)Random.Range(1, (height - currentPosY));
                    Debug.Log("VORWAERTS um " + pathLength);

                    pathEnd = currentPosY + pathLength;
                    for (int indx = (currentPosY + 1); indx <= pathEnd; indx++)
                    {
                        Debug.Log("Indx: " + indx + ", Bedingung: indx >= " + "currPosX(" + currentPosY + ") + pathLength(" + pathLength + ")");
                        tilesField[indx, currentPosX] = isCorrectTile;
                        currentPosY = indx;
                    }
                    break;

                // PFADRICHTUNG RECHTS
                case 1:
                    pathLength = (int)Random.Range(1, (width - currentPosX));
                    Debug.Log("RECHTS um " + pathLength);

                    pathEnd = currentPosX + pathLength;
                    for (int indx = (currentPosX + 1); indx <= pathEnd; indx++)
                    {
                        Debug.Log("Indx: " + indx + ", Bedingung: indx >= " + "currPosX(" + currentPosX + ") + pathLength(" + pathLength + ")"); ;
                        tilesField[currentPosY, indx] = isCorrectTile;
                        currentPosX = indx;
                    }
                    break;

                // DEFAULT CASE
                default:
                    Debug.Log("neu wuerfeln!");
                    currentPosY++;
                    tilesField[currentPosY, currentPosX] = isCorrectTile;
                    break;
            } // ENDE SWITCH
        } // ENDE WHILE

        placeTiles();
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
        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);

        if (tileType == isCorrectTile)
        {
            tile.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {

        }

        return tile;
    }

    /* 
     * ##### placeTiles #####
     * 
     * erwartet keine Parameter und liefert void zurueck.
     * Durchlaeuft das Spielfeld-Array und legt so die Bodenplatten
     */
    void placeTiles()
    {
        Vector3 startV = new Vector3(0, 0, 0); // Startpunkt, ab dem die Tiles gesetzt werden

        for (int i = 0; i < tilesField.GetLength(0); i++)
        {
            for (int j = 0; j < tilesField.GetLength(1); j++)
            {
                // korrekte Bodenplatte legen bei Übereinstimmung
                if (tilesField[i, j] == isCorrectTile)
                {
                    createTile(isCorrectTile, startV);
                }
                // falsche Bodenplatte legen
                else
                {
                    createTile(isWrongTile, startV);
                }

                startV.x += 3.5f;
            }

            startV.x = 0;
            startV.z += 3.5f;
        }
    }

    /* 
     * ##### debugField #####
     * 
     * zu Debug-Zwecken. Zeigt Koordinaten an
     */
    void debugField()
    {
        for (int i = 0; i < tilesField.GetLength(0); i++)
        {
            for (int j = 0; j < tilesField.GetLength(1); j++)
            {
                Debug.Log("[" + i + ", " + j + "]: " + tilesField[i, j]);
            }
        }
    }
}
