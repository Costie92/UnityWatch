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

    public Image MyHeroImage;
    public Text MyNameText;
    public Image ReadyFrame;
    public hcp.E_HeroType myherotype;

    [SerializeField] private bool ReadyCheck;
    [SerializeField] private string MyTeam, MyName;
    [SerializeField] private Button BtnReady, BtnSoldier, BtnHook;
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
                //각 버튼에 리스너 추가
                NetworkManager.instance.buttons.SetActive(true);
                BtnReady = NetworkManager.instance.buttons.transform.GetChild(2).GetComponent<Button>();
                BtnSoldier = NetworkManager.instance.buttons.transform.GetChild(3).GetComponent<Button>();
                BtnHook = NetworkManager.instance.buttons.transform.GetChild(4).GetComponent<Button>();
                BtnSoldier.GetComponent<Button>().onClick.AddListener(delegate { onClickHeroButton(hcp.E_HeroType.Soldier); });
                BtnHook.GetComponent<Button>().onClick.AddListener(delegate { onClickHeroButton(hcp.E_HeroType.Hook); });
                if (PhotonNetwork.IsMasterClient)
                {
                    BecomeToMaster();
                }
                else
                    BtnReady.GetComponent<Button>().onClick.AddListener(onClickReady);
                /*
                 * BtnTeamA = NetworkManager.instance.buttons.transform.GetChild(0).GetComponent<Button
                 * BtnTeamB = NetworkManager.instance.buttons.transform.GetChild(1).GetComponent<Button>();
                 * BtnTeamA.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("A"); });
                 * BtnTeamB.GetComponent<Button>().onClick.AddListener(delegate { onClicKTeamButton("B"); });
                 */
                
            }
            else
            {
                //m.enabled = false;
            }
        }

        //각 플레이어에게 접속했다고 RPC전송
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
        //나의 레디 / 영웅 / 이름 정보 업데이트
        ReadyFrame.sprite = ReadyCheck ? NetworkManager.instance.imageReady : NetworkManager.instance.imageUnReady;
        MyHeroImage.sprite = myherotype == hcp.E_HeroType.Soldier ? NetworkManager.instance.imageSoldier : NetworkManager.instance.imageHook;
        MyNameText.text = MyName;
    }
    public void PosTeam(string TeamLayer,int Pos)
    {
        //팀에 접속한 인원 만큼 나의 위치는 아래로 위치함
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
        //딕셔너리의 팀원의 숫자 만큼 나의 위치가 지정됨
        MyTeam = NetworkManager.instance.Teams[photonView.ViewID / 1000];
        int MyPos = 0;
        foreach (KeyValuePair<int, string> pair in NetworkManager.instance.Teams) {
            if (pair.Key == photonView.ViewID / 1000)
            {
                break;
            }
            if (MyTeam == pair.Value) {
                MyPos++;
            }
        }
        
        PosTeam(MyTeam, MyPos);
    }
    public void onClicKTeamButton(string TeamString)
    {
        NetworkManager.instance.photonView.RPC("SelectTeam", RpcTarget.All, photonView.ViewID, TeamString);
    }
    //각 버튼 클릭시 RPC를 플레이어들에게 전송
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

    //마스터 클라이언트가 되었을 시 버튼 기능 변경
    public void BecomeToMaster() {
        BtnReady.GetComponent<Button>().onClick.RemoveAllListeners();
        BtnReady.GetComponent<Button>().onClick.AddListener(onClickPlay);
        BtnReady.GetComponentInChildren<Text>().text = "Play";
    }

    //내 상태가 변경 될 때 포톤으로 정보 전송
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