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
                        damage = 0;
                        attack = 0;
                    }
                }
                private State mState; //한 스텝마다의 상태 업데이트
                private SoldierInfo mSoldierInfo;
                private Map map;
                private Battle mBattle = null;
                private float damage = 0;
                delegate Rating FnEstimation(Dictionary<int, Soldier> myTeam, Dictionary<int, Soldier> opponentTeam, Tactic tactic);
                delegate void FnAction(int id);
                private Dictionary<BehaviourType, FnAction> mDicFunc = new Dictionary<BehaviourType, FnAction>();
                private List<FnEstimation> mListEstimation;
                private Random rand = new Random();
                public Soldier(SoldierInfo info, Map map, bool isHome) {
                    this.mSoldierInfo = info;
                    this.mSoldierInfo.isHome = isHome;
                    this.map = map;

                    mState = new State(this);
                    mDicFunc[BehaviourType.RECOVERY] = Recovery;
                    mDicFunc[BehaviourType.MOVE] = Move;
                    mDicFunc[BehaviourType.ATTACK] = Attack;
                    mDicFunc[BehaviourType.KEEP] = Keep;

                    mListEstimation = new List<FnEstimation>() { GetRatingRecovery, GetRatingMove, GetRatingAttack, GetRatingKeep };
                }
                public void SetBattle(Battle battle) {
                    mBattle = battle;
                }
                public Rating Update(Dictionary<int, Soldier> myTeam, Dictionary<int, Soldier> opponentTeam, Tactic tactic) {
                    mState.Reset();
                    Rating maxRating = mListEstimation[0](myTeam, opponentTeam, tactic);

                    for(int i = 1; i < mListEstimation.Count; i++) {
                        Rating rating = mListEstimation[i](myTeam, opponentTeam, tactic);
                        if(maxRating.rating < rating.rating) {
                            RatingPool.Instance.GetPool().Release(maxRating);
                            maxRating = rating;
                        } else {
                            RatingPool.Instance.GetPool().Release(rating);
                        }
                    }
                    return maxRating;
                }
                public void Action(Rating rating) {
                    mDicFunc[rating.type](rating.targetId);
                    RatingPool.Instance.GetPool().Release(rating);
                }
                public SoldierInfo GetInfo() {
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
                public bool IsHome() {
                    return mSoldierInfo.isHome;
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
                private Rating SetRating(BehaviourType type) {
                    Rating rating = RatingPool.Instance.GetPool().Alloc();
                    rating.isHome = this.mSoldierInfo.isHome;
                    rating.soldierId = this.mSoldierInfo.id;
                    rating.type = type;
                    rating.rating = 0;

                    return rating;
                }
                /* ==================================================
                    Move
                ===================================================== */
                // Weight ------------------
                private float GetMoveWeight(Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
                    //상하좌우에 몇개의 obstacle이 있는가
                    var obstacleList = map.GetObstacles();
                    var obstaclesNearby = from obstacle in obstacleList where pos.GetDistance(obstacle.position) <= mSoldierInfo.ability.movingDistance select obstacle;
                    int obstacleConcentration = obstaclesNearby.Count() / obstacleList.Count;
                    
                    float weight = 0;
                    switch(this.mSoldierInfo.movingType) {
                        case MOVING_TYPE.OVER:
                        break; 
                        default: //공격형
                        weight = GetMoveWeightDefault(obstacleConcentration, pos, myTeam, opponentTeam);
                        break;
                    }
                    return weight;
                }
                private float GetMoveWeightDefault(float obstacleConcentration, Position pos, List<Soldier> myTeam, List<Soldier> opponentTeam) {
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
                        }
                    }
                    return false;
                }

                private Rating GetRatingMove(Dictionary<int, Soldier> myTeam, Dictionary<int, Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.MOVE);
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetMovalbleList(mSoldierInfo.isHome, mSoldierInfo.position, mSoldierInfo.movingType, mSoldierInfo.ability.movingDistance);
                    //obstacle
                    var obstacles = (from node in list where node.isObstacle select node.position).ToList();
                    IEnumerable<MapNode>? ret = null;
                    //장애물 뒷편은 제거
                    switch(mSoldierInfo.movingType) {
                        case MOVING_TYPE.FORWARD: 
                        case MOVING_TYPE.STRAIGHT: {
                            ret =   from node in list 
                                    where CheckUnMovablePositionStraight(node.position, obstacles) == false && node.isObstacle == false 
                                    orderby GetMoveWeight(node.position, myTeam.Values.ToList(), opponentTeam.Values.ToList()) descending
                                    select node;
                        }
                        break;
                        case MOVING_TYPE.CROSS: {
                            ret =   from node in list 
                                    where CheckUnMovablePositionCross(node.position, obstacles) == false && node.isObstacle == false 
                                    orderby GetMoveWeight(node.position, myTeam.Values.ToList(), opponentTeam.Values.ToList()) descending
                                    select node;
                        }
                        break;
                    }
                    /*
                    var temp = ret.ToList();
                    for(int i = 0; i < temp.Count; i++) {
                        Console.WriteLine("{0}", temp[i].position.ToString());
                    }
                    */

                    //do ...
                    if(ret != null && ret.Count() > 0) {
                        Position pos = ret.First().position;
                        rating.rating = GetMoveWeight(pos, myTeam.Values.ToList(), opponentTeam.Values.ToList());
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
                private int GetEnemyId(Position pos, List<Soldier> opponentTeam) {
                    for(int i = 0; i < opponentTeam.Count; i++) {
                        Soldier target = opponentTeam[i];
                        Position p = target.GetPosition();
                        if(p.x == pos.x && p.y == pos.y && !target.IsDie()) {
                            return opponentTeam[i].GetID();
                        }
                    }
                    return -1;
                }
                public bool IsRetreat() {
                    if(mState.damagePre > 0 && GetHP() <= mSoldierInfo.ability.avoidance) {
                        return true;
                    }
                    return false;
                }
                private Rating GetRatingAttack(Dictionary<int, Soldier> myTeam, Dictionary<int, Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.ATTACK);
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetAttackableList(mSoldierInfo.position, mSoldierInfo.ability.attackRange);
                    //obstacle
                    var obstacles = (from node in list where node.isObstacle select node.position).ToList();
                    IEnumerable<MapNode>? ret = null;
                    //장애물 뒷편은 제거
                    ret =   from    node in list 
                            where   CheckUnMovablePositionStraight(node.position, obstacles) == false && 
                                    CheckUnMovablePositionCross(node.position, obstacles) == false &&
                                    node.isObstacle == false &&
                                    IsThereEnemy(node.position, opponentTeam.Values.ToList()) //누구를 먼저 공격할 것인가
                            select  node;
                    
                    if(ret == null || ret.Count() == 0 || IsRetreat()) {
                        rating.rating = 0;
                        rating.targetId = -1;
                    } else {
                        rating.rating = 1.0f;
                        //solider id로 변경
                        rating.targetId = GetEnemyId(ret.First().position, opponentTeam.Values.ToList());
                    }
                            
                    return rating;
                }
                /* ==================================================
                    Keep
                ===================================================== */
                private Rating GetRatingKeep(Dictionary<int, Soldier> myTeam, Dictionary<int, Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.KEEP);
                    rating.rating = 0.1f;
                    return rating;
                }
                /* ==================================================
                    Recovery
                ===================================================== */
                private Rating GetRatingRecovery(Dictionary<int, Soldier> myTeam, Dictionary<int, Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.RECOVERY);
                    if(GetHP() <= mSoldierInfo.nextFirstAidHP && mSoldierInfo.item.firstAid > 0) { 
                        rating.rating = 1.1f;
                    }
                    return rating;
                }
                //---------------------------------------------------
                private void Recovery(int id) {
                    mSoldierInfo.item.firstAid--;
                    mSoldierInfo.nextFirstAidHP = GetHP() * 0.5f; //구급약 섭취 시점. 처음 HP의 반이 될때, 그다음은 섭취한 시점의 반이 될때 so on
                    damage = MathF.Max(0, damage - mSoldierInfo.item.firstAidEffect);
                }
                private void Move(int id) {
                    mSoldierInfo.position = map.GetPosition(id);
                }
                private void Attack(int id) {
                    Soldier enemy = mBattle.GetSoldier(!mSoldierInfo.isHome, id);
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
                private void Keep(int id) {

                }
            }
        }
    }
}