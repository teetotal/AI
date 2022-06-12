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
                    Set(x, y, z);
                }
                public void Set(float x, float y, float z) {
                    this.x = x;
                    this.y = y;
                    this.z = z;
                }
                public double GetDistance(Position to) {
                    return Math.Sqrt(Math.Pow(to.x - x, 2) + Math.Pow(to.y - y, 2) + Math.Pow(to.z - z, 2));
                }
            }  
            public class Actor {     
                public enum CALLBACK_TYPE {
                    SET_READY,
                    TAKE_TASK,
                    DO_TASK,
                    RESERVE,
                    RESERVED,
                    ASK,
                    ASKED,
                    INTERRUPT,
                    INTERRUPTED,
                    REFUSAL
                }
                public delegate void Callback(CALLBACK_TYPE type, string actorId); 
                public enum STATE {
                    READY,                    
                    TASKED,    
                    RESERVED, //누군가에 의해 ASK요청을 받은 상태                
                }     
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
                public class TaskContext {
                    // Task 수행 횟수 저장 for level up
                    public Int64 taskCounter { get; set; }
                    public STATE state = STATE.READY;
                    public FnTask? currentTask = null;
                    //target이 actor일 경우 actorId
                    public Tuple<bool, string>? target;       
                    public Actor? interactionFromActor;             
                    public void Release() {
                        this.currentTask = null;
                        this.target = null;
                        this.interactionFromActor = null;
                        state = STATE.READY;
                    }
                    public void Set(FnTask task, Tuple<bool, string> target) {
                        this.currentTask = task;
                        this.target = target;
                        state = STATE.TASKED;
                    }
                    public void IncreaseTaskCounter() {
                        taskCounter++;
                    }
                }      
                private class ItemContext {
                    //item key, quantity
                    public Dictionary<string, int> inventory = new Dictionary<string, int>();
                    //장착중인 아이템.만료처리는 mInvoking랑 같이 업데이트 해줘야함.
                    public Dictionary<string, List<ItemUsage>> installation = new Dictionary<string, List<ItemUsage>>();
                    //발동중인 아이템 리스트                
                    public Dictionary<string, List<ItemUsage>> invoking = new Dictionary<string, List<ItemUsage>>();
                }          
                private class QuestContext {                    
                    public List<string> questList { get; set; } = new List<string>(); //Quest handler에서 top만큼씩 수행 완료 처리.
                    public Dictionary<string, double> accumulationSatisfaction = new Dictionary<string, double>(); //누적 Satisfaction
                    public Dictionary<string, Int64> accumulationTask = new Dictionary<string, Int64>(); //Task별 수행 횟수. taskhandler에서 호출
                    public void AddSatisfaction(string satisfactionId, float value) {
                        if(accumulationSatisfaction.ContainsKey(satisfactionId)) {
                            accumulationSatisfaction[satisfactionId] += value;
                        } else {
                            accumulationSatisfaction[satisfactionId] = value;
                        }
                    }
                    public double GetSatisfaction(string satisfactionId) {
                        if(accumulationSatisfaction.ContainsKey(satisfactionId))
                            return accumulationSatisfaction[satisfactionId];
                        return 0;
                    }
                    public void IncreaseTaskCount(string taskId) {
                         if(accumulationTask.ContainsKey(taskId)) {
                            accumulationTask[taskId] ++;
                        } else {
                            accumulationTask[taskId] = 1;
                        }       
                    }
                }

                public int mType;
                public string mUniqueId;
                public int mLevel;
                public string? prefab;
                public Position? position;       
                private Callback? mCallback;
                private Dictionary<string, Satisfaction> mSatisfaction = new Dictionary<string, Satisfaction>();
                // Relation
                // Actor id, Satisfaction id, amount
                private Dictionary<string, Dictionary<string, float>> mRelation = new Dictionary<string, Dictionary<string, float>>();                
                //Task --------------------------------------------------------------------------------------------------
                private TaskContext mTaskContext = new TaskContext();                
                //Quest ------------------------------------------------------------------------------------------------
                private QuestContext mQuestContext = new QuestContext();
                //Item -------------------------------------------------------------------------------------------------
                private ItemContext mItemContext = new ItemContext();
                //-------------------------------------------------------------------------------------------------
                public Actor(int type, string uniqueId, int level, string? prefab, List<float>? position, List<string> quests) {
                    this.mType = type;
                    this.mUniqueId = uniqueId;
                    this.mLevel = level;
                    this.prefab = prefab;
                    if(position != null && position.Count == 3)
                        this.position = new Position(position[0], position[1], position[2]);
                    this.mQuestContext.questList = quests;
                    this.mCallback = null;
                }
                public void SetCallback(Callback fn) {
                    mCallback = fn;
                }
                public void CallCallback(CALLBACK_TYPE type) {
                    if(mCallback != null) {
                        mCallback(type, mUniqueId);
                    }
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
                public string GetTaskString() {
                    if(mTaskContext.currentTask == null || mTaskContext.target == null)
                        return "error";

                    var values = mTaskContext.currentTask.mInfo.satisfactions;                    
                    string sz = string.Empty;
                    if(values == null) return sz;
                    foreach(var p in values) {
                        var s = SatisfactionDefine.Instance.Get(p.Key);
                        if(s == null) {
                            Console.WriteLine("Invalid SatisfactionDefine id");
                        } else {
                            sz += String.Format("{0}({1}) ", s.title, p.Value );                                                        
                        }                        
                    }
                    //sz += mTaskContext.target.ToString();
                    return sz;
                }
                //Satisfaction update ---------------------------------------------------------------------------------------------------------------------
                public bool Discharge(string satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, -amount, 0, null);
                }

                public bool Obtain(string satisfactionId, float amount) {
                    return ApplySatisfaction(satisfactionId, amount, 0, null);
                }     
                //Ask ----------------------------------------------------------       
                private bool SetReserveToTarget(string targetActorId) {
                    var targetActor = ActorHandler.Instance.GetActor(targetActorId);
                    if(targetActor == null)
                        return false;
                    
                    if(!targetActor.SetAskReserve(this))
                        return false;
                    
                    CallCallback(CALLBACK_TYPE.RESERVE);
                    
                    return true;                    
                }         
                public bool SetAskReserve(Actor actorFrom) {
                    if(mTaskContext.state != STATE.READY) 
                        return false;
                    mTaskContext.state = STATE.RESERVED;
                    mTaskContext.interactionFromActor = actorFrom;

                    CallCallback(CALLBACK_TYPE.RESERVED);
                    return true;
                }                                
                public bool SendAskTaskToTarget(string taskId) {
                    if(mTaskContext.target == null || mTaskContext.target.Item1 == false) 
                        return false;
                    var targetActor = ActorHandler.Instance.GetActor(mTaskContext.target.Item2);
                    if(targetActor == null)
                        return false;
                    if(!targetActor.SetCurrentTask(taskId))
                        return false;
                    CallCallback(CALLBACK_TYPE.ASK);                    
                    targetActor.CallCallback(CALLBACK_TYPE.TAKE_TASK);
                    return true;
                }
                // ---------------------------------------------------------------------
                public TaskContext GetTaskContext() {
                    return mTaskContext;
                }
                public bool DoTaskBefore() {
                    if(mTaskContext.currentTask != null && mTaskContext.target != null && mTaskContext.target.Item1) {
                        var targetActor = ActorHandler.Instance.GetActor(mTaskContext.target.Item2);
                        if(targetActor == null)
                            return false;

                        //ask에 대한 응답을 할지 않할지 판단
                        if(mTaskContext.currentTask.mInfo.type == TASK_TYPE.REACTION) {
                            /*
                            고려사항.
                            - 응답했을 때 얻게될 satisfaction * relation을 보고 가중치
                            - 응답 할 시간에 다른 task를 하면 얻게될 satisfaction
                            */
                            //거절하면
                            CallCallback(CALLBACK_TYPE.REFUSAL);
                            mTaskContext.Release();
                            CallCallback(CALLBACK_TYPE.SET_READY);
                        }
                    
                        var interaction = mTaskContext.currentTask.mInfo.target.interaction;                    
                        //ask, interrupt 처리
                        switch(interaction.type) {
                            case TASK_INTERACTION_TYPE.ASK:
                            if(interaction.taskId == null || !SendAskTaskToTarget(interaction.taskId)) //상대에게 task를 실행하라고 던진다.
                                return false;
                            break;
                            case TASK_INTERACTION_TYPE.INTERRUPT:                        
                            //상대의 현재 task를 중단 시키고 재설정 시킨다.
                            break;
                            default:
                            break;
                        }                        
                    }
                    return true;
                }
                //ret DoTask, islevelup
                public Tuple<bool, bool> DoTask() {
                    if(mTaskContext.currentTask == null || mTaskContext.currentTask.mTaskId == null || mTaskContext.target == null)
                        return new Tuple<bool, bool>(false, false);
                    
                    //accumulation                    
                    mQuestContext.IncreaseTaskCount(mTaskContext.currentTask.mTaskId);
                    //satisfaction
                    //이 시점엔 relation을 찾을 수 없기 때문에 걍 보상을 준다.
                    Dictionary<string, float> values = mTaskContext.currentTask.GetSatisfactions(this);                    
                    foreach(var p in values) {
                        Obtain(p.Key, p.Value);
                    }
                    mTaskContext.IncreaseTaskCounter();                    

                    //Levelup 처리                           
                    bool isLevelup = checkLevelUp();
                    if(isLevelup) {
                        var reward = LevelHandler.Instance.Get(mType, mLevel);
                        if(reward != null && reward.next != null && reward.next.rewards != null) {
                            LevelUp(reward.next.rewards);
                        }
                    }
                    CallCallback(CALLBACK_TYPE.DO_TASK);
                    mTaskContext.Release();
                    CallCallback(CALLBACK_TYPE.SET_READY);

                    return new Tuple<bool, bool>(true, isLevelup);
                }
                /*
                ask, interrupt 처리
                fromActor처리
                */               
                public bool TakeTask() {
                    if(mTaskContext.state != STATE.READY)
                        return false;

                    string taskId = string.Empty;
                    float maxValue = 0.0f;                    
                    var tasks = TaskHandler.Instance.GetTasks(this); 
                    foreach(var p in tasks) {
                        float expecedValue = GetExpectedValue(p.Value);                        
                        if(taskId == string.Empty || expecedValue > maxValue) {
                            maxValue = expecedValue;
                            taskId = p.Key;
                        }
                    }   
                    if(!SetCurrentTask(taskId))
                        return false;
                    CallCallback(CALLBACK_TYPE.TAKE_TASK);
                    return true;
                }       
                private bool SetCurrentTask(string taskId) {
                    //task가져오고
                    FnTask? task = TaskHandler.Instance.GetTask(taskId);
                    if(task == null) {
                        return false;
                    }
                    //target가져오고
                    Tuple<bool, string> target = task.GetTargetObject(this);
                    //ASK 처리
                    switch(task.mInfo.target.interaction.type) {
                        case TASK_INTERACTION_TYPE.ASK:
                        if(!SetReserveToTarget(target.Item2)) 
                            return false;
                        break;
                        case TASK_INTERACTION_TYPE.INTERRUPT: //interrupt를 걸면 어차피 중단 시켜 버리니까 reserve를 하지 않는다.                        
                        break;
                        default:
                        break;
                    }                    
                    mTaskContext.Set(task, target);
                            
                    return true;
                }
                public FnTask? GetCurrentTask() {
                    return mTaskContext.currentTask;
                }       
                public STATE GetState() {
                    return mTaskContext.state;
                }
                public void SetPosition(float x, float y, float z) {
                    if(position == null) {
                        position = new Position(x, y, z);
                    } else {
                        position.x = x;
                        position.y = y;
                        position.z = z;
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
                        mQuestContext.AddSatisfaction(satisfactionId, value);
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
                                if( mTaskContext.taskCounter < t.value) {
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
                    foreach(string quest in mQuestContext.questList) {
                        if(i >= top) {
                            break;
                        }
                        ret.Add(quest);
                        i++;                        
                    }
                    return ret;
                }
                public bool RemoveQuest(string questId) {
                    return mQuestContext.questList.Remove(questId);
                }
                public double GetAccumulationSatisfaction(string satisfactionId) {
                    return mQuestContext.GetSatisfaction(satisfactionId);
                }
                // -------------------------------------------------------------------------------------------------------------                 
                private float GetExpectedValue(FnTask fn) {
                    
                    //1. satisfaction loop
                    //2. if check in fn then sum
                    //3. cal normalization
                    //4. get mean                      
                    float sum = 0;
                    float sumRefusal = 0;
                    var taskSatisfaction = fn.GetValues(this);    
                    if(taskSatisfaction == null)
                        return float.MinValue;
                        
                    float val, normVal;
                    foreach(var p in mSatisfaction) {
                        val = p.Value.Value;
                        if(taskSatisfaction.Item1.ContainsKey(p.Key)) {
                            val += taskSatisfaction.Item1[p.Key];
                        }
                        normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        sum += normVal;

                        //refusal
                        val = p.Value.Value;
                        if(taskSatisfaction.Item2.ContainsKey(p.Key)) {
                            val += taskSatisfaction.Item2[p.Key];
                        }
                        normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        sumRefusal += normVal;
                    }

                    if(taskSatisfaction.Item2.Count == 0)
                        return sum / mSatisfaction.Count;
                    else {
                        /*
                        (sum / mSatisfaction.Count) * 0.5f + (sumRefusal / mSatisfaction.Count) * 0.5f
                        = (0.5 / mSatisfaction.Count) * (sum + sumRefusal)
                        */
                        float ret = (0.5f / mSatisfaction.Count) * (sum + sumRefusal);
                        return ret;
                    }
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
                    foreach(var p in mItemContext.inventory) {
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
                    if(mItemContext.inventory.ContainsKey(itemKey)) {
                        mItemContext.inventory[itemKey] += quantity;
                    } else {
                        mItemContext.inventory[itemKey] = quantity;
                    }
                }
                //아이템 사용은 한번에 하나씩만
                public bool UseItemFromInventory(string itemKey) {
                    if(mItemContext.inventory.ContainsKey(itemKey) == false || mItemContext.inventory[itemKey] <= 0) {
                        return false;
                    }   
                    if(!InvokeItem(itemKey, 0)) {
                        return false;
                    }
                    mItemContext.inventory[itemKey] --;
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
                                if(mItemContext.installation.ContainsKey(itemKey) == false) {
                                    mItemContext.installation[itemKey] = new List<ItemUsage>();
                                }
                                mItemContext.installation[itemKey].Add(new ItemUsage(itemKey, item.invoke.expire, usage));
                            }                             
                            //발동
                            if(mItemContext.invoking.ContainsKey(itemKey) == false) {
                                mItemContext.invoking[itemKey] = new List<ItemUsage>();
                            }
                            mItemContext.invoking[itemKey].Add(new ItemUsage(itemKey, item.invoke.expire, usage));
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
