using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecayScript : MonoBehaviour
{
	//Referenzen
    private Transform _board;

    public float TickRate = 0.3f;       // Tick-Rate
    public float DecayTimer = 2.0f;     // Ab wann soll das Objekt verschwinden?
    private float _decayTime;           // die "Schrumpfzeit", abhaengig von der Groesse des Objekts 
    private bool _isGrounded = false;   // noetig zur Ueberpruefung in Update

    // Use this for initialization
    void Start()
    {
        _decayTime = TickRate * transform.localScale.x;
        _board = GameObject.Find("boardGameManager").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGrounded() || transform.position.y < _board.position.y - 10f)
        {
            _isGrounded = true;
        }
        if (_isGrounded)
        {
            DecayTimer -= Time.deltaTime;
        }
        if (DecayTimer <= 0f)
        {
            if (transform.GetComponent<Rigidbody>() != null)
            {
                //transform.GetComponent<Rigidbody>().isKinematic = true;
            }
            transform.localScale = new Vector3 (
                                                transform.localScale.x - (_decayTime * Time.deltaTime),
                                                transform.localScale.y - (_decayTime * Time.deltaTime),
                                                transform.localScale.z - (_decayTime * Time.deltaTime)
                                                );
        }

        if (transform.localScale.x <= 0)
        {
            Destroy(gameObject);
        }
    }

    /* 
     * ##### IsGrounded #####
     * liefert einen bool-Wert zurueck, je nachdem, ob der Raycast auf ein Objekt traf oder nicht
     */
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2f);
    }

}