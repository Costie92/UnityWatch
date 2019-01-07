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



    private void Awake()
    {
        amount = 50f;   //솔져 궁의 데미지 양
        velocity = 1f;
    }

    public void Activate()
    {
        isActivated = true;
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
        //플레이어 거나 벽이면 조건 부텽주기..

        isActivated = false;
        Boom();
        Collider[] bombed = Physics.OverlapSphere(transform.position, explosionRange);
        //적이면 뭐 해주기.
        
    }
    public void Boom()
    {
        //그냥 폭발하는 효과만 붙여주기.
        //폭발위치도 함께 RPC 할것.
    }
}
