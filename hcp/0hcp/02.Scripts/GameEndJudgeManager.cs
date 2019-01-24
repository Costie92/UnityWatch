using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Pun;
namespace hcp
{
    public class GameEndJudgeManager : MonoBehaviourPun
    {
        [SerializeField]
        Canvas gameEndCanvas;
        [SerializeField]
        Image gameEndScreen;
        [SerializeField]
        Text gameEndText;
        [SerializeField]
        Material geScreenDissolveMat;

        [SerializeField]
        Payload payload;
        [SerializeField]
        bool payloadArrived ;
        [SerializeField]
        bool judgeDone;

        Color winColor = new Color(0/255, 166/255, 255/255);
        Color loseColor = new Color(255 / 255, 0 / 255, 44 / 255);

        private void Awake()
        {
            judgeDone = false;
            payloadArrived = false;
            
        }
        void Start()
        {/*
            Vector2 myVector = new Vector2(Screen.width, Screen.height);
            gameEndScreen.GetComponent<RectTransform>().sizeDelta = myVector;
            */
            geScreenDissolveMat = new Material(gameEndScreen.material);
            gameEndScreen.material = geScreenDissolveMat;
            gameEndScreen.gameObject.SetActive(false);

            payload.AddListenerPayloadArrive(PayloadArrive);
        }
        void PayloadArrive()
        {
            payloadArrived = true;
        }
        bool IsMatchTimeDone()
        {
            //게임 시간 받아오기.
            return false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (judgeDone) return;

            if (payloadArrived)
            {
                judgeDone = true;
                //화물 도착, 게임 종료.
                photonView.RPC("GameJudgeReceived", RpcTarget.All, JudgeWhichTeamWin());
                return;
            }
            if (IsMatchTimeDone())
            {
                if (!payload.HeroClose)
                {
                    judgeDone = true;
                    //화물 도착, 게임 종료.
                    photonView.RPC("GameJudgeReceived", RpcTarget.All, JudgeWhichTeamWin());
                    return;
                }
            }
        }

        E_Team JudgeWhichTeamWin()
        {
            float farFromA = payload.GetHowFarFromTeamA();
            float farFromB = payload.GetHowFarFromTeamB();

            if (farFromA >= farFromB)
            {
                // B팀 승리.
                return E_Team.Team_B;
            }
            else {
                //A팀 승리.
                return E_Team.Team_A;
            }
        }

        [PunRPC]
        public void GameJudgeReceived(E_Team winTeam)
        {
            int winTeamLayer = Constants.GetLayerByE_Team(winTeam);

            int myLayer = TeamInfo.GetInstance().MyTeamLayer;

            if (winTeamLayer == myLayer)
            {
                //나 이겼음.

                StartCoroutine(GameEndShow(true));
                
            }
            else
            {
                StartCoroutine(GameEndShow(false));
                //나 쟜음.
            }
        }
        
        IEnumerator GameEndShow(bool win)
        {
            gameEndScreen.gameObject.SetActive(true);
            gameEndText.gameObject.SetActive(false);
            geScreenDissolveMat.SetColor(" _EdgeColour2", (win)?winColor:loseColor  );
            float startTime = 0f;
            while (startTime < 1f)
            {
                startTime += Time.deltaTime;
                geScreenDissolveMat.SetFloat("_Level", 1 - startTime);
                yield return null;
            }
            geScreenDissolveMat.SetFloat("_Level", 0);
            if (win)
            {
                gameEndText.text = "WIN";
                gameEndText.gameObject.SetActive(true);
            }
            else {
                gameEndText.text = "LOSE";
                gameEndText.gameObject.SetActive(true);
            }

        }
      
    }
}