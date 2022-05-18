using System;
using System.Collections.Generic;
using System.Linq;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {            
            //Discharge가 호출되면 Counter값을 읽어와 시나리오별로 Period와 LastDischargeTime을 계산해서 실행한다.            
            class DischargeScenario {
                public DischargeScenario(string satisfactionId, float amout, Int64 period) {
                    this.SatisfactionId = satisfactionId;
                    this.Amount = amout;
                    this.Period = period;
                }
                public string SatisfactionId { get; set; }
                public float Amount { get; set; }
                public Int64 LastDischargeTime { get; set; }
                public Int64 Period { get; set; }
            }
            public class DischargeHandler {                
                private List<DischargeScenario> mList = new List<DischargeScenario>();
                private static readonly Lazy<DischargeHandler> instance =
                    new Lazy<DischargeHandler>(() => new DischargeHandler());
                public static DischargeHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private DischargeHandler() {
                }

                public void Add(string satisfactionId, float amout, Int64 period)
                {
                    mList.Add(new DischargeScenario(satisfactionId, amout, period));
                }
                public void Discharge(int actorType) {
                    Int64 count = Counter.Instance.GetCount();
                    var d = ActorHandler.Instance.GetActors(actorType);
                    if(d != null) {
                        
                        for(int i = 0; i < mList.Count(); i++) {
                            var p = mList[i];
                            if(count - p.LastDischargeTime >= p.Period) {
                                mList[i].LastDischargeTime = count;
                                foreach(var a in d) {
                                    a.Value.Discharge(p.SatisfactionId, p.Amount);
                                }
                            }
                        }                        
                    }
                }
            }
        }
    }
}