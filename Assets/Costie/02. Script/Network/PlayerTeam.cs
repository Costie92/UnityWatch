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
    private PhotonView photonView;

    // Use this for initialization
    void Start()
    {

        photonView = GetComponent<PhotonView>();
        Debug.Log(photonView.ViewID);

        foreach (MonoBehaviour m in LobbyControlscripts)
        {
            if (photonView.IsMine) { }
            else
            {
                m.enabled = false;
            }
        }
        NetworkManager.instance.photonView.RPC("Join", RpcTarget.All, photonView.ViewID);
        NetworkManager.instance.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID);
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
                        NetworkManager.instance.photonView.RPC("Play", RpcTarget.All);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        NetworkManager.instance.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
                    }
                }
            }
        }
    }
}