using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class HappeningInfo {
                public ConfigSatisfaction_Happening? Info { get; set; }
                public int Counter { get; set; }
                public Int64 LastTime { get; set; }
                public string? SatisfactionId { get; set; }
            }
            public class HappeningHandler {
                private Dictionary<int, HappeningInfo> mHappeningTable = new Dictionary<int, HappeningInfo>();
                //type id, satisfaction id, happening id
                private Dictionary<int, Dictionary<string, List<int>>> mDict = new Dictionary<int, Dictionary<string, List<int>>>();
                private static readonly Lazy<HappeningHandler> instance =
                        new Lazy<HappeningHandler>(() => new HappeningHandler());
                public static HappeningHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private HappeningHandler() { }

                public void Add(int type, string satisfactionId, ConfigSatisfaction_Happening info) {
                    int happeningId = info.id;
                    if(mHappeningTable.ContainsKey(happeningId) == false) {
                        HappeningInfo p = new HappeningInfo();
                        p.Info = info;        
                        p.SatisfactionId = satisfactionId;
                        mHappeningTable.Add(happeningId, p);
                    }
                    //type
                    if(mDict.ContainsKey(type) == false) {
                        mDict[type] = new Dictionary<string, List<int>>();                                                
                    }
                    if(mDict[type].ContainsKey(satisfactionId) == false) {
                        mDict[type][satisfactionId] = new List<int>();
                    }

                    mDict[type][satisfactionId].Add(happeningId);
                    
                }
                public bool Do(int type, int happeningId) {                    
                    if(mHappeningTable.ContainsKey(happeningId) == false) {
                        return false;
                    }

                    HappeningInfo info = mHappeningTable[happeningId];
                    var actors = ActorHandler.Instance.GetActors(type);
                    if(actors is null || info.Info is null || info.SatisfactionId is null) {
                        return false;
                    }

                    Int64 counter = CounterHandler.Instance.GetCount();
                    if(mHappeningTable[happeningId].LastTime >= counter) {
                        return false;
                    }

                    foreach(var actor in actors) {
                        actor.Value.ApplySatisfaction(info.SatisfactionId, info.Info.amount, info.Info.measure, null);
                    }
                    mHappeningTable[happeningId].LastTime = counter;
                    mHappeningTable[happeningId].Counter ++;

                    return true;
                }

                public List<HappeningInfo> GetHappeningCandidates(int type) {
                    List<HappeningInfo> ret = new List<HappeningInfo>();
                    var sums = ActorHandler.Instance.GetSatisfactionSum(type);
                    if(sums != null && mDict.ContainsKey(type) == true) {
                        var dict = mDict[type];
                        foreach(var sum in sums) {
                            string satisfactionId = sum.Key;
                            float value = sum.Value;

                            //check happening
                            if(dict.ContainsKey(satisfactionId) == true) {
                                var list = dict[satisfactionId];
                                foreach(var happeningId in list) {
                                    if(mHappeningTable.ContainsKey(happeningId) == false) {
                                        Console.WriteLine("Error");
                                        return ret;
                                    }
                                    var info = mHappeningTable[happeningId];
                                    //check range
                                    if(info.Info != null && info.Info.range1 <= value && info.Info.range2 >= value)
                                    {
                                        ret.Add(info);
                                    }
                                }                                
                            }
                        }
                    }

                    return ret;
                }
                public void PrintCandidates(List<HappeningInfo> list) {                    
                    Console.WriteLine("Happening Candidates");
                    foreach(var p in list) {
                        if(p.Info is null || p.SatisfactionId is null) {
                            continue;
                        }
                        string satisfactionName = SatisfactionDefine.Instance.GetTitle(p.SatisfactionId);
                        string measure = "";
                        if(p.Info.measure == 1) {
                            measure = "%";
                        }
                        Console.WriteLine(" > {0}: {1} {2}({3}) {4}{5} Range({6}~{7})", p.Info.id 
                        , satisfactionName                        
                        , p.Info.title
                        , p.Info.desc
                        , p.Info.amount
                        , measure
                        , p.Info.range1
                        , p.Info.range2
                        );
                    }
                }
            }
        }
    }
}