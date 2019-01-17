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
        float maxLength=20f;
        [SerializeField]
        float hookVelocity = 1f;

        [Tooltip("same with HeroHook's hookOriginPos property")]
        [SerializeField]
        Transform originPosFromheroHook;
        [SerializeField]
        float withDrawTime = 5f;
        [SerializeField]
        float withDrawVelocity = 0.5f;
        [SerializeField]
        Transform rope;
        [SerializeField]
        Renderer ropeRenderer;

        [SerializeField]
        Material ropeMat;
        [Tooltip("로프 머테리얼의 텍스쳐 타일링과 실제 로프 길이 사이 스케일 팩터. 1f를 도출해냈음. 길이*1f 를 머테리얼 타일링 y에 주면 됨.")]
        [SerializeField]
        float ropeToMaterialTileScaleFactor=1f;

        [Tooltip("갈고리가 처음 뻗어나오는 위치에서 갈고리 까지의 z 차에 따른 로프의 스케일 조정값.. 5f 도출값.")]
        [SerializeField]
        float disToRopeScaleFactor = 5f;

        [SerializeField]
        float hookedDestDis = 1f;

        [SerializeField]
        float withDrawHookedDuration = 3f;


        protected override void Awake()
        {
            base.Awake();
            ropeMat = new Material(ropeRenderer.material);
            ropeRenderer.material = ropeMat;
        }


        public void Activate()
        {
            gameObject.SetActive(true);
            velocity = hookVelocity;
            state = HookState.Activate;
        }
        public void DeActivate()
        {
            state = HookState.DeActivate;
            transform.SetPositionAndRotation(originPosFromheroHook.position, originPosFromheroHook.rotation);
            gameObject.SetActive(false);
        }
        public void HookFail()
        {
            state = HookState.HookFail;
        }
        public void HookSuccess()
        {
            state = HookState.HookSuccess;
        }

        private void Update()
        {
            switch (state)
            {
                case HookState.Activate:
                    transform.Translate(Vector3.back * velocity * Time.deltaTime , Space.Self);
                    MakeRope();
                    if (attachingHero.photonView.IsMine)
                    {
                        if (transform.localPosition.z > maxLength)
                        {
                            attachingHero.photonView.RPC("HookFailed", Photon.Pun.RpcTarget.All);
                        }
                    }
                  
                    break;
                case HookState.HookFail:
                case HookState.HookSuccess:
                    transform.Translate(Vector3.back * withDrawVelocity * Time.deltaTime , Space.Self);
                    MakeRope();
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
                Hero enemy = other.gameObject.GetComponent<Hero>();
                if (enemy == null)
                {
                    Debug.Log("갈고리로 끌었으나 적이 히어로가 아님");
                    attachingHero.photonView.RPC("HookFailed", Photon.Pun.RpcTarget.All);
                    return;
                }

                Vector3 enemyPos = enemy.transform.position;
                Vector3 destPos =  (enemyPos - attachingHero. transform.position).normalized * hookedDestDis;
                enemy.photonView.RPC("Hooked", Photon.Pun.RpcTarget.All, enemyPos, destPos, withDrawHookedDuration);
                attachingHero.photonView.RPC("HookingSuccessed", Photon.Pun.RpcTarget.All);
            }
        }
        void MakeRope()
        {
            float dis = transform.localPosition.z;  //어차피 부모의 위치에서 출발함.
            Vector3 ropeLocalScale = rope.localScale;
            if (dis < Mathf.Epsilon)
            {
                ropeLocalScale.z = 0f;
                rope.localScale= ropeLocalScale;
                return;
            }
            ropeLocalScale.z = dis * disToRopeScaleFactor;
            rope.localScale = ropeLocalScale;
            ropeMat.mainTextureScale = new Vector2(1, ropeLocalScale.z * ropeToMaterialTileScaleFactor);
        }
    }
}