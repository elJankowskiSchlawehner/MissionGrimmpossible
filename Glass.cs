using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// steuert Spielstart

public class Glass : MonoBehaviour {

    public GameObject[] m_glassPrefabs;

    private GameObject _player;

	// Use this for initialization
	void Start () {
        _player = GameObject.Find("player");
        Debug.Log("zum Spielen: Space druecken!");
	}
	
	// Update is called once per frame
	void Update () {
        // Spielstart bei Space???
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Glass instanzieren
            Debug.Log("das Spiel beginnt ...");
            GameObject glassBroken = Instantiate(m_glassPrefabs[Random.Range(0, m_glassPrefabs.Length)], gameObject.transform.position, gameObject.transform.rotation);
            glassBroken.name = "glassBroken";

            Rigidbody[] pieces = glassBroken.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in pieces)
            {
                rb.gameObject.AddComponent<DecayScript>();
                rb.AddExplosionForce(50.0f, transform.position, 5.0f, -5.0f);
                Physics.IgnoreCollision(_player.gameObject.GetComponent<Collider>(), rb.gameObject.GetComponent<Collider>());
            }
            glassBroken.transform.DetachChildren();
            Destroy(glassBroken);

            // Spielereinstellungen vornehmen
            _player.GetComponent<Rigidbody>().isKinematic = false;
            _player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ
                                                            | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // zerstoere das urspruengliche Glass
            Destroy(gameObject);
            
        }
    }
}
