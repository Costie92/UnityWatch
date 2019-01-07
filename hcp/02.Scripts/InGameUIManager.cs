using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InGameUIManager : MonoBehaviour {
    public GameObject cont;
    public GameObject contBack;
    public GameObject contMax;

    [SerializeField]
    Vector3 charactorMoveV;

    [SerializeField]
    Vector3 mouseTouched;//임시로 회전 용으로 사용. 에디터에서 터치를 못 읽어서
    [SerializeField]
    bool contTouched;//임시로 회전 용으로 사용. 에디터에서 터치를 못 읽어서

    MoveController moveController;

    public GameObject tempChara;
    
    // Use this for initialization
    void Start () {
        moveController = new MoveController(contBack.transform.position, contMax.transform.position);   //스케일 렌더 모드 캔버스의 실제 ui 포지션을 얻고 싶으면
        //스타트 에서 포지션을 접근해야함.
    }
	
	// Update is called once per frame
	void Update () {
        tempChara.transform.Translate(charactorMoveV * Time.deltaTime*5, Space.Self);


        //임시로 하는 것 뿐임.
        if (Input.GetMouseButtonDown(0) && !contTouched)
        {
            if (mouseTouched == Vector3.zero)
            {
                mouseTouched = Input.mousePosition;
                return;
            }
        }
        else if (Input.GetMouseButtonUp(0) && !contTouched)
        {
            mouseTouched = Vector3.zero;
        }
        else if (Input.GetMouseButton(0) && !contTouched)
        {

            Debug.Log("마우스 이동중2");

            Vector3 mousePos = Input.mousePosition;
            Vector3 rotateV = mousePos - mouseTouched;
            tempChara.transform.Rotate(new Vector3(0, rotateV.x / Screen.width, 0), Space.Self);
        }

        /*
        Debug.Log("터치카운트" + Input.touchCount);

        if (Input.touchCount > 0)   //나중에 핸드폰 연결해서 터치로 받게 하기.
        {
            Debug.Log("터치처리" + Input.touchCount);
            Vector2 deltaSwipe = Input.touches[0].deltaPosition;
            Vector3 rotate = new Vector3(0, deltaSwipe.x, 0);  //y가 -90 ~ 90로 돌면 됨 양수가 오른쪽으로 로테이트

            tempChara.transform.Rotate(rotate, Space.Self);
        }
        */

    }
    public void On_MoveCont()  //회전 상관 없이 이동만 관장함.
        //이제 쉐이더 넣고 해주면 됨.
    {
        contTouched = true; //임시 (회전 값 보정 위해.)
        Vector3 contV;
        Vector3 moveV = moveController.GetMoveVector(Input.mousePosition , out contV);
        cont.transform.position = contV;
        charactorMoveV = moveV;
       
    }
    public void On_MoveStop()
    {
        contTouched = false;
        cont.transform.position = contBack.transform.position;
        charactorMoveV = Vector3.zero;
    }
}
