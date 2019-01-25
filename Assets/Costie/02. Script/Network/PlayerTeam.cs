using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class PlayerTeam : MonoBehaviourPun, IPunObservable
{
    //[SerializeField] private Material Mycolor;
    public Image MyHeroImage;
    public hcp.E_HeroType myherotype;
    
    [SerializeField] private Button BtnTeamA, BtnTeamB, BtnReady, BtnSoldier, BtnHook;
    [SerializeField] private MonoBehaviour[] LobbyControlscripts;
    public PhotonView photonView;

    private void Awake()
    {
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>());
        this.transform.localScale = new Vector3(1, 1, 1);
    }
    // Use this for initialization
    void Start()
    {
        MyHeroImage.sprite = NetworkManager.instance.imageSoldier;
        myherotype = hcp.E_HeroType.Soldier;
        photonView = GetComponent<PhotonView>();
        foreach (MonoBehaviour m in LobbyControlscripts)
        {
            if (photonView.IsMine)
            {
                NetworkManager.instance.buttons.SetActive(true);
                BtnTeamA = NetworkManager.instance.buttons.transform.GetChild(0).GetComponent<Button>();
                BtnTeamB = NetworkManager.instance.buttons.transform.GetChild(1).GetComponent<Button>();
                BtnReady = NetworkManager.instance.buttons.transform.GetChild(2).GetComponent<Button>();
                BtnSoldier = NetworkManager.instance.buttons.transform.GetChild(3).GetComponent<Button>();
                BtnHook = NetworkManager.instance.buttons.transform.GetChild(4).GetComponent<Button>();
                //BtnTeamA.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("A"); });
                //BtnTeamB.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("B"); });
                BtnSoldier.GetComponent<Button>().onClick.AddListener(delegate { onClickHeroButton(hcp.E_HeroType.Soldier); });
                BtnHook.GetComponent<Button>().onClick.AddListener(delegate { onClickHeroButton(hcp.E_HeroType.Hook); });
                if (PhotonNetwork.IsMasterClient)
                {
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickPlay);
                    BtnReady.GetComponentInChildren<Text>().text = "Play";
                }
                else
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickReady);
                //this.GetComponent<MeshRenderer>().material = Mycolor;
            }
            else
            {
                //m.enabled = false;
            }
        }
        NetworkManager.instance.photonView.RPC("Join", RpcTarget.All, photonView.ViewID);
        NetworkManager.instance.photonView.RPC("Named", RpcTarget.AllBufferedViaServer, photonView.ViewID, PlayerName.instance.MyName);
        PosTeam((photonView.ViewID / 1000) % 2 == 1);
    }

    // Update is called once per frame
    void Update()
    {
        MyHeroImage.sprite = myherotype == hcp.E_HeroType.Soldier ? NetworkManager.instance.imageSoldier : NetworkManager.instance.imageHook;
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
    public void PosTeam(bool isTeamA)
    {
        int Pos = photonView.ViewID / 1000;
        if (isTeamA)
        {
            this.transform.localPosition = new Vector3(-750, 350 - Pos * 250, 0);
        }
        else
        {
            this.transform.localPosition = new Vector3(750, 350 - Pos * 250, 0);
        }
    }
    public void onClicKTeamButton(string TeamString)
    {
        PosTeam(TeamString == "A");
        NetworkManager.instance.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID, TeamString);
    }
    public void onClickHeroButton(hcp.E_HeroType heroType)
    {
        myherotype = heroType;
        NetworkManager.instance.photonView.RPC("SelectHeroo", RpcTarget.All, photonView.ViewID, heroType);
    }
    public void onClickReady()
    {
        NetworkManager.instance.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
    }
    public void onClickPlay()
    {
        NetworkManager.instance.photonView.RPC("Play", RpcTarget.All);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myherotype);
            //stream.SendNext(name);
        }
        else
        {
            myherotype = (hcp.E_HeroType)stream.ReceiveNext();
            //name = (string)stream.ReceiveNext();
        }
    }
}