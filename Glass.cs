using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class glass : MonoBehaviour {

    //private Rigidbody[] rbChildren;
    private List<Rigidbody> rbChildren;
    private float spawnY;

    public float decayTime = 50.0f;

	// Use this for initialization
	void Start () {
        rbChildren = new List<Rigidbody>(transform.GetComponentsInChildren<Rigidbody>());
        spawnY = GameObject.Find("boardGameManager").transform.position.y;

        foreach (Rigidbody rb in rbChildren)
        {
            rb.AddExplosionForce(50.0f, transform.position, 5.0f, -5.0f);
        }
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < rbChildren.Count; i++) 
        {
            if (rbChildren[i].velocity.magnitude == 0 || rbChildren[i].transform.position.y < spawnY)
            {
                rbChildren[i].isKinematic = true;
                rbChildren[i].transform.localScale = new Vector3    (rbChildren[i].transform.localScale.x - (decayTime * Time.deltaTime),
                                                                    rbChildren[i].transform.localScale.y - (decayTime * Time.deltaTime),
                                                                    rbChildren[i].transform.localScale.z - (decayTime * Time.deltaTime));
            }

            if (rbChildren[i].transform.localScale.x <= 0)
            {
                Destroy(rbChildren[i]);
                rbChildren.RemoveAt(i);
            }
        }

        if (transform.childCount == 0)
        {
            Destroy(gameObject);
            Debug.Log("Glass vollstaendig zerstoert");
        }
    }
}
