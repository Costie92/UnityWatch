using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;
namespace hcp
{
    public class test : MonoBehaviourPun, IPunObservable
    {
        [SerializeField]
        Image portrait;
        [SerializeField]
        Sprite[] heroPort;

        [SerializeField]
        E_HeroType type;
        [SerializeField]
        string name;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(type);
                stream.SendNext(name);
            }
            else {
                type = (E_HeroType)stream.ReceiveNext();
                name = (string)stream.ReceiveNext();

            }
        }



        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            portrait.sprite = heroPort[(int) type];
            
        }
    }
}