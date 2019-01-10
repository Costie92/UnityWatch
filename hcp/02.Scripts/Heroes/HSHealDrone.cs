using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class HSHealDrone : MonoBehaviour
{
    [Tooltip("hero this drone attached")]
    [SerializeField]
    HeroSoldier attachingHero;

    [SerializeField]
    float healAmount = 10f;
    [SerializeField]
    float healCoolTime = 0.3f;
    [SerializeField]
    float healRange = 40f;
    [SerializeField]
    float moveUpAmount;
    [SerializeField]
    float moveDownAmount;
    [SerializeField]
    Vector3 originPos;

    [Tooltip("heal drone activating Time")]
    [SerializeField]
    float activeMaxTime = 7f;

    [SerializeField]
    Animator anim;

    [SerializeField]
    Material droneDissolveMat;

    float SqrHealRange;
    WaitForSeconds ws;

    float activateTime;

    Transform[] initPoses;
    Vector3[] localInitPoses;
    Quaternion[] localInitRotes;

    private void Awake()
    {
        originPos = transform.position;
        anim = GetComponent<Animator>();
        SqrHealRange = healRange * healRange;
        ws = new WaitForSeconds(healCoolTime);
        gameObject.SetActive(false); //임시로.

        initPoses = gameObject.GetComponentsInChildren<Transform>();
        localInitPoses = new Vector3[initPoses.Length];
        localInitRotes = new Quaternion[initPoses.Length];
        for (int i = 0; i < initPoses.Length; i++)
        {
            localInitPoses[i] = initPoses[i].localPosition;
            localInitRotes[i] = initPoses[i].localRotation;
        }
    }
    void SetPosToInit()
    {
        for (int i = 0; i < initPoses.Length; i++)
        {
            initPoses[i].localPosition = localInitPoses[i];
            initPoses[i].localRotation = localInitRotes[i] ;
        }
    }
    
    public void Appear()
    {
        gameObject.SetActive(true); //나중에 쉐이더 디졸빙으로 해석. 
        anim.SetTrigger("show");
        StartCoroutine(MoveHealDrone());
        StartCoroutine(AppearEffect());
    }
    IEnumerator AppearEffect()
    {
        float startTime = 0f;
        while (startTime < 1f)
        {
            startTime += Time.deltaTime;
            droneDissolveMat.SetFloat("_Level", 1 - startTime);
            yield return null;
        }
        droneDissolveMat.SetFloat("_Level", 0);
    }
    
    public void DisAppear()
    {
        StartCoroutine(DisAppearEffect());
    }

    IEnumerator DisAppearEffect()
    {
        float startTime = 0f;
        while (startTime < 1f)
        {
            startTime += Time.deltaTime;
            droneDissolveMat.SetFloat("_Level", startTime);
            yield return null;
        }
        droneDissolveMat.SetFloat("_Level", 1);
        StopCoroutine(MoveHealDrone());
        SetPosToInit();
        
        gameObject.SetActive(false); 
    }


    public void Activate() 
        //포톤뷰 마인인 쪽에서만 부르게 할것.
    {
        if(attachingHero.photonView.IsMine)
        StartCoroutine(StartingHealingProtocol());
    }

    IEnumerator StartingHealingProtocol()
    {
        activateTime = Time.time;
        Debug.Log("힐드론 액티베이트 시간 =" + activateTime);
        while (true)
        {
            Hero[] sameSideHeroes = new Hero[0];   //나중에 우리편 히어로만 받아올 수 있게.

            for (int i = 0; i < sameSideHeroes.Length; i++)
            {
                if (SqrHealRange >= (sameSideHeroes[i].transform.position - transform.position).sqrMagnitude)   //힐 범위에 아군이 있으면
                {
                    //  sameSideHeroes[i].GetHealed(healAmount); //이거 RPC로 바꿔주기.
                    if (attachingHero.photonView.IsMine)
                    {
                        attachingHero.photonView.RPC("DroneHeal", RpcTarget.All, sameSideHeroes[i] , healAmount);
                    }
                }
            }
            if (activateTime + activeMaxTime < Time.time)
            {
                break;
            }
            yield return ws;
        }
        Debug.Log("힐드론 액티베이트 종료 시간 =" + Time.time+"총 가동시간="+(Time.time - activateTime));
        if (attachingHero.photonView.IsMine)
        {
            attachingHero.photonView.RPC("DroneDisAppear", RpcTarget.All);
        }
    }

    IEnumerator MoveHealDrone()
    {
        float time =0f;
        Vector3 localed = transform.localPosition;
        while (true)
        {
            time += Time.deltaTime;
            transform.localPosition = localed+new Vector3(0, Mathf.Sin(time*2f) * 0.13f, 0);
            
            yield return null;
        }
    }
}
