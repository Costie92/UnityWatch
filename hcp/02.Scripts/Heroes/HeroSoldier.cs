using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;


public class HeroSoldier : Hero {
    [SerializeField]
    HSHealDrone healDrone;

    [Tooltip("Hero- Soldier Ultimate, Object Pooling.")]
    [SerializeField]
    HSUltMissile[] ultMissiles;

    [Tooltip("Hero- Soldier Ultimate, max Missile Counts.")]
    [SerializeField]
    int ultMissilesMaxCount = 5;

    [SerializeField]
    int ultShootCount = 0;

    [Tooltip("Hero- Soldier Ultimate, max Missile Shot Time.")]
    [SerializeField]
    float ultMaxTime = 10f;

    [Tooltip("Hero- Soldier Ultimate, fire rate.")]
    [SerializeField]
    float ultFireRate = 0.7f;
    

    [SerializeField]
    bool isUltOn=false;

    [SerializeField]
    float ultActivateTime = 0f;



    [SerializeField]
    Vector3 healDroneOffset = new Vector3(-0.2f,1f,0);
    [SerializeField]
    float normalFireDamage = 5f;
    [Tooltip("fireRate for normal attack")]
    [SerializeField]
    float fireRate = 0.2f;
    [SerializeField]
    int currBullet;
    [SerializeField]
    int maxBullet;

    bool reloading;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < ultMissiles.Length; i++)
        {
            ultMissiles[i].DeActivate();
        }
        healDrone.DeActivate();
        maxHP = 100f;
        currHP = maxHP;
        neededUltAmount = 10000f;
        nowUltAmount = 0f;
    }

    protected override void SetActiveCtrls()//어웨이크 시 불러온다든지... 스킬들 세팅해주는 함수임.
    {
        base.SetActiveCtrls();
        activeCtrlDic.Add(E_ControlParam.NormalAttack,  new DelegateCtrl(E_ControlParam.NormalAttack,   fireRate,       NormalAttack));
        activeCtrlDic.Add(E_ControlParam.FirstSkill,    new DelegateCtrl(E_ControlParam.FirstSkill,     10f,        FirstSkill_HealDrone));
        activeCtrlDic.Add(E_ControlParam.Reload,        new DelegateCtrl(E_ControlParam.Reload,         1f,         Reloading));
        activeCtrlDic.Add(E_ControlParam.Ultimate,      new DelegateCtrl(E_ControlParam.Ultimate,       ultFireRate,     Ult_ShotMissile));
    }

    public override void MoveHero(Vector3 moveV)
    {
        if (!photonView.IsMine || badState == E_BadState.Stun || IsDie)//원래 스턴을 이런식으로 처리하면 매우 곤란한데.
        {
            return;
        }
        transform.Translate(moveV, Space.Self);
    }

    public override void RotateHero(Vector3 rotateV)
    {
        if (!photonView.IsMine || badState == E_BadState.Stun || IsDie)
        {
            return;
        }

        transform.Rotate(rotateV, Space.Self);
    }

    public override void ControlHero(E_ControlParam param)
    {
        if (!photonView.IsMine || badState == E_BadState.Stun || IsDie || reloading)
        {
            return;
        }

        if (param == E_ControlParam.Ultimate)
        {
            if (UltAmountPercent < 1 && ! isUltOn)
                return;

            if (isUltOn)
            {
                if (! activeCtrlDic[E_ControlParam.Ultimate].IsCoolTimeOver() || ultShootCount >= ultMissilesMaxCount)
                    return;

                //궁 유지중인 부분.
                activeCtrlDic[param].Activate();
                return;
            }
            else {
                //궁 처음 쏘는 초기화 부분.
                nowUltAmount = 0f;
                isUltOn = true;
                ultShootCount = 0;
                ultActivateTime = 0f;
                activeCtrlDic[param].Activate();
                return;
            }
        }
        

        if (! activeCtrlDic[param].IsCoolTimeOver())
            return;

        activeCtrlDic[param].Activate();
    }

    void NormalAttack()
    {
        if (isUltOn) return;
        if (currBullet <= 0)
        {
            Reloading();
            return;
        }

        currBullet--;


        float correctionRange = 1f;
        
        //보정해서 레이캐스트로 바로 쏘는 작업. 히트스캔.
        Hero[] enemyHeroes = new Hero[2];   //나중에 받아오는 걸로 교체..

        Vector3 shotVector = transform.position * 5000  - transform.position;//현자 자기 보는 곳 z 앞으로 5000 짜리 벡터.
        Vector3 enemyPosition;
        Hero selectedTarget = null;
        float minEnemyDistance = 0f;

        for (int i = 0; i < enemyHeroes.Length; i++)    //보정 더해 맞을 놈 구함.
        {
            enemyPosition = enemyHeroes[i].transform.position - transform.position; //현재 내 위치에서 적의 위치
            float dot = Vector3.Dot(shotVector, enemyPosition);
            if (dot < Mathf.Epsilon)   //내적 값이 0보다 작으므로 뒤에 있드니 함으로 논외.
                continue;

            float enemyDistance = enemyPosition.sqrMagnitude;
            float farFromShootSqr = enemyDistance - (dot * dot);  //쏘는 벡터에 적 위치에서 수선으로 내린 길이의 제곱

            if (farFromShootSqr > correctionRange * correctionRange)    //보정 범위의 밖. 논외.
                continue;

            //보정 범위 안에 들어옴.

            if (selectedTarget == null) //선택 된게 없으면 바로 선택.
            {
                selectedTarget = enemyHeroes[i];
                minEnemyDistance = enemyDistance;
                continue;
            }

            if ( selectedTarget!= null &&  enemyDistance < minEnemyDistance)   //더 가까이 있는 경우라면
            {
                selectedTarget = enemyHeroes[i];
                minEnemyDistance = enemyDistance;
            }
        }

        if (selectedTarget == null) //맞은 놈이 없음.
        {
            //총 쏘는 이펙트만 RPC해주기.
            return;
        }

        //총쏘는 이펙트와 적의 체력 깎기 해주기. normalFireDamage
    }

    void FirstSkill_HealDrone() //주위를 힐 하는 힐 드론 소환. 얘는 애니메이션 동기화 해줄 필요 없음
    {
        healDrone.Activate();   //rpc로 호출해야함.
    }

    void Reloading()
    {
        if (isUltOn) return;
        if (currBullet == maxBullet) return;
        reloading = true; //리로드 애니메이션 후 풀어주기.
        //리로드 애니메이션 해주기.
        currBullet = maxBullet;
    }

    void Ult_ShotMissile()
    {
        //궁로직.
        ultShootCount++;

        //궁 발사 로직.
        for (int i = 0; i < ultMissiles.Length; i++)
        {
            if (!ultMissiles[i].gameObject.activeSelf)
            {
                ultMissiles[i].Activate();
            }
        }
    }
    
    private void Update()
    {
        if (!photonView.IsMine) return;

        if (!isUltOn)
        {
            PlusUltAmount(Time.deltaTime);
        }
        else
        {
            ultActivateTime += Time.deltaTime;

            if (ultActivateTime > ultMaxTime || ultShootCount >= ultMissilesMaxCount)
            {
                isUltOn = false;    //궁 종료 시점.
            }
        }
    }
}
