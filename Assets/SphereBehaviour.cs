using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SphereBehaviour : NetworkBehaviour {
    Rigidbody rb;
    float color = 0;
	// Use this for initialization
	void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
        //gameObject.GetComponent<Rigidbody>().AddForce(0, 0, 50);
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /*private void FixedUpdate()
    {
        if ((color / 10) % 1 == 0)
            up = !up;
        if (up)
        {
            color += Time.deltaTime;
        }
        else
        {
            color -= Time.deltaTime;
        }
        
        gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Alpha", (color/5) % 1);
    }*/

    private void OnCollisionEnter(Collision collision)
    {
        gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Alpha",color);
        color = (color + 1) % 2;
    }

    private void OnCollisionExit(Collision collision)
    {
        rb.velocity = rb.velocity * 1.1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject.Find("GameManager").GetComponent<Manager>().CmdAddScore(other.gameObject.name);
    }

    
}
