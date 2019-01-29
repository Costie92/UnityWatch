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
    public bool GameEnd;
    public GameObject buttons;
    public Sprite imageSoldier, imageHook, imageReady, imageUnReady;
    private System.TimeSpan timeSpan;
    private double PhotonTime;
    private System.TimeSpan nowTime;
    //[SerializeField] private GameObject lobbycamera;

    private Dictionary<int, bool> players;
    private Dictionary<int, string> teams;
    private Dictionary<int, hcp.E_HeroType> heros;
    private Dictionary<int, string> names;
    [SerializeField] private int myID;
    public int ReadyCount, TeamACount = 0, TeamBCount = 0;
    [SerializeField] private int RandomNumber;

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
        GameEnd = false;
        TimerStart = false;
        timeSpan = new System.TimeSpan(0, 10, 0);
        nowTime = new System.TimeSpan(0, 10, 0);
        buttons = GameObject.Find("Buttons");
        buttons.SetActive(false);
        myID = 0;
        Players = new Dictionary<int, bool>();
        Teams = new Dictionary<int, string>();
        Heros = new Dictionary<int, hcp.E_HeroType>();
        names = new Dictionary<int, string>();
        ReadyCount = 0;
        PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.ConnectUsingSettings();
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    // Update is called once per frame
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

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if (arg1.name == "MultiScenes")
        {
            foreach (KeyValuePair<int, string> pair in Names) {
                Debug.Log("IDs : " + pair.Key + "Names : " + pair.Value);
            }
            if (PhotonNetwork.InRoom)
            {
                TimerText = GameObject.Find("Timer").GetComponent<Text>();
                StartCoroutine(CreatePlayer());
            }
        }
        string currentName = arg0.name;

        if (currentName == null)
        {
            // Scene1 has been removed
            currentName = "Replaced";
        }

        Debug.Log("Scenes: " + currentName + ", " + arg1.name);
    }

    IEnumerator CreatePlayer()
    {

        yield return new WaitForSeconds(1.0f);
        //lobbycamera = GameObject.Find("LobbyCamera");
        //if (lobbycamera)
        //    lobbycamera.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            TimerStart = true;
            PhotonTime = PhotonNetwork.Time;
            photonView.RPC("SendTimerSetting", RpcTarget.AllBufferedViaServer, PhotonTime, true);
        }
        string Soldierpath = hcp.Constants.GetHeroPhotonNetworkInstanciatePath(hcp.E_HeroType.Soldier);
        string Hookpath = hcp.Constants.GetHeroPhotonNetworkInstanciatePath(hcp.E_HeroType.Hook);
        //Destroy(photonView);
        if (Teams[myID] == hcp.Constants.teamA_LayerName)
        {
            SpawnPoint = MapInfo.instance.ASpawnPoint;
            Debug.Log(myID + " : Create at A");

        }
        else if (Teams[myID] == hcp.Constants.teamB_LayerName)
        {
            SpawnPoint = MapInfo.instance.BSpawnPoint;
            Debug.Log(myID + " : Create at B");
        }
        else if (Teams[myID] == hcp.Constants.teamC_LayerName)
        {
            SpawnPoint = MapInfo.instance.CSpawnPoint;
            Debug.Log(myID + " : Create at C");
        }
        else if (Teams[myID] == hcp.Constants.teamD_LayerName)
        {
            SpawnPoint = MapInfo.instance.DSpawnPoint;
            Debug.Log(myID + " : Create at D");
        }
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

    System.Action onClientLeft;
    public void AddListenerOnClientLeft(System.Action ac)
    {
        onClientLeft += ac;
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
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
        //Debug.Log(newPlayer.ActorNumber + " Enter");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
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

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        //Debug.Log("Joined Lobby");
        //PhotonNetwork.JoinRandomRoom();
        //RoomOptions roomOptions = new RoomOptions();
        if (PlayerName.instance.HaveName)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        Hashtable hashTable = new Hashtable();
        PhotonNetwork.Instantiate(LobbyPlyaers.name, this.transform.position, this.transform.rotation, 0);
        //Debug.Log("Joined Room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.Log("Can't join random room!");
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)4 };
        RandomNumber = Random.Range(0, 10000);
        PhotonNetwork.CreateRoom(RandomNumber.ToString("N"), roomOps);
    }

    [PunRPC]
    public void RequestTime() {
        photonView.RPC("SendTimerSetting", RpcTarget.AllBufferedViaServer, PhotonTime, true);
    }

    [PunRPC]
    public void SendTimerSetting(double time, bool Start) {
        PhotonTime = time;
        //TimeStarted = System.DateTime.Parse(date);
        TimerStart = Start;
    }

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
    [PunRPC]
    public void Named(int pViewID, string name)
    {
        int pVID = pViewID / 1000;
        if (!Names.ContainsKey(pVID))
        {
            Names.Add(pVID, name);
            //Debug.Log(" ID : " + pVID + "Name is : " + name);
        }
    }

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
                PhotonNetwork.LoadLevel(1);
            }
        }
    }

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

    public void TryJoinRandomRoom() {
        PhotonNetwork.JoinRandomRoom();
    }
}