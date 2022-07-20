using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public delegate void FnVillageLevelUpCallback(string villageId, int level);
            //Village도 관리.
            public class TaxContext {
                //village, satisfactionId, value
                private Dictionary<string, Dictionary<string, float>> mTax = new Dictionary<string, Dictionary<string, float>>();
                //village
                private Dictionary<string, ConfigVillage_Detail> mVillage = new Dictionary<string, ConfigVillage_Detail>();
                private Dictionary<string, int> mVillageLevel = new Dictionary<string, int>();
                //last collection time
                private Dictionary<string, long> mLastCollection = new Dictionary<string, long>();
                private FnVillageLevelUpCallback? mFnVillageLevelUpCallback = null;
                public void SetCallback(FnVillageLevelUpCallback cb) {
                    mFnVillageLevelUpCallback = cb;
                }
                public ConfigVillage_Detail GetVillageInfo(string villageId) {
                    return mVillage[villageId];
                }
                public int GetVillageLevel(string villageId) {
                    return mVillageLevel[villageId];
                }
                public void SetVillageInfo(Dictionary<string, ConfigVillage_Detail> villageInfo) {
                    foreach(var p in villageInfo) {
                        mVillageLevel.Add(p.Key, p.Value.level.current);
                        mLastCollection.Add(p.Key, 0);

                        if(!mTax.ContainsKey(p.Key))
                            mTax[p.Key] = new Dictionary<string, float>();
                        foreach(var f in p.Value.finances) {
                            mTax[p.Key].Add(f.Key, f.Value);
                        }
                    }
                    mVillage = villageInfo;
                }
                public float GetLevelProgression(string villageId) {
                    ConfigVillage_Detail villageInfo = mVillage[villageId];
                    int villageLevel = mVillageLevel[villageId];

                    float sum = 0;
                    int cnt = 1;
                    if(villageInfo.level.threshold.ContainsKey(villageLevel.ToString())) {
                        var threshold = villageInfo.level.threshold[villageLevel.ToString()];
                        cnt = threshold.Count;
                        foreach(var sa in threshold) {
                            if(mTax[villageId].ContainsKey(sa.Key)) {
                                sum += MathF.Min(mTax[villageId][sa.Key] / sa.Value, 1.0f);
                            }
                        }
                    }
                    return sum / cnt;
                }
                public bool TaxCollection(Dictionary<string, Dictionary<string, Actor>> villageActor) {
                    long currentCount = CounterHandler.Instance.GetCount();

                    bool ret = false; 
                    foreach(var village in villageActor) {
                        string villageId = village.Key;
                        ConfigVillage_Detail villageInfo = mVillage[villageId];
                        int villageLevel = mVillageLevel[villageId];
                        //징수 duration 확인
                        if(currentCount - mLastCollection[villageId] < mVillage[villageId].collectionDuration) {
                            continue;
                        }

                        foreach(var actor in village.Value) {
                            var tax = mVillage[villageId].tax;
                            foreach(var satisfaction in actor.Value.GetSatisfactions()) {
                                if(tax.ContainsKey(satisfaction.Key) && satisfaction.Value.Value > 0) {
                                    float rate = tax[satisfaction.Key];
                                    int val = (int)(satisfaction.Value.Value * rate);
                                    
                                    actor.Value.ApplySatisfaction(satisfaction.Key, -val, 0, null, true);
                                    
                                    if(!mTax.ContainsKey(villageId))
                                        mTax[villageId] = new Dictionary<string, float>();
                                    
                                    if(mTax[villageId].ContainsKey(satisfaction.Key))
                                        mTax[villageId][satisfaction.Key] += val;
                                    else 
                                        mTax[villageId][satisfaction.Key] = val;
                                    
                                    ret = true;
                                }
                            }
                            actor.Value.CallCallback(Actor.LOOP_STATE.TAX_COLLECTION);
                        }
                        mLastCollection[villageId] = currentCount;
                        //check village levelup
                        if(villageInfo.level.threshold.ContainsKey(villageLevel.ToString())) {
                            var threshold = villageInfo.level.threshold[villageLevel.ToString()];
                            bool isLevelUp = true;
                            foreach(var sa in threshold) {
                                if(mTax[villageId].ContainsKey(sa.Key) == false || mTax[villageId][sa.Key] < sa.Value) {
                                    isLevelUp = false;
                                    break;
                                }
                            }
                            if(isLevelUp) {
                                foreach(var sa in threshold) {
                                    mTax[villageId][sa.Key] -= sa.Value;
                                }
                                mVillageLevel[villageId]++;
                                //callback
                                if(mFnVillageLevelUpCallback != null) {
                                    mFnVillageLevelUpCallback(villageId, mVillageLevel[villageId]);
                                }
                            }
                        }
                        
                    }
                    return ret;
                }
            }
            public class ActorHandler {
                private static readonly Lazy<ActorHandler> instance =
                    new Lazy<ActorHandler>(() => new ActorHandler());
                
                //Actor name, Actor object
                private Dictionary<string, Actor> mDict = new Dictionary<string, Actor>();
                //type별 actor
                private Dictionary<int, Dictionary<string, Actor>> mDictType = new Dictionary<int, Dictionary<string, Actor>>();
                //village별 actor
                private Dictionary<string, Dictionary<string, Actor>> mDictVillage = new Dictionary<string, Dictionary<string, Actor>>();
                private TaxContext mTaxContext = new TaxContext();    
            
                public static ActorHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private ActorHandler() {
                }
                // Quest list가 Null이면 받아온다.
                public Actor AddActor(string actorId, ConfigActor_Detail info, List<string>? questList) {
                    //quest
                    List<string> quests;
                    if(questList == null) {                        
                        quests = QuestHandler.Instance.GetQuestList(info.type);
                    } else {
                        quests = questList;
                    }
                    Actor a = new Actor(actorId, info, quests);
                    mDict.Add(actorId, a);
                    if(mDictType.ContainsKey(info.type) == false) {
                        mDictType[info.type] = new Dictionary<string, Actor>();
                    }
                    mDictType[info.type][actorId] = a;
                    //village
                    if(a.mInfo.village != string.Empty) {
                        if(!mDictVillage.ContainsKey(a.mInfo.village)) {
                            mDictVillage[a.mInfo.village] = new Dictionary<string, Actor>();
                        }
                        mDictVillage[a.mInfo.village].Add(actorId, a);
                    }
                    return a;
                }
                public void PostInit(FnVillageLevelUpCallback cb) {
                    mTaxContext.SetCallback(cb);
                    //Actor instance가 모두 생성되고 난 후 Pets 설정
                    foreach(var p in mDict) {
                        p.Value.SetPets();
                    }
                }
                public void SetVillageInfo(Dictionary<string, ConfigVillage_Detail> villageInfo) {
                    mTaxContext.SetVillageInfo(villageInfo);
                }
                public Actor? GetActor(string uniqueId) {
                    if(mDict.ContainsKey(uniqueId) == true) {
                        return mDict[uniqueId];
                    }
                    return null;
                }
                public Dictionary<string, Actor> GetActors() {                    
                    if(mDict == null)
                        return new Dictionary<string, Actor>();
                    return mDict;
                }

                public Dictionary<string, Actor>? GetActors(int type) {
                    if(mDictType.ContainsKey(type) == true) {
                        return mDictType[type];
                    }
                    return null;
                }
                public bool TaxCollection() {
                    return mTaxContext.TaxCollection(mDictVillage);
                }
                public ConfigVillage_Detail GetVillageInfo(string villageId) {
                    return mTaxContext.GetVillageInfo(villageId);
                }
                public int GetVillageLevel(string villageId) {
                    return mTaxContext.GetVillageLevel(villageId);
                }
                public float GetVillageProgression(string villageId) {
                    return mTaxContext.GetLevelProgression(villageId);
                }
            }
        }
    }
}