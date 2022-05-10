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
            public abstract class SatisfactionValue {
                public int SatisfactionId{ get; set; } //satisfaction id
                public abstract float GetValue();
            }
            public class SatisfactionTable {
                /* 
                SatisfactionTable id, list
                SatisfactionTable id를 행위에 연결 시켜서 사용하면 됨
                */
                private Dictionary<int, List<SatisfactionValue>> mTable = new Dictionary<int, List<SatisfactionValue>>();
                private static readonly Lazy<SatisfactionTable> instance =
                        new Lazy<SatisfactionTable>(() => new SatisfactionTable());
                public static SatisfactionTable Instance {
                    get {
                        return instance.Value;
                    }
                }

                private SatisfactionTable() {
                }

                public void SetSatisfactionTable(int satisfactionTableId, SatisfactionValue p) {
                    if(mTable.ContainsKey(satisfactionTableId) == false) {
                        mTable[satisfactionTableId] = new List<SatisfactionValue>();
                    }
                    mTable[satisfactionTableId].Add(p);
                }

                public bool ApplySatisfaction(int satisfactionTableId, string actorId) {
                    if(mTable.ContainsKey(satisfactionTableId) == false) {
                        return false;
                    }

                    var actor = ActorHandler.Instance.GetActor(actorId);
                    if(actor == null) {
                        return false;
                    }

                    foreach(var p in mTable[satisfactionTableId]) {
                        actor.Obtain(p.SatisfactionId, p.GetValue());
                    }
                    return true;
                }
            }
        }        
    }
}