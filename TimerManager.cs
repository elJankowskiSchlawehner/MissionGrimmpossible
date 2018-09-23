using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour {

    private PlayfieldObserver _boardObserver;

    private Text _timerText;
    private Color _textColor;

    // Use this for initialization
    void Start () {

        _boardObserver = GameObject.Find("boardGameManager").GetComponent<PlayfieldObserver>();

        _timerText = GetComponent<Text>();
        _timerText.text = ShowRemainingTime();
        _textColor = _timerText.color;
    }
	
	// Update is called once per frame
	void Update () {
        if (_boardObserver.GameTimer <= 0f)
        {
            _timerText.color = Color.red;
        }

        if (_boardObserver.GameTimer > 0 && _boardObserver.GameTimer <= 10.0f)
        {
            _timerText.color = Color.Lerp(_textColor, Color.red, Mathf.PingPong(Time.time, 1));
        }

        _timerText.text = ShowRemainingTime();
    }

    private string ShowRemainingTime()
    {
        string minutes = ((int)_boardObserver.GameTimer / 60).ToString();
        string seconds = (_boardObserver.GameTimer % 60).ToString("F2");
        return minutes + ":" + seconds;
    }

    private void TimeLoss()
    {

    }
}
