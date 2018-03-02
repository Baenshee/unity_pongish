using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnBubble : NetworkBehaviour {
    [SerializeField]
    private GameObject m_Paddle;

	// Use this for initialization
	void Start () {
        GameObject[] toIgnore = GameObject.FindGameObjectsWithTag("Terrain");

        foreach (GameObject ignore in toIgnore)
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), ignore.GetComponent<Collider>());
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnCollisionEnter(Collision collision)
    {
        CmdSpawnPaddle();
    }

    [Command]
    private void CmdSpawnPaddle()
    {
        GameObject tmp = Instantiate(m_Paddle, gameObject.transform.position, Quaternion.identity);
        NetworkServer.Spawn(tmp);
        Destroy(tmp, 5.0f);
        Destroy(gameObject);

    }
}
