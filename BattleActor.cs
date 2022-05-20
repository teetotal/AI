using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class BattleActorAbility {
                public int Sight { get; set; }
                public float Speed { get; set; }
                /*
                Move
                전방 돌격형
                수비라인 유지형
                후방 안정형
                */
                public float MoveForward { get; set; } //전방
                public float MoveBack { get; set; } // 후방 
                public float MoveSide { get; set; } // 후방 
            }
            public enum BATTLE_SIDE {
                NONE,
                HOME,
                AWAY
            }
            public class BattleActor {
                public BattleActorAbility mAbility { get; set; }
                public Actor mActor { get; set; }
                public BATTLE_SIDE mSide { get; set; } 
                //일단 대충 해놓고 나중에 리팩토링.
                public BattleActor(Actor actor, BattleActorAbility ability, BATTLE_SIDE side) {
                    mActor = actor;
                    mAbility = ability;
                    mSide = side;
                }
            }
            public class BattleActorHandler {
                private Dictionary<BATTLE_SIDE, Dictionary<string, BattleActor>> mDicSide = new Dictionary<BATTLE_SIDE, Dictionary<string, BattleActor>>();
                private Dictionary<string, BattleActor> mDicActor = new Dictionary<string, BattleActor>();
                public bool CreateBattleActor(BATTLE_SIDE side, Actor actor, BattleActorAbility ability) {
                    if(mDicSide.ContainsKey(side) == false) {
                        mDicSide[side] = new Dictionary<string, BattleActor>();
                    }
                    if(mDicSide[side].ContainsKey(actor.mUniqueId) || mDicActor.ContainsKey(actor.mUniqueId)) {
                        return false;
                    }
                    BattleActor p = new BattleActor(actor, ability, side);
                    mDicSide[side].Add(actor.mUniqueId, p);
                    mDicActor.Add(actor.mUniqueId, p);
                    return true;
                }
                public BattleActor? GetBattleActor(string actorId) {
                    if(mDicActor.ContainsKey(actorId) == false) {
                        return null;
                    }
                    return mDicActor[actorId];
                }
                public Dictionary<string, BattleActor>? GetBattleActors(BATTLE_SIDE side) {
                    if(mDicSide.ContainsKey(side) == false) {
                        return null;
                    }
                    return mDicSide[side];
                }
            }
        }
    }
}