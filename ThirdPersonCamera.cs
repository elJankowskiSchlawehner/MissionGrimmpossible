using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    private GameObject _player;
    private PlayfieldObserver _gameObserver;

	// Use this for initialization
	void Start ()
    {

	}

    private void OnEnable()
    {
        Vector3 camOffset_V = new Vector3(0f, 15f, -10f);
        gameObject.GetComponent<Camera>().orthographic = false;
        _player = GameObject.Find("Player");
        _gameObserver = GameObject.Find("boardGameManager").GetComponent<PlayfieldObserver>();
        transform.SetParent(_player.transform);
        transform.localPosition = camOffset_V;
        transform.rotation = Quaternion.Euler(new Vector3(40f, 0f, 0f));
    }
}
