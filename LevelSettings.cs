using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Uebertraegt die Level-Groesse ueber die Szenen hinweg
 * Wird unter anderem fuer das Menue benoetigt
 */
public class LevelSettings : MonoBehaviour {

    //default Werte
    public static int LevelWidth = 5;
    public static int LevelHeight = 5;

    public static int GetLevelWidth()
    {
        return LevelWidth;
    }

    public static int GetLevelHeight()
    {
        return LevelHeight;
    }

    public static void SetLevelWidth(int newLevelWidth)
    {
        LevelWidth = newLevelWidth;
    }

    public static void SetLevelHeight (int newLevelHeight)
    {
        LevelHeight = newLevelHeight;
    }
}
