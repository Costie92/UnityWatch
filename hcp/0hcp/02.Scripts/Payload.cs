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

        void SetWholePath()
        {
            wholeWayPoints = new List<Transform>();
            for (int i =AWayPoints.Count-1;i >=0;i--)
            {
                wholeWayPoints.Add(AWayPoints[i]);
            }

            nowTargetPoint = wholeWayPoints.Count; //초기에 스타트 포인트로 넥스트 타겟 지정.

            wholeWayPoints.Add(startPoint);

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

            Vector3 dir;
            if (MoveSideCheck(out dir) == false || arrive) return;
            
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
                 ATeamCount = GetCountOfCloseHeroes(ATeamHeroes);
                 BTeamCount = GetCountOfCloseHeroes(BTeamHeroes);

                if (ATeamCount > 0 && BTeamCount == 0)  //팀 b로 밀음.
                {
                dir = GetDir(team.TeamB);


              //      PayloadMovePathSet(team.TeamB); //a팀이 우세하므로 b팀 쪽으로 밀음.
                judgedTeam = team.TeamB;
                    return true;
                }
                else if (ATeamCount == 0 && BTeamCount > 0)
                {
                dir = GetDir(team.TeamA);
                   // PayloadMovePathSet(team.TeamA);
                judgedTeam = team.TeamA;
                return true;
                }
            judgedTeam = team.None;
            dir = Vector3.zero;
            return false;
        }

        Vector3 GetDir(team teamDir)
        {
            Vector3 dir = Vector3.zero;
            switch (teamDir)
            {
                case team.TeamA:
                    if (nowTargetPoint == 0)
                        return dir;
                    if (WayPointClose(nowTargetPoint - 1))
                    {
                        nowTargetPoint = nowTargetPoint - 1;
                    }
                    if (nowTargetPoint == 0)
                        return dir;

                        return (wholeWayPoints[nowTargetPoint - 1].position - wholeWayPoints[nowTargetPoint].position).normalized;
                case team.TeamB:
                    if (nowTargetPoint == wholeWayPoints.Count - 1)
                        return dir;
                    if (WayPointClose(nowTargetPoint + 1))
                    {
                        nowTargetPoint = nowTargetPoint + 1;
                    }
                    if (nowTargetPoint == wholeWayPoints.Count - 1)
                        return dir;

                    return (wholeWayPoints[nowTargetPoint + 1].position - wholeWayPoints[nowTargetPoint].position).normalized;
            }
            return dir;
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
        }





        void PayloadMovePathSet(team whichTeam)
        {
            switch (whichTeam)
            {

            }



            /*
            if (nextPath.nowMoveTeam == team.None && nextPath.targetNum == -1)  //맨처음
            {
                nextPath.Set(whichTeam, 0);
                return;
            }
            if (nextPath.nowMoveTeam == whichTeam)  //이건 현재 가고 있는 영역과 같냐지.
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
                else
                {
                    //앞으로 가던 중이냐 뒤로 가던중이냐?
                    return;
                }
            }
            else
            {
                //현재 영역은 다른 팀인데 새로 들어와서 경로 방향이 바뀌는 타이밍.
                if (nextPath.targetNum == 0 || 
                    (
                    nextPath.targetNum==1 && WayPointClose(nextPath)
                    )
                    )
                {
                    nextPath.SwitchTeam();
                    nextPath.targetNum = 0;
                    if (WayPointClose(nextPath))
                        nextPath.targetNum = 1;
                    return;
                }
                else
                {
                    if (WayPointClose(nextPath.nowMoveTeam, nextPath.targetNum-1))
                    {
                        nextPath.targetNum = nextPath.targetNum - 2;
                    }
                    else
                    nextPath.targetNum--;
                }
                

                
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
                else
                {
                    nextPath.targetNum--;
                    return;
                }
                
            }

            

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