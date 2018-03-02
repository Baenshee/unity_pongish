using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirderBehaviour : MonoBehaviour
{
    private enum GirderType
    {
        Basic,
        Sticky,
        Explosive
    }
    private bool m_collided = false;
    private int collidedSince = 0;
    private int spawnedSince = 0;
    private GirderType chosen = 0;
    // Use this for initialization
    void Start()
    {
        GameObject[] toIgnore = GameObject.FindGameObjectsWithTag("Terrain");
        
        foreach (GameObject ignore in toIgnore)
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), ignore.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            Debug.Log("yo");
            if (chosen.CompareTo(GirderType.Explosive) == 0)
                chosen = 0;
            else
                chosen++;
            Debug.Log(chosen);

        }

    }

    private void FixedUpdate()
    {
        spawnedSince++;
        if (m_collided)
        {
            collidedSince++;
        }
        if (collidedSince == 5)
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Collider>().enabled = true;

            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            m_collided = false;
            Debug.Log(gameObject.GetComponent<Rigidbody>().detectCollisions);
        }
        if (spawnedSince > 400)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_collided = true;
        //.GetComponent<Collider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

        m_collided = true;
        //gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        gameObject.GetComponent<Collider>().enabled = false;

    }
}
