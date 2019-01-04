using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
public class NetworkManager : Photon.MonoBehaviour {

    [SerializeField] private Text connectText;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject lobbycamera;

    // Use this for initialization
    void Start () {
        PhotonNetwork.ConnectUsingSettings("0.1");
        
	}

    // Update is called once per frame
    void Update () {
        //Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        connectText.text = PhotonNetwork.connectionStateDetailed.ToString();
	}

    public virtual void OnJoinedLobby()
    {
        //PhotonNetwork.JoinRandomRoom();
        //RoomOptions roomOptions = new RoomOptions();
        PhotonNetwork.JoinOrCreateRoom("Test",null,null);
    }
    public virtual void OnJoinedRoom() {
        PhotonNetwork.Instantiate(player.name, spawnPoint.position, spawnPoint.rotation, 0);
        lobbycamera.SetActive(false);
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Can't join random room!");
        PhotonNetwork.CreateRoom(null);
    }

}
