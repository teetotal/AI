namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            /*
            Discharge가 호출되면 mCounter가 증가하고 시나리오별로 Period와 LastDischargeTime을 계산해서 실행한다.
            */
            class DischargeScenario {
                public DischargeScenario(int satisfactionId, float amout, Int64 period) {
                    this.SatisfactionId = satisfactionId;
                    this.Amount = amout;
                    this.Period = period;
                }
                public int SatisfactionId { get; set; }
                public float Amount { get; set; }
                public Int64 LastDischargeTime { get; set; }
                public Int64 Period { get; set; }
            }
            public class DischargeHandler {
                private Int64 mCounter = 0;
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

                public void Add(int motivationId, float amout, Int64 period)
                {
                    mList.Add(new DischargeScenario(motivationId, amout, period));
                }
                public Int64 Discharge(int actorType) {
                    var d = ActorHandler.Instance.GetActors(actorType);
                    if(d != null) {
                        mCounter ++;
                        for(int i = 0; i < mList.Count(); i++) {
                            var p = mList[i];
                            if(mCounter - p.LastDischargeTime >= p.Period) {
                                mList[i].LastDischargeTime = mCounter;
                                foreach(var a in d) {
                                    a.Value.Discharge(p.SatisfactionId, p.Amount);
                                }
                            }
                        }                        
                    }
                    return mCounter;
                }
            }
        }
    }
}