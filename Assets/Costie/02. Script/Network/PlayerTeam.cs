using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class PlayerTeam : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] LobbyControlscripts;
    private NetworkManager networkManager;
    private PhotonView photonView;

    // Use this for initialization
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        Debug.Log(photonView.ViewID);

        foreach (MonoBehaviour m in LobbyControlscripts)
        {
            if (photonView.IsMine) { }
            else
            {
                m.enabled = false;
            }
        }
        networkManager.photonView.RPC("Join", RpcTarget.All, photonView.ViewID);
        networkManager.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            if (SceneManager.GetActiveScene().name == "LobbyScene")
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        networkManager.photonView.RPC("Play", RpcTarget.All);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        networkManager.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
                    }
                }
            }
        }
    }
}