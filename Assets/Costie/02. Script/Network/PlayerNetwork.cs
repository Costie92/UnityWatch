using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNetwork : MonoBehaviour {

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
        if (!photonView.isMine)
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
        if (photonView.isMine)
        {
            m_UIText.text = Health.ToString();
        }
        else {
            m_UIText.text = "";
            Playercamera.SetActive(false);
            foreach (MonoBehaviour m in playerControlscripts) {
                m.enabled = false;
            }
        }
        updateUI(Health);
    }

    private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.isWriting)
        {
            Debug.Log("Write");
            stream.SendNext(Health);
        }
        else if (stream.isReading)
        {
            Debug.Log("Read");
            Health = (int)stream.ReceiveNext();
            updateUI(Health);
        }
    }
}
