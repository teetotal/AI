namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public enum ITEM_INVOKE_TYPE : int {
                IMMEDIATELY = 0,
                INVENTORY
            }
            public enum ITEM_INVOKE_EXPIRE : int {
                FOREVER = 0,
                LIMITED
            }
            public enum ITEM_SATISFACTION_MEASURE : int {
                ABSOLUTE  = 0,
                PERCENT,
                INCREASE
            }

            public class ItemUsage {
                public ItemUsage(string itemKey, int expire, int usage) {
                    this.itemKey = itemKey;
                    this.expire = expire;
                    this.usage = usage; //사용량이 필요한 아이템일 경우 사용
                }
                public string itemKey { get; set; }
                public int expire { get; set; }
                public int usage { get; set; }
            }            
            public class Actor {                
                public int mType;
                public string mUniqueId;
                public int mLevel;
                private Dictionary<string, Satisfaction> mSatisfaction = new Dictionary<string, Satisfaction>();
                // Relation
                // Actor id, Satisfaction id, amount
                private Dictionary<string, Dictionary<string, float>> mRelation = new Dictionary<string, Dictionary<string, float>>();
                // Task 수행 횟수 저장 for level up
                public Int64 mTaskCounter { get; set; }
                //Item -------------------------------------------------------------------------------------------------
                //item key, quantity
                private Dictionary<string, int> mInventory = new Dictionary<string, int>();
                //장착중인 아이템.만료처리는 mInvoking랑 같이 업데이트 해줘야함.
                private Dictionary<string, List<ItemUsage>> mInstallation = new Dictionary<string, List<ItemUsage>>();
                //발동중인 아이템 리스트                
                private Dictionary<string, List<ItemUsage>> mInvoking = new Dictionary<string, List<ItemUsage>>();
                //-------------------------------------------------------------------------------------------------
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
                public bool LevelUp(List<ConfigLevel_Rewards>? rewards) {                    
                    mLevel++;
                    
                    if(rewards is not null) {
                        foreach(var reward  in rewards) {
                            if(reward.itemId is not null ) {
                                if(!this.ReceiveItem(reward.itemId, reward.quantity)) {
                                    return false;
                                }
                            }
                        }
                    }

                    return true;
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
                //Item-------------------------------------------------------
                public string PrintInventory() {
                    string sz = "";
                    foreach(var p in mInventory) {
                        var info = ItemHandler.Instance.GetItemInfo(p.Key);
                        if(info is not null)
                            sz += String.Format("> {0} {1}\n", info.name, p.Value);
                    }
                    return sz;
                }
                public bool ReceiveItem(string itemKey, int quantity) {
                    var item = ItemHandler.Instance.GetItemInfo(itemKey);
                    if(item is null || item.invoke is null) {
                        return false;
                    }

                    switch((ITEM_INVOKE_TYPE)item.invoke.type) {
                        case ITEM_INVOKE_TYPE.IMMEDIATELY:
                        if(item.satisfaction is not null) {
                            for(int i = 0; i < quantity; i++)
                                this.ApplyItemSatisfaction(item.satisfaction);
                        }                        
                        break;
                        case ITEM_INVOKE_TYPE.INVENTORY:                        
                        AddInventory(itemKey, quantity);
                        break;
                    }

                    return true;
                }
                public void AddInventory(string itemKey, int quantity) {
                    if(mInventory.ContainsKey(itemKey)) {
                        mInventory[itemKey] += quantity;
                    } else {
                        mInventory[itemKey] = quantity;
                    }
                }
                //아이템 사용은 한번에 하나씩만
                public bool UseItemFromInventory(string itemKey) {
                    if(mInventory.ContainsKey(itemKey) == false || mInventory[itemKey] <= 0) {
                        return false;
                    }   
                    if(!InvokeItem(itemKey, 0)) {
                        return false;
                    }
                    mInventory[itemKey] --;
                    return true;
                }
                public bool InvokeItem(string itemKey, int usage) {
                    var item = ItemHandler.Instance.GetItemInfo(itemKey);
                    if(item is null || item.invoke is null) {
                        return false;
                    }
                    //satisfaction 적용
                    if(item.satisfaction is not null) {
                        if(item.invoke.expire == (int)ITEM_INVOKE_EXPIRE.FOREVER) {
                            ApplyItemSatisfaction(item.satisfaction); //적용하고 끝
                        } else {                            
                            if(item.installationKey is not null) {
                                //몸에 탑재하는 아이템
                                //몸에 연결은 front에서 처리
                                if(mInstallation.ContainsKey(itemKey) == false) {
                                    mInstallation[itemKey] = new List<ItemUsage>();
                                }
                                mInstallation[itemKey].Add(new ItemUsage(itemKey, item.invoke.expire, usage));
                            }                             
                            //발동
                            if(mInvoking.ContainsKey(itemKey) == false) {
                                mInvoking[itemKey] = new List<ItemUsage>();
                            }
                            mInvoking[itemKey].Add(new ItemUsage(itemKey, item.invoke.expire, usage));
                        }                            
                    }

                    return true;
                }
                private bool ApplyItemSatisfaction(List<ConfigItem_Satisfaction> list) {

                    for(int i =0; i < list.Count(); i++) {
                        ConfigItem_Satisfaction p = list[i];
                        if(p.measure is null || p.satisfactionId is null) {
                            return false;
                        }
                        if(mSatisfaction.ContainsKey(p.satisfactionId) == false) {
                            continue;
                        }
                        //min
                        switch((ITEM_SATISFACTION_MEASURE)p.measure.min) {
                            case ITEM_SATISFACTION_MEASURE.ABSOLUTE:
                            mSatisfaction[p.satisfactionId].Min = p.min;
                            break;
                            case ITEM_SATISFACTION_MEASURE.PERCENT:
                            mSatisfaction[p.satisfactionId].Min += (mSatisfaction[p.satisfactionId].Min * (p.min / 100));
                            break;
                            case ITEM_SATISFACTION_MEASURE.INCREASE:
                            mSatisfaction[p.satisfactionId].Min += p.min;
                            break;
                        }
                        //max
                        switch((ITEM_SATISFACTION_MEASURE)p.measure.max) {
                            case ITEM_SATISFACTION_MEASURE.ABSOLUTE:
                            mSatisfaction[p.satisfactionId].Max = p.max;
                            break;
                            case ITEM_SATISFACTION_MEASURE.PERCENT:
                            mSatisfaction[p.satisfactionId].Max += (mSatisfaction[p.satisfactionId].Max * (p.max / 100));
                            break;
                            case ITEM_SATISFACTION_MEASURE.INCREASE:
                            mSatisfaction[p.satisfactionId].Max += p.max;
                            break;
                        }
                        //value
                        switch((ITEM_SATISFACTION_MEASURE)p.measure.value) {
                            case ITEM_SATISFACTION_MEASURE.ABSOLUTE:
                            mSatisfaction[p.satisfactionId].Value = p.value;
                            break;
                            case ITEM_SATISFACTION_MEASURE.PERCENT:
                            mSatisfaction[p.satisfactionId].Value += (mSatisfaction[p.satisfactionId].Value * (p.value / 100));
                            break;
                            case ITEM_SATISFACTION_MEASURE.INCREASE:
                            mSatisfaction[p.satisfactionId].Value += p.value;
                            break;
                        }                                                
                    }

                    return true;
                }
            }
        }
    }
}