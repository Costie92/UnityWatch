﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public class HSUltMissile : Projectile
    {
        [SerializeField]
        ParticleSystem boomEffect;
        float boomEffectLength;

        [SerializeField]
        float knockBackPower = 5f;

        [SerializeField]
        float explosionRange = 5f;

        float explosionRangeDiv;
        float explosionRangeSqr;
        float knockBackPowerInterValue;

        [SerializeField]
        bool isActivated = false;

        [SerializeField]
        float maxVelocity = 3f;
        [SerializeField]
        float startVelocity = 1f;

        [SerializeField]
        Collider coll;

        MeshRenderer[] renderers;

        public int attachedNumber;

        protected override void Awake()
        {
            base.Awake();
            boomEffectLength = boomEffect.main.duration;
            renderers = GetComponentsInChildren<MeshRenderer>();
            coll = GetComponent<Collider>();

            /*
            if (!attachingHero.photonView.IsMine)
            {
                Destroy(GetComponent<Rigidbody>());
            }*/

            knockBackPowerInterValue = knockBackPower / explosionRange;
            explosionRangeDiv = 1 / explosionRange;
            explosionRangeSqr = explosionRange * explosionRange;
            amount = 50f;   //솔져 궁의 데미지 양
            velocity = 1f;
        }


        public void Activate(Vector3 shootStartPos, Quaternion shotDir)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }
            transform.SetPositionAndRotation(shootStartPos, shotDir);
            velocity = startVelocity;
            isActivated = true;

            gameObject.SetActive(true);
        }
        public void DeActivate()
        {
            transform.SetPositionAndRotation(transform.parent.position, transform.rotation);
            isActivated = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (isActivated)
            {
                if (velocity < maxVelocity)
                {
                    velocity += Time.deltaTime;    //속도 올려주기.
                }

                transform.Translate(Vector3.forward * velocity * Time.deltaTime, Space.Self);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!attachingHero.photonView.IsMine)
                return;

            if (!(other.gameObject.layer == Constants.mapLayerMask || TeamInfo.GetInstance().IsThisLayerEnemy(other.gameObject.layer)))
                return;

            //적이나 맵에 트리거 시 폭발.

            Vector3 collCenter = coll.bounds.center;    //콜라이더의 센터를 폭발 지점으로.

            attachingHero.photonView.RPC("BoomUltMissile", Photon.Pun.RpcTarget.All, attachedNumber, transform.position);   //효과만.

            List<Hero> enemyHeroes = TeamInfo.GetInstance().EnemyHeroes;   //나중에 적 히어로 받아오기로.

            for (int i = 0; i < enemyHeroes.Count; i++)
            {
                Vector3 enemyPosition = enemyHeroes[i].transform.position - collCenter;
                if (enemyPosition.sqrMagnitude <= explosionRangeSqr)
                {
                    RaycastHit hit;
                    float dis = enemyPosition.magnitude;

                    if (Physics.Raycast(collCenter, enemyPosition, out hit, dis, 1 << Constants.mapLayerMask))
                    {
                        continue;   //중간에 벽 있으므로 패스.
                    }
                    Vector3 dir = enemyPosition.normalized;

                    dir *= ((explosionRange - dis) * knockBackPowerInterValue); //폭발 지점과 거리 계산해서 알맞게 넉백 파워를 조절해줌.

                    enemyHeroes[i].photonView.RPC("Knock", Photon.Pun.RpcTarget.All, dir);
                    enemyHeroes[i].photonView.RPC("GetDamaged", Photon.Pun.RpcTarget.All, amount * (explosionRange - dis) * explosionRangeDiv);
                }
            }
        }
        public void Boom(Vector3 boomedPos)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
            isActivated = false;
            transform.position = boomedPos;
            boomEffect.Play();

            StartCoroutine(boomEffectWait());

            //그냥 폭발하는 효과만 붙여주기.
            //폭발위치도 함께 RPC 할것.
            //폭발 효과 기다렸다가 셋 액티브 펄스 해주기.
        }
        IEnumerator boomEffectWait()
        {
            yield return new WaitForSeconds(boomEffectLength);
            DeActivate();
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, explosionRange);
        }

    }
}