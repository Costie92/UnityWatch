﻿using System.Collections;
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

    [SerializeField] private Text connectText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject Lobby;
    [SerializeField] private Transform AspawnPoint;
    [SerializeField] private Transform BspawnPoint;
    [SerializeField] private Transform CspawnPoint;
    [SerializeField] private Transform DspawnPoint;
    [SerializeField] private GameObject TeamInfo;
    public GameObject buttons;
    //[SerializeField] private GameObject lobbycamera;

    [SerializeField] private Dictionary<int, bool> players;
    [SerializeField] private Dictionary<int, string> teams;
    [SerializeField] private int myID;
    [SerializeField] private int ReadyCount;
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

    private void Awake()
    {
        _instance = this;
    }
    // Use this for initialization
    void Start()
    {
        buttons = GameObject.Find("Buttons");
        buttons.SetActive(false);
        myID = 0;
        Players = new Dictionary<int, bool>();
        Teams = new Dictionary<int, string>();
        ReadyCount = 0;
        PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.ConnectUsingSettings();
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        Debug.Log(myID + " SceneManagement");
        if (arg1.name == "MultiScenes")
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.Log(myID + " : StartCoroutine");
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

        string playerpath = hcp.Constants.GetHeroPhotonNetworkInstanciatePath(hcp.E_HeroType.Soldier);
        Destroy(photonView);
        Debug.Log(myID);
        Debug.Log(Teams[myID]);
        if (Teams[myID] == hcp.Constants.teamA_LayerName)
        {
            AspawnPoint = GameObject.Find("AspawnPoint").transform;
            PhotonNetwork.Instantiate(playerpath, AspawnPoint.position, AspawnPoint.rotation, 0);

            Debug.Log(myID + " : Create at A");

        }
        else if (Teams[myID] == hcp.Constants.teamB_LayerName)
        {
            BspawnPoint = GameObject.Find("BspawnPoint").transform;
            PhotonNetwork.Instantiate(playerpath, BspawnPoint.position, BspawnPoint.rotation, 0);

            Debug.Log(myID + " : Create at B");
        }
        else if (Teams[myID] == hcp.Constants.teamC_LayerName)
        {
            CspawnPoint = GameObject.Find("CspawnPoint").transform;
            GameObject obj = PhotonNetwork.Instantiate(playerpath, CspawnPoint.position, CspawnPoint.rotation, 0);
            Debug.Log(myID + " : Create at C");
        }
        else if (Teams[myID] == hcp.Constants.teamD_LayerName)
        {
            DspawnPoint = GameObject.Find("DspawnPoint").transform;
            GameObject obj = PhotonNetwork.Instantiate(playerpath, DspawnPoint.position, DspawnPoint.rotation, 0);
            Debug.Log(myID + " : Create at D");
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        //connectText.text = PhotonNetwork.connectionStateDetailed.ToString();

    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        //PhotonNetwork.JoinRandomRoom();
        //RoomOptions roomOptions = new RoomOptions();
            PhotonNetwork.JoinOrCreateRoom("Test", null, null);
    }
    public override void OnJoinedRoom()
    {
        Hashtable hashTable = new Hashtable();
        Debug.Log(" Player Count : " + PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.Instantiate(Lobby.name, this.transform.position, this.transform.rotation, 0);
        Debug.Log("Joined Room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Can't join random room!");
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)4 };
        RandomNumber = Random.Range(0, 10000);
        PhotonNetwork.CreateRoom(RandomNumber.ToString("N"));
    }

    [PunRPC]
    public void SelectTeam(int pViewID,string TeamString)
    {
        int pVID = pViewID / 1000;
        switch (TeamString) {
            case "A":
                Debug.Log(pVID + " : Assigned A");
                Teams[pVID] = hcp.Constants.teamA_LayerName;
                break;
            case "B":
                Debug.Log(pVID + " : Assigned B");
                Teams[pVID] = hcp.Constants.teamB_LayerName;
                break;
            default:
                break;
        }
        
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
            Debug.Log(" ID : " + pVID + " Joined");
        }
        if (!Teams.ContainsKey(pVID))
        {

            if ((pVID % 2 == 0))
            {
                Debug.Log("Assigned A");
                Teams.Add(pVID, hcp.Constants.teamA_LayerName);
            }
            else
            {
                Debug.Log("Assigned B");
                Teams.Add(pVID, hcp.Constants.teamB_LayerName);
            }
        }
    }
    [PunRPC]
    public void Ready(int pViewID)
    {
        int pVID = pViewID / 1000;
        Players[pVID] = !Players[pVID];
        if (Players[pVID])
        {
            Debug.Log(" ID : " + pVID + " Ready");
        }
        else
        {
            Debug.Log(" ID : " + pVID + " UnReady");
        }
    }
    [PunRPC]
    public void Play()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Photon.Realtime.Player[] photonPlayers = PhotonNetwork.PlayerListOthers;
            Debug.Log("Plyaer Count : " + photonPlayers.Length);
            var enumerator = Players.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<int, bool> items = enumerator.Current;
                if (items.Value == true)
                {
                    ReadyCount++;
                }
                Debug.Log("Key : " + items.Key + ", Value : " + items.Value);
                if (photonPlayers.Length == ReadyCount)
                {
                    buttons = null;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.LoadLevel(1);
                }
            }
        }
    }
}