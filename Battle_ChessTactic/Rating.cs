using System;
using System.Collections.Generic;
using ENGINE; 
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public enum BehaviourType {
                AVOIDANCE,
                RECOVERY,
                ATTACK,
                MOVE,
                KEEP,
                MAX
            }
            public class Rating {
                public int side;
                public int soldierId;
                public BehaviourType type;
                public float rating;
                public int targetSide;
                public int targetId;
            }
            public class Plan { // 현재 진행 중인 액션. 아직 완료하기 전 상태
                public BehaviourType type;
                public int targetId;
                public Position position = new Position(); //행위하는 위치
                public Plan() {
                    type = BehaviourType.MAX;
                    targetId = -1;
                }
                public void Set(Rating rating, Soldier soldier) {
                    type = rating.type;
                    targetId = rating.targetId;
                    switch(rating.type) {
                        case BehaviourType.AVOIDANCE:
                        case BehaviourType.MOVE:
                            position.Set(soldier.GetMap().GetPosition(targetId));
                        break;
                        default: {
                            position.Set(soldier.GetPosition());
                        }
                        break;
                    }
                }
                public bool IsEqualPosition(Plan p) {
                    return position.IsEqual(p.position);
                }
                public bool IsEqual(Rating p) {
                    if(type == p.type && targetId == p.targetId)
                        return true;
                    return false;
                }
            }
            public class RatingPool : Singleton<RatingPool> {
                private ObjectPool<Rating> mPool = new ObjectPool<Rating>();
                public RatingPool() { }
                public ObjectPool<Rating> GetPool() {
                    return mPool;
                }
            }
        }
    }
}