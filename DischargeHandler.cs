namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            /*
            Discharge가 호출되면 mCounter가 증가하고 시나리오별로 Period와 LastDischargeTime을 계산해서 실행한다.
            */
            class DischargeScenario {
                public DischargeScenario(int id, int satisfactionId, float amout, Int64 period) {
                    this.Id = id;
                    this.SatisfactionId = satisfactionId;
                    this.Amount = amout;
                    this.Period = period;
                }
                public int Id { get; set; }
                public int SatisfactionId { get; set; }
                public float Amount { get; set; }
                public Int64 LastDischargeTime { get; set; }
                public Int64 Period { get; set; }
            }
            public class DischargeHandler {
                private Int64 mCounter = 0;
                private Dictionary<int, DischargeScenario> mScenarios = new Dictionary<int, DischargeScenario>();
                private static readonly Lazy<DischargeHandler> instance =
                    new Lazy<DischargeHandler>(() => new DischargeHandler());
                public static DischargeHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private DischargeHandler() {
                }

                public bool SetScenario(int id, int motivationId, float amout, Int64 period)
                {
                    if(mScenarios.ContainsKey(id) == true) {
                        return false;
                    }

                    mScenarios.Add(id, new DischargeScenario(id, motivationId, amout, period));
                    return true;
                }
                public Int64 Discharge(int actorType) {
                    var d = ActorHandler.Instance.GetActors(actorType);
                    if(d != null) {
                        mCounter ++;

                        foreach(var p in mScenarios) {
                            if(mCounter - p.Value.LastDischargeTime >= p.Value.Period) {
                                mScenarios[p.Key].LastDischargeTime = mCounter;
                                foreach(var a in d) {
                                    a.Value.Discharge(p.Value.SatisfactionId, p.Value.Amount);
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