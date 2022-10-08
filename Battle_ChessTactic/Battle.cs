using System.Linq;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class Battle {
                Map mMap;
                Dictionary<int, Dictionary<int, Soldier>> soldiers = new Dictionary<int, Dictionary<int, Soldier>>();
                Dictionary<int, List<Soldier>> enemies = new Dictionary<int, List<Soldier>>();
                Dictionary<int, ChessTactic_Info> infos = new Dictionary<int, ChessTactic_Info>();
                /*
                Tactic homeTactic;
                Tactic awayTactic;
                
                Dictionary<int, Soldier> homeSoldiers = new Dictionary<int, Soldier>();
                Dictionary<int, Soldier> awaySoldiers = new Dictionary<int, Soldier>();
                */
                List<Rating> mTemp = new List<Rating>();
                List<Soldier.State> mTempState = new List<Soldier.State>();
                public Battle(Map map, Dictionary<string, ChessTactic> config) 
                {
                    this.mMap = map;
                    foreach(var p in config) {
                        int side = int.Parse(p.Key);
                        //init enemy
                        enemies.Add(side, new List<Soldier>());
                        //info
                        infos.Add(side, p.Value.info);
                        //soldier
                        var s = p.Value.soldiers;
                        for(int  i = 0; i < s.Count; i++) {
                            if(!soldiers.ContainsKey(side)) {
                                soldiers.Add(side, new Dictionary<int, Soldier>());
                            }
                            Soldier soldier = new Soldier(s[i], p.Value.info, map, this);
                            soldiers[side].Add(s[i].id, soldier);
                        }
                    }
                    foreach(int side in infos.Keys) {
                        foreach(var s in soldiers) {
                            if(s.Key == side)
                                continue;

                            foreach(var soldier in s.Value) {
                                enemies[side].Add(soldier.Value);
                            }
                        }
                    }
                    /*
                    
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
                    */
                }
                public Dictionary<int, Dictionary<int, Soldier>> GetSoldiers() {
                    return soldiers;
                }
                public Dictionary<int, Soldier> GetSoldiers(int side) {
                    return soldiers[side];
                    /*
                    if(isHome)
                        return homeSoldiers;
                    else 
                        return awaySoldiers;
                    */
                }
                public List<Soldier> GetEnemies(int side) {
                    return enemies[side];
                }
                public ChessTactic_Info GetTactic(int side) {
                    return infos[side];
                    /*
                    if(isHome)
                        return homeTactic;
                    else
                        return awayTactic;
                    */
                }
                public bool IsFinish() {
                    //전멸전. 
                    int remainSide = 0;
                    foreach(var side in soldiers) {
                        foreach(var soldier in side.Value) {
                            if(!soldier.Value.IsDie()) {
                                remainSide++;
                                break;
                            }
                        }
                    }
                    if(remainSide == 1)
                        return true;
                    
                    return false;
                    /*
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
                    */
                }
                public List<Rating> Update() {
                    mTemp.Clear();
                    foreach(var side in soldiers) {
                        foreach(var soldier in side.Value) {
                            if(!soldier.Value.IsDie())
                                mTemp.Add(soldier.Value.Update(soldiers[side.Key].Values.ToList(), enemies[side.Key]));
                        }
                    }
                    /*
                    foreach(var p in homeSoldiers) {
                        if(!p.Value.IsDie())
                            mTemp.Add(p.Value.Update(homeSoldiers, awaySoldiers, homeTactic));
                    }
                    foreach(var p in awaySoldiers) {
                        if(!p.Value.IsDie())
                            mTemp.Add(p.Value.Update(awaySoldiers, homeSoldiers, awayTactic));
                    }
                    */
                    return mTemp;
                }
                public void ResetState() {
                    mTempState.Clear();
                }
                public void Action(Rating rating) {
                    soldiers[rating.side][rating.soldierId].Action(rating);
                    /*
                    int id = rating.soldierId;
                    if(rating.isHome) {
                        homeSoldiers[id].Action(rating);
                    } else {
                        awaySoldiers[id].Action(rating);
                    }
                    */
                }
                public List<Soldier.State> GetActionResult() {
                    foreach(var side in soldiers) {
                        foreach(var soldier in side.Value) {
                            Soldier.State state = soldier.Value.GetState();
                            if(soldier.Value.IsDie() && !state.isDie)
                                continue;
                            mTempState.Add(state);
                        }
                    }
                    return mTempState;
                    /*
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
                    */
                }

                public Soldier GetSoldier(Rating rating) {
                    return soldiers[rating.side][rating.soldierId];
                    /*
                    if(rating.isHome)
                        return homeSoldiers[rating.soldierId];
                    else
                        return awaySoldiers[rating.soldierId];
                    */
                }
                public Soldier GetSoldier(int side, int id) {
                    return soldiers[side][id];
                    /*
                    if(isHome)
                        return homeSoldiers[id];
                    else 
                        return awaySoldiers[id];
                    */
                } 
            }
        }
    }
}