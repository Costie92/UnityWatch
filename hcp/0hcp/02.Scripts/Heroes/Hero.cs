using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
namespace hcp
{
    public abstract class Hero : MonoBehaviourPun, IConnectionCallbacks
    {
        
        protected IBadState badState = NoneBadState.instance;

        protected Animator anim;

        [Header("Hero's Property")]
        [Space(10)]
        [SerializeField]
        protected float maxHP;
        [SerializeField]
        protected float currHP;
        [SerializeField]
        protected bool IsDie = false;
        protected Dictionary<E_ControlParam, ActiveCtrl> activeCtrlDic = new Dictionary<E_ControlParam, ActiveCtrl>();

        [Tooltip("needed amount for Ult Activate")]
        [SerializeField]
        protected float neededUltAmount;
        [Tooltip("now Amount for Ult Activate")]
        [SerializeField]
        protected float nowUltAmount;

        [Tooltip("Screen Center Point Vector")]
        [SerializeField]
        protected Vector3 screenCenterPoint;

        [Tooltip("Max Shot Length (For infinite Range)")]
        [SerializeField]
        protected float maxShotLength = 5000;

        protected float maxShotLengthDiv;

        [SerializeField]
        public Sprite[] crossHairs;

        [Tooltip("local ! height headShot")]
        [SerializeField]
        float headShotOffset;
        public float HeadShotOffset
        {
            get { return headShotOffset; }
        }
        [SerializeField]
        Transform camPos;
        [Tooltip("gameobject has fpscamperhero component")]
        [SerializeField]
        GameObject FPSCamPerHeroGO;

        [Tooltip("attached FPS Cam, only take a handle for animation thing for fps cam")]
        [SerializeField]
        protected FPSCameraPerHero FPSCamPerHero;


        [SerializeField]
        public HeroHpBar hpBar;

        [Tooltip("local base . to apply center Position Offset")]
        [SerializeField]
        protected float centerOffset;

        /*
         center position for apply this hero's center
             */
        public Vector3 CenterPos
        {
            get
            {
                Vector3 v = transform.position;
                v.y += centerOffset;
                return v;
            }
        }

        
        Rigidbody rb;
        public Rigidbody GetRigidBody
        {
            get
            {
                return rb;
            }
        }
        
        protected virtual void Awake()
        {
            rb = this.gameObject.GetComponent<Rigidbody>();
            maxShotLengthDiv = 1 / maxShotLength;
            screenCenterPoint = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, Camera.main.nearClipPlane);
            anim = this.gameObject.GetComponent<Animator>();
            SetActiveCtrls();

            if (photonView.IsMine)
            {
                InGameUIManager.Instance.SetTargetHero(this);
                Camera mainCam = Camera.main;
                mainCam.transform.SetParent(transform);
                mainCam.transform.SetPositionAndRotation(camPos.position, camPos.rotation);
                GameObject fpsCam = Resources.Load("hcp/FPSCamera") as GameObject;
             
                GameObject fpsCamIns = GameObject.Instantiate<GameObject>(fpsCam,mainCam.transform);
                if (fpsCam == null)
                {
                    Debug.LogError("fpsCam 자체가 리소스에서 읽어오기 불가능.");
                }
                Debug.Log(fpsCamIns.name);

                FPSCamPerHeroGO = GameObject.Instantiate<GameObject>(FPSCamPerHeroGO);
                if (FPSCamPerHeroGO == null)
                {
                    Debug.LogError("fpsCam 히어로가 생성 불가능.");
                }
                Debug.Log("퍼히어로도 생성완료 "+FPSCamPerHeroGO.name);

                FPSCamPerHeroGO.transform.SetParent(fpsCamIns.transform);
                FPSCamPerHeroGO.transform.SetPositionAndRotation(FPSCamPerHeroGO.transform.parent.position, FPSCamPerHeroGO.transform.parent.rotation);

                FPSCamPerHero = FPSCamPerHeroGO.GetComponent<FPSCameraPerHero>();
                if (FPSCamPerHero == null)
                {
                    Debug.LogError("영웅별 fps cam 이 어태치 되지 않았음.");
                }
            }
            else
            {
                //내 것이 아님.
                rb.isKinematic = true;
            }

        }

        protected virtual void SetActiveCtrls()//어웨이크 시 불러온다든지... 스킬들 세팅해주는 함수임.
        {
            activeCtrlDic.Clear();
        }
        public virtual void MoveHero(Vector3 moveV)
        {
        }
        public virtual void RotateHero(Vector3 rotateV)
        {
        }
        public virtual void ControlHero(E_ControlParam param)
        {
        }
        [PunRPC]
        public virtual void GetDamaged(float damage)
        {
            currHP -= damage;
            Debug.Log("겟 데미지드"+damage);
        }
        [PunRPC]
        public virtual void GetHealed(float heal)
        {
            currHP += heal;
            Debug.Log("겟 힐"+heal);
        }
        public virtual void PlusUltAmount(float value)
        {
            if (nowUltAmount < neededUltAmount)
            {
                nowUltAmount += value;
            }
            if (nowUltAmount > neededUltAmount)
            {
                nowUltAmount = neededUltAmount;
            }
        }
        public float UltAmountPercent
        {
            get
            {
                return nowUltAmount / neededUltAmount;
            }
        }

