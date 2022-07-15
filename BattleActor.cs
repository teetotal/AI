using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class BattleActorAbility {
                public enum ATTACK_STYLE {
                    DEFENDER,
                    MOVER,
                    ATTACKER
                }
                //공격 스타일, 이동이 먼저냐 공격이 먼저냐. true이면 무조건 자리에서 계속 공격, false면 이동
                public ATTACK_STYLE AttackStyle { get; set; } 
                //HP
                public float HP { get; set; } 
                //이동 가능 범위
                public int Sight { get; set; }
                //이동 속도
                public float Speed { get; set; }
                /*
                Move
                전방 돌격형
                수비라인 유지형
                후방 안정형
                */
                public float MoveForward { get; set; } //전방
                public float MoveBack { get; set; } // 후방 
                public float MoveSide { get; set; } //  측면
                /*
                Attack
                공격거리. 
                공격력. 
                명중률                
                이타심. 주변 동료를 도우러 가는 정도
                */
                public int AttackDistance { get; set; } 
                public float AttackPower { get; set; } 
                public float AttackAccuracy { get; set; }                 
                public float Altruism { get; set; } 
            }
            public enum BATTLE_SIDE {
                NONE,
                HOME,
                AWAY
            }
            public enum BATTLE_ACTOR_ACTION_TYPE {
                INVALID,
                NONE,
                MOVING,
                ATTACKING,
                ATTACKED,
                READY_ATTACKING,
                READY_ATTACKED,
                DYING
            }
            public class BattleActorAction {
                public Int64 Counter { get; set; }
                public BATTLE_ACTOR_ACTION_TYPE Type { get; set; }
                public string FromPosition { get; set; }
                public string TargetPosition { get; set; }
                public string TargetActorId { get; set; }
                public float AttackAmount { get; set; }

                public BattleActorAction(Int64 counter, string fromPosition, string toPostion, BATTLE_ACTOR_ACTION_TYPE type = BATTLE_ACTOR_ACTION_TYPE.INVALID, string targetActorId = "") {
                    this.Counter = counter;
                    this.Type = type;
                    this.FromPosition = fromPosition;
                    this.TargetPosition = toPostion;
                    this.TargetActorId = targetActorId;
                    this.AttackAmount = 0;
                }
                public BattleActorAction Clone() {
                    BattleActorAction ret = new BattleActorAction(Counter, FromPosition, TargetPosition);
                    ret.Type = Type;
                    ret.TargetActorId = TargetActorId;
                    ret.AttackAmount = AttackAmount;
                    return ret;
                }
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
                public const Int64 INVALID_LAST_TIME = -1;
                private Dictionary<BATTLE_SIDE, Dictionary<string, BattleActor>> mDicSide = new Dictionary<BATTLE_SIDE, Dictionary<string, BattleActor>>();
                private Dictionary<string, BattleActor> mDicActor = new Dictionary<string, BattleActor>();
                //actor id별 마지막 act counter값
                private Dictionary<string, Int64> mDicCount = new Dictionary<string, Int64>();
                //action 정보
                //private Dictionary<string, BATTLE_ACTOR_ACTION_TYPE> mDicAction = new Dictionary<string, BATTLE_ACTOR_ACTION_TYPE>();
                private Dictionary<string, float> mDicHP = new Dictionary<string, float>();
                public bool CreateBattleActor(BATTLE_SIDE side, Actor actor, BattleActorAbility ability, Int64 counter) {
                    if(mDicSide.ContainsKey(side) == false) {
                        mDicSide[side] = new Dictionary<string, BattleActor>();
                    }
                    if(mDicSide[side].ContainsKey(actor.mUniqueId) || mDicActor.ContainsKey(actor.mUniqueId)) {
                        return false;
                    }
                    BattleActor p = new BattleActor(actor, ability, side);
                    mDicSide[side].Add(actor.mUniqueId, p);
                    mDicActor.Add(actor.mUniqueId, p);
                    mDicCount.Add(actor.mUniqueId, counter);
                    //mDicAction.Add(actor.mUniqueId, BATTLE_ACTOR_ACTION_TYPE.NONE);
                    mDicHP.Add(actor.mUniqueId, ability.HP);
                    return true;
                }
                public void RemoveActor(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    mDicSide[actor.mSide].Remove(actorId);
                    mDicActor.Remove(actorId);
                    mDicCount.Remove(actorId);
                    //mDicAction.Remove(actorId);
                    mDicHP.Remove(actorId);
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
                public Int64 GetLastActTime(string actorId) {
                    if(mDicCount.ContainsKey(actorId) == false) {
                        return -1;
                    }
                    return mDicCount[actorId];
                }
                public bool SetLastActTime(string actorId, Int64 counter) {
                    if(mDicCount.ContainsKey(actorId) == false) {
                        return false;
                    }

                    mDicCount[actorId] = counter;
                    return true;
                }
                /*
                public void ReleaseAction() {
                    List<string> list = new List<string>();
                    foreach(var p in mDicAction) {
                        list.Add(p.Key);
                    }
                    foreach(var actorId in list) {

                        switch(mDicAction[actorId]){
                            case BATTLE_ACTOR_ACTION_TYPE.ATTACKING:
                            case BATTLE_ACTOR_ACTION_TYPE.ATTACKED:
                            mDicAction[actorId] = BATTLE_ACTOR_ACTION_TYPE.NONE;
                            break;
                        } 
                    }
                }
                public bool SetActionState(string actorId, BATTLE_ACTOR_ACTION_TYPE type) {
                    if(mDicAction.ContainsKey(actorId) == false) {
                        return false;
                    }
                    mDicAction[actorId] = type;
                    return true;
                }
                public BATTLE_ACTOR_ACTION_TYPE GetActionState(string actorId) {
                    if(mDicAction.ContainsKey(actorId) == false) {
                        return BATTLE_ACTOR_ACTION_TYPE.INVALID;
                    }
                    return mDicAction[actorId];
                }
                */
                public float GetHP(string actorId) {
                    if(mDicHP.ContainsKey(actorId) == false) {
                        return -1;
                    }
                    return mDicHP[actorId];
                }
                public float DecreaseHP(string actorId, float amount) {
                    if(mDicHP.ContainsKey(actorId) == false) {
                        return -1;
                    }
                    mDicHP[actorId] = Math.Max(0, mDicHP[actorId] -amount);                   
                    return mDicHP[actorId];
                }
            }
        }
    }
}