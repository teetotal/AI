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
                Dictionary<int, Soldier> homeSoldiers = new Dictionary<int, Soldier>();
                Dictionary<int, Soldier> awaySoldiers = new Dictionary<int, Soldier>();
                List<Rating> mTemp = new List<Rating>();
                public Battle(Map map, List<Soldier> home, List<Soldier> away, Tactic homeTactic, Tactic awayTactic) {
                    this.mMap = map;
                    this.homeTactic = homeTactic;
                    this.awayTactic = awayTactic;
                    for(int i = 0; i < home.Count; i++)
                        this.homeSoldiers.Add(home[i].GetID(), home[i]);
                    for(int i = 0; i < away.Count; i++)
                        this.awaySoldiers.Add(away[i].GetID(), away[i]);
                }
                public bool IsFinish() {
                    return false;
                }
                public List<Rating> Update() {
                    mTemp.Clear();
                    foreach(var p in homeSoldiers) {
                        mTemp.Add(p.Value.Update(homeSoldiers, awaySoldiers, homeTactic));
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