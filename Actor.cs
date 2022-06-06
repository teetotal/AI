using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
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
            public class Position {
                public float x { get; set; }
                public float y { get; set; }
                public float z { get; set; }
                public Position(float x, float y, float z) {
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }
                public double GetDistance(Position to) {
                    return Math.Sqrt(Math.Pow(to.x - x, 2) + Math.Pow(to.y - y, 2) + Math.Pow(to.z - z, 2));
                }
            }  
            public class Actor {           
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
                public int mType;
                public string mUniqueId;
                public int mLevel;
                public string? prefab;
                public Position? position;
                private Dictionary<string, Satisfaction> mSatisfaction = new Dictionary<string, Satisfaction>();
                // Relation
                // Actor id, Satisfaction id, amount
                private Dictionary<string, Dictionary<string, float>> mRelation = new Dictionary<string, Dictionary<string, float>>();
                // Task 수행 횟수 저장 for level up
                public Int64 mTaskCounter { get; set; }
                //Quest ------------------------------------------------------------------------------------------------
                //Quest handler에서 top만큼씩 수행 완료 처리.
                public List<string> mQuestList { get; set; }
                private Dictionary<string, double> mAccumulationSatisfaction = new Dictionary<string, double>(); //누적 Satisfaction
                private Dictionary<string, Int64> mAccumulationTask = new Dictionary<string, Int64>(); //Task별 수행 횟수. taskhandler에서 호출
                //Item -------------------------------------------------------------------------------------------------
                //item key, quantity
                private Dictionary<string, int> mInventory = new Dictionary<string, int>();
                //장착중인 아이템.만료처리는 mInvoking랑 같이 업데이트 해줘야함.
                private Dictionary<string, List<ItemUsage>> mInstallation = new Dictionary<string, List<ItemUsage>>();
                //발동중인 아이템 리스트                
                private Dictionary<string, List<ItemUsage>> mInvoking = new Dictionary<string, List<ItemUsage>>();
                //-------------------------------------------------------------------------------------------------
                public Actor(int type, string uniqueId, int level, string? prefab, List<float>? position, List<string> quests) {
                    this.mType = type;
                    this.mUniqueId = uniqueId;
                    this.mLevel = level;
                    this.prefab = prefab;
                    if(position != null && position.Count == 3)
                        this.position = new Position(position[0], position[1], position[2]);
                    this.mQuestList = quests;
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
                //Satisfaction update ---------------------------------------------------------------------------------------------------------------------
                public bool Discharge(string satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, -amount, 0, null);
                }

                public bool Obtain(string satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, amount, 0, null);
                }     
                //Task 수행횟수 기록
                public void DoTask(string taskId) {
                    if(mAccumulationTask.ContainsKey(taskId)) {
                        mAccumulationTask[taskId] ++;
                    } else {
                        mAccumulationTask[taskId] = 1;
                    }
                    
                }
                public bool ApplySatisfaction(string satisfactionId, float amount, int measure, string? from, bool skipAccumulation = false) {
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
                    //quest를 위한 누적 집계. +만 집계한다. skipAccumulation값은 보상에 의한 건 skip하기 위한 flag
                    if(value > 0 && skipAccumulation == false) {
                        if(mAccumulationSatisfaction.ContainsKey(satisfactionId)) {
                            mAccumulationSatisfaction[satisfactionId] += value;
                        } else {
                            mAccumulationSatisfaction[satisfactionId] = value;
                        }
                    }                    
                    
                    // update Relation 
                    if(from != null) {
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

                // Level up-------------------------------------------------------------------------------------------------------------
                public bool checkLevelUp() {
                    //check level up                   
                    var info = LevelHandler.Instance.Get(mType, mLevel);
                    if(info != null && info.next != null && info.next.threshold != null) {                        
                        foreach(Config_KV_SF t in info.next.threshold) {
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
                public bool LevelUp(List<Config_Reward>? rewards) {                    
                    mLevel++;
                    
                    if(rewards != null) {
                        foreach(var reward  in rewards) {
                            if(reward.itemId != null ) {
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
                // quest -------------------------------------------------------------------------------------------------------------
                public List<string> GetQuest() {
                    List<string> ret = new List<string>();
                    int top = QuestHandler.Instance.GetTop(mType);
                    int i = 0;
                    foreach(string quest in this.mQuestList) {
                        if(i >= top) {
                            break;
                        }
                        ret.Add(quest);
                        i++;                        
                    }
                    return ret;
                }
                public bool RemoveQuest(string questId) {
                    return mQuestList.Remove(questId);
                }
                public double GetAccumulationSatisfaction(string satisfactionId) {
                    if(mAccumulationSatisfaction.ContainsKey(satisfactionId))
                        return mAccumulationSatisfaction[satisfactionId];
                    return 0;
                }
                // task -------------------------------------------------------------------------------------------------------------
                //return task id                
                public string? GetTaskId() {
                    string? taskId = null;
                    float maxValue = 0.0f;                    
                    var tasks = TaskHandler.Instance.GetTasks();
                    foreach(var task in tasks) {
                        //일정레벨 이상인 task가 조회하는거 구현해야 함
                        float expecedValue = GetExpectedValue(task.Value);
                        if(expecedValue > maxValue) {
                            maxValue = expecedValue;
                            taskId = task.Key;
                        }
                    }                    
                    return taskId;
                }
                private float GetExpectedValue(FnTask fn) {
                    
                    //1. satisfaction loop
                    //2. if check in fn then sum
                    //3. cal normalization
                    //4. get mean                      
                    float sum = 0;
                    var taskSatisfaction = fn.GetValues(this);                  
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
                
                //return satisfaction id                
                public Tuple<string, float> GetMotivation()
                {                                                            
                    //1. get mean
                    //2. finding max(norm(value) - avg)
                     
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
                        if(info != null)
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
                        if(item.satisfaction != null) {
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
                    if(item.satisfaction != null) {
                        if(item.invoke.expire == (int)ITEM_INVOKE_EXPIRE.FOREVER) {
                            ApplyItemSatisfaction(item.satisfaction); //적용하고 끝
                        } else {                            
                            if(item.installationKey != null) {
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
