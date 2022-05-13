namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class Actor {                
                public int mType;
                public string mUniqueId;
                public int mLevel;
                private Dictionary<string, Satisfaction> mSatisfaction = new Dictionary<string, Satisfaction>();
                // Relation
                // Actor id, Satisfaction id, amount
                private Dictionary<string, Dictionary<string, float>> mRelation = new Dictionary<string, Dictionary<string, float>>();
                // Task 수행 횟수 저장.
                // level up을 위해서
                public Int64 mTaskCounter { get; set; }
                public Actor(int type, string uniqueId, int level) {
                    this.mType = type;
                    this.mUniqueId = uniqueId;
                    this.mLevel = level;
                }
                public bool SetSatisfaction(string satisfactionId, float min, float max, float value)
                {
                    mSatisfaction.Add(satisfactionId, new Satisfaction(satisfactionId, min, max, value));
                    return true;
                }
                public void Print() {
                    foreach(var p in mSatisfaction) {    
                        Satisfaction s = p.Value;
                        System.Console.WriteLine("{0} {1} ({2}) {3}/{4}, {5}", 
                        this.mUniqueId, SatisfactionDefine.Instance.GetTitle(s.SatisfactionId), s.Value, s.Min, s.Max, GetNormValue(s));
                    }
                }
                public bool Discharge(string satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, -amount, 0, null);
                }

                public bool Obtain(string satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, amount, 0, null);
                }

                public bool ApplySatisfaction(string satisfactionId, float amount, int measure, string? from) {
                    if(mSatisfaction.ContainsKey(satisfactionId) == false) {
                        return false;
                    }

                    float value;
                    switch(measure) {                        
                        case 1: //percent
                        value = mSatisfaction[satisfactionId].Value * (amount / 100);
                        break;
                        default:
                        value = amount;
                        break;
                    }

                    mSatisfaction[satisfactionId].Value += value;
                    
                    // update Relation 
                    if(from is not null) {
                        if(mRelation.ContainsKey(from) == false) {
                            mRelation[from] = new Dictionary<string, float>();
                        }
                        if(mRelation[from].ContainsKey(satisfactionId) == false) {
                            mRelation[from][satisfactionId] = value;
                        } else {
                            mRelation[from][satisfactionId] += value;
                        }
                    }
                    return true;
                }

                public bool checkLevelUp() {
                    //check level up                   
                    var info = LevelHandler.Instance.Get(mType, mLevel);
                    if(info is not null && info.next is not null && info.next.threshold is not null) {                        
                        foreach(ConfigLevel_Threshold t in info.next.threshold) {
                            if(t.key is null) {
                                return false;
                            }
                            //나중에 필요하면 추가. 지금은 task수행 횟수만 구현
                            switch(t.key.ToUpper()) {
                                case "TASKCOUNTER":
                                if(mTaskCounter < t.value) {
                                    return false;
                                }
                                break;                                
                            }
                        }
                        return true;
                    }
                    return false;
                }

                public void LevelUp() {
                    mLevel++;
                }

                public Satisfaction? GetSatisfaction(string id) {
                    if(mSatisfaction.ContainsKey(id)) {
                        return mSatisfaction[id];
                    }
                    return null;        
                }
                public Dictionary<string, Satisfaction> GetSatisfactions() {
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
                    var taskSatisfaction = fn.GetValues(mUniqueId);                  
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
                public Tuple<string, float> GetMotivation()
                {
                    /*                                        
                    1. get mean
                    2. finding max(norm(value) - avg)
                    */ 
                    string satisfactionId = mSatisfaction.First().Key;  
                    float minVal = 0;
                    float mean = GetMean();
                    foreach(var p in mSatisfaction) {
                        Satisfaction v = p.Value;
                        float norm = GetNormValue(v.Value, v.Min, v.Max);
                        float diff = norm - mean;
                        if(diff < minVal) {
                            minVal = diff;
                            satisfactionId = p.Key;
                        }
                    }                    
                    
                    return new Tuple<string, float>(satisfactionId, mean);
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