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
                List<Soldier.State> mTempState = new List<Soldier.State>();
                public Battle(Map map, List<Soldier> home, List<Soldier> away, Tactic homeTactic, Tactic awayTactic) {
                    this.mMap = map;
                    this.homeTactic = homeTactic;
                    this.awayTactic = awayTactic;
                    for(int i = 0; i < home.Count; i++) {
                        home[i].SetBattle(this);
                        this.homeSoldiers.Add(home[i].GetID(), home[i]);
                    }
                        
                    for(int i = 0; i < away.Count; i++) {
                        away[i].SetBattle(this);
                        this.awaySoldiers.Add(away[i].GetID(), away[i]);
                    }       
                }
                public bool IsFinish() {
                    //전멸전. 
                    int home = 0;
                    int away = 0;
                    foreach(var p in homeSoldiers) {
                        if(!p.Value.IsDie()) home ++;
                    }
                    foreach(var p in awaySoldiers) {
                        if(!p.Value.IsDie()) away ++;
                    }
                    if(home == 0 || away == 0)
                        return true;
                    return false;
                }
                public List<Rating> Update() {
                    mTemp.Clear();
                    foreach(var p in homeSoldiers) {
                        if(!p.Value.IsDie())
                            mTemp.Add(p.Value.Update(homeSoldiers, awaySoldiers, homeTactic));
                    }
                    foreach(var p in awaySoldiers) {
                        if(!p.Value.IsDie())
                            mTemp.Add(p.Value.Update(awaySoldiers, homeSoldiers, awayTactic));
                    }

                    return mTemp;
                }
                public void ResetState() {
                    mTempState.Clear();
                }
                public void Action(Rating rating) {
                    int id = rating.soldierId;
                    if(rating.isHome) {
                        homeSoldiers[id].Action(rating);
                    } else {
                        awaySoldiers[id].Action(rating);
                    }
                }
                public List<Soldier.State> GetActionResult() {
                    foreach(var p in homeSoldiers) {
                        Soldier.State state = p.Value.GetState();
                        if(p.Value.IsDie() && !state.isDie)
                            continue;
                        mTempState.Add(p.Value.GetState());
                    }
                    foreach(var p in awaySoldiers) {
                        Soldier.State state = p.Value.GetState();
                        if(p.Value.IsDie() && !state.isDie)
                            continue;
                        mTempState.Add(p.Value.GetState());
                    }
                    return mTempState;
                }

                public Soldier GetSoldier(Rating rating) {
                    if(rating.isHome)
                        return homeSoldiers[rating.soldierId];
                    else
                        return awaySoldiers[rating.soldierId];
                }
                public Soldier GetSoldier(bool isHome, int id) {
                    if(isHome)
                        return homeSoldiers[id];
                    else 
                        return awaySoldiers[id];
                } 
            }
        }
    }
}