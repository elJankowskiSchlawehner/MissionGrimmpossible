using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ShowPathUI : MonoBehaviour {

    private ShowPath _manager;
    private int _listCount;                     // Anzahl aller korrekten Platten
    private float _totalAnimTime;               // die Zeit, die benoetigt wird, bis PathHelp vollstaendig angezeigt wird

    public Image PhoneImage;

    public Text LoadingText;                    // FORTSCHRITTSANZEIGE: ROTIERENDE ANIMATION
    private int _loadingStep = 0;               // der jeweilige Schritt in der Lade-Animation
    private float _loadingAnimTime = 0.15f;     // die Zeit zwischen den einzelnen Lade-Animations-Schritten
    private bool _loadingAnimStarted = false;   // uberprueft, ob die Lade-Animation bereits gestartet wurde und startet dann einmalig die Routine

    public Text ProgressText;                   // FORTSCHRITTSANZEIGE: BALKEN
    private const int TOTAL_PROGRESS_BARS = 25; // die Anzahl der anzuzeigenden Progress-Balken
    private float _progressStep = 0f;           // Instanziiert einen neuen Balken

    public Text PercentageText;                 // FORTSCHRITTSANZEIGE: PROZENT
    private float _currentAnimTime = 0f;        // die vergangene Zeit der Animation

    // Use this for initialization
    void Start () {
        _manager = GameObject.Find("boardGameManager").GetComponent<ShowPath>();
        _listCount = _manager.GetCorrectTilesCount();
        _totalAnimTime = _listCount * _manager.GetDisplayAnimTime();

        LoadingText.text = "initialising ...";
        ProgressText.text = "";
        PercentageText.text = "0.00";

        Debug.Log("UNITS: " + PhoneImage.rectTransform.rect.width / 100);
	}
	
	// Update is called once per frame
	void Update () {
		if (_manager._displayStarted == true && (ProgressText.text.Length < TOTAL_PROGRESS_BARS))
        {
            // Rotierende Animation
            DisplayLoadingAnimation();
            // Balken Animation
            DisplayProgressAnimation();
            // Prozent Animation
            DisplayPercentage();
        }
	}

    /* 
    * ##### DisplayLoadingAnimation #####
    *
    * Started bei Spielstart einmalig die Routine
    */
    private void DisplayLoadingAnimation()
    {
        if (!_loadingAnimStarted)
        {
            StartCoroutine(LoadingAnimationRoutine());
            _loadingAnimStarted = true;
        }
    }

    /* 
    * ##### LoadingAnimationRoutine #####
    *
    * Routine, die eine Lade-Animation abspielt,
    * bis die Ausgabe der richtigen Platten beendet wurde
    */
    private IEnumerator LoadingAnimationRoutine ()
    {
        while (!_manager._displayFinished)
        {
            switch (_loadingStep)
            {
                case 0:
                    LoadingText.text = "hacking |";
                    _loadingStep++;
                    yield return new WaitForSeconds(_loadingAnimTime);
                    break;
                case 1:
                    LoadingText.text = "hacking /";
                    _loadingStep++;
                    yield return new WaitForSeconds(_loadingAnimTime);
                    break;
                case 2:
                    LoadingText.text = "hacking -";
                    _loadingStep++;
                    yield return new WaitForSeconds(_loadingAnimTime);
                    break;
                case 3:
                    LoadingText.text = "hacking \\";
                    _loadingStep++;
                    yield return new WaitForSeconds(_loadingAnimTime);
                    break;
                default:
                    _loadingStep = 0;
                    break;
            }
        }
        LoadingText.text = "finished";
    }

    /* 
    * ##### DisplayProgressAnimation #####
    *
    * Stellt den Fortschritt als Anreihung von Balken dar
    */
    private void DisplayProgressAnimation()
    {
        _progressStep += Time.deltaTime;
        if (_progressStep >= _totalAnimTime / TOTAL_PROGRESS_BARS)
        {
            int progressBarCount = ProgressText.text.Length + 1;
            ProgressText.text = "";
            for (int i = 0; i < progressBarCount; i++)
            {
                ProgressText.text += "#";
            }

            _progressStep = 0f;
        }
    }

    /* 
    * ##### DisplayPercentage #####
    *
    * Stellt den Fortschritt als erhoehenden Prozentwert dar
    */
    private void DisplayPercentage()
    {
        _currentAnimTime += Time.deltaTime;

        if (_currentAnimTime >= _totalAnimTime)
        {
            _currentAnimTime = _totalAnimTime;
        }
        PercentageText.text = ((_currentAnimTime / _totalAnimTime) * 100).ToString("F2");
    }
}
