using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class HSHealDrone : MonoBehaviourPun
{


    [SerializeField]
    float healAmount = 10f;
    [SerializeField]
    float healCoolTime = 0.3f;
    [SerializeField]
    float healRange = 40f;

    [Tooltip("heal drone activating Time")]
    [SerializeField]
    float activeMaxTime = 7f;

    float SqrHealRange;
    WaitForSeconds ws;

    float activateTime;

    private void Awake()
    {
        SqrHealRange = healRange * healRange;
        ws = new WaitForSeconds(healCoolTime);
    }

    public void Activate()  //애니메이션 처리도 해주기. 그냥 키면 되는데, 처음으로 키라고.
        //셰이더 써서 멋있게 나오게 하는 것도 좋음.
    {
        if (photonView.IsMine)
        {
            StartCoroutine(StartingHealingProtocol());
        }
    }

    IEnumerator StartingHealingProtocol()
    {
        activateTime = Time.time;
        while (true)
        {
            Hero[] sameSideHeroes = null;   //나중에 우리편 히어로만 받아올 수 있게.

            for (int i = 0; i < sameSideHeroes.Length; i++)
            {
                if (SqrHealRange >= (sameSideHeroes[i].transform.position - transform.position).sqrMagnitude)   //힐 범위에 아군이 있으면
                {
                   //  sameSideHeroes[i].GetHealed(healAmount); //이거 RPC로 바꿔주기.
                }
            }
            if (activateTime + activeMaxTime < Time.time)
            {
                break;
            }
            yield return ws;
        }

        DeActivate();
    }
    [PunRPC]
    public void DeActivate()   //드론 사라지게 만듦. 얘도 RPC로 해주기.
    {

    }
    
}
