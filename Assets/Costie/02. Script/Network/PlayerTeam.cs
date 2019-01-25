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
    public Text MyNameText;
    public Image ReadyFrame;
    public hcp.E_HeroType myherotype;
    [SerializeField] private bool ReadyCheck;
    [SerializeField] private string MyTeam, MyName;
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
        ReadyCheck = false;
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
                    BecomeToMaster();
                }
                else
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickReady);
            }
            else
            {
                //m.enabled = false;
            }
        }
        if(photonView.IsMine)
            NetworkManager.instance.photonView.RPC("Join", RpcTarget.MasterClient, photonView.ViewID);
        NetworkManager.instance.photonView.RPC("Named", RpcTarget.AllBufferedViaServer, photonView.ViewID, PlayerName.instance.MyName);
        MyName = PlayerName.instance.MyName;
        if (PhotonNetwork.IsMasterClient) {
            PosAfterDictionary();
        }
    }

    // Update is called once per frame
    void Update()
    {
        ReadyFrame.sprite = ReadyCheck ? NetworkManager.instance.imageReady : NetworkManager.instance.imageUnReady;
        MyHeroImage.sprite = myherotype == hcp.E_HeroType.Soldier ? NetworkManager.instance.imageSoldier : NetworkManager.instance.imageHook;
        MyNameText.text = PlayerName.instance.MyName;
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
    public void PosTeam(string TeamLayer,int Pos)
    {
        if (TeamLayer == hcp.Constants.teamA_LayerName)
        {
            this.transform.localPosition = new Vector3(-750, 100 - Pos * 450, 0);
        }
        else
        {
            this.transform.localPosition = new Vector3(750, 100 - Pos * 450, 0);
        }
    }
    public void PosAfterDictionary() {
        Debug.Log("PosAfterDictionary");
        Debug.Log("MY ID IS : " + photonView.ViewID);
        Debug.Log(NetworkManager.instance.Teams.Count);
        MyTeam = NetworkManager.instance.Teams[photonView.ViewID / 1000];
        //int MyPos = MyTeam == hcp.Constants.teamA_LayerName ? NetworkManager.instance.TeamACount : NetworkManager.instance.TeamBCount;
        int MyPos = 0;
        foreach (KeyValuePair<int, string> pair in NetworkManager.instance.Teams) {
            if (pair.Key == photonView.ViewID / 1000)
            {
                break;
            }
            if (MyTeam == pair.Value) {
                MyPos++;
            }
            Debug.Log("My POS IS : " + MyPos);
        }
        
        PosTeam(MyTeam, MyPos);
    }
    public void onClicKTeamButton(string TeamString)
    {
        //PosTeam(TeamString == "A");
        NetworkManager.instance.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID, TeamString);
    }
    public void onClickHeroButton(hcp.E_HeroType heroType)
    {
        myherotype = heroType;
        NetworkManager.instance.photonView.RPC("SelectHeroo", RpcTarget.All, photonView.ViewID, heroType);
    }
    public void onClickReady()
    {
        ReadyCheck = !ReadyCheck;
        NetworkManager.instance.photonView.RPC("Ready", RpcTarget.All, photonView.ViewID);
    }
    public void onClickPlay()
    {
        NetworkManager.instance.photonView.RPC("Play", RpcTarget.All);
    }
    public void BecomeToMaster() {
        BtnReady.GetComponent<Button>().onClick.RemoveAllListeners();
        BtnReady.GetComponent<Button>().onClick.AddListener(onClickPlay);
        BtnReady.GetComponentInChildren<Text>().text = "Play";
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myherotype);
            stream.SendNext(ReadyCheck);
            stream.SendNext(MyName);
        }
        else
        {
            myherotype = (hcp.E_HeroType)stream.ReceiveNext();
            ReadyCheck = (bool)stream.ReceiveNext();
            MyName = (string)stream.ReceiveNext();
        }
    }
}