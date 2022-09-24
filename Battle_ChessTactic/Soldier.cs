using System;
using System.Collections.Generic;
using System.Linq;
using ENGINE;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class SoldierAbility {
                public int distance; //speed
                public float attackPower;
                public float attackRange; // 공격 범위
                public float HP;
            }
            
            public class Soldier {
                private bool isHome;
                private int id;
                private Position position;
                private Map map;
                private MOVING_TYPE mMovingType;
                private SoldierAbility mSoldierAbility;
                delegate Rating FnEstimation(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic);
                delegate void FnAction(int id);
                private Dictionary<BehaviourType, FnAction> mDicFunc = new Dictionary<BehaviourType, FnAction>();
                private List<FnEstimation> mListEstimation = new List<FnEstimation>();
                public Soldier(bool isHome, int id, MOVING_TYPE type, SoldierAbility ability, Position position, Map map) {
                    this.isHome = isHome;
                    this.id = id;
                    this.position = position;
                    this.map = map;

                    mMovingType = type;
                    mSoldierAbility = ability;

                    mDicFunc[BehaviourType.MOVE] = Move;
                    mDicFunc[BehaviourType.ATTACK] = Attack;
                    mDicFunc[BehaviourType.KEEP] = Keep;

                    mListEstimation.Add(GetRatingMove);
                    mListEstimation.Add(GetRatingAttack);
                    mListEstimation.Add(GetRatingKeep);
                }
                public Rating Update(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    
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
                public bool IsHome() {
                    return isHome;
                }
                public int GetID() {
                    return id;
                }
                public Position GetPosition() {
                    return position;
                }
                public Map GetMap() {
                    return map;
                }
                private Rating SetRating(BehaviourType type) {
                    Rating rating = RatingPool.Instance.GetPool().Alloc();
                    rating.isHome = isHome;
                    rating.soldierId = id;
                    rating.type = type;

                    return rating;
                }
                /* ==================================================
                    Move
                ===================================================== */
                //straight의 장애물 뒷편 좌표 체크
                private bool CheckUnMovablePositionStraight(Position pos, List<Position> obstacles) {
                    for(int i = 0; i < obstacles.Count; i++) {
                        Position obstacle = obstacles[i];
                        if(pos.x == obstacle.x && pos.y == obstacle.y)
                            return true;
                        //obstacle과 일직선 상에 있을경우
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
                private float GetMoveWeight(Position pos) {
                    //상하좌우에 몇개의 obstacle이 있는가
                    var obstacles = map.GetObstacles();
                    var ret = from obstacle in obstacles where pos.GetDistance(obstacle.position) <= mSoldierAbility.distance select obstacle;
                    
                    //0~1사이로 클수록 갈 이유가 없는 것
                    float uneasy = 0;
                    if(mMovingType == MOVING_TYPE.OVER) {

                    } else {
                        uneasy = ret.Count() / obstacles.Count;
                        //앞으로 가는것 보다 적 근처로 가는게 더 중요.
                        
                        if(isHome)
                            uneasy -= (pos.y - position.y) * 0.5f;
                        else 
                            uneasy -= (position.y - pos.y) * 0.5f;
                        
                        
                    }
                    //Console.WriteLine("W: {0}, {1}", pos.ToString(), weight);
                    
                    float weight = (1 - uneasy) * 0.5f; //max = 0.5
                    return weight;
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

                private Rating GetRatingMove(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.MOVE);
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetMovalbleList(position, mMovingType, mSoldierAbility.distance);
                    //obstacle
                    var obstacles = (from node in list where node.isObstacle select node.position).ToList();
                    IEnumerable<MapNode>? ret = null;
                    //장애물 뒷편은 제거
                    switch(mMovingType) {
                        case MOVING_TYPE.STRAIGHT: {
                            ret =   from node in list 
                                    where CheckUnMovablePositionStraight(node.position, obstacles) == false && node.isObstacle == false 
                                    orderby GetMoveWeight(node.position) descending
                                    select node;
                        }
                        break;
                        case MOVING_TYPE.CROSS: {
                            ret =   from node in list 
                                    where CheckUnMovablePositionCross(node.position, obstacles) == false && node.isObstacle == false 
                                    orderby GetMoveWeight(node.position) descending
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
                    Position pos = ret.First().position;
                    rating.rating = GetMoveWeight(pos);
                    rating.targetId = map.GetPositionId(pos);


                    return rating;
                }
                /* ==================================================
                    Attack
                ===================================================== */
                private bool IsThereEnemy(Position pos, List<Soldier> opponentTeam) {
                    for(int i = 0; i < opponentTeam.Count; i++) {
                        Position p = opponentTeam[i].GetPosition();
                        if(p.x == pos.x && p.y == pos.y) {
                            return true;
                        }
                    }
                    return false;
                }
                private Rating GetRatingAttack(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.ATTACK);
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetAttackableList(position, mSoldierAbility.attackRange);
                    //obstacle
                    var obstacles = (from node in list where node.isObstacle select node.position).ToList();
                    IEnumerable<MapNode>? ret = null;
                    //장애물 뒷편은 제거
                    ret =   from    node in list 
                            where   CheckUnMovablePositionStraight(node.position, obstacles) == false && 
                                    CheckUnMovablePositionCross(node.position, obstacles) == false &&
                                    node.isObstacle == false &&
                                    IsThereEnemy(node.position, opponentTeam)
                            select  node;
                    
                    if(ret == null || ret.Count() == 0) {
                        rating.rating = 0;
                        rating.targetId = -1;
                    } else {
                        rating.rating = 1.0f;
                        rating.targetId = map.GetPositionId(ret.First().position);
                    }
                            
                    return rating;
                }
                /* ==================================================
                    Keep
                ===================================================== */
                private Rating GetRatingKeep(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.KEEP);
                    rating.rating = 0.1f;
                    return rating;
                }
                private void Move(int id) {
                    position = map.GetPosition(id);
                }
                private void Attack(int id) {

                }
                private void Keep(int id) {

                }
            }
        }
    }
}