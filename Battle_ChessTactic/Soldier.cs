using System;
using System.Collections.Generic;
using System.Linq;
using ENGINE;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class Soldier {
                public class State {
                    public Soldier mSoldier;
                    public bool isDie = false;
                    public bool isHit = false; //명중
                    public bool isRetreat = false; //피신
                    public float damage = 0;
                    public float damagePre = 0; //이전에 받은 데미지.
                    public float attack = 0;
                    public State(Soldier soldier) {
                        mSoldier = soldier;
                    }
                    public void Reset() {
                        damagePre = damage;
                        isDie = false;
                        isHit = false;
                        isRetreat = false;
                        damage = 0;
                        attack = 0;
                    }
                }
                private State mState; //한 스텝마다의 상태 업데이트
                private ChessTactic_SoldierInfo mSoldierInfo;
                private ChessTactic_Info mTeamInfo;
                private Map map;
                private Battle mBattle = null;
                private float damage = 0;
                private BehaviourType preAction = BehaviourType.MAX; //이전에 했던 액션
                private bool mIsHold = false;
                private List<Plan> mPlanQueue = new List<Plan>();
                private const int PLAN_QUEUE_SIZE = 3;
                delegate Rating FnEstimation(List<Soldier> my, List<Soldier> enemy);
                delegate void FnAction(Rating rating);
                private Dictionary<BehaviourType, FnAction> mDicFunc = new Dictionary<BehaviourType, FnAction>();
                private List<FnEstimation> mListEstimation;
                private Random rand = new Random();
                public Soldier(ChessTactic_SoldierInfo soldier, ChessTactic_Info info, Map map, Battle battle) {
                    this.mSoldierInfo = soldier;
                    this.mTeamInfo = info;
                    this.mSoldierInfo.nextAvoidHP = this.mSoldierInfo.ability.avoidance;
                    this.map = map;
                    mBattle = battle;

                    mState = new State(this);
                    mDicFunc[BehaviourType.AVOIDANCE] = Avoidance;  // 1.2
                    mDicFunc[BehaviourType.RECOVERY] = Recovery;    // 1.1
                    mDicFunc[BehaviourType.ATTACK] = Attack;        // 1.0
                    mDicFunc[BehaviourType.MOVE] = Move;            // 0.9
                    mDicFunc[BehaviourType.KEEP] = Keep;            // 0.1

                    mListEstimation = new List<FnEstimation>() { GetRatingAvoidance, GetRatingRecovery, GetRatingAttack, GetRatingMove, GetRatingKeep };
                }
                public Rating Update(List<Soldier> my, List<Soldier> enemy) {
                    mState.Reset();
                    for(int i = 0; i < mListEstimation.Count; i++) {
                        Rating rating = mListEstimation[i](my, enemy);
                        if(rating.rating > 0) {
                            //plan queue. pooling
                            Plan p = new Plan();
                            p.Set(rating, this);
                            mPlanQueue.Add(p);
                            if(mPlanQueue.Count > PLAN_QUEUE_SIZE)
                                mPlanQueue.RemoveAt(0);

                            return rating;
                        }
                        else
                            RatingPool.Instance.GetPool().Release(rating);
                    }
                    throw new Exception("Estimation Failure");
                }
                public void Action(Rating rating) {
                    preAction = rating.type;
                    mDicFunc[rating.type](rating);
                    RatingPool.Instance.GetPool().Release(rating);
                }
                public ChessTactic_SoldierInfo GetInfo() {
                    return mSoldierInfo;
                }
                public State GetState() {
                    return mState;
                }
                public void SetDie() {
                    mState.Reset();
                }
                public float GetHP() {
                    return (mSoldierInfo.ability.HP - damage) / mSoldierInfo.ability.HP;
                }
                public bool IsDie() {
                    if(mSoldierInfo.ability.HP <= damage)
                        return true;
                    return false;
                }
                public int GetSide() {
                    return mSoldierInfo.side;
                }
                public int GetID() {
                    return mSoldierInfo.id;
                }
                public string GetName() {
                    return mSoldierInfo.name;
                }
                public Position GetPosition() {
                    return mSoldierInfo.position;
                }
                public SoldierAbility GetAbility() {
                    return mSoldierInfo.ability;
                }
                public Map GetMap() {
                    return map;
                }
                //이전 단계와 같은 위치인지 확인
                public bool IsEqualPreTargetPosition() {
                    Plan a = mPlanQueue.Last();
                    int index = mPlanQueue.Count -2;
                    if(index < 0)
                        return false;

                    Plan b = mPlanQueue[index];
                    return a.position.IsEqual(b.position);
                }

                // 이전 단계와 현재 비교
                // 현재 0, 전단계 1, 전전단계 2
                public bool IsEqualPreAction(int preStep, BehaviourType type, Position position) {
                    int index = mPlanQueue.Count -1;
                    index -= preStep;
                    if(index < 0)
                        return false;

                    Plan a = mPlanQueue[index];
                    if(a.type == type && a.position.IsEqual(position))
                        return true;
                    return false;
                }
                private Rating SetRating(BehaviourType type) {
                    Rating rating = RatingPool.Instance.GetPool().Alloc();
                    rating.side = this.mSoldierInfo.side;
                    rating.soldierId = this.mSoldierInfo.id;
                    rating.type = type;
                    rating.rating = 0;
                    rating.targetSide = rating.side;

                    return rating;
                }
                public bool IsHold() {
                    return mIsHold;
                }
                public void ToggleHold() {
                    mIsHold = !mIsHold;
                }
                /* ==================================================
                    Move
                ===================================================== */
                // Weight ------------------
                private float GetMoveWeight(Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //상하좌우에 몇개의 obstacle이 있는가
                    var obstacleList = map.GetObstacles();
                    var obstaclesNearby = from obstacle in obstacleList where pos.GetDistance(obstacle.position) <= mSoldierInfo.ability.movingDistance select obstacle;
                    int obstacleConcentration = obstacleList.Count == 0 ? 0 : obstaclesNearby.Count() / obstacleList.Count;

                    return GetMoveWeightDefault(obstacleConcentration, pos, myTeam, opponentTeam);
                    /*
                    float weight = 0;
                    switch(this.mSoldierInfo.movingType) {
                        case MOVING_TYPE.OVER:
                        break; 
                        default: //공격형
                        weight = GetMoveWeightDefault(obstacleConcentration, pos, myTeam, opponentTeam);
                        break;
                    }
                    return weight;
                    */
                }
                private float GetMoveWeightDefault(float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //반복 이동 제거
                    if(IsEqualPreAction(1, BehaviourType.MOVE, pos)) {
                        return 0;
                    }
                    List<MoveWeight.Fn> list = MoveWeight.Instance.GetFnDefault();
                    for(int i =0; i < list.Count; i++) {
                        float ret = list[i](this, obstacleConcentration, pos, myTeam, opponentTeam, i);
                        if(ret > 0) {
                            return ret;
                        }
                    }
                    return 0;
                }
                //straight의 장애물 뒷편 좌표 체크
                private bool CheckUnMovablePositionStraight(Position pos, List<Position> obstacles) {
                    for(int i = 0; i < obstacles.Count; i++) {
                        Position obstacle = obstacles[i];
                        if(pos.x == obstacle.x && pos.y == obstacle.y)
                            return true;
                        //obstacle과 일직선 상에 있을경우
                        Position position = mSoldierInfo.position; 
                        if(obstacle.x == position.x && pos.x == position.x && 
                            ((obstacle.y > position.y && pos.y > obstacle.y) || (obstacle.y < position.y && pos.y < obstacle.y) )) {
                            return true;
                        } else if(obstacle.y == position.y && pos.y == position.y &&
                                ((obstacle.x > position.x && pos.x > obstacle.x) || (obstacle.x < position.x && pos.x < obstacle.x) )) {
                            return true;
                        }
                    }
                    return false;
                }
                //cross
                private bool CheckUnMovablePositionCross(Position pos, List<Position> obstacles) {
                    for(int i = 0; i < obstacles.Count; i++) {
                        Position obstacle = obstacles[i];
                        if(pos.x == obstacle.x && pos.y == obstacle.y)
                            return true;
                        //obstacle과 기울기가 1인 관계에 있을경우
                        float gradient = MathF.Abs((obstacle.y - pos.y ) / (obstacle.x - pos.x));
                        //pos와 내 위치 기울기가 1일 경우 
                        Position position = mSoldierInfo.position; 
                        float gradient2 = MathF.Abs((pos.y - position.y ) / (pos.x - position.x));
                        if(gradient == 1 && gradient2 == 1) {
                            if(position.x < obstacle.x && obstacle.x < pos.x)
                                return true;
                            else if(position.x > obstacle.x && obstacle.x > pos.x)
                                return true;
                        } else if(gradient2 != 1) //기울기가 1이 아닌경우
                            return true;
                    }
                    return false;
                }
                private IEnumerable<MapNode>? GetMovableArea(bool isPlan = false) {
                    Position position = mSoldierInfo.position;
                    if(isPlan && mPlanQueue.Count > 0 && mPlanQueue.Last().type == BehaviourType.MOVE) {
                        position = map.GetPosition(mPlanQueue.Last().targetId);
                    }
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetMovalbleList(position, mSoldierInfo.movingType, mSoldierInfo.ability.movingDistance);
                    //obstacle
                    var obstacles = (from node in list where node.isObstacle select node.position).ToList();
                    IEnumerable<MapNode>? ret = null;
                    //장애물 뒷편은 제거
                    switch(mSoldierInfo.movingType) {
                        case MOVING_TYPE.OVER_STRAIGHT: 
                        case MOVING_TYPE.STRAIGHT: {
                            ret =   from node in list 
                                    where CheckUnMovablePositionStraight(node.position, obstacles) == false && node.isObstacle == false 
                                    select node;
                        }
                        break;
                        case MOVING_TYPE.OVER_CROSS:
                        case MOVING_TYPE.CROSS: {
                            ret =   from node in list 
                                    where CheckUnMovablePositionCross(node.position, obstacles) == false && node.isObstacle == false 
                                    select node;
                        }
                        break;
                    }
                    return ret;
                }
                public List<MapNode> GetMovableAreaList() {
                    //이동 중 일때, plan 기준으로 이동 가능한 영역 표시.
                    return GetMovableArea(true).ToList();
                }
                private Rating GetRatingMove(List<Soldier> my, List<Soldier> enemy) {
                    Rating rating = SetRating(BehaviourType.MOVE);
                    if(preAction == BehaviourType.AVOIDANCE || mIsHold) //이전 액션이 회피면 move하지 않는다. move하면 다시 상대에게 달려든다.
                        return rating;
                    var list = GetMovableArea();
                    if(list != null && list.Count() > 0) {
                        switch(mSoldierInfo.movingType) {
                            case MOVING_TYPE.OVER_STRAIGHT: 
                            case MOVING_TYPE.STRAIGHT: {
                                list = list.OrderByDescending(node=>GetMoveWeight(node.position, my, enemy));
                            }
                            break;
                            case MOVING_TYPE.OVER_CROSS: 
                            case MOVING_TYPE.CROSS: {
                                list = list.OrderByDescending(node=>GetMoveWeight(node.position, my, enemy));
                            }
                            break;
                        }
                        Position pos = list.First().position;
                        rating.rating = 0.9f;//weight값은 내부에서 경쟁할때만 의미 있음. GetMoveWeight(pos, myTeam.Values.ToList(), opponentTeam.Values.ToList());
                        rating.targetId = map.GetPositionId(pos);
                    }
                    return rating;
                }
                /* ==================================================
                    Attack
                ===================================================== */
                private bool IsThereEnemy(Position pos, List<Soldier> opponentTeam) {
                    for(int i = 0; i < opponentTeam.Count; i++) {
                        Soldier target = opponentTeam[i];
                        Position p = target.GetPosition();
                        if(p.x == pos.x && p.y == pos.y && !target.IsDie()) {
                            return true;
                        }
                    }
                    return false;
                }
                private Soldier GetEnemyId(Position pos, List<Soldier> opponentTeam) {
                    for(int i = 0; i < opponentTeam.Count; i++) {
                        Soldier target = opponentTeam[i];
                        Position p = target.GetPosition();
                        if(p.x == pos.x && p.y == pos.y && !target.IsDie()) {
                            return target;
                        }
                    }
                    throw new Exception("GetEnemyId Error");
                }
                private bool CheckUnAttackablePositionCross(Position pos, List<Position> obstacles) {
                    for(int i = 0; i < obstacles.Count; i++) {
                        Position obstacle = obstacles[i];
                        if(pos.x == obstacle.x && pos.y == obstacle.y)
                            return true;
                        //obstacle과 기울기가 1인 관계에 있을경우
                        float gradient = MathF.Abs((obstacle.y - pos.y ) / (obstacle.x - pos.x));
                        //pos와 내 위치 기울기가 1일 경우 
                        Position position = mSoldierInfo.position; 
                        float gradient2 = MathF.Abs((pos.y - position.y ) / (pos.x - position.x));
                        if(gradient == 1 && gradient2 == 1) {
                            if(position.x < obstacle.x && obstacle.x < pos.x)
                                return true;
                            else if(position.x > obstacle.x && obstacle.x > pos.x)
                                return true;
                        } 
                    }
                    return false;
                }
                private Rating GetRatingAttack(List<Soldier> my, List<Soldier> enemy) {
                    Rating rating = SetRating(BehaviourType.ATTACK);
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetAttackableList(mSoldierInfo.position, mSoldierInfo.ability.attackRange);
                    //obstacle
                    var obstacles = (from node in list where node.isObstacle select node.position).ToList();
                    IEnumerable<MapNode>? ret = null;
                    switch(mSoldierInfo.movingType) {
                        //장애물과 상관없이 공격 가능
                        case MOVING_TYPE.OVER_STRAIGHT:
                        case MOVING_TYPE.OVER_CROSS: {
                            ret =   from    node in list 
                                    where   node.isObstacle == false &&
                                            IsThereEnemy(node.position, enemy) 
                                    select  node;
                        }
                        break;
                        default: {
                            //장애물 뒷편은 제거
                            ret =   from    node in list 
                                    where   CheckUnMovablePositionStraight(node.position, obstacles) == false && 
                                            CheckUnAttackablePositionCross(node.position, obstacles) == false &&
                                            node.isObstacle == false &&
                                            IsThereEnemy(node.position, enemy) 
                                    select  node;
                        }
                        break;
                    }
                    
                    
                    if(ret == null || ret.Count() == 0) {
                        rating.rating = 0;
                        rating.targetId = -1;
                    } else {
                        rating.rating = 1.0f;
                        //solider id로 변경
                        Soldier target = GetEnemyId(ret.First().position, enemy);
                        rating.targetId = target.GetID();
                        rating.targetSide = target.GetSide();
                    }
                            
                    return rating;
                }
                /* ==================================================
                    Keep
                ===================================================== */
                private Rating GetRatingKeep(List<Soldier> my, List<Soldier> enemy) {
                    Rating rating = SetRating(BehaviourType.KEEP);
                    rating.rating = 0.1f;
                    return rating;
                }
                /* ==================================================
                    Recovery
                ===================================================== */
                private Rating GetRatingRecovery(List<Soldier> my, List<Soldier> enemy) {
                    Rating rating = SetRating(BehaviourType.RECOVERY);
                    if(GetHP() <= mSoldierInfo.nextFirstAidHP && mSoldierInfo.item.firstAid > 0) { 
                        rating.rating = 1.1f;
                    }
                    return rating;
                }
                /* ==================================================
                    Avoidance
                ===================================================== */
                private float GetAvoidanceWeight(Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //상하좌우에 몇개의 obstacle이 있는가
                    var obstacleList = map.GetObstacles();
                    var obstaclesNearby = from obstacle in obstacleList where pos.GetDistance(obstacle.position) <= mSoldierInfo.ability.movingDistance select obstacle;
                    int obstacleConcentration = obstacleList.Count == 0 ? 0 : obstaclesNearby.Count() / obstacleList.Count;

                    return AvoidanceWeight.Instance.GetWeightDefault(this, obstacleConcentration, pos, myTeam, opponentTeam);
                    /*
                    float weight = 0;
                    switch(this.mSoldierInfo.movingType) {
                        case MOVING_TYPE.OVER:
                        break; 
                        default:
                        weight =  AvoidanceWeight.Instance.GetWeightDefault(this, obstacleConcentration, pos, myTeam, opponentTeam);
                        break;
                    }
                    return weight;
                    */
                }
                private Rating GetRatingAvoidance(List<Soldier> my, List<Soldier> enemy) {
                    Rating rating = SetRating(BehaviourType.AVOIDANCE);
                    if(mIsHold)
                        return rating;
                        
                    float hp = GetHP();
                    if(mState.damagePre > 0 && hp <= mSoldierInfo.nextAvoidHP) {
                        var list = GetMovableArea();
                        if(list != null && list.Count() > 0) {
                            list = list.OrderByDescending(node=>GetAvoidanceWeight(node.position, my, enemy));
                            Position targetPos = list.First().position;
                            rating.targetId = map.GetPositionId(targetPos);
                            rating.rating = 1.2f;

                            if(hp < 0.1f) //hp가 10% 이하면 도망치지 않는다.
                                mSoldierInfo.nextAvoidHP = -1;
                            else {
                                mSoldierInfo.nextAvoidHP *= 0.5f; //다음번 회피 시점
                                mState.isRetreat = true;
                            }
                        }
                    }

                    return rating;
                }
                //---------------------------------------------------
                private void Avoidance(Rating rating) {
                    mSoldierInfo.position = map.GetPosition(rating.targetId);
                }
                private void Recovery(Rating rating) {
                    mSoldierInfo.item.firstAid--;
                    mSoldierInfo.nextFirstAidHP = GetHP() * 0.5f; //구급약 섭취 시점. 처음 HP의 반이 될때, 그다음은 섭취한 시점의 반이 될때 so on
                    damage = MathF.Max(0, damage - mSoldierInfo.item.firstAidEffect);
                }
                private void Move(Rating rating) {
                    mSoldierInfo.position = map.GetPosition(rating.targetId);
                }
                private void Attack(Rating rating) {
                    Soldier enemy = mBattle.GetSoldier(rating.targetSide, rating.targetId);
                    //사정거리 안에 있는지 확인
                    if(mSoldierInfo.position.GetDistance(enemy.GetPosition()) <= mSoldierInfo.ability.attackRange ){
                        float damage = 0;
                        int randVal = rand.Next(100);
                        if(randVal < mSoldierInfo.ability.attackAccuracy) {//명중률
                            damage = mSoldierInfo.ability.attackPower; 
                            mState.isHit = true;
                        }
                        else
                            damage = mSoldierInfo.ability.attackPowerMin;
                        mState.attack += damage;
                        enemy.UnderAttack(damage);
                    }
                }
                public void UnderAttack(float damage) {
                    mState.damage += damage;
                    this.damage += damage;
                    if(this.damage >= mSoldierInfo.ability.HP) {
                        mState.isDie = true;
                    }
                }
                private void Keep(Rating rating) { }
            }
        }
    }
}