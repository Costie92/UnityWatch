using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSUltMissile : Projectile
{
    [SerializeField]
    float explosionRange = 10f;

    [SerializeField]
    bool isActivated = false;
    
    [SerializeField]
    float maxVelocity = 3f;
    [SerializeField]
    float startVelocity = 1f;

    [SerializeField]
    Transform firePos;

    public int attachedNumber;
    
    private void Awake()
    {
        amount = 50f;   //솔져 궁의 데미지 양
        velocity = 1f;
    }

    public void Activate(Vector3 shootStartPos)
    {
        //dshootStartPos 이걸 시작 포지션이르ㅗ.
        velocity = startVelocity;
        isActivated = true;

        transform.SetPositionAndRotation(firePos.position, firePos.rotation);
        
        gameObject.SetActive(true);
    }
    public void DeActivate()
    {
        isActivated = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isActivated)
        {
            if (velocity < maxVelocity)
            {
                velocity += Time.deltaTime ;    //속도 올려주기.
            }

            transform.Translate(Vector3.forward * velocity * Time.deltaTime, Space.Self);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!attachingHero.photonView.IsMine)
            return;

        //내꺼면 충돌 체크.


        //플레이어 거나 벽이면 조건 부텽주기..

        attachingHero.photonView.RPC("BoomUltMissile",Photon.Pun.RpcTarget.All,  attachedNumber, transform.position);

        Collider[] bombed = Physics.OverlapSphere(transform.position, explosionRange);
        //적이면 뭐 해주기.
        for (int i = 0; i < bombed.Length; i++)
        {

        }
    }
    public void Boom(Vector3 boomedPos)
    {
        transform.position = boomedPos;
        isActivated = false;
        //그냥 폭발하는 효과만 붙여주기.
        //폭발위치도 함께 RPC 할것.
    }
}
