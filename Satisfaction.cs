namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class Satisfaction {
                public Satisfaction(int satisfactionId, float min, float max, float value) {
                    this.SatisfactionId = satisfactionId;        
                    this.Min = min;
                    this.Max = max;
                    this.Value = value;
                }
                public int SatisfactionId{ get; set; }
                public float Min { get; set; }
                public float Max { get; set; }
                public float Value { get; set; }
            }       
            public class SatisfactionDefine {
                private Dictionary<int, ConfigSatisfaction_Define> mDefines = new Dictionary<int, ConfigSatisfaction_Define>();
                private static readonly Lazy<SatisfactionDefine> instance =
                        new Lazy<SatisfactionDefine>(() => new SatisfactionDefine());
                public static SatisfactionDefine Instance {
                    get {
                        return instance.Value;
                    }
                }

                private SatisfactionDefine() { }
                public void Add(int satisfactionId, ConfigSatisfaction_Define p) {
                    mDefines.Add(satisfactionId, p);
                }
                public ConfigSatisfaction_Define? Get(int satisfactionId) {
                    if(mDefines.ContainsKey(satisfactionId) == false) {
                        return null;
                    }
                    return mDefines[satisfactionId];
                }
            }     
        }        
    }
}