using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace hcp {
    public class Payload : MonoBehaviourPun {

        System.Action payLoadArrive;

        [System.Serializable]
        enum team
        {
            None,
            TeamA,
            TeamB,
            MAX
        }
        [System.Serializable]
        struct WPRange
        {
            public int Aside;
            public int Bside;
            public WPRange(int a, int b )
            {
                Aside = a;
                Bside = b;
            }

            public void MoveSide(team team)
            {
                switch (team)
                {
                    case team.TeamA:
                        Aside--;
                        Bside--;
                        break;

                    case team.TeamB:
                        Aside++;
                        Bside++;
                        break;
                }
            }
        }
        

        [SerializeField]
        WPRange nowRange;

        [SerializeField]
        float moveSpeed;
        [SerializeField]
        Transform startPoint;
        [SerializeField]
        List<Transform> AWayPoints;
        [SerializeField]
        List<Transform> BWayPoints;
        [SerializeField]
        List<Transform> wholeWayPoints;
        

        [SerializeField]
        Transform nextTarget;


        [SerializeField]
        float wayPointcloseEnough;
        float wayPointcloseEnoughSqr;

        [SerializeField]
        float distance;
        float distanceSqr;

        [SerializeField]
        float checkTime;
        WaitForSeconds cws;

        [SerializeField]
        bool arrive = false;

        [SerializeField]
        List<Hero> ATeamHeroes;
        [SerializeField]
        List<Hero> BTeamHeroes;

        [SerializeField]
        int nowTargetPoint;



        IEnumerator Start() {
            if (!PhotonNetwork.IsMasterClient)
                yield break;
            SetWholePath();

            wayPointcloseEnoughSqr = wayPointcloseEnough * wayPointcloseEnough;
            distanceSqr = distance * distance;
            
            payLoadArrive += PayloadArrive;

            cws = new WaitForSeconds(checkTime);

            while (!TeamInfo.GetInstance().isTeamSettingDone)
                yield return cws;
            Debug.Log("팀 세팅 끝남 확인.");
            
            GetABHeroes();

            if (TeamInfo.GetInstance().EnemyTeamLayer.Count != 1 )
            {
                Debug.LogError("이 게임은 2 팀 대결이 아님.");
            }
        }

        void SetWholePath()
        {
            wholeWayPoints = new List<Transform>();
            for (int i =AWayPoints.Count-1;i >=0;i--)
            {
                wholeWayPoints.Add(AWayPoints[i]);
            }

            nowRange = new WPRange(wholeWayPoints.Count-1 , wholeWayPoints.Count);
            
            for (int i = 0; i < BWayPoints.Count; i++)
            {
                wholeWayPoints.Add(BWayPoints[i]);
            }
        }

        void GetABHeroes()
        {
            ATeamHeroes = new List<Hero>();
            BTeamHeroes = new List<Hero>();
            Hero[] heroes = GameObject.FindObjectsOfType<Hero>();

            for (int i = 0; i < heroes.Length; i++)
            {
                int photonKey = heroes[i].photonView.ViewID / 1000;
                string teamName =  NetworkManager.instance.Teams[photonKey];
                if (teamName == Constants.teamA_LayerName)
                {
                    ATeamHeroes.Add(heroes[i]);
                }
                else if (teamName == Constants.teamB_LayerName)
                {
                    BTeamHeroes.Add(heroes[i]);
                }
                else {
                    Debug.LogError("GetABHeroes 양팀 대결이 아님.");
                }
            }
        }

        void Update()
        {
            if (!TeamInfo.GetInstance().isTeamSettingDone) return;
            if (!PhotonNetwork.IsMasterClient) return;
            if (arrive) return;
            Vector3 dir;
            if (MoveSideCheck(out dir) == false ) return;
            
            transform.Translate(dir * Time.deltaTime * moveSpeed, Space.World);
        }

        //프로퍼티에서 볼려구 그냥 뻈음.
        [SerializeField]
        int ATeamCount;
        [SerializeField]
        int BTeamCount;
        [SerializeField]
        team judgedTeam;

        bool MoveSideCheck(out Vector3 dir)
        {
            dir = Vector3.zero;
                 ATeamCount = GetCountOfCloseHeroes(ATeamHeroes);
                 BTeamCount = GetCountOfCloseHeroes(BTeamHeroes);

            if (ATeamCount > 0 && BTeamCount == 0)  //팀 b로 밀음.
            {
                if (nowRange.Bside == wholeWayPoints.Count - 1 && WayPointClose(nowRange.Bside))
                {
                    payLoadArrive();
                    return false;
                }

                if (WayPointClose(nowRange.Bside) )
                {
                    nowRange.MoveSide(team.TeamB);
                }
                dir = (wholeWayPoints[nowRange.Bside].position - transform.position).normalized;
                
                judgedTeam = team.TeamB;
                    return true;
            }

                else if (ATeamCount == 0 && BTeamCount > 0)
            {
                if (nowRange.Aside == 0 && WayPointClose(nowRange.Aside))
                {
                    payLoadArrive();
                    return false;
                }

                if (WayPointClose(nowRange.Aside))
                {
                    nowRange.MoveSide(team.TeamA);
                }
                dir = (wholeWayPoints[nowRange.Aside].position - transform.position).normalized;
                judgedTeam = team.TeamA;
                return true;
            }
            judgedTeam = team.None;
            dir = Vector3.zero;
            return false;
        }
        
        bool WayPointClose(int num)
        {
            if ((wholeWayPoints[num].position - transform.position).sqrMagnitude < wayPointcloseEnoughSqr)
                return true;
            return false;
        }

        //페이로드가 완전히 도착하면 델리게이트 호출하고 이게 불려짐.
        void PayloadArrive()
        {
            arrive = true;
            Debug.Log("화물이 종단점에 도착했습니다.");
        }
        
        bool IsHeroClose(Hero hero)
        {
            if (hero.Die) return false;
            Vector3 heroPos = hero.transform.position - transform.position;
            if (heroPos.sqrMagnitude <= distanceSqr)
            {
                return true;
            }
            return false;
        }
        
        int GetCountOfCloseHeroes(List<Hero> heroes)
        {
            int result = 0;
            for (int i = 0; i < heroes.Count; i++)
            {
                if (IsHeroClose(heroes[i]))
                    result++;
            }
            return result;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, distance);
            Gizmos.DrawWireSphere(transform.position, wayPointcloseEnough);
        }

        public void AddListenerPayloadArrive(System.Action ac)
        {
            payLoadArrive += ac;
        }
    }
}