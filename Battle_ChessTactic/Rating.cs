using System;
using System.Collections.Generic;
using ENGINE; 
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public enum BehaviourType {
                RECOVERY,
                MOVE,
                ATTACK,
                KEEP,
            }
            public class Rating {
                public bool isHome;
                public int soldierId;
                public BehaviourType type;
                public float rating;
                public int targetId;
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