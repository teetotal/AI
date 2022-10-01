using System;
using System.Collections.Generic;
using System.Linq;
using ENGINE;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class AvoidanceWeight : Singleton<AvoidanceWeight> {
                public AvoidanceWeight() {

                }
                public float GetWeightDefault(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    // 적군의 사정거리에서 먼곳으로
                    int cnt = 0;
                    double distanceSum = 0;
                    for(int i = 0; i < opponentTeam.Count; i++) {
                        Soldier solier = opponentTeam[i];
                        if(solier.GetPosition().GetDistance(pos) > solier.GetAbility().attackRange)
                            cnt++;
                        distanceSum += solier.GetPosition().GetDistance(pos);
                    }

                    //적들과의 거리에서 가장 먼곳으로
                    float ret = ((cnt / opponentTeam.Count) * 0.5f) + (float)((distanceSum / (opponentTeam.Count * soldier.GetMap().GetMaxDistance()) * 0.5f));
                    return ret;
                }
                /*
                public float GetWeightOrver(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    float a = 0, b = 0;
                    //가장 가까이 있는 아군
                    var q = from my in myTeam
                            orderby my.GetPosition().GetDistance(pos)
                            select my.GetPosition();

                    if(q != null && q.Count() > 0) {
                        a = 1 - (float)(pos.GetDistance(q.First()) / soldier.GetMap().GetMaxDistance());
                    } 
                    // 최대한 멀리
                    b  = (float)(soldier.GetPosition().GetDistance(pos) / soldier.GetMap().GetMaxDistance());
                    
                    float retreat = soldier.GetInfo().ability.retreat;
                    return (a * (1 - retreat)) + (b * retreat);
                }
                */
                
            }
            public class MoveWeight : Singleton<MoveWeight> {
                public delegate float Fn(Soldier solidier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam, int seq);
                private List<Fn> mFnDefault;
                float[] biases;
                float range;
                public MoveWeight() { 
                    mFnDefault = new List<Fn>() { GetWeightDefault_OnTarget, GetWeightDefault_InnerTarget, GetWeightDefault_ApproachingEnemy };
                    int level = mFnDefault.Count;
                    range = 0.8f / level;
                    biases = new float[level]; 
                    for(int i = 0; i < level; i++) {
                        biases[i] = (((level - i) -1) * range) + 0.1f;
                    }
                }
                public List<Fn> GetFnDefault() {
                    return mFnDefault;
                }
                public float GetWeight(float val, int seq) {
                    return (val * range) + biases[seq];
                }
                public float GetWeightDefault_OnTarget(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam, int seq) {
                    //사정거리에 정확히 걸려 있는 적
                    
                    //거리안에 있는 같은 편수. 팀웍 능력치
                    //float myTeamRate = (from my in myTeam where pos.GetDistance(my.GetPosition()) <= mSoldierAbility.teamwork select my).Count() / myTeam.Count;

                    var q = from opp in opponentTeam where pos.GetDistance(opp.GetPosition()) == soldier.GetAbility().attackRange select opp;
                    if(q != null && q.Count() > 0) {
                        return GetWeight(q.Count() / opponentTeam.Count, seq);
                    }
                    return 0;
                }
                public float GetWeightDefault_InnerTarget(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam, int seq) {
                    //사정거리내 적의 수
                    var q = from opp in opponentTeam where pos.GetDistance(opp.GetPosition()) <= soldier.GetAbility().attackRange select opp;
                    if(q != null && q.Count() > 0) {
                        return GetWeight(q.Count() / opponentTeam.Count, seq);
                    }
                    return 0;
                }
                public float GetWeightDefault_ApproachingEnemy(Soldier soldier, float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam, int seq) {
                    //적과의 거리.
                    var distanceToEnemy = (from opp in opponentTeam orderby pos.GetDistance(opp.GetPosition()) select opp.GetPosition());
                    if(distanceToEnemy != null && distanceToEnemy.Count() > 0) {
                        float weight = 1 - (float)(pos.GetDistance(distanceToEnemy.First()) / soldier.GetMap().GetMaxDistance());
                        //weight = (weight * 0.5f) + (obstacleConcentration * 0.5f);
                        return GetWeight(weight, seq);
                    }
                    return 0;
                }
            }
        }
    }
}