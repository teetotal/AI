using System;
using System.Collections.Generic;
using System.Linq;
using ENGINE;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class MoveWeight : Singleton<MoveWeight> {
                public delegate float Fn(Soldier solidier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam);
                private List<Fn> mFnDefault;
                float[] bias;
                float range;
                public MoveWeight() { 
                    mFnDefault = new List<Fn>() {GetWeightDefault_OnTarget, GetWeightDefault_InnerTarget, GetWeightDefault_ApproachingEnemy};
                    int level = mFnDefault.Count;
                    range = 0.8f / level;
                    bias = new float[level]; 
                    for(int i = 0; i < level; i++) {
                        bias[i] = (((level - i) -1) * range) + 0.1f;
                    }
                }
                public List<Fn> GetFnDefault() {
                    return mFnDefault;
                }
                public float GetWeightDefault_OnTarget(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //사정거리에 정확히 걸려 있는 적
                    
                    //거리안에 있는 같은 편수. 팀웍 능력치
                    //float myTeamRate = (from my in myTeam where pos.GetDistance(my.GetPosition()) <= mSoldierAbility.teamwork select my).Count() / myTeam.Count;

                    var q = from opp in opponentTeam where pos.GetDistance(opp.GetPosition()) == soldier.GetAbility().attackRange select opp;
                    if(q != null && q.Count() > 0) {
                        return ((q.Count() / opponentTeam.Count) * range) + bias[0];
                    }
                    return 0;
                }
                public float GetWeightDefault_InnerTarget(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //사정거리내 적의 수
                    var q = from opp in opponentTeam where pos.GetDistance(opp.GetPosition()) <= soldier.GetAbility().attackRange select opp;
                    if(q != null && q.Count() > 0) {
                        return ((q.Count() / opponentTeam.Count) * range) + bias[1];
                    }
                    return 0;
                }
                public float GetWeightDefault_ApproachingEnemy(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //적과의 거리. 장애물 고려
                    var distanceToEnemy = (from opp in opponentTeam orderby pos.GetDistance(opp.GetPosition()) select opp.GetPosition());
                    if(distanceToEnemy != null && distanceToEnemy.Count() > 0) {
                        float weight = 1 - (float)(pos.GetDistance(distanceToEnemy.First()) / soldier.GetMap().GetMaxDistance());
                        //거리 : 장애물 비중 = 1:1
                        weight = (weight * 0.5f) + (obstacleConcentration * 0.5f);
                        return (weight * range) + bias[2];
                    }
                    return 0;
                }
            }
        }
    }
}