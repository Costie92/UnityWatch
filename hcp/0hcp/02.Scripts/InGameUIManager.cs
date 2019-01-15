using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace hcp
{
    public class InGameUIManager : MonoBehaviour
    {
        [Tooltip("controller point")]
        [SerializeField]
        GameObject cont;
        [Tooltip("controller range")]
        [SerializeField]
        GameObject contBack;
        [Tooltip("controller max point")]
        [SerializeField]
        GameObject contMax;

        [SerializeField]
        Vector3 charactorMoveV = Vector3.zero;

        [SerializeField]
        Vector3 mouseTouched;//임시로 회전 용으로 사용. 에디터에서 터치를 못 읽어서
        [SerializeField]
        bool contTouched;//임시로 회전 용으로 사용. 에디터에서 터치를 못 읽어서

        [SerializeField]
        Image crossHair;

        MoveController moveController;
        [SerializeField]
        Hero targetHero;

        public Text[] ct;

        static InGameUIManager instance;
        public static InGameUIManager Instance
        {
            get { return instance; }
        }

        private void Awake()
        {
            instance = this;
        }


        // Use this for initialization
        void Start()
        {
            moveController = new MoveController(contBack.transform.position, contMax.transform.position);   //스케일 렌더 모드 캔버스의 실제 ui 포지션을 얻고 싶으면
                                                                                                            //스타트 에서 포지션을 접근해야함.
        }

        // Update is called once per frame
        void Update()
        {
            if (targetHero == null) return;
            for (int i = 0; i < (int)E_ControlParam.MAX; i++)
            {
                ct[i].text = targetHero.GetReUseRemainTime((E_ControlParam)i).ToString();
            }


            targetHero.MoveHero(charactorMoveV * Time.deltaTime * 5);


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

                //  Debug.Log("마우스 이동중2");

                Vector3 mousePos = Input.mousePosition;
                Vector3 rotateV = mousePos - mouseTouched;
                // targetHero.transform.Rotate(new Vector3(0, rotateV.x / Screen.width, 0), Space.Self);
                targetHero.RotateHero(new Vector3(rotateV.y/Screen.height, rotateV.x / Screen.width, 0));
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
            Vector3 moveV = moveController.GetMoveVector(Input.mousePosition, out contV);
            cont.transform.position = contV;
            charactorMoveV = moveV;
        }
        public void On_MoveStop()
        {
            contTouched = false;
            cont.transform.position = contBack.transform.position;
            charactorMoveV = Vector3.zero;
        }

        public void OnClick_NormalAttack()
        {
            targetHero.ControlHero(E_ControlParam.NormalAttack);
        }
        public void OnClick_Reload()
        {
            targetHero.ControlHero(E_ControlParam.Reload);
        }
        public void OnClick_FirstSkill()
        {
            targetHero.ControlHero(E_ControlParam.FirstSkill);
        }
        public void OnClick_Ultimate()
        {
            targetHero.ControlHero(E_ControlParam.Ultimate);
        }
        public void CrossHairChange(Sprite crossHair)
        {
            this.crossHair.sprite = crossHair;
        }
        public void SetTargetHero(Hero hero)
        {
            targetHero = hero;
            CrossHairChange(targetHero.crossHairs[0]);
        }
    }
}