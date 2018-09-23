using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Uebertraegt die Level-Groesse ueber die Szenen hinweg
 * Wird unter anderem fuer das Menu benoetigt
 */
public class LevelSettings : MonoBehaviour {

    //default Werte
    private static int _levelWidth = 7;
    private static int _levelHeight = 10;

    public static int GetLevelWidth()
    {
        return _levelWidth;
    }

    public static int GetLevelHeight()
    {
        return _levelHeight;
    }

    public static void SetLevelWidth(int newLevelWidth)
    {
        _levelWidth = newLevelWidth;
    }

    public static void SetLevelHeight (int newLevelHeight)
    {
        _levelHeight = newLevelHeight;
    }
}
