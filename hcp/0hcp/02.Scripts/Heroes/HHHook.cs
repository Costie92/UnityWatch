using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp {
    public class HHHook : Projectile  {

        enum HookState
        {
            Activate,
            HookFail,
            HookSuccess,
            DeActivate,
            MAX
        }
        [SerializeField]
        HookState state = HookState.DeActivate;
        [SerializeField]
        float maxLength;
        [SerializeField]
        float hookVelocity;

        [Tooltip("same with HeroHook's hookOriginPos property")]
        [SerializeField]
        Transform originPosFromheroHook;
        [SerializeField]
        float withDrawTime;
        [SerializeField]
        float withDrawVelocity;
      

        protected override void Awake()
        {
            base.Awake();

        }


        public void Activate()
        {
            velocity = hookVelocity;
            state = HookState.Activate;
        }
        public void DeActivate()
        {
            state = HookState.DeActivate;
            transform.SetPositionAndRotation(originPosFromheroHook.position, originPosFromheroHook.rotation);
        }
        public void HookFail()
        {
            state = HookState.HookFail;
        }

        private void Update()
        {
            switch (state)
            {
                case HookState.Activate:
                    transform.Translate(Vector3.back * withDrawVelocity * Time.deltaTime * withDrawTime, Space.Self);


                    break;
                case HookState.HookFail:
                    transform.Translate(Vector3.back * withDrawVelocity * Time.deltaTime * withDrawTime, Space.Self);
                    if (transform.localPosition.z + Mathf.Epsilon < originPosFromheroHook.localPosition.z)
                    {
                        state = HookState.DeActivate;
                        if(attachingHero.photonView.IsMine)
                        attachingHero.photonView.RPC("HookIsDone", Photon.Pun.RpcTarget.All);
                    }
                    break;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!attachingHero.photonView.IsMine) return;
            if (state != HookState.Activate) return;

            int layer = other.gameObject.layer;
            if (layer == Constants.mapLayerMask)
            {
                attachingHero.photonView.RPC("HookFailed", Photon.Pun.RpcTarget.All);
            }
            if (TeamInfo.GetInstance().IsThisLayerEnemy(layer))
            {

            }
        }
        
    }
}