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

    [SerializeField] private Text connectText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject Lobby;
    [SerializeField] private Transform AspawnPoint;
    [SerializeField] private Transform BspawnPoint;
    [SerializeField] private Transform CspawnPoint;
    [SerializeField] private Transform DspawnPoint;
    [SerializeField] private GameObject lobbycamera;

    [SerializeField] private Dictionary<int, bool> players;
    [SerializeField] private Dictionary<int, string> teams;
    [SerializeField] private int myID;
    [SerializeField] private int ReadyCount;

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

    // Use this for initialization
    void Start()
    {
        myID = 0;
        Players = new Dictionary<int, bool>();
        Teams = new Dictionary<int, string>();
        ReadyCount = 0;
        PhotonNetwork.AutomaticallySyncScene = true;
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
        lobbycamera = GameObject.Find("LobbyCamera");
        if (lobbycamera)
            lobbycamera.SetActive(false);
        
        
        Destroy(photonView);
        Debug.Log(myID);
        if (Teams[myID] == "A")
        {
            AspawnPoint = GameObject.Find("AspawnPoint").transform;
            PhotonNetwork.Instantiate(player.name, AspawnPoint.position, AspawnPoint.rotation, 0);
            Debug.Log(myID + " : Create at A");

        }
        else if (Teams[myID] == "B")
        {
            BspawnPoint = GameObject.Find("BspawnPoint").transform;
            PhotonNetwork.Instantiate(player.name, BspawnPoint.position, BspawnPoint.rotation, 0);

            Debug.Log(myID + " : Create at B");
        }
        else if (Teams[myID] == "C")
        {
            CspawnPoint = GameObject.Find("CspawnPoint").transform;
            GameObject obj = PhotonNetwork.Instantiate(player.name, CspawnPoint.position, CspawnPoint.rotation, 0);
            Debug.Log(myID + " : Create at C");
        }
        else if (Teams[myID] == "D")
        {
            DspawnPoint = GameObject.Find("DspawnPoint").transform;
            GameObject obj = PhotonNetwork.Instantiate(player.name, DspawnPoint.position, DspawnPoint.rotation, 0);
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
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
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

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Can't join random room!");
        PhotonNetwork.CreateRoom(null);
    }

    [PunRPC]
    public void SelectTeam(int pViewID)
    {
        if (!Teams.ContainsKey(pViewID))
        {
            if ((pViewID / 1000) % 2 == 0)
            {
                Debug.Log("Assigned A");
                Teams.Add(pViewID, "A");
            }
            else
            {
                Debug.Log("Assigned B");
                Teams.Add(pViewID, "B");
            }
        }
    }

    [PunRPC]
    public void Join(int pViewID)
    {

        if (!Players.ContainsKey(pViewID))
        {
            if (myID == 0)
                myID = pViewID;
            Players.Add(pViewID, false);
            Debug.Log(" ID : " + pViewID + " Joined");
        }
    }
    [PunRPC]
    public void Ready(int pViewID)
    {
        Players[pViewID] = !Players[pViewID];
        if (Players[pViewID])
        {
            Debug.Log(" ID : " + pViewID + " Ready");
        }
        else
        {
            Debug.Log(" ID : " + pViewID + " UnReady");
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
                    PhotonNetwork.LoadLevel(1);
                }
            }
        }
    }
}