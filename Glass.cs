using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// steuert Spielstart

public class Glass : MonoBehaviour {

    public GameObject[] GlassPrefabs;

    private GameObject _player;
    private PlayfieldObserver _gameObserver;

	// Use this for initialization
	void Start () {
        _player = GameObject.Find("Player");
        _gameObserver = GameObject.Find("boardGameManager").GetComponent<PlayfieldObserver>();
	}
	
	// Update is called once per frame
	void Update () {
        // Spielstart bei Space
        if (_gameObserver.GameStarted)
        {
            // Glass instanzieren
            GameObject glassBroken = Instantiate(GlassPrefabs[Random.Range(0, GlassPrefabs.Length)], gameObject.transform.position, gameObject.transform.rotation);
            glassBroken.name = "glassBroken";

            Transform[] pieces = glassBroken.GetComponentsInChildren<Transform>();
            foreach (Transform child in pieces)
            {
                MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
                Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                child.gameObject.AddComponent<DecayScript>();
                rb.AddExplosionForce(50.0f, transform.position, 5.0f, -5.0f);
                Physics.IgnoreCollision(_player.gameObject.GetComponent<Collider>(), child.gameObject.GetComponent<Collider>());
            }
            glassBroken.transform.DetachChildren();
            Destroy(glassBroken);

            // Spielereinstellungen vornehmen
            _player.GetComponent<Player>().CanMove = true;
            _player.GetComponent<Rigidbody>().isKinematic = false;
            _player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ
                                                            | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            // zerstoere das urspruengliche Glass
            Destroy(gameObject);
        }
    }
}