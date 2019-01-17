using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

namespace hcp
{
    public class HeroHook : Hero
    {
        [Space(20)]
        [Header("Hero - Hook's Property")]
        [Space(10)]
        [SerializeField]
        float normalAttackLength;
        [SerializeField]
        float normalAttackLengthDiv;
        [SerializeField]
        float correctionRange;
        [SerializeField]
        float correctionRangeSqr;
        [SerializeField]
        float normalAttackDamage;
        [SerializeField]
        float normalAttackFireRate;

        [Space(10)]
        [Header("   Hero - Hook - First Skill Hook")]
        [SerializeField]
        HHHook hookProjectile;
        [SerializeField]
        Transform hookOriginPos;

        protected override void Awake()
        {
            base.Awake();
            normalAttackLengthDiv = 1 / normalAttackLength;
            correctionRangeSqr = correctionRange * correctionRange;
        }
        protected override void SetActiveCtrls()
        {
            base.SetActiveCtrls();

        }

        #region Basic Control

        #endregion
        
        #region NormalAttack
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

        #region FirstSkill - Hook

        void DoHook()
        {
            //자가 정지 시키기.
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            hookOriginPos.transform.LookAt(ray.direction * maxShotLength);
            photonView.RPC("ActivateHook", RpcTarget.All);
        }
        [PunRPC]
        public void ActivateHook()
        {
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
        public void HookIsDone()
        {
            hookProjectile.DeActivate();
        }

        #endregion

        #region Ultimate

        #endregion

    }
}