        public float GetReUseRemainTime(E_ControlParam param)
        {
            return activeCtrlDic[param].ReUseRemainingTime;
        }




        //테스트용도. 
        public void OnConnected()
        {
            Debug.Log(photonView + "의 결과는? 온커넥트에서 = " + photonView.IsMine + "시간은 = " + Time.time);
            if (!photonView.IsMine)
                rb.isKinematic = true;
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            //throw new System.NotImplementedException();
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            // throw new System.NotImplementedException();
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            // throw new System.NotImplementedException();
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            //  throw new System.NotImplementedException();
        }

        public void OnConnectedToMaster()
        {
            Debug.Log(photonView + "의 결과는? OnConnectedToMaster = " + photonView.IsMine + "시간은 = " + Time.time);
            if (!photonView.IsMine)
                rb.isKinematic = true;
        }
        public bool IsCannotMoveState()
        {
            if (badState.GetType().IsAssignableFrom(typeof(ICanNotMove)))
                return true;
            return false;
        }
        public bool IsCannotActiveState()
        {
            if (badState.GetType().IsAssignableFrom(typeof(ICanNotActive)))
                return true;
            return false;
        }
        protected enum E_MoveDir
        {
            NONE,
            Forward,
            Backward,
            Left,
            Right,
            MAX
        }
        protected E_MoveDir GetMostMoveDir(Vector3 moveDir)
        {
            E_MoveDir dir = E_MoveDir.NONE;
            if (moveDir.sqrMagnitude < Mathf.Epsilon)
                return dir;

            float x = moveDir.x;
            float z = moveDir.z;
            if (Mathf.Abs(x) > Mathf.Abs(z))
            {
                if (x > Mathf.Epsilon)
                    dir = E_MoveDir.Right;
                else dir = E_MoveDir.Left;
            }
            else
            {
                if (z > Mathf.Epsilon)
                    dir = E_MoveDir.Forward;
                else dir = E_MoveDir.Backward;
            }
            /*
            float maxDir=0f;
            float dot = Vector3.Dot(moveDir, Vector3.forward);
            if (dot > maxDir)
            {
                dir = E_MoveDir.Forward;
                maxDir = dot;
            }
            dot = Vector3.Dot(moveDir, Vector3.back);
            if (dot > maxDir)
            {
                dir = E_MoveDir.Backward;
                maxDir = dot;
            }
            dot = Vector3.Dot(moveDir, Vector3.left);
            if (dot > maxDir)
            {
                dir = E_MoveDir.Left;
                maxDir = dot;
            }
            dot = Vector3.Dot(moveDir, Vector3.right);
            if (dot > maxDir)
            {
                dir = E_MoveDir.Right;
                maxDir = dot;
            }
            */
            return dir;
        }
        public bool IsHeadShot(Vector3 worldHitPos)
        {
            if (transform.worldToLocalMatrix.MultiplyPoint3x4(worldHitPos).y > headShotOffset)
                return true;
            return false;
        }
        /*
         넉백 등이 일어날 때 호출.
             */
        [PunRPC]
        public void Knock(Vector3 worldForceVector)
        {
            if (photonView.IsMine)

            {
                Debug.Log("넉 받음");
                rb.AddForce(worldForceVector*1000f, ForceMode.Force); //이 넉백 rpc 경우는 트랜스폼은 알아서 연결되니까
                                                                //이즈마인을 체크하지만
                                                                //데미지 같은 경우는 모든 클라이언트에서 다 닳아있어야함.
            }
        }

        //갈고리에 걸린 위치, 땡겨올 위치. 몇 초 동안 의 정보
        [PunRPC]
        public void Hooked(Vector3 hookedStartWorldPos, Vector3 hookedDestWorldPos, float duration)
        {
            if (!photonView.IsMine)
                return;
            //트랜스폼은 알아서 포톤 전송 되니까 신경 쓸 필요 없음.
            transform.position = hookedStartWorldPos;
            StartCoroutine(HookedMove(hookedStartWorldPos, hookedDestWorldPos, duration));
        }

        IEnumerator HookedMove(Vector3 hookedStartWorldPos, Vector3 hookedDestWorldPos, float duration)
        {
            Quaternion startRot = transform.rotation;
            Quaternion destRot = Quaternion.LookRotation(hookedStartWorldPos - hookedDestWorldPos);

            float startTime = 0f;
            float durationDiv = 1 / duration;

            while (startTime < duration)
            {
                startTime += Time.deltaTime;
                float progress = startTime * durationDiv;
                transform.position = Vector3.Lerp(hookedStartWorldPos, hookedDestWorldPos, progress);
                transform.rotation = Quaternion.Lerp(startRot, destRot, progress);
                yield return null;
            }
            transform.SetPositionAndRotation(hookedDestWorldPos, destRot);
        }

        
    }
}