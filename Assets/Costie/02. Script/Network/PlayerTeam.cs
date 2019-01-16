using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class PlayerTeam : MonoBehaviour
{
    [SerializeField] private Button BtnTeamA, BtnTeamB, BtnReady;
    [SerializeField] private MonoBehaviour[] LobbyControlscripts;
    private PhotonView photonView;

    // Use this for initialization
    void Start()
    {
        
        photonView = GetComponent<PhotonView>();
        Debug.Log(photonView.ViewID);

        foreach (MonoBehaviour m in LobbyControlscripts)
        {
            if (photonView.IsMine) {
                NetworkManager.instance.buttons.SetActive(true);
                BtnTeamA = NetworkManager.instance.buttons.transform.GetChild(0).GetComponent<Button>();
                BtnTeamB = NetworkManager.instance.buttons.transform.GetChild(1).GetComponent<Button>();
                BtnReady = NetworkManager.instance.buttons.transform.GetChild(2).GetComponent<Button>();
                BtnTeamA.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("A"); });
                BtnTeamB.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("B"); });
                if (PhotonNetwork.IsMasterClient)
                {
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickPlay);
                    BtnReady.GetComponentInChildren<Text>().text = "Play";
                }
                else
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickReady);
            }
            else
            {
                m.enabled = false;
            }
        }
        NetworkManager.instance.photonView.RPC("Join", RpcTarget.All, photonView.ViewID);
    }

    // Update is called once per frame
    void Update()
    {
        //if (PhotonNetwork.InRoom)
        //{
        //    if (SceneManager.GetActiveScene().name == "LobbyScene")
        //    {
        //        if (PhotonNetwork.IsMasterClient)
        //        {
        //            if (Input.GetKeyDown(KeyCode.P))
        //            {
        //                NetworkManager.instance.photonView.RPC("Play", RpcTarget.All);
        //            }
        //        }
        //        else
        //        {
        //            if (Input.GetKeyDown(KeyCode.R))
        //            {
        //                NetworkManager.instance.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
        //            }
        //        }
        //    }
        //}
    }
    public void onClicKTeamButton(string TeamString)
    {
        NetworkManager.instance.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID, TeamString);
    }
    public void onClickReady()
    {
        NetworkManager.instance.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
    }
    public void onClickPlay() {
        NetworkManager.instance.photonView.RPC("Play", RpcTarget.All);
    }
}