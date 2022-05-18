using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ActorHandler {
                private static readonly Lazy<ActorHandler> instance =
                    new Lazy<ActorHandler>(() => new ActorHandler());
                
                //Actor name, Actor object
                private Dictionary<string, Actor> mDict = new Dictionary<string, Actor>();
                //type별 actor
                private Dictionary<int, Dictionary<string, Actor>> mDictType = new Dictionary<int, Dictionary<string, Actor>>();
                //type별 satisfaction 합
                private Dictionary<int, Dictionary<string, float>> mSatisfactionSums = new Dictionary<int, Dictionary<string, float>>();
            
                public static ActorHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private ActorHandler() {
                }
                // Quest list가 Null이면 받아온다.
                public Actor AddActor(int type, string uniqueId, int level, List<string>? questList) {
                    //quest
                    List<string> quests;
                    if(questList == null) {                        
                        quests = QuestHandler.Instance.GetQuestList(type);
                    } else {
                        quests = questList;
                    }
                    Actor a = new Actor(type, uniqueId, level, quests);
                    mDict.Add(uniqueId, a);
                    if(mDictType.ContainsKey(type) == false) {
                        mDictType[type] = new Dictionary<string, Actor>();
                    }
                    mDictType[type][uniqueId] = a;
                    
                    return a;
                }
                public Actor? GetActor(string uniqueId) {
                    if(mDict.ContainsKey(uniqueId) == true) {
                        return mDict[uniqueId];
                    }
                    return null;
                }

                public Dictionary<string, Actor>? GetActors(int type) {
                    if(mDictType.ContainsKey(type) == true) {
                        return mDictType[type];
                    }
                    return null;
                }

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