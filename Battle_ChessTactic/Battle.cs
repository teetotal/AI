using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class Battle {
                Map mMap;
                Tactic homeTactic;
                Tactic awayTactic;
                List<Soldier> homeSoldiers = new List<Soldier>();
                List<Soldier> awaySoldiers = new List<Soldier>();
                List<Rating> mTemp = new List<Rating>();
                public Battle(Map map, List<Soldier> home, List<Soldier> away, Tactic homeTactic, Tactic awayTactic) {
                    this.mMap = map;
                    this.homeTactic = homeTactic;
                    this.awayTactic = awayTactic;
                    this.homeSoldiers = home;
                    this.awaySoldiers = away;
                }
                public bool IsFinish() {
                    return false;
                }
                public List<Rating> Update() {
                    mTemp.Clear();
                    for(int i = 0; i < homeSoldiers.Count; i++) {
                        mTemp.Add(homeSoldiers[i].Update(homeSoldiers, awaySoldiers, homeTactic));
                    }
                    for(int i = 0; i < awaySoldiers.Count; i++) {
                        mTemp.Add(awaySoldiers[i].Update(awaySoldiers, homeSoldiers, awayTactic));
                    }

                    return mTemp;
                }
                public void Action(Rating rating) {
                    int id = rating.soldierId;
                    if(rating.isHome) {
                        homeSoldiers[id].Action(rating);
                    } else {
                        awaySoldiers[id].Action(rating);
                    }
                }
                public Soldier GetSoldier(Rating rating) {
                    if(rating.isHome)
                        return homeSoldiers[rating.soldierId];
                    else
                        return awaySoldiers[rating.soldierId];
                }
            }
        }
    }
}