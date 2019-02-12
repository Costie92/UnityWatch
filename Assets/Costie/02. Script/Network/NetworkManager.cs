using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject LobbyPlyaers;
    [SerializeField] private GameObject TeamInfo;
    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private bool TimerStart;
    [SerializeField] private Text TimerText;
    [SerializeField] private int myID;
    [SerializeField] private int RandomNumber;

    public bool GameEnd;
    public GameObject buttons;
    public Sprite imageSoldier, imageHook, imageReady, imageUnReady;
    public int ReadyCount, TeamACount = 0, TeamBCount = 0;

    private System.TimeSpan timeSpan;
    private double PhotonTime;
    private System.TimeSpan nowTime;

    private Dictionary<int, bool> players;
    private Dictionary<int, string> teams;
    private Dictionary<int, hcp.E_HeroType> heros;
    private Dictionary<int, string> names;
    

    private static NetworkManager _instance = null;
    public static NetworkManager instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("NetworkManager is NULL");
            return _instance;
        }
    }

    public Dictionary<int, bool> Players
    {
        get
        {
            return players;
        }

        set
        {
            players = value;
        }
    }
    public Dictionary<int, string> Teams
    {
        get
        {
            return teams;
        }

        set
        {
            teams = value;
        }
    }
    public Dictionary<int, hcp.E_HeroType> Heros
    {
        get
        {
            return heros;
        }

        set
        {
            heros = value;
        }
    }
    public Dictionary<int, string> Names
    {
        get
        {
            return names;
        }

        set
        {
            names = value;
        }
    }


    private void Awake()
    {
        _instance = this;
    }
    // Use this for initialization
    void Start()
    {

        ///게임 시간을 5분으로 설정
        GameEnd = false;
        TimerStart = false;
        timeSpan = new System.TimeSpan(0, 5, 0);
        nowTime = new System.TimeSpan(0, 5, 0);
        buttons = GameObject.Find("Buttons");
        buttons.SetActive(false);


        ///딕셔너리 초기화
        Players = new Dictionary<int, bool>();
        Teams = new Dictionary<int, string>();
        Heros = new Dictionary<int, hcp.E_HeroType>();
        names = new Dictionary<int, string>();

        myID = 0;
        ReadyCount = 0;


        ///포톤 세팅
        PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.ConnectUsingSettings();

        DontDestroyOnLoad(this);

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (TimerStart)
            {
                if (nowTime.TotalSeconds > 0)
                {
                    nowTime = (timeSpan - System.TimeSpan.FromSeconds(PhotonNetwork.Time - PhotonTime));
                }
                else
                {
                    if (!GameEnd)
                    {
                        //do victory check;
                        Debug.Log(nowTime.TotalSeconds);
                        GameEnd = true;
                    }
                }
                string DisplayText = string.Format("{0}:{1:00}", (int)nowTime.TotalMinutes, nowTime.Seconds);
                TimerText.text = DisplayText;
            }
        }
    }

    ///멀티 씬으로 전환시 플레이어 생성
    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if (arg1.name == "MultiScenes")
        {
            if (PhotonNetwork.InRoom)
            {
                TimerText = GameObject.Find("Timer").GetComponent<Text>();
                StartCoroutine(CreatePlayer());
            }
        }
        string currentName = arg0.name;

        if (currentName == null)
        {
            currentName = "Replaced";
        }
    }

    IEnumerator CreatePlayer()
    {
        int myPos = 0;
        yield return new WaitForSeconds(1.0f);

        ///마스터 클라이언트의 생성시간 전송
        if (PhotonNetwork.IsMasterClient)
        {
            TimerStart = true;
            PhotonTime = PhotonNetwork.Time;
            photonView.RPC("SendTimerSetting", RpcTarget.AllBufferedViaServer, PhotonTime, true);
        }
        string Soldierpath = hcp.Constants.GetHeroPhotonNetworkInstanciatePath(hcp.E_HeroType.Soldier);
        string Hookpath = hcp.Constants.GetHeroPhotonNetworkInstanciatePath(hcp.E_HeroType.Hook);


        ////스폰 위치 설정
        foreach (KeyValuePair<int, string> pair in Teams)
        {
            if (pair.Key == myID)
            {
                break;
            }
            if (pair.Value == Teams[myID]) {
                myPos++;
            }
        }
        if (Teams[myID] == hcp.Constants.teamA_LayerName)
        {
            SpawnPoint = MapInfo.instance.ASpawnPoint;
            //Debug.Log(myID + " : Create at A");
        }
        else if (Teams[myID] == hcp.Constants.teamB_LayerName)
        {
            SpawnPoint = MapInfo.instance.BSpawnPoint;
            //Debug.Log(myID + " : Create at B");
        }
        else if (Teams[myID] == hcp.Constants.teamC_LayerName)
        {
            SpawnPoint = MapInfo.instance.CSpawnPoint;
            //Debug.Log(myID + " : Create at C");
        }
        else if (Teams[myID] == hcp.Constants.teamD_LayerName)
        {
            SpawnPoint = MapInfo.instance.DSpawnPoint;
            //Debug.Log(myID + " : Create at D");
        }
        SpawnPoint.position += new Vector3(0, 0, 2 * myPos);

        if (Heros[myID] == hcp.E_HeroType.Soldier)
        {
            PhotonNetwork.Instantiate(Soldierpath, SpawnPoint.position, SpawnPoint.rotation, 0);
        }
        else {
            PhotonNetwork.Instantiate(Hookpath, SpawnPoint.position, SpawnPoint.rotation, 0);
        }
        if (!TimerStart && !PhotonNetwork.IsMasterClient) {
            photonView.RPC("RequestTime", RpcTarget.MasterClient);
        }
    }

    #region photon networking
    System.Action onClientLeft;
    public void AddListenerOnClientLeft(System.Action ac)
    {
        onClientLeft += ac;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        ///플레이어가 나갔을 시 딕셔너리에서 제거
        if (SceneManager.GetActiveScene().buildIndex == 1) {
            RemoveDictionary(otherPlayer.ActorNumber);
        }
        if (onClientLeft != null)
        {
            onClientLeft();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {

        ///마스터 클라이언트가 되었을 때 버튼 설정 변경
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PlayerTeam[] objs = FindObjectsOfType<PlayerTeam>();
                foreach (PlayerTeam items in objs)
                {
                    if (items.photonView.ViewID / 1000 == myID)
                    {
                        items.BecomeToMaster();
                    }
                }
            }
        }
    }

    //포톤 서버 접속 시 로비 입장
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }
    //로비 입장시 아이디 확인 이후 랜덤 룸 접속
    public override void OnJoinedLobby()
    {
        if (PlayerName.instance.HaveName)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }
    //룸에 접속 성공하면 로비 캐릭터 생성
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(LobbyPlyaers.name, this.transform.position, this.transform.rotation, 0);
        //Debug.Log("Joined Room");
    }
    //룸에 접속 실패 시 룸 생성
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log("Can't join random room!");
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)4 };
        RandomNumber = Random.Range(0, 10000);
        PhotonNetwork.CreateRoom(RandomNumber.ToString("N"), roomOps);
    }

    #endregion

    #region RPCs
    //게임 시작 시간 요청 / 전송
    [PunRPC]
    public void RequestTime() {
        photonView.RPC("SendTimerSetting", RpcTarget.AllBufferedViaServer, PhotonTime, true);
    }
    [PunRPC]
    public void SendTimerSetting(double time, bool Start) {
        PhotonTime = time;
        TimerStart = Start;
    }
    //팀 선택 시 딕셔너리 저장
    [PunRPC]
    public void SelectTeam(int pViewID,string TeamString)
    {
        int pVID = pViewID / 1000;
        switch (TeamString) {
            case "A":
                //Debug.Log(pVID + " : Assigned A");
                Teams[pVID] = hcp.Constants.teamA_LayerName;
                break;
            case "B":
                //Debug.Log(pVID + " : Assigned B");
                Teams[pVID] = hcp.Constants.teamB_LayerName;
                break;
            default:
                break;
        }
    }
    //영웅 선택 시 딕셔너리 저장
    [PunRPC]
    public void SelectHeroo(int pViewID, hcp.E_HeroType heroType) {
        int pVID = pViewID / 1000;
        switch (heroType) {
            case hcp.E_HeroType.Soldier:
                //Debug.Log(pVID + " : Select Soldier");
                break;
            case hcp.E_HeroType.Hook:
                //Debug.Log(pVID + " : Select Hook");
                break;
            default:
                break;
        }
        Heros[pVID] = heroType;
        
    }
    //룸에 입장 시 딕셔너리 생성
    [PunRPC]
    public void Join(int pViewID)
    {
        int pVID = pViewID / 1000;
        if (!Players.ContainsKey(pVID))
        {
            if (myID == 0)
                myID = pVID;
            Players.Add(pVID, false);
            //Debug.Log(" ID : " + pVID + " Joined");
        }
        if (!Heros.ContainsKey(pVID)) {
            Heros.Add(pVID, hcp.E_HeroType.Soldier);
            //Debug.Log("My Hero is Soldier");
        }
        if (!Teams.ContainsKey(pVID))
        {
            TeamACount = 0;
            TeamBCount = 0;
            foreach (KeyValuePair<int, string> pair in Teams) {
                if (pair.Value == hcp.Constants.teamA_LayerName)
                {
                    TeamACount++;
                }
                else if (pair.Value == hcp.Constants.teamB_LayerName) {
                    TeamBCount++;
                }
            }
            if ((TeamACount <= TeamBCount))
            {
                //Debug.Log("Assigned A");
                Teams.Add(pVID, hcp.Constants.teamA_LayerName);
            }
            else
            {
                //Debug.Log("Assigned B");
                Teams.Add(pVID, hcp.Constants.teamB_LayerName);
            }
        }
        int[] DicpView = new int[Players.Count];
        Players.Keys.CopyTo(DicpView, 0);
        string[] DicTeam = new string[Teams.Count];
        Teams.Values.CopyTo(DicTeam, 0);
        bool[] DicPlayer = new bool[Players.Count];
        Players.Values.CopyTo(DicPlayer, 0);
        int[] DicHero = new int[Heros.Count];
        int count = 0;
        foreach (KeyValuePair<int, hcp.E_HeroType> items in Heros) {
            DicHero[count] = (int)items.Value;
            count++;
        }

        photonView.RPC("MasterDictionary", RpcTarget.Others, DicpView, DicTeam, DicPlayer, DicHero, pViewID);
    }

    //마스터 클라이언트 딕셔너리 동기화
    [PunRPC]
    public void MasterDictionary(int[] DicpView, string[] DicTeam, bool[] DicPlayer, int[] DicHero, int pViewID)
    {
        int pVID = pViewID / 1000;
        if (myID == 0)
        {
            myID = pVID;
        }
        for (int i = 0; i < DicpView.Length; i++)
        {
            if (!Players.ContainsKey(DicpView[i]))
            {
                Players.Add(DicpView[i], DicPlayer[i]);
            }
            else
            {
                Players[DicpView[i]] = DicPlayer[i];
            }
            if (!Teams.ContainsKey(DicpView[i]))
            {
                Teams.Add(DicpView[i], DicTeam[i]);
            }
            else
            {
                Teams[DicpView[i]] = DicTeam[i];
            }
            if (!Heros.ContainsKey(DicpView[i]))
            {
                Heros.Add(DicpView[i], (hcp.E_HeroType)DicHero[i]);
            }
            else
            {
                Heros[DicpView[i]] = (hcp.E_HeroType)DicHero[i];
            }
        }
        PlayerTeam[] objs = FindObjectsOfType<PlayerTeam>();
        foreach (PlayerTeam items in objs)
        {
            if (items.photonView.IsMine)
            {
                //Debug.Log("HI");
            }
            items.PosAfterDictionary();
        }
    }
    //룸에 자신의 이름 딕셔너리 추가
    [PunRPC]
    public void Named(int pViewID, string name)
    {
        int pVID = pViewID / 1000;
        if (!Names.ContainsKey(pVID))
        {
            Names.Add(pVID, name);
        }
    }
    //레디 시 상태정보 전송
    [PunRPC]
    public void Ready(int pViewID)
    {
        int pVID = pViewID / 1000;
        Players[pVID] = !Players[pVID];
        if (Players[pVID])
        {
            //Debug.Log(" ID : " + pVID + " Ready");
        }
        else
        {
            //Debug.Log(" ID : " + pVID + " UnReady");
        }
    }
    //플레이어 레디 상태 체크 이후 씬 전환
    [PunRPC]
    public void Play()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Photon.Realtime.Player[] photonPlayers = PhotonNetwork.PlayerListOthers;
            var enumerator = Players.GetEnumerator();
            ReadyCount = 0;
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, bool> items = enumerator.Current;
                if (items.Value == true)
                {
                    ReadyCount++;
                }
            }
            var Teamenumerator = Teams.GetEnumerator();

            TeamACount = 0;
            TeamBCount = 0;
            while (Teamenumerator.MoveNext())
            {
                KeyValuePair<int, string> items = Teamenumerator.Current;
                if (items.Value == hcp.Constants.teamA_LayerName)
                {
                    TeamACount++;
                }
                else {
                    TeamBCount++;
                }
            }
            if ((photonPlayers.Length == ReadyCount && TeamACount == TeamBCount) || photonPlayers.Length == 0)
            {
                buttons = null;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.LoadLevel(2);
            }
        }
    }
    #endregion

    //플레이어가 나갈 경우 딕셔너리 제거 성공 여부
    public void RemoveDictionary(int pViewID) {
        bool RemoveSuccess = true;
        if (Players.ContainsKey(pViewID)) {
            RemoveSuccess = RemoveSuccess && Players.Remove(pViewID);
        }
        if (Teams.ContainsKey(pViewID)) {
            RemoveSuccess = RemoveSuccess && Teams.Remove(pViewID);
        }
        if (Heros.ContainsKey(pViewID)){
            RemoveSuccess = RemoveSuccess && Heros.Remove(pViewID);
        }
        if (Names.ContainsKey(pViewID)) {
            RemoveSuccess = RemoveSuccess && Names.Remove(pViewID);
        }
        //Debug.Log(RemoveSuccess);
    }

    //랜덤한 방에 접속 시도
    public void TryJoinRandomRoom() {
        PhotonNetwork.JoinRandomRoom();
    }
}