using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

namespace hcp
{
    public class HeroHook : Hero
    {
        enum E_HeroHookState
        {
            Idle,
            Hooking,
            Ultimate,
            MAX
        }

        [Space(20)]
        [Header("Hero - Hook's Property")]
        [Space(10)]
        [SerializeField]
        E_HeroHookState state = E_HeroHookState.Idle;
        [SerializeField]
        float normalAttackLength = 5f;
        [SerializeField]
        float normalAttackLengthDiv;
        [SerializeField]
        float correctionRange = 3f;
        [SerializeField]
        float correctionRangeSqr;
        [SerializeField]
        float normalAttackDamage = 30f;
        [SerializeField]
        float normalAttackFireRate = 1f;

        [Space(10)]
        [Header("   Hero - Hook - First Skill Hook")]
        [SerializeField]
        HHHook hookProjectile;
        [SerializeField]
        Transform hookOriginPos;
        [SerializeField]
        float hookFireRate;

        [Space(10)]
        [Header("   Hero - Hook - Ultimate")]
        [SerializeField]
        float ultFireRate;



        protected override void Awake()
        {
            base.Awake();
            normalAttackLengthDiv = 1 / normalAttackLength;
            correctionRangeSqr = correctionRange * correctionRange;

            moveSpeed = 3f;
            rotateSpeed = 2f;


            centerOffset = 1.0f;
            
            maxHP = 100f;
            currHP = maxHP;
            neededUltAmount = 10000f;
            nowUltAmount = 0f;
        }
        private void Start()
        {
            hookProjectile.DeActivate();
        }
        protected override void SetActiveCtrls()
        {
            base.SetActiveCtrls();
            activeCtrlDic.Add(E_ControlParam.NormalAttack, new DelegateCtrl(E_ControlParam.NormalAttack, normalAttackFireRate, NormalAttack,
              NormalAttackMeetCondition));
            activeCtrlDic.Add(E_ControlParam.FirstSkill, new DelegateCtrl(E_ControlParam.FirstSkill, hookFireRate, DoHook, HookMeetCondition));
            activeCtrlDic.Add(E_ControlParam.Reload, new DelegateCtrl(E_ControlParam.Reload, 1f, Reload, ()=> { return true; }));
            activeCtrlDic.Add(E_ControlParam.Ultimate, new DelegateCtrl(E_ControlParam.Ultimate, ultFireRate, HHUlt, UltMeetCondition));

        }

        #region Basic Control
        public override void MoveHero(Vector3 moveV)
        {
            if (!photonView.IsMine || IsCannotMoveState() || IsDie)
            {
                Debug.Log("무브히어로가 묵살되었음." + moveV + "포톤이 내것인지? = " + photonView.IsMine);
                return;
            }
            transform.Translate(moveV * moveSpeed, Space.Self);
        }

        public override void RotateHero(Vector3 rotateV)
        {
            if (!photonView.IsMine || IsCannotMoveState() || IsDie)
            {
                return;
            }
            // Debug.Log("RotateHero" + rotateV);
            transform.Rotate(rotateV * rotateSpeed, Space.Self);
        }


        public override void ControlHero(E_ControlParam param)
        {
            if (!photonView.IsMine || IsCannotActiveState() || IsDie )
            {
                return;
            }
            Debug.Log("ControlHero" + param);
            /*
            if (param == E_ControlParam.Ultimate)
            {
                if (UltAmountPercent < 1 && !isUltOn)
                    return;

                if (isUltOn)
                {
                    if (!activeCtrlDic[E_ControlParam.Ultimate].IsCoolTimeOver() || ultShootCount >= ultMissilesMaxCount)
                        return;

                    //궁 유지중인 부분.
                    activeCtrlDic[param].Activate();
                    return;
                }
                else
                {
                    //궁 처음 쏘는 초기화 부분.
                    nowUltAmount = 0f;
                    isUltOn = true;
                    ultShootCount = 0;
                    ultActivateTime = 0f;
                    activeCtrlDic[param].Activate();
                    return;
                }
            }
            */

            if (!activeCtrlDic[param].IsCoolTimeOver())
                return;
            Debug.Log(param + "입력 - 쿨타임 검사통과");

            activeCtrlDic[param].Activate();
        }





        #endregion

        #region NormalAttack
        bool NormalAttackMeetCondition()
        {
            return true;
        }
        void NormalAttack()
        {
            List<Hero> enemyHeroes = TeamInfo.GetInstance().EnemyHeroes;
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            Vector3 normalAttackVector = ray.direction * normalAttackLength;

            for (int i = 0; i < enemyHeroes.Count; i++)
            {
                Hero enemy = enemyHeroes[i];
                Vector3 enemyPosition = enemy.CenterPos - ray.origin;
                float dot = Vector3.Dot(enemyPosition, normalAttackVector);
                if (dot < Mathf.Epsilon)
                {
                    Debug.Log(enemy.photonView.ViewID+ "HH NA enemy Behind, no attack");
                    continue;
                }
                float projectedDis = dot * normalAttackLengthDiv;
                if (projectedDis > normalAttackLength)
                {
                    Debug.Log(enemy.photonView.ViewID + "HH NA enemy too far, no attack");
                    continue;
                }
                float projectedDisSqr = projectedDis * projectedDis;
                float orthogonalDisSqr = enemyPosition.sqrMagnitude - projectedDisSqr;
                if (orthogonalDisSqr > correctionRangeSqr)
                {
                    Debug.Log(enemy.photonView.ViewID + "HH NA enemy orthogonalDis too far, no attack");
                    continue;
                }
                enemy.photonView.RPC("GetDamaged", Photon.Pun.RpcTarget.All, normalAttackDamage);
            }
        }
        #endregion

        #region Reload

        void Reload()
        {

        }

        #endregion

        #region FirstSkill - Hook
        bool HookMeetCondition()
        {
            return true;
        }
        void DoHook()
        {
            state = E_HeroHookState.Hooking;
            //자가 정지 시키기.
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            hookOriginPos.transform.LookAt(ray.direction * maxShotLength);
            photonView.RPC("ActivateHook", RpcTarget.All , hookOriginPos.transform.rotation);
        }
        [PunRPC]
        public void ActivateHook(Quaternion originPosRot)
        {
            hookOriginPos.transform.rotation = originPosRot;
            //후킹은 자가 정지 후에 발동하는 로직이니까 훅의 위치 동기화 까지는 필요 없을듯
            hookProjectile.Activate();
        }
        [PunRPC]
        public void HookFailed()
        {
            //후킹 실패로 훅이 돌아오는 타이밍일뿐
            hookProjectile.HookFail();
        }
        [PunRPC]
        public void HookSucessed()
        {
            //후킹 성공으로 훅이 돌아오는 타이밍일뿐
            hookProjectile.HookSuccess();
        }
        [PunRPC]    //후크가 제자리로 돌아오면 알피씨를 쏨.
        public void HookIsDone()
        {
            hookProjectile.DeActivate();
            state = E_HeroHookState.Idle;
        }

        #endregion

        #region Ultimate
        bool UltMeetCondition()
        {
            return true;
        }

        void HHUlt()
        {

        }



        #endregion


        public override bool IsCannotMoveState()
        {
            if (state == E_HeroHookState.Hooking)
            {
                return true;
            }
           return base.IsCannotMoveState();
        }
        public override bool IsCannotActiveState()
        {
            if (state == E_HeroHookState.Hooking)
            {
                return true;
            }
            return base.IsCannotActiveState();
        }
    }
}