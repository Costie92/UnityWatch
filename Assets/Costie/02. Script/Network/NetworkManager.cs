using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : Photon.MonoBehaviour {

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
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings("0.1");
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if (arg1.name == "MultiScenes")
        {
            if (PhotonNetwork.inRoom)
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
        spawnPoint = GameObject.Find("spawnPoint").transform;
        PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
    }

    // Update is called once per frame
    void Update () {
        
        //Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        //connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
        if (PhotonNetwork.inRoom) {
            if (PhotonNetwork.isMasterClient)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    PhotonPlayer[] photonPlayers = PhotonNetwork.otherPlayers;
                    Debug.Log("Plyaer Count : " + photonPlayers.Length);
                    foreach (KeyValuePair<int,bool> items in players) {
                        if (players[items.Key] == true) {
                            ReadyCount++;
                        } 
                    }
                    if (photonPlayers.Length == ReadyCount)
                    {
                        PhotonNetwork.LoadLevelAsync(1);
                    }
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.R)) {
                    photonView.RPC("Ready", PhotonTargets.All);
                }

            }
        }
	}

    [PunRPC]
    public void Join(PhotonMessageInfo mi)
    {
        players.Add(mi.sender.ID, false);
        Debug.Log(mi.sender.ID + " Joined");
    }
    [PunRPC]
    public void Ready(PhotonMessageInfo mi)
    {
        players[mi.sender.ID] = true;
        Debug.Log(mi.sender.ID + " Ready");
    }
    public virtual void OnJoinedLobby()
    {
        //PhotonNetwork.JoinRandomRoom();
        //RoomOptions roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom("Test",null,null);
    }
    public virtual void OnJoinedRoom() {

        Debug.Log("Joined Room");
        if (!PhotonNetwork.isMasterClient) {
            photonView.RPC("Join", PhotonTargets.All);
        }
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Can't join random room!");
        PhotonNetwork.CreateRoom(null);
    }

}