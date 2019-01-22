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
        struct OnGoingPath
        {
            public team nowMoveTeam;
            public int targetNum;

            public void Set(team team, int num)
            {
                nowMoveTeam = team;
                targetNum = num;
            }
            public void SwitchTeam()
            {
                switch (nowMoveTeam)
                {
                    case team.TeamA:
                        nowMoveTeam = team.TeamB;
                        break;
                    case team.TeamB:
                        nowMoveTeam = team.TeamA;
                        break;
                }
            }
        }

        [SerializeField]
        OnGoingPath nextPath;

        [SerializeField]
        float moveSpeed;

        [SerializeField]
        List<Transform> AWayPoints;
        [SerializeField]
        List<Transform> BWayPoints;
        

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



        IEnumerator Start() {
            if (!PhotonNetwork.IsMasterClient)
                yield break;
            wayPointcloseEnoughSqr = wayPointcloseEnough * wayPointcloseEnough;
            distanceSqr = distance * distance;

            nextPath = new OnGoingPath();
            nextPath.nowMoveTeam = team.None;
            nextPath.targetNum = -1;

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

            if (MoveSideCheck() == false || arrive) return;

            Vector3 dir = (GetNextMoveTarget(nextPath).position - transform.position).normalized;
            transform.Translate(dir * Time.deltaTime * moveSpeed, Space.World);
        }
        

        bool MoveSideCheck()
        {
                int ATeamCount = GetCountOfCloseHeroes(ATeamHeroes);
                int BTeamCount = GetCountOfCloseHeroes(BTeamHeroes);

                if (ATeamCount > 0 && BTeamCount == 0)
                {
                    PayloadMovePathSet(team.TeamB); //a팀이 우세하므로 b팀 쪽으로 밀음.
                    return true;
                }
                else if (ATeamCount == 0 && BTeamCount > 0)
                {
                    PayloadMovePathSet(team.TeamA);
                    return true;
                }
                return false;
        }

        void PayloadMovePathSet(team whichTeam)
        {
            if (nextPath.nowMoveTeam == team.None && nextPath.targetNum == -1)  //맨처음
            {
                nextPath.Set(whichTeam, 0);
                return;
            }
            if (nextPath.nowMoveTeam == whichTeam)  //똑같은 팀이 진행방향인 경우.
            {
                if (WayPointClose(nextPath))
                {
                    if (IsLastWayPoint(nextPath))
                    {
                        return;
                    }
                    else
                    {
                        nextPath.targetNum++;
                    }
                }
                else return;
            }
            else
            {
                //진행 방향이 다른 팀으로 변경된 경우
                if (WayPointClose(nextPath))
                {
                    if (nextPath.targetNum == 0)    //다른팀으로 완전히 넘어가는 시기.
                    {
                        nextPath.SwitchTeam();

                    }
                    else
                    {
                        nextPath.targetNum--;
                    }
                }
                else return;

            }

            /*

            if (nowSetDestWayPointNum == -1 && movingTeam == team.None)    //맨 처음.
            {
                movingTeam = whichTeam;
                nowSetDestWayPointNum = 0;
            }
            else {
                if (movingTeam == whichTeam)
                {
                    nowSetDestWayPointNum = GetNextNumberOfWayPoint(true, nowSetDestWayPointNum);
                }
                else
                {
                    nowSetDestWayPointNum = GetNextNumberOfWayPoint(false, nowSetDestWayPointNum);
                    if (nowSetDestWayPointNum == 0) //완전히 이동경로가 바뀌는 타이밍
                    {
                        movingTeam = whichTeam;
                    }
                }
            }

            Transform target = GetNextMoveTarget(whichTeam, nowSetDestWayPointNum);
            Vector3 dir = target.position - transform.position;
            
            transform.Translate(dir.normalized * Time.deltaTime * moveSpeed);
            */
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

       
        //페이로드가 완전히 도착하면 델리게이트 호출하고 이게 불려짐.
        void PayloadArrive()
        {
            arrive = true;
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

        Transform GetNextMoveTarget(team team, int num)
        {
            Transform tr = null;
            switch (team)
            {
                case team.TeamA:
                    tr= AWayPoints[num];
                    break;
                case team.TeamB:
                    tr= BWayPoints[num];
                    break;
            }
            return tr;
        }
        Transform GetNextMoveTarget(OnGoingPath path)
        {
            Transform tr = null;
            
            switch (path.nowMoveTeam)
            {
                case team.TeamA:
                    tr = AWayPoints[path.targetNum];
                    break;
                case team.TeamB:
                    tr = BWayPoints[path.targetNum];
                    break;
            }
            return tr;
        }
        

        bool WayPointClose(team whichTeam, int num)
        {
            Transform target =  GetNextMoveTarget(whichTeam, num);
            if ((target.position - transform.position).sqrMagnitude < wayPointcloseEnoughSqr)   //충분히 가까움.
            {
                return true;
            }
            return false;
        }
        bool WayPointClose(OnGoingPath path)
        {
            return WayPointClose(path.nowMoveTeam, path.targetNum);
        }
        bool IsLastWayPoint(team team, int num)
        {
            switch (team)
            {
                case team.TeamA:
                    if (num == AWayPoints.Count - 1)
                    {
                        return true;
                    }
                    return false;
                case team.TeamB:
                    if (num == BWayPoints.Count - 1)
                    {
                        return true;
                    }
                    return false;
            }
            return false;
        }
        bool IsLastWayPoint(OnGoingPath path)
        {
            return IsLastWayPoint(path.nowMoveTeam, path.targetNum);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, distance);
        }


    }
}