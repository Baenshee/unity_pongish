using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerControllerPong : NetworkBehaviour {

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer)
            return;
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(Input.GetAxis("Vertical")*100, 0, -Input.GetAxis("Horizontal") * 100);
        //Debug.Log(Input.GetAxis("Horizontal"));
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("yo");
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
