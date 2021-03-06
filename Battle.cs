using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {            
            public class Battle {
                public Counter mCounter = new Counter(); //전체 counter랑 별개로 전투 인스턴스별로 가지고 있는 counter. 이걸 기준으로 Next 적용
                public BattleMap mMap;
                private BattleActorHandler mBattleActor = new BattleActorHandler();
                private Dictionary<string, BattleActorAction> mNextAction = new Dictionary<string, BattleActorAction>();
                public Battle(int mapWidth, int mapHeight) {
                     mMap = new BattleMap(mapWidth, mapHeight);
                }
                //직관적인 이해를 위해 x,y의 위치를 바꿔서 저장한다.
                public bool Init(int[,] advantage1, int[,] advantage2) {
                    for(int y = 0; y < advantage1.GetLength(0); y++) {
                        for(int x = 0; x < advantage1.GetLength(1); x++) {
                            if(!mMap.AppendInitMapTile(x, y, advantage1[y,x], advantage2[y,x])) {
                                return false;
                            } 
                        }            
                    }   

                    return true;
                }
                public bool AppendActor(int x, int y, Actor actor, BATTLE_SIDE side, BattleActorAbility ability) {
                    mBattleActor.CreateBattleActor(side, actor, ability, mCounter.GetCount());
                    return mMap.AppendActor(x, y, actor.mUniqueId);
                }
                public BattleActor? GetBattleActor(string actorId) {
                    return mBattleActor.GetBattleActor(actorId);
                }
                // actorid, action
                // 동시에 하는건 없다. 무조건 한번에 하나의 액션
                // 단 동시에 공격당했을 경우 합산은 한다.
                public Dictionary<string, BattleActorAction> Next() {
                    mCounter.Next();
                    Dictionary<string, BattleActorAction> ret = new Dictionary<string, BattleActorAction>();                    
                    if(mNextAction.Count > 0) {
                        foreach(var p in mNextAction) {
                            ret.Add(p.Key, p.Value);
                        }
                        mNextAction = new Dictionary<string, BattleActorAction>();
                    }
                    //미리 예약된 next가 있으면 next에 없는 경우만 처리
                    List<string> list = GetReadyActors(ret);
                    foreach(string actorId in list) {                        
                        var actor = mBattleActor.GetBattleActor(actorId);
                        if(actor == null) {
                            continue;
                        }       
                        //Attacked 상태에서는 act하지 않는다.
                        if(ret.ContainsKey(actorId) && (ret[actorId].Type == BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKED || ret[actorId].Type == BATTLE_ACTOR_ACTION_TYPE.ATTACKED)) {
                            continue;
                        }
                        BattleActorAction action = Act(actor);                                
                        ret.Add(actorId, action);                                                        
                        
                        if(action.Type == BATTLE_ACTOR_ACTION_TYPE.ATTACKING) {
                            string from = mMap.GetActorPosition(actorId);
                            string to = mMap.GetActorPosition(action.TargetActorId);                                    
                            //다음 스텝에 아무것도 안한다.
                            mNextAction.Add(actorId, new BattleActorAction(mCounter.GetNextCount(), from, to, BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKING, action.TargetActorId));
                            
                            //맞은 상대 처리                                      
                            //당장은 아무것도 하지 않는다.
                            BattleActorAction attackedReady = new BattleActorAction(mCounter.GetCount(), from, to, BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKED, actorId); 
                            attackedReady.TargetActorId = actorId;
                            if(ret.ContainsKey(action.TargetActorId)) {
                                ret[action.TargetActorId] = GetBattleActionMerge(action.TargetActorId, ret[action.TargetActorId], attackedReady);                                    
                            } else {
                                ret.Add(action.TargetActorId, attackedReady);
                            }
                            
                            //next에서 맞은거 처리
                            BattleActorAction attacked = new BattleActorAction(mCounter.GetNextCount(), from, to, BATTLE_ACTOR_ACTION_TYPE.ATTACKED, actorId);
                            attacked.AttackAmount = action.AttackAmount;
                            attacked.TargetActorId = actorId;

                            if(mNextAction.ContainsKey(action.TargetActorId)) {
                                mNextAction[action.TargetActorId] = GetBattleActionMerge(action.TargetActorId, mNextAction[action.TargetActorId], attacked);
                            } else {
                                mNextAction.Add(action.TargetActorId, attacked);
                            }
                            
                        }           
                    }                    
                    return ret;
                }
                private BattleActorAction? GetBattleActionMerge(string actorId, BattleActorAction preAction, BattleActorAction newAction) {
                    BATTLE_ACTOR_ACTION_TYPE[] readyType = {BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKED , BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKING};
                    BATTLE_ACTOR_ACTION_TYPE[] attackedType = {BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKED , BATTLE_ACTOR_ACTION_TYPE.ATTACKED};
                    BATTLE_ACTOR_ACTION_TYPE[] readyAttacked_attacking = {BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKED , BATTLE_ACTOR_ACTION_TYPE.ATTACKING};
                    BATTLE_ACTOR_ACTION_TYPE[] readyAttacking_attacked = {BATTLE_ACTOR_ACTION_TYPE.READY_ATTACKING , BATTLE_ACTOR_ACTION_TYPE.ATTACKED};
                    if(preAction.Type == newAction.Type) {
                        if(preAction.Type == BATTLE_ACTOR_ACTION_TYPE.ATTACKED) {
                            //여러명한테 맞을 수 있다. 최초 공격자로 하고 AttackAmount를 합한다.
                            BattleActorAction attacked = preAction.Clone();    
                            attacked.AttackAmount += newAction.AttackAmount; 
                            return attacked;
                        } else {
                            return newAction;
                        }
                    }
                    else if(preAction.Type != newAction.Type) {
                        if(readyType.Contains(preAction.Type) && readyType.Contains(newAction.Type)) {
                            return newAction; 
                        } else if(attackedType.Contains(preAction.Type) && attackedType.Contains(newAction.Type)) {                            
                            if(preAction.Type == BATTLE_ACTOR_ACTION_TYPE.ATTACKED) return preAction;
                            else return newAction;
                        } else if(readyAttacked_attacking.Contains(preAction.Type) && readyAttacked_attacking.Contains(newAction.Type)) {
                            if(preAction.Type == BATTLE_ACTOR_ACTION_TYPE.ATTACKING) return preAction;
                            else return newAction;
                        } else if(readyAttacking_attacked.Contains(preAction.Type) && readyAttacking_attacked.Contains(newAction.Type)) {
                            if(preAction.Type == BATTLE_ACTOR_ACTION_TYPE.ATTACKED) return preAction;
                            else return newAction;
                        }
                    }                    

                    return null; //다른 케이스를 찾기 위해 일부러 null리턴해서 에러발생         
                }
                public float GetHP(string actorId) {
                    return mBattleActor.GetHP(actorId);
                }
                public float GetHPRatio(string actorId) {
                    var actor = mBattleActor.GetBattleActor(actorId);
                    if(actor == null) {
                        return -1;
                    }
                    return GetHP(actorId) / actor.mAbility.HP;
                }
                //Attacked 적용
                public float Attacked(string actorId, BattleActorAction action) {
                    //공격대상에게 피해 적용
                    float remain = mBattleActor.DecreaseHP(actorId, action.AttackAmount);
                    if(remain == 0) {                                                                         
                        //actor삭제
                        var actor = mBattleActor.GetBattleActor(actorId); 
                        if(actor != null) {
                            //map에서 삭제
                            mMap.RemoveActor(actorId);
                            //BattleActorHandler 에서 삭제
                            mBattleActor.RemoveActor(actor);
                        }                        
                    }
                    return remain;                    
                }
                public List<string> GetReadyActors(Dictionary<string, BattleActorAction> next) {
                    List<string> ret = new List<string>();
                    var actorPositions = mMap.GetActorPositions();
                    foreach(var p in actorPositions) {                        
                        string actorId = p.Key;
                        string position = p.Value;
                        if(next.ContainsKey(actorId)) continue;

                        Int64 lastTime = mBattleActor.GetLastActTime(actorId);

                        if(lastTime == BattleActorHandler.INVALID_LAST_TIME || lastTime >= mCounter.GetCount()) {
                            continue;
                        }
                        var tile = mMap.GetBattleMapTile(position);
                        if(tile != null) {
                            if(tile.state == BATTLEMAPTILE_STATE.OCCUPIED) {                                
                                ret.Add(actorId);
                            }
                        }
                    }
                    //랜덤하게 섞는다.
                    var rnd = new Random();
                    var randomized = ret.OrderBy(item => rnd.Next());
                    List<string> retRandom = new List<string>();
                    foreach(string p in randomized) {
                        retRandom.Add(p);
                    }

                    return retRandom;
                }
                public BattleActorAction Act(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    string from = mMap.GetActorPosition(actorId);
                    BattleActorAction ret = new BattleActorAction(mCounter.GetCount(), from, from);
                    
                    //Move 할지 Attack할지 
                    switch(actor.mAbility.AttackStyle) {                        
                        case BattleActorAbility.ATTACK_STYLE.ATTACKER:                        
                        //공격할 상대 찾아 헤메는 스타일. Attacker는 무의미한 moving없음.
                        case BattleActorAbility.ATTACK_STYLE.DEFENDER:
                        {
                            ret = Attacking(actor, from);
                            if(ret.Type == BATTLE_ACTOR_ACTION_TYPE.NONE) {// 공격대상 없음 moving
                                ret = Moving(actor, from);
                            } 
                        }
                        break;
                        case BattleActorAbility.ATTACK_STYLE.MOVER:
                        {   
                            ret = Moving(actor, from);
                            if(ret.Type == BATTLE_ACTOR_ACTION_TYPE.NONE) {// 이동대상 없음 attacking
                                ret = Attacking(actor, from);
                            } 
                        }
                        break;
                    }

                    if(ret.Type > BATTLE_ACTOR_ACTION_TYPE.NONE) {                        
                        mBattleActor.SetLastActTime(actorId, mCounter.GetCount());
                    }
                        
                    
                    return ret;
                } 
                private BattleActorAction Attacking(BattleActor actor, string from) {
                    BattleActorAction ret = new BattleActorAction(mCounter.GetCount(), from, from);
                    ret.Type = BATTLE_ACTOR_ACTION_TYPE.NONE;
                    //공속 처리. 넣을것인가....
                    List<string> list = mMap.LookOut(actor);
                    if(list.Count() == 0) {
                        return ret;
                    }   
                    
                    string maxPos = "";
                    float max = 0;     
                    string targetActorId = "";

                    foreach(var to in list) {
                        var tile = mMap.GetBattleMapTile(to);
                        if(tile != null) {                            
                            var opponent = mBattleActor.GetBattleActor(tile.actorId);
                            if(opponent != null && opponent.mSide != actor.mSide) {
                                //공격 대상. 공격시 이득
                                float v = GetAttackEstimation(actor, from, to); 
                                if(v > max) {
                                    maxPos = to;
                                    max = v;
                                    targetActorId = tile.actorId;
                                }
                            }
                        }                        
                    }

                    if(maxPos.Length > 0) {
                        //mBattleActor.SetActionState(actor.mActor.mUniqueId, BATTLE_ACTOR_ACTION_TYPE.ATTACKING);
                        ret.Type = BATTLE_ACTOR_ACTION_TYPE.ATTACKING;
                        ret.TargetActorId = targetActorId;
                        ret.TargetPosition = maxPos;
                        ret.AttackAmount = actor.mAbility.AttackPower * actor.mAbility.AttackAccuracy;      

                        //mBattleActor.SetActionState(targetActorId, BATTLE_ACTOR_ACTION_TYPE.ATTACKED); //여러명한테서 맞을 수도 있다.
                    }
                    return ret;
                    
                }
                private BattleActorAction Moving(BattleActor actor, string from) {
                    BattleActorAction ret = new BattleActorAction(mCounter.GetCount(), from, from);
                    ret.Type = BATTLE_ACTOR_ACTION_TYPE.NONE;
                    List<string> list = Sight(actor);
                    if(list.Count() == 0) {
                        return ret;
                    }             
                    string maxPos = list[0];
                    float max = GetMoveEstimation(actor, from, list[0]);

                    //같은 패턴 반복을 막기 위해 
                    var rnd = new Random();
                    var randomized = list.OrderBy(item => rnd.Next());
                    foreach(var to in randomized) {
                        float v = GetMoveEstimation(actor, from, to); 
                        if(v > max) {
                            maxPos = to;
                            max = v;
                        }
                    }
                    if(from != maxPos) {
                        mMap.MoveTo(actor.mActor.mUniqueId, maxPos);
                        //mBattleActor.SetActionState(actor.mActor.mUniqueId, BATTLE_ACTOR_ACTION_TYPE.MOVING);
                        ret.Type = BATTLE_ACTOR_ACTION_TYPE.MOVING;                        
                        ret.TargetPosition = maxPos;
                    }
                    return ret;
                }
                //candidates of possible position
                private List<string> Sight(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    string currPos = mMap.GetActorPosition(actorId);
                    List<string> list = mMap.GetNearPostions(currPos, actor.mAbility.Sight);
                    //check occupied
                    List<string> ret = new List<string>();
                    foreach(string pos in list) {
                        var tile = mMap.GetBattleMapTile(pos);
                        switch(actor.mAbility.AttackStyle) {
                            case BattleActorAbility.ATTACK_STYLE.ATTACKER:
                            {
                                //1. 상대방을 찾고                                
                                if(tile != null && tile.state == BATTLEMAPTILE_STATE.OCCUPIED ) {
                                    var opponent = mBattleActor.GetBattleActor(tile.actorId);
                                    if(opponent != null && opponent.mSide != actor.mSide) {
                                        //2. 상대방 주변의 빈곳을 찾는다.
                                        List<string> nears = mMap.GetNearPositionsByState(pos, BATTLEMAPTILE_STATE.EMPTY);
                                        if(nears.Count > 0) {
                                            ret.Add(nears[0]); //첫번째. 봐서 나중에 랜덤하게 할지 고민.
                                        }
                                    }
                                }
                            }
                            break;
                            default: 
                            if(tile != null && tile.state == BATTLEMAPTILE_STATE.EMPTY) 
                               ret.Add(pos);                            
                            break;
                        }
                    }
                    ret.Add(currPos); //현재 위치 추가.
                    return ret;
                }
                public void Occupy(string actorId) {                                        
                    string position = mMap.GetActorPosition(actorId);
                    if(IsValidPosition(position)) {
                        mMap.Occupy(actorId, position);
                        //mBattleActor.SetActionState(actorId, BATTLE_ACTOR_ACTION_TYPE.NONE);
                    }                    
                }
                public bool Validate() {                    
                    return mMap.Validate();
                }
                private bool IsValidPosition(string position) {                    
                    if(position.Length == 0) return false;
                    return true;
                }    
                // ----------------------------------------------------------------------
                //Attack시 이득 계산. 
                //adv - disadv + cost
                private float GetAttackEstimation(BattleActor actor, string from, string to) {
                    var opponentTile = mMap.GetBattleMapTile(to);
                    if(opponentTile != null) {
                        var opponent = mBattleActor.GetBattleActor(opponentTile.actorId);
                        if(opponent != null) {
                            // 만만한 상대를 먼저. HP가 약한 상대를 먼저 때린다.
                            return actor.mAbility.AttackPower / opponent.mAbility.HP;
                        }
                    }
                    return -1;
                }

                // ----------------------------------------------------------------------
                //이동시 이득 계산. 
                //adv - disadv + cost
                private float GetMoveEstimation(BattleActor actor, string from, string to) {
                    var fromTile = mMap.GetBattleMapTile(from);
                    var toTile = mMap.GetBattleMapTile(to);

                    float ret = 0;
                    if(fromTile != null && toTile != null) {
                        switch(actor.mSide) {
                            case BATTLE_SIDE.HOME:
                            ret = toTile.advantage1 - fromTile.advantage1;
                            break;
                            case BATTLE_SIDE.AWAY:
                            ret = toTile.advantage2 - fromTile.advantage2;
                            break;
                        }
                    }

                    ret = ret + GetMoveCost(actor, from, to, actor.mSide);
                    return ret;
                }
                private float GetMoveCost(BattleActor actor, string from, string to, BATTLE_SIDE mySide) {
                    int[] fromInt = mMap.GetPositionInt(from);
                    int[] toInt = mMap.GetPositionInt(to);

                    float cost = 0;
                    
                    switch(actor.mSide) {
                        case BATTLE_SIDE.HOME:
                        {
                            if(actor.mAbility.MoveForward != 0) {
                                if(toInt[0] - fromInt[0] > 0) { //전방
                                    cost += actor.mAbility.MoveForward;
                                }
                            }
                            
                            if(actor.mAbility.MoveBack != 0) {
                                if(toInt[0] - fromInt[0] < 0) { //후방
                                    cost += actor.mAbility.MoveBack;
                                }
                            }
                        }
                        break;
                        case BATTLE_SIDE.AWAY:
                        {
                            if(actor.mAbility.MoveForward != 0) {
                                if(toInt[0] - fromInt[0] < 0) { //전방
                                    cost += actor.mAbility.MoveForward;
                                }
                            }
                            if(actor.mAbility.MoveBack != 0) {
                                if(toInt[0] - fromInt[0] > 0) { //후방
                                    cost += actor.mAbility.MoveBack;
                                }
                            }
                        }
                        break;
                    }
                    //라인 유지
                    if(actor.mAbility.MoveSide != 0) {
                        if(fromInt[0] == toInt[0] && fromInt[1] != toInt[1]) { 
                            cost += actor.mAbility.MoveSide;
                        }
                    }
                    //Console.WriteLine("{0} [{1}] > [{2}] : {3}", actor.mActor.mUniqueId,  from, to, cost);

                    return cost;
                }    
                public void Print() {
                    Console.WriteLine("-----------------------");
                    for(int y = 0; y < mMap.mHeight; y++) {
                        string sz = "";
                        for(int x = 0; x < mMap.mWidth; x ++) {
                            string pos = mMap.GetPositionString(x,y);
                            var tile = mMap.GetBattleMapTile(pos);
                            if(tile != null) {
                                float hp = mBattleActor.GetHP(tile.actorId);
                                if(tile != null) {                                
                                    sz += string.Format("[{0}-{1}] ", tile.actorId, hp);
                                }                            
                            } else {
                                sz += "[error] ";
                            }
                            
                        }
                        Console.WriteLine(sz);
                    }
                    Console.WriteLine("-----------------------");
                } 
            }
        }        
    }
}