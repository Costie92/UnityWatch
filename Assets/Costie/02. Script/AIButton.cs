using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class AIButton : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private GameObject LobbyPlyaers;
    // Use this for initialization
    void Start () {
        button = GetComponent<Button>();
        button.onClick.AddListener(onClickAI);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void onClickAI() {
        PhotonNetwork.Instantiate(LobbyPlyaers.name, this.transform.position, this.transform.rotation, 0);
    }
}
