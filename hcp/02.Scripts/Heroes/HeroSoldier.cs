using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;


public class HeroSoldier : Hero {
    [Tooltip("attached FPS Cam, only take a handle for animation thing for fps cam")]
    [SerializeField]
    HeroSoldierFPSCam fpsCam;

    [SerializeField]
    Transform firePos;
    [SerializeField]
    ParticleSystem normalMuzzleFlash;

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

    [SerializeField]
    float correctionRange = 1f;

    [SerializeField]
    float correctionRangeSqr;
    [SerializeField]
    bool reloading = false;

    [SerializeField]
    AnimationClip reloadClip;

    

    protected override void Awake()
    {
        correctionRangeSqr = correctionRange * correctionRange;
        base.Awake();
        for (int i = 0; i < ultMissiles.Length; i++)
        {
            ultMissiles[i].DeActivate();
            ultMissiles[i].attachedNumber = i;
        }
        maxHP = 100f;
        currHP = maxHP;
        maxBullet = 25;
        currBullet = maxBullet;
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
            Debug.Log("무브히어로가 묵살되었음." + moveV + "포톤이 내것인지? = "+photonView.IsMine);

            return;
        }
        Debug.Log("무브히어로" + moveV);
        transform.Translate(moveV, Space.Self);
    }

    public override void RotateHero(Vector3 rotateV)
    {
        if (!photonView.IsMine || badState == E_BadState.Stun || IsDie)
        {
            return;
        }
        Debug.Log("RotateHero" + rotateV);
        transform.Rotate(rotateV, Space.Self);
    }

    public override void ControlHero(E_ControlParam param)
    {
        if (!photonView.IsMine || badState == E_BadState.Stun || IsDie || reloading)
        {
            return;
        }
        Debug.Log("ControlHero" + param);

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
        Debug.Log(param+"입력 - 쿨타임 검사통과");

        activeCtrlDic[param].Activate();
    }


    #region Normal Attack

    void NormalAttack()
    {
        if (isUltOn) return;
        if (currBullet <= 0)
        {
            Reloading();
            return;
        }

        currBullet--;
        photonView.RPC("normalMuzzleFlashPlay", RpcTarget.Others);
        fpsCam.HSNormalMuzzleFlash();// 나자신의 시각효과만 담당.
                                     // normalMuzzleFlash.Play();   //fps 카메라라서 다른 곳의 파티클을 뿜어줘야함.

 
        Camera camera = Camera.main;

        Ray screenCenterRay = camera.ScreenPointToRay(screenCenterPoint);
        RaycastHit hitInfo;
        if (Physics.Raycast(screenCenterRay, out hitInfo, maxShotLength))
        {
            //레이캐스트 쏴서 뭐 맞았음. 벽이나, 적팀이나 이런 것을 검출.
            Debug.Log("쏴서 맞았음." + hitInfo);
            return;
        }

        //이제는 혹시 보정 값에 걸리나 체크.
        Hero[] enemyHeroes = new Hero[0];   //나중에 받아오는 걸로 교체..
        Vector3 shotVector = screenCenterRay.direction * maxShotLength;

        for (int i = 0; i < enemyHeroes.Length; i++)
        {
            Vector3 enemyPosition = enemyHeroes[i].transform.position - screenCenterPoint;
            float enemyScreenCenterDot = Vector3.Dot(enemyPosition, shotVector);

            if (enemyScreenCenterDot < Mathf.Epsilon)   //벡터가 마이너스 , 뒤에 있음
                continue;
            float projectedEnemyDis = enemyScreenCenterDot * maxShotLengthDiv;
            float farFromShotVectorSqr = enemyPosition.sqrMagnitude - (projectedEnemyDis * projectedEnemyDis) ;

            if (farFromShotVectorSqr > correctionRangeSqr)  //보정 범위 밖
                continue;

            //보정으로 히트됨.

            //벽 등에 가려졌는지 레이를 한번 더쏴야하나...????

            if (Physics.Raycast(screenCenterRay.origin, enemyPosition, out hitInfo, maxShotLength))
            {
                if (hitInfo.collider.gameObject.layer.Equals(mapLayer))
                {
                    //벽에 맞았을 경우.
                }
                //적에 맞았을 경우도 체크하기.
            }
        }
    
        
    }

    [PunRPC]
    void normalMuzzleFlashPlay()
    {
        normalMuzzleFlash.Play();
    }

#endregion


    #region First Skill HealDrone

    void FirstSkill_HealDrone() //주위를 힐 하는 힐 드론 소환. 얘는 애니메이션 동기화 해줄 필요 없음
    {
        if (!photonView.IsMine) return;

        photonView.RPC("DroneAppear", RpcTarget.All);
        
        healDrone.Activate();   
    }

    [PunRPC]
    void DroneAppear()
    {
        healDrone.Appear();
    }
    [PunRPC]
    public void DroneDisAppear()
    {
        healDrone.DisAppear();
    }
    [PunRPC]
    void DroneHeal(Hero hero, float healAmount)
    {
        hero.GetHealed(healAmount);
    }



    #endregion

    #region Reloading

    void Reloading()
    {
        if (isUltOn) return;
        if (currBullet == maxBullet) return;
        reloading = true; //리로드 애니메이션 후 풀어주기.
        //리로드 애니메이션 해주기.

        StartCoroutine(ReloadingCheck());
        currBullet = maxBullet;
    }

    IEnumerator ReloadingCheck()
    {
        anim.SetTrigger("reload");
        /*
        float time = 0f;
        while (time < reloadClip.length)
        {
            Debug.Log("리로딩 기다리는중" + time);
            time += 0.3f;
            yield return new WaitForSeconds(0.3f);
        }*/
        yield return new WaitForSeconds(reloadClip.length);
        /*

      //  yield return new WaitForSeconds(0.1f);
        Debug.Log(anim.GetCurrentAnimatorStateInfo(0).length +"   "+ anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

       Debug.Log("리로드 이름 맞음?"+anim.GetCurrentAnimatorStateInfo(0).IsName("Reload") );
      //  Debug.Log("리로드 이름 맞음?" + anim.GetCurrentAnimatorStateInfo(0).IsName("Reload"));
        while (
          //  anim.GetCurrentAnimatorStateInfo(0).IsName("Reload") && 
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            Debug.Log("리로딩 애니메이션 체크중");
            
            yield return new WaitForSeconds(0.3f);
        }
        */
        reloading = false;
    }
    #endregion

    #region Ultimate

    void Ult_ShotMissile()
    {
        photonView.RPC("ShootUltimate", RpcTarget.All, ultShootCount);
        ultShootCount++;
        //궁로직.
    }

    [PunRPC]
    void ShootUltimate(int num,Vector3 shootStartPos)
    {
        ultMissiles[num].Activate(shootStartPos);
    }
    [PunRPC]
    void BoomUltMissile(int num,Vector3 boomedPos)
    {
        ultMissiles[num].Boom(boomedPos);
    }

    #endregion

    private void Update()
    {
        /*
        if (Input.GetKeyDown("w"))
        {
            normalMuzzleFlash.Play();   
        }
        */

        if (!photonView.IsMine) return;

        if (!isUltOn)
        {
            PlusUltAmount(100*Time.deltaTime);//1초에 100씩 차게.
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
