namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class Actor {
                public int mType;
                public string mUniqueId;
                protected Dictionary<int, Satisfaction> mSatisfaction = new Dictionary<int, Satisfaction>();
                public Actor(int type, string uniqueId) {
                    this.mType = type;
                    this.mUniqueId = uniqueId;
                }
                public bool SetSatisfaction(int satisfactionId, float min, float max, float value)
                {
                    mSatisfaction.Add(satisfactionId, new Satisfaction(satisfactionId, min, max, value));
                    return true;
                }
                public void Print() {
                    foreach(var p in mSatisfaction) {    
                        Satisfaction s = p.Value;
                        System.Console.WriteLine("{0} {1} ({2}) {3}/{4}, {5}", 
                        this.mUniqueId, SatisfactionDefine.Instance.Get(s.SatisfactionId).title, s.Value, s.Min, s.Max, GetNormValue(s));
                    }
                }
                public bool Discharge(int satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, -amount, 0);
                }

                public bool Obtain(int satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, amount, 0);
                }

                public bool ApplySatisfaction(int satisfactionId, float amount, int measure) {
                    if(mSatisfaction.ContainsKey(satisfactionId) == false) {
                        return false;
                    }

                    switch(measure) {
                        case 0: mSatisfaction[satisfactionId].Value += amount;
                        break;
                        case 1: mSatisfaction[satisfactionId].Value += mSatisfaction[satisfactionId].Value * (amount / 100);
                        break;
                    }

                    return true;

                }


                public Satisfaction? GetSatisfaction(int id) {
                    if(mSatisfaction.ContainsKey(id)) {
                        return mSatisfaction[id];
                    }
                    return null;        
                }
                public Dictionary<int, Satisfaction> GetSatisfactions() {
                    return mSatisfaction;
                }
                /*
                return task id
                */
                public int GetTaskId() {
                    int taskId = 0;
                    float maxValue = 0.0f;                    
                    var tasks = TaskHandler.Instance.GetTasks();
                    for(int i = 0; i < tasks.Count(); i++) {
                        float expecedValue = GetExpectedValue(tasks[i]);
                        if(expecedValue > maxValue) {
                            maxValue = expecedValue;
                            taskId = i;
                        }
                    }
                    return taskId;
                }

                private float GetExpectedValue(FnTask fn) {
                    /*
                    1. satisfaction loop
                    2. if check in fn then sum
                    3. cal normalization
                    4. get mean
                    */  
                    float sum = 0;
                    var taskSatisfaction = fn.GetValues();                  
                    foreach(var p in mSatisfaction) {
                        float val = p.Value.Value;
                        if(taskSatisfaction.ContainsKey(p.Key)) {
                            val += taskSatisfaction[p.Key];
                        }
                        var normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        sum += normVal;
                    }
                    return sum / mSatisfaction.Count();
                }
                private float GetNormValue(Satisfaction p) {
                    return GetNormValue(p.Value, p.Min, p.Max);
                }

                private float GetNormValue(float value, float min, float max) {                    
                    float v = value;
                    if(value > max) {
                        v = max * (float)Math.Log(value, max);
                    } else if(value <= min) {
                        //v = value * (float)Math.Log(value, max);

                        //급격하게 떨어지고 급격하게 올라가야 한다.
                        //음수로 처리
                        const float weight = 2.0f;
                        v = (value - min) * weight;
                        //Console.WriteLine("Min origin = {0}, diff={1}, nom={2}", value, v, v/min);
                        return v / min;
                    }

                    return v / max;
                }

                /*
                return satisfaction id
                */
                public Tuple<int, float> GetMotivation()
                {
                    /*                                        
                    1. get mean
                    2. finding max(norm(value) - avg)
                    */ 
                    int idx = mSatisfaction.First().Key;  
                    float minVal = 0;
                    float mean = GetMean();
                    foreach(var p in mSatisfaction) {
                        Satisfaction v = p.Value;
                        float norm = GetNormValue(v.Value, v.Min, v.Max);
                        float diff = norm - mean;
                        if(diff < minVal) {
                            minVal = diff;
                            idx = p.Key;
                        }

                    }                    
                    
                    return new Tuple<int, float>(idx, mean);
                }                

                private float GetMean() {
                    float sum = 0.0f;
                    foreach(var p in mSatisfaction) {
                        Satisfaction v = p.Value;
                        sum += GetNormValue(v.Value, v.Min, v.Max);
                    }
                    return sum / mSatisfaction.Count();
                }
            }
        }
    }
}