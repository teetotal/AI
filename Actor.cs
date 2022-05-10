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
                        System.Console.WriteLine("{0} - SatisfactionId = {1}\t Min = {2}\t Max = {3}\t Value = {4}\t V/Max = {5}\t V/Min = {6}", 
                        this.mUniqueId, s.SatisfactionId, s.Min, s.Max, s.Value, s.Value / s.Max, s.Value / s.Min);
                    }
                }
                public bool Discharge(int id, float amount) {
                    if(mSatisfaction.ContainsKey(id) == false) {
                        return false;
                    }

                    mSatisfaction[id].Value = Math.Max(0.0f, mSatisfaction[id].Value - amount);
                    return true;
                }

                public bool Obtain(int id, float amout) {
                    if(mSatisfaction.ContainsKey(id) == false) {
                        return false;
                    }

                    mSatisfaction[id].Value += amout;
                    return true;
                }
                public Satisfaction? GetSatisfaction(int id) {
                    if(mSatisfaction.ContainsKey(id)) {
                        return mSatisfaction[id];
                    }
                    return null;        
                }
                /*
                return task id
                */
                public int GetTaskId() {
                    int taskId = 0;
                    foreach(var p in TaskHandler.Instance.GetTasks()) {
                        //
                        var r = p.GetValues();
                        // 계산
                        Console.WriteLine(p.ToString());
                    }
                    return taskId;
                }
                /*
                return satisfaction id
                */
                public int GetMotivation()
                {
                    /*
                    0. min check
                    1. norm
                    2. get mean
                    3. finding max(value - avg)
                    */        
                    bool isMinList = true;
                    List<int> list = CheckMinVal();
                    if(list.Count() == 0) {
                        foreach(var p in mSatisfaction) {
                            list.Add(p.Key);
                        }
                        isMinList = false;
                    }
                    float mean = GetMean(list, isMinList);
                    int idx = list[0];
                    float minVal = mSatisfaction[list[0]].Value;
                    foreach(int id in list) {
                        float v = (mSatisfaction[id].Value / (isMinList ? mSatisfaction[id].Min : mSatisfaction[id].Max) ) - mean;            
                        if(v < minVal) {
                            minVal = v;
                            idx = id;
                        }
                    }
                    
                    return idx;
                }
                private List<int> CheckMinVal() {
                    List<int> ret = new List<int>();
                    foreach(var p in mSatisfaction) {
                        if(p.Value.Value <= p.Value.Min ) {
                            ret.Add(p.Key);
                        }
                    }
                    
                    return ret;
                }

                private float GetMean(List<int> list, bool isMinList) {
                    float sum = 0.0f;
                    foreach(int id in list) {
                        sum += (mSatisfaction[id].Value / (isMinList ? mSatisfaction[id].Min : mSatisfaction[id].Max));
                    }

                    return sum / mSatisfaction.Count();
                }
            }
        }
    }
}