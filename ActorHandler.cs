using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            //Village도 관리.
            public class ActorHandler {
                private static readonly Lazy<ActorHandler> instance =
                    new Lazy<ActorHandler>(() => new ActorHandler());
                
                //Actor name, Actor object
                private Dictionary<string, Actor> mDict = new Dictionary<string, Actor>();
                //type별 actor
                private Dictionary<int, Dictionary<string, Actor>> mDictType = new Dictionary<int, Dictionary<string, Actor>>();
                //village별 actor
                private Dictionary<string, Dictionary<string, Actor>> mDictVillage = new Dictionary<string, Dictionary<string, Actor>>();
                //type별 satisfaction 합
                private Dictionary<int, Dictionary<string, float>> mSatisfactionSums = new Dictionary<int, Dictionary<string, float>>();

                //village
                Dictionary<string, ConfigVillage_Detail> mVillage = new Dictionary<string, ConfigVillage_Detail>();
            
                public static ActorHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private ActorHandler() {
                }
                // Quest list가 Null이면 받아온다.
                public Actor AddActor(string actorId, ConfigActors_Detail info, List<string>? questList) {
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
                //Actor instance가 모두 생성되고 난 후 Pets 설정
                public void SetPets() {
                    foreach(var p in mDict) {
                        p.Value.SetPets();
                    }
                }
                public void SetVillageInfo(Dictionary<string, ConfigVillage_Detail> p) {
                    mVillage = p;
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
                //마을별 합산으로 변경. happening 과 엮여 있다. 암튼 이거 해야함!
                public void UpdateSatisfactionSum() {
                    mSatisfactionSums.Clear();

                    foreach(var pType in mDictType) {
                        int type = pType.Key;
                        foreach(var pActor in pType.Value) {
                            Actor actor = pActor.Value;
                            var satisfactions = actor.GetSatisfactions();
                            foreach(var pSatisfaction in satisfactions) {
                                string satisfactionId = pSatisfaction.Key;
                                var satisfaction = pSatisfaction.Value;

                                if(mSatisfactionSums.ContainsKey(type) == false) {
                                    mSatisfactionSums.Add(type, new Dictionary<string, float>());
                                }

                                if(mSatisfactionSums[type].ContainsKey(satisfactionId) == false) {
                                    mSatisfactionSums[type].Add(satisfactionId, satisfaction.Value);
                                } else {
                                    mSatisfactionSums[type][satisfactionId] += satisfaction.Value;
                                }                                
                            }
                        }
                    }
                }
                public Dictionary<string, float>? GetSatisfactionSum(int type) {
                    if(mSatisfactionSums.ContainsKey(type) == false) {
                        return null;
                    }
                    return mSatisfactionSums[type];
                }
                public void PrintSatisfactionSum(int type) {
                    var p = GetSatisfactionSum(type);
                    if(p == null) {
                        Console.WriteLine("Invalid type");
                        return;
                    }
                    Console.WriteLine("Sum Type: {0}", type);
                    foreach(var s in p) {
                        Console.WriteLine(" > {0}: {1}", SatisfactionDefine.Instance.GetTitle(s.Key), s.Value);
                    }
                }
            }
        }
    }
}