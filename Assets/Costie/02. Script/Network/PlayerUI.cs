using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;

public class PlayerUI : MonoBehaviour {

    [SerializeField] private TextMesh m_Text;

    private PhotonView photonView;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void UpdateUI(int newHealth) {
        
        Debug.Log("My Health is : " + newHealth + ", my ID is : " + photonView.ViewID);
        m_Text.text = newHealth.ToString();
    }
}
