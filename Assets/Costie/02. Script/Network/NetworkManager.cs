using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks {

    [SerializeField] private Text connectText;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject lobbycamera;
    [SerializeField] private Dictionary<int,bool> players;
    [SerializeField] private int ReadyCount;
    // Use this for initialization
    void Start () {
        players = new Dictionary<int, bool>();
        ReadyCount = 0;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.ConnectUsingSettings();
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if (arg1.name == "MultiScenes")
        {
            if (PhotonNetwork.InRoom)
            {
                Destroy(photonView);
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

    IEnumerator CreatePlayer() {
        yield return new WaitForSeconds(1.0f);
        lobbycamera = GameObject.Find("LobbyCamera");
        lobbycamera.SetActive(false);
        spawnPoint = GameObject.Find("AspawnPoint").transform;
        PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
    }

    // Update is called once per frame
    void Update () {
        
        //Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        //connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
        if (PhotonNetwork.InRoom) {
            if (PhotonNetwork.IsMasterClient)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    Photon.Realtime.Player[] photonPlayers = PhotonNetwork.PlayerListOthers;
                    Debug.Log("Plyaer Count : " + photonPlayers.Length);
                    foreach (KeyValuePair<int,bool> items in players) {
                        if (players[items.Key] == true) {
                            ReadyCount++;
                        } 
                    }
                    if (photonPlayers.Length == ReadyCount)
                    {
                        PhotonNetwork.LoadLevel(1);
                    }
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.R)) {
                    photonView.RPC("Ready", RpcTarget.All);
                }

            }
        }
	}

    [PunRPC]
    public void Join(PhotonMessageInfo mi)
    {
        
        players.Add(mi.Sender.ActorNumber, false);
        Debug.Log(" ID : " + mi.Sender.ActorNumber + " Joined");
    }
    [PunRPC]
    public void Ready(PhotonMessageInfo mi)
    {
        players[mi.Sender.ActorNumber] = !players[mi.Sender.ActorNumber];
        if (players[mi.Sender.ActorNumber])
        {
            Debug.Log(" ID : " + mi.Sender.ActorNumber + " Ready");
        }
        else {
            Debug.Log(" ID : " + mi.Sender.ActorNumber + " UnReady");
        }
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        //PhotonNetwork.JoinRandomRoom();
        //RoomOptions roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom("Test",null,null);
    }
    public override void OnJoinedRoom() {

        Debug.Log("Joined Room");
        if (photonView.IsMine) {
            Debug.Log("Is Mine");
        }
        if (!PhotonNetwork.IsMasterClient) {
            photonView.RPC("Join", RpcTarget.All);
        }
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Can't join random room!");
        PhotonNetwork.CreateRoom(null);
    }

}