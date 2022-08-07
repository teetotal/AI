using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class Satisfaction {
                public Satisfaction(string satisfactionId, float min, float max, float value) {
                    this.SatisfactionId = satisfactionId;        
                    this.Min = min;
                    this.Max = max;
                    this.Value = value;
                }
                public string SatisfactionId{ get; set; }
                public float Min { get; set; }
                public float Max { get; set; }
                public float Value { get; set; }
            }       
            public class SatisfactionDefine {
                private Dictionary<string, ConfigSatisfaction_Define> mDefines = new Dictionary<string, ConfigSatisfaction_Define>();
                private static readonly Lazy<SatisfactionDefine> instance =
                        new Lazy<SatisfactionDefine>(() => new SatisfactionDefine());
                public static SatisfactionDefine Instance {
                    get {
                        return instance.Value;
                    }
                }

                private SatisfactionDefine() { }
                public void Add(string satisfactionId, ConfigSatisfaction_Define p) {
                    mDefines.Add(satisfactionId, p);
                }
                public ConfigSatisfaction_Define Get(string satisfactionId) {
                    if(mDefines.ContainsKey(satisfactionId) == false) {
                        throw new Exception("Invalid Satisfaction id. " + satisfactionId);
                    }
                    return mDefines[satisfactionId];
                }
                public string GetTitle(string satisfactionId) {
                    var p = Get(satisfactionId);
                    if(p is null || p.title is null) {
                        return "";
                    }
                    return p.title;
                
                }
            }   
            //market price
            public class SatisfactionMarketPrice {
                private Dictionary<string, float> mDicQuantity = new Dictionary<string, float>();
                private static readonly Lazy<SatisfactionMarketPrice> instance =
                        new Lazy<SatisfactionMarketPrice>(() => new SatisfactionMarketPrice());
                public static SatisfactionMarketPrice Instance {
                    get {
                        return instance.Value;
                    }
                }
                private SatisfactionMarketPrice() { }
                private const int duration = 10;
                private long lastUpdateCount = 0;
                
                public bool Update() {
                    long count = CounterHandler.Instance.GetCount();
                    if(lastUpdateCount > 0 && count - lastUpdateCount < duration) {
                        return false;
                    }
                    //quantity
                    mDicQuantity.Clear();
                    
                    foreach(var actor in ActorHandler.Instance.GetActors()) {
                        foreach(var s in actor.Value.GetSatisfactions()) {
                            string id = s.Value.SatisfactionId;
                            if(SatisfactionDefine.Instance.Get(id).resource) {
                                if(mDicQuantity.ContainsKey(id)) {
                                    mDicQuantity[id] += s.Value.Value;
                                } else {
                                    mDicQuantity.Add(id, s.Value.Value);
                                }
                            }
                        }
                    }
                    lastUpdateCount = count;
                    return true;
                }
                public float GetTotalQuantity(string satisfactionId) {
                    if(!mDicQuantity.ContainsKey(satisfactionId))
                        return 0;
                    
                    return mDicQuantity[satisfactionId];
                }
                public float GetMarketPrice(string satisfactionId) {
                    float quantity = GetTotalQuantity(satisfactionId);
                    var marketPrice = SatisfactionDefine.Instance.Get(satisfactionId).marketPrice;
                    if(marketPrice == null)
                        return -1;
                    return (marketPrice.gradient * quantity) + marketPrice.bias;
                }
            }
        }        
    }
}