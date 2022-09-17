using System;
using System.Collections.Generic;
using System.Linq;
using ENGINE;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class SoldierAbility {
                public float power;
                public int distance; //speed
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
                private Rating SetRating(BehaviourType type) {
                    Rating rating = RatingPool.Instance.GetPool().Alloc();
                    rating.isHome = isHome;
                    rating.soldierId = id;
                    rating.type = type;

                    return rating;
                }

                private Rating GetRatingMove(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.MOVE);
                    //옮겨 갈수 있는 모든 영역
                    var list = map.GetMovalbleList(position, mMovingType, mSoldierAbility);
                    //do ...
                    var ret = list.OrderByDescending(e=> e.y);

                    rating.rating = 1.0f;
                    rating.targetId = map.GetPositionId(ret.First());


                    return rating;
                }
                private Rating GetRatingAttack(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.ATTACK);
                    
                    return rating;
                }
                private Rating GetRatingKeep(List<Soldier> myTeam, List<Soldier> opponentTeam, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.KEEP);
                    
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