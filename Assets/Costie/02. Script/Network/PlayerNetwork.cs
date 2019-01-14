using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class PlayerNetwork : MonoBehaviour, IPunObservable {

    [SerializeField] private GameObject Playercamera;
    [SerializeField] private MonoBehaviour[] playerControlscripts;
    [SerializeField] private Text m_UIText;
    private delegate void UpdateUI(int newHealth);
    private event UpdateUI updateUI;

    private PhotonView photonView;

    public int Health = 100;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        updateUI += FindObjectOfType<PlayerUI>().UpdateUI;
        Initiliaze();
    }
    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        /*
        if (Input.GetKeyDown(KeyCode.E))
        {
            Health -= 5;
        }
        */
        m_UIText.text = Health.ToString();
    }

    [PunRPC]
    public void ApplyDamage(int Damage) {
        Health -= Damage;
        
    }
    private void Initiliaze() {
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        int TeamKey = (photonView.ViewID / 1000) * 1000 + 1;
        switch (networkManager.Teams[TeamKey]) {
            case "A":
                this.gameObject.layer = 9;
                break;
            case "B":
                this.gameObject.layer = 10;
                break;
            case "C":
                this.gameObject.layer = 11;
                break;
            case "D":
                this.gameObject.layer = 12;
                break;
        }
        
        if (photonView.IsMine)
        {
            m_UIText.text = Health.ToString();
        }
        else {
            Debug.Log(photonView.ViewID);
            m_UIText.text = "";
            Playercamera.SetActive(false);
            foreach (MonoBehaviour m in playerControlscripts) {
                m.enabled = false;
            }
        }
        updateUI(Health);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting)
        {
            stream.SendNext(Health);
        }
        else if (stream.IsReading)
        {
            Health = (int)stream.ReceiveNext();
            updateUI(Health);
        }
    }
}
