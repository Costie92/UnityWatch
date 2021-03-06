﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HHUltWolves : Projectile
    {
        enum E_HHUltState
        {
            Idle,
            Activate,
            DeActivate,
            MAX
        }
        [SerializeField]
        Animator[] wolvesAnimator;
        [SerializeField]
        GameObject[] wolvesGO;
        [SerializeField]
        E_HHUltState state;
        [SerializeField]
        float distance;
        float distanceSqr;
        [SerializeField]
        float damageTick;
        int animParamRunHash = Animator.StringToHash("run");

        protected override void Awake()
        {
            base.Awake();
            distanceSqr = distance * distance;
        }

        private void Start()
        {
            DeActivate();
        }
        public void Activate(Vector3 activatePos, Quaternion activeRot)
        {
            StopAllCoroutines();
            transform.SetPositionAndRotation(activatePos, activeRot);
            state = E_HHUltState.Activate;
            gameObject.SetActive(true);
            wolvesRun(true);
            StartCoroutine(ActivateMove());
        }
        public void DeActivate()
        {
            wolvesRun(false);
            state = E_HHUltState.DeActivate;
            transform.SetPositionAndRotation(transform.parent.position, transform.parent.rotation);
            gameObject.SetActive(false);
        }

        IEnumerator ActivateMove()
        {
            HitEnemy(); //처음 생겼을 떄 한번 히트 설정해줌.

            float time = 0f;
            while (state == E_HHUltState.Activate)
            {
                time += Time.deltaTime;
                transform.Translate(Vector3.forward * velocity*Time.deltaTime, Space.Self);
                if (time > damageTick)
                {
                    time = 0f;
                    HitEnemy();
                }
                yield return null;
            }
        }

        void HitEnemy()
        {
            if (!attachingHero.photonView.IsMine) return;

            List<Hero> enemyHeroes = TeamInfo.GetInstance().EnemyHeroes;
            for (int i = 0; i < enemyHeroes.Count; i++)
            {
                Vector3 enemyPosition = enemyHeroes[i].CenterPos - transform.position;
                if ( enemyPosition.sqrMagnitude< distanceSqr)
                {
                    enemyHeroes[i].photonView.RPC("GetDamaged", Photon.Pun.RpcTarget.All, amount,attachingHero.photonView.ViewID);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!attachingHero.photonView.IsMine) return;
            if (other.gameObject.CompareTag(Constants.outLineTag))
            {
                state = E_HHUltState.DeActivate;
                attachingHero.photonView.RPC("HHHUltDeActivate", Photon.Pun.RpcTarget.All);
            }
        }

        void wolvesRun(bool run)
        {
            for (int i = 0; i < wolvesAnimator.Length; i++)
            {
                wolvesAnimator[i].SetBool(animParamRunHash, run);
            }
        }
    }
}