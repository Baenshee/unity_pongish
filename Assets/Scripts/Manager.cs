﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Manager : NetworkBehaviour
{
    [SyncVar]
    private int m_scorePlayerOne = 0;
    [SyncVar]
    private int m_scorePlayerTwo = 0;
    private GameObject[] m_Score;
    [SerializeField]
    private GameObject m_Ball;
    [SerializeField]
    private GameObject m_BallSpawn;
    [SerializeField]
    private GameObject m_BallPrefab;
    private int direction = 1;
    private int m_maxScore = 5;

    // Use this for initialization
    void Start()
    {
        if(isServer)
            StartCoroutine("Instantiate");
    }

    // Update is called once per frame
    void Update()
    {
    }

    [Command]
    public void CmdAddScore(string name)
    {
        if (name == "Goal1")
        {
            m_scorePlayerOne++;
            direction = 1;
        }
        else if (name == "Goal2")
        {
            direction = -1;
            m_scorePlayerTwo++;
        }
        else
            return;
        RpcBroadcast(m_scorePlayerOne, m_scorePlayerTwo);
        if (m_scorePlayerOne < m_maxScore && m_scorePlayerTwo < m_maxScore)
        {
            RpcChangeScore();
            CmdResetBall();
        }
        else
        {
            RpcWin();
            CmdEnd();
        }

    }

    [ClientRpc]
    void RpcChangeScore()
    {
        m_Score = GameObject.FindGameObjectsWithTag("Score");
        Debug.Log(m_Score.Length);
        foreach (GameObject score in m_Score)
        {
            score.GetComponent<UnityEngine.UI.Text>().text = m_scorePlayerOne + " - " + m_scorePlayerTwo;
        }
    }

    [ClientRpc]
    void RpcWin()
    {
        m_Score = GameObject.FindGameObjectsWithTag("Score");
        string msg;
        if (m_scorePlayerOne == m_maxScore)
            msg = "Player 1 wins: " + m_scorePlayerOne + " to " + m_scorePlayerTwo;
        else
            msg = "Player 2 wins: " + m_scorePlayerTwo + " to " + m_scorePlayerOne;
        Debug.Log(msg);
        foreach (GameObject score in m_Score)
        {
            score.GetComponent<UnityEngine.UI.Text>().text = msg;
        }
    }

    [Command]
    void CmdBroadcast()
    {
        RpcBroadcast(m_scorePlayerOne, m_scorePlayerTwo);
        RpcChangeScore();
    }

    [ClientRpc]
    void RpcBroadcast(int scoreOne, int scoreTwo)
    {
        m_scorePlayerOne = scoreOne;
        m_scorePlayerTwo = scoreTwo;
    }

    [Command]
    void CmdResetBall()
    {
        NetworkServer.Destroy(m_Ball);
        Destroy(m_Ball);
        m_Ball = Instantiate(m_BallPrefab, m_BallSpawn.transform.position, m_BallSpawn.transform.rotation);
        int factor = Random.Range(-1, 1) > 0 ? 1 : -1;
        Debug.Log(factor);
        m_Ball.GetComponent<Rigidbody>().AddForce(Random.Range(-25, 25), 0, 50 * direction);
        NetworkServer.Spawn(m_Ball);
    }

    [Command]
    void CmdEnd()
    {
        NetworkServer.Destroy(m_Ball);
        Destroy(m_Ball);
    }

    IEnumerator Instantiate()
    {
        yield return new WaitUntil(() => GameObject.Find("Score") != null);
        m_Score = GameObject.FindGameObjectsWithTag("Score");
        CmdBroadcast();
        Debug.Log(m_scorePlayerOne + " " + m_scorePlayerTwo);
        Debug.Log(Network.connections.Length);
        while(NetworkServer.connections.Count != 2)
        {
            yield return new WaitForSeconds(1);
        }
        m_Ball = Instantiate(m_BallPrefab, m_BallSpawn.transform.position, m_BallSpawn.transform.rotation);
        m_Ball.GetComponent<Rigidbody>().AddForce(Random.Range(-25, 25), 0, 50 * direction);
        NetworkServer.Spawn(m_Ball);
        yield return null;

    }
}