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
    [SerializeField] private Material Mycolor;
    [SerializeField] private Button BtnTeamA, BtnTeamB, BtnReady, BtnSoldier, BtnHook;
    [SerializeField] private MonoBehaviour[] LobbyControlscripts;
    private PhotonView photonView;

    // Use this for initialization
    void Start()
    {
        
        photonView = GetComponent<PhotonView>();

        foreach (MonoBehaviour m in LobbyControlscripts)
        {
            if (photonView.IsMine) {
                NetworkManager.instance.buttons.SetActive(true);
                BtnTeamA = NetworkManager.instance.buttons.transform.GetChild(0).GetComponent<Button>();
                BtnTeamB = NetworkManager.instance.buttons.transform.GetChild(1).GetComponent<Button>();
                BtnReady = NetworkManager.instance.buttons.transform.GetChild(2).GetComponent<Button>();
                BtnSoldier = NetworkManager.instance.buttons.transform.GetChild(3).GetComponent<Button>();
                BtnHook = NetworkManager.instance.buttons.transform.GetChild(4).GetComponent<Button>();
                BtnTeamA.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("A"); });
                BtnTeamB.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("B"); });
                BtnSoldier.GetComponent<Button>().onClick.AddListener(delegate { onClickHeroButton(hcp.E_HeroType.Soldier); }); 
                BtnHook.GetComponent<Button>().onClick.AddListener(delegate { onClickHeroButton(hcp.E_HeroType.Hook); });
                if (PhotonNetwork.IsMasterClient)
                {
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickPlay);
                    BtnReady.GetComponentInChildren<Text>().text = "Play";
                }
                else
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickReady);
                this.GetComponent<MeshRenderer>().material = Mycolor;
            }
            else
            {
                m.enabled = false;
            }
        }
        NetworkManager.instance.photonView.RPC("Join", RpcTarget.All, photonView.ViewID);
        PosTeam((photonView.ViewID / 1000) % 2 == 1);
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
    public void PosTeam(bool isTeamA) {
        int Pos = photonView.ViewID / 1000;
        if (isTeamA)
            this.transform.position = new Vector3(-5, 2 - Pos, 0);
        else {
            this.transform.position = new Vector3(5, 2 - Pos, 0);
        }
    }
    public void onClicKTeamButton(string TeamString)
    {
        PosTeam(TeamString == "A");
        NetworkManager.instance.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID, TeamString);
    }
    public void onClickHeroButton(hcp.E_HeroType heroType) {
        NetworkManager.instance.photonView.RPC("SelectHero", RpcTarget.All, photonView.ViewID, heroType);
    }
    public void onClickReady()
    {
        NetworkManager.instance.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
    }
    public void onClickPlay() {
        NetworkManager.instance.photonView.RPC("Play", RpcTarget.All);
    }
}