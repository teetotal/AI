using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class SoldierAbility {
                public float power;
                public int distance;
                public float speed;
                public float HP;
            }
            
            public class Soldier {
                private bool isHome;
                private int id;
                private MovingType mMovingType;
                private SoldierAbility mSoldierAbility;
                delegate Rating FnEstimation(List<Soldier> myTeam, List<Soldier> opponentTeam, Map map, Tactic tactic);
                delegate void FnAction(int id);
                private Dictionary<BehaviourType, FnAction> mDicFunc = new Dictionary<BehaviourType, FnAction>();
                private List<FnEstimation> mListEstimation = new List<FnEstimation>();

                public Soldier(bool isHome, int id, MovingType type, SoldierAbility ability) {
                    this.isHome = isHome;
                    this.id = id;

                    mMovingType = type;
                    mSoldierAbility = ability;

                    mDicFunc[BehaviourType.MOVE] = Move;
                    mDicFunc[BehaviourType.ATTACK] = Attack;
                    mDicFunc[BehaviourType.KEEP] = Keep;

                    mListEstimation.Add(GetRatingMove);
                    mListEstimation.Add(GetRatingAttack);
                    mListEstimation.Add(GetRatingKeep);
                }
                public Rating Update(List<Soldier> myTeam, List<Soldier> opponentTeam, Map map, Tactic tactic) {
                    
                    Rating maxRating = mListEstimation[0](myTeam, opponentTeam, map, tactic);

                    for(int i = 1; i < mListEstimation.Count; i++) {
                        Rating rating = mListEstimation[i](myTeam, opponentTeam, map, tactic);
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

                private Rating GetRatingMove(List<Soldier> myTeam, List<Soldier> opponentTeam, Map map, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.MOVE);

                    return rating;
                }
                private Rating GetRatingAttack(List<Soldier> myTeam, List<Soldier> opponentTeam, Map map, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.ATTACK);
                    
                    return rating;
                }
                private Rating GetRatingKeep(List<Soldier> myTeam, List<Soldier> opponentTeam, Map map, Tactic tactic) {
                    Rating rating = SetRating(BehaviourType.KEEP);
                    
                    return rating;
                }
                private void Move(int id) {

                }
                private void Attack(int id) {

                }
                private void Keep(int id) {

                }
            }
        }
    }
}