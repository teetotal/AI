using System;
using System.Collections.Generic;
using ENGINE.GAMEPLAY;
//using UnityEngine;
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
                public delegate void Callback(LOOP_STATE type, Actor actor);     
                public enum TASKCONTEXT_TARGET_TYPE {
                    INVALID,
                    NON_TARGET,
                    OBJECT,
                    ACTOR,
                    POSITION,
                    FLY
                }
                // Contexts -----------------------------------------------------------------
                public class TaskContext_Target {
                    public TASKCONTEXT_TARGET_TYPE type = TASKCONTEXT_TARGET_TYPE.INVALID;                    
                    public string objectName = string.Empty;
                    public Position? position = null;
                    public Position? lookAt = null;
                    public void Set(TASKCONTEXT_TARGET_TYPE type, string? objectName, Position? position, Position? lookAt) {
                        this.type = type;
                        if(objectName == null)
                            this.objectName = string.Empty;
                        else
                            this.objectName = objectName;

                        if(position != null) {
                            this.position = new Position(position.x, position.y, position.z);
                        }

                        if(lookAt != null) {
                            this.lookAt = new Position(lookAt.x, lookAt.y, lookAt.z);
                        }
                    }
                    public void Release() {
                        type = TASKCONTEXT_TARGET_TYPE.INVALID;                    
                        objectName = string.Empty;
                        position = null;
                        lookAt = null;
                    }
                }    
                public class TaskContext {
                    public class ReserveContext {
                        public Actor? fromActor;
                        public FnTask? fromTask;                            
                        public void Release() {
                            this.fromActor = null;
                            this.fromTask = null;
                        }      
                    }
                    // Task 수행 횟수 저장 for level up
                    public Int64 taskCounter { get; set; }
                    //마지막 task 시점의 counter
                    public Int64 lastCount { get; set; }
                    //public STATE state = STATE.READY;
                    public FnTask? currentTask = null;
                    //ask에 대한 응답 taskId저장. refusal이 아니면 이때 take task
                    public string ackTaskId = string.Empty;
                    //target이 actor일 경우 actorId
                    public TaskContext_Target target = new TaskContext_Target();
                    public ReserveContext reserveContext = new ReserveContext();
                    public Actor? GetTargetActor() {
                        if(target.type == TASKCONTEXT_TARGET_TYPE.ACTOR) {
                            var targetActor = ActorHandler.Instance.GetActor(target.objectName);
                            if(targetActor == null)
                                throw new Exception("Invalid ActorId. " + target.objectName);
                            return targetActor;
                        }
                        return null;
                    }       
                    public void Release() {
                        this.ackTaskId = string.Empty;
                        this.target.Release();
                        this.reserveContext.Release();

                        if(this.currentTask == null) 
                            return;
                        //release refcount
                        TaskHandler.Instance.ReleaseRef(this.currentTask.mTaskId);
                        this.currentTask = null;
                    }                    
                    public void Set(FnTask task, TASKCONTEXT_TARGET_TYPE targetType, string? targetName, Position? position, Position? lookAt) {
                        this.currentTask = task;                        
                        this.target.Set(targetType, targetName, position, lookAt);
                        //state = STATE.TASKED;
                        //increase refcount
                        TaskHandler.Instance.IncreaseRef(task.mTaskId);
                        this.lastCount = CounterHandler.Instance.GetCount();
                    }
                    public void IncreaseTaskCounter() {
                        taskCounter++;
                    }
                }      
                public class ItemContext {
                    //item key, quantity
                    public Dictionary<string, int> inventory = new Dictionary<string, int>();
                    //장착중인 아이템.만료처리는 mInvoking랑 같이 업데이트 해줘야함.
                    public Dictionary<string, List<ItemUsage>> installation = new Dictionary<string, List<ItemUsage>>();
                    //발동중인 아이템 리스트                
                    public Dictionary<string, List<ItemUsage>> invoking = new Dictionary<string, List<ItemUsage>>();
                    //item 뽑기 랜덤
                    public Random mRandItem = new Random();
                    public List<ConfigTask_Item> mObtainItemList = new List<ConfigTask_Item>();
                    //자원일 경우 시장에 풀린 수량을 확인
                    public bool CheckAvailable(ConfigTask_Item item) {
                        string itemId = item.itemId; 
                        int quantity = item.quantity;

                        var satisfactions = ItemHandler.Instance.GetItemInfo(itemId).satisfaction;
                        if(satisfactions == null) {
                            return true;
                        }
                        for(int i = 0; i < satisfactions.Count; i ++) {
                            ConfigSatisfaction_Define info = SatisfactionDefine.Instance.Get(satisfactions[i].satisfactionId);
                            if(info.resource) {
                                if(info.marketPrice == null)
                                    throw new Exception("market price must be existed." + satisfactions[i].satisfactionId);
                                if(SatisfactionMarketPrice.Instance.GetTotalQuantity(satisfactions[i].satisfactionId) + quantity > info.marketPrice.maxQuantity) 
                                {
                                        return false;
                                }
                            }
                        }
                        return true;
                    }
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
                private class PetContext {
                    public Dictionary<string, Actor> pets = new Dictionary<string, Actor>();
                    public bool AddPet(string actorId) {
                        if(pets.ContainsKey(actorId))
                            return false;
                        var pet = ActorHandler.Instance.GetActor(actorId);
                        if(pet == null)
                            return false;
                        pets.Add(pet.mUniqueId, pet);
                        return true;
                    }
                    public bool RemovePet(string actorId) {
                        if(!pets.ContainsKey(actorId))
                            return false;
                        pets.Remove(actorId);
                        return true;
                    }
                    public Dictionary<string, Actor> GetPets() {
                        return pets;
                    }
                    public Actor GetPet(string actorId) {
                        if(!pets.ContainsKey(actorId))
                            throw new Exception("Invalid Pet Actor Id. " + actorId);
                        return pets[actorId];
                    }
                    public Actor? GetDoingTaskPet() {
                        foreach(var pet in pets) {
                            if(pet.Value.GetState() != LOOP_STATE.READY)
                                return pet.Value;
                        }
                        return null;
                    }
                }
                // --------------------------------------------------------------------------
                public int mType;     
                public bool follower;           
                public string mUniqueId;
                public int level;
                public Position position = new Position(0, 0, 0);
                public ConfigActor_Detail mInfo;     
                private Actor? master;           
                private Callback? mCallback;
                //ask에 대한 의사결정 클래스
                private DecideClass? mDecide;
                private Dictionary<string, Satisfaction> mSatisfaction = new Dictionary<string, Satisfaction>();
                // Relation
                // Actor id, Satisfaction id, amount
                private Dictionary<string, Dictionary<string, float>> mRelation = new Dictionary<string, Dictionary<string, float>>();                
                //Task ------------------------------------------------------------------------------------------------
                private TaskContext mTaskContext = new TaskContext();     
                //표준편차 구하기 위한 임시 list 
                private List<float> mTaskTempExpectValueList = new List<float>();
                //refusal 표준편차용
                private List<float> mTaskTempRefusalExpectValueList = new List<float>();
                //Quest -----------------------------------------------------------------------------------------------
                private QuestContext mQuestContext = new QuestContext();
                //Item ------------------------------------------------------------------------------------------------
                private ItemContext mItemContext = new ItemContext();
                //Pet -------------------------------------------------------------------------------------------------
                private PetContext mPetContext = new PetContext();
                //reserved scene  --------------------------------------------------------------------------------------
                private string mReservedScene = string.Empty;
                // -----------------------------------------------------------------------------------------------------
                public Actor(string actorId, ConfigActor_Detail info, List<string> quests) {
                    this.mType = info.type;
                    this.mUniqueId = actorId;
                    this.level = info.level;
                    //set init position
                    if(info.position != null && info.position.Count == 3)
                        this.position = new Position(info.position[0], info.position[1], info.position[2]);
                    
                    //set follower
                    this.follower = info.follower;

                    this.mQuestContext.questList = quests;
                    this.mCallback = null;
                    this.mInfo = info;

                    //satisfaction
                    foreach(var s in info.satisfactions) {
                        if(s.satisfactionId == null)
                            throw new Exception("null satisfaction id");

                        SetSatisfaction(s.satisfactionId, s.min, s.max, s.value);
                    }
                    //inventory. installation 처리해야함
                    foreach(var i in info.inventory) {
                        //i.installation
                        AddInventory(i.itemId, i.quantity);
                    }
                }
                public bool IsAutoTakeable() {
                    //follower는 스스로 task를 가질 수 없고, master에 의해서 set task된다.
                    if(follower)
                        return false;
                    if(GetState() == Actor.LOOP_STATE.READY) {
                        if(GetPets().Count == 0 || mPetContext.GetDoingTaskPet() == null)
                            return true;
                    }
                        
                    return false;
                }
                public void SetVillage(string village) {
                    mInfo.village = village;
                }
                
                // Loop ===================================================================================================
                public enum LOOP_STATE {
                    INVALID,
                    READY,         
                    CHANGE_SCENE,           
                    TASK_UI,
                    TAKE_TASK,
                    MOVE,
                    ANIMATION,
                    RESERVED,
                    LOOKAT,
                    DIALOGUE,
                    DECIDE,
                    SET_TASK,
                    DO_TASK,
                    GET_IN_VEHICLE,
                    GET_OFF_VEHICLE,
                    AUTO_DO_TASK,
                    LEVELUP,
                    REFUSAL,
                    CHAIN,
                    RELEASE,
                    DISCHARGE,
                    COMPLETE_QUEST,
                    TAX_COLLECTION,
                    ITEM
                }
                private LOOP_STATE mLOOP_STATE = LOOP_STATE.INVALID;
                public LOOP_STATE GetState() {
                    return mLOOP_STATE;
                }
                public void Loop_Ready() {
                    mLOOP_STATE = LOOP_STATE.READY; 
                    CallCallback(LOOP_STATE.READY);
                    if(mReservedScene != string.Empty)
                        Loop_ChangeScene();
                }    
                public void Loop_ChangeScene() {
                    CallCallback(LOOP_STATE.CHANGE_SCENE);
                }           
                public string PopSceneName() {
                    string sz = mReservedScene;
                    mReservedScene = string.Empty;
                    return sz;
                }
                public void Loop_TaskUI() {
                    mLOOP_STATE = LOOP_STATE.TASK_UI; 
                    CallCallback(LOOP_STATE.TASK_UI);
                }
                // Loop_TakeTask ----------------------------------------
                //trigger때문에 false 할 수도 있다.
                public bool Loop_TakeTask() {
                    mLOOP_STATE = LOOP_STATE.TAKE_TASK; 
                    if(!TakeTask())
                        return false;
                    CallCallback(LOOP_STATE.TAKE_TASK);
                    return true;
                }
                private bool TakeTask() {
                    //trigger확인
                    if(!CheckTrigger()) {                        
                        return false;
                    }
                    //master와 pets중 누가 더 불행한지...
                    float minSC = GetSatisfactionCoefficient();
                    Actor actor = this;
                    foreach(var pet in mPetContext.pets) {
                        if(pet.Value.GetState() != LOOP_STATE.READY) 
                            continue;
                        float sc = pet.Value.GetSatisfactionCoefficient();
                        if(minSC > sc) {
                            minSC = sc;
                            actor = pet.Value;
                        }
                    }

                    string taskId = string.Empty;
                    /*
                    float maxValue = float.MinValue;   

                    //master tasks
                    foreach(var p in TaskHandler.Instance.GetTasks(actor)) {
                        float expecedValue = GetExpectedValue(p.Value, actor);                        
                        if(taskId == string.Empty || expecedValue > maxValue) {
                            maxValue = expecedValue;
                            taskId = p.Key;
                        }
                    } */
                    float preAvg = float.MinValue;
                    float preStd = float.MaxValue;
                    foreach(var p in TaskHandler.Instance.GetTasks(actor, string.Empty)) {
                        if(GetExpectedValue(p.Value, actor, ref preAvg, ref preStd)) {
                            taskId = p.Key;
                        }                      
                    }
                    
                    if(taskId == string.Empty) {
                        return false;
                    }
                    
                    if(actor.mUniqueId == mUniqueId) {
                        if(SetCurrentTask(taskId) != SET_TASK_ERROR.SUCCESS)
                            return false;
                        return true;
                    }
                    else {
                        //pet에게 task set하고
                        actor.Loop_SetTask(taskId);
                        //master는 false 리턴해서 다시 ready상태로.
                        return false;
                    }
                }       
                public enum SET_TASK_ERROR {
                    SUCCESS,
                    NOT_ENOUGH_SATISFACTION,
                    NOT_ENOUGH_MATERIAL_ITEM,
                    OVER_MAX_REF,
                    CHECK_INTEGRATION_FAILURE,
                    FAILURE,
                    GET_IN,
                    GET_OFF
                }
                public SET_TASK_ERROR SetCurrentTask(string taskId) {
                    //task가져오고
                    FnTask? task = TaskHandler.Instance.GetTask(taskId);
                    if(task == null) {
                        throw new Exception("Invalid Task id. " + taskId + " " + mUniqueId);
                    }
                    //check satisfaction
                    if(!TaskHandler.Instance.CheckSatisfaction(this, task))
                        return SET_TASK_ERROR.NOT_ENOUGH_SATISFACTION;
                    //check ref
                    if(!TaskHandler.Instance.CheckRef(task))
                        return SET_TASK_ERROR.OVER_MAX_REF;
                    //check material item
                    if(!TaskHandler.Instance.CheckMaterialItems(this, task))
                        return SET_TASK_ERROR.NOT_ENOUGH_MATERIAL_ITEM;
                    //check integration
                    if(!TaskHandler.Instance.CheckIntegrtion(this, task))
                        return SET_TASK_ERROR.CHECK_INTEGRATION_FAILURE;
                    //target가져오고
                    Tuple<Actor.TASKCONTEXT_TARGET_TYPE, string, Position?, Position?> target = task.GetTargetObject(this);
                    if(target.Item1 == Actor.TASKCONTEXT_TARGET_TYPE.INVALID) {
                        return SET_TASK_ERROR.FAILURE;
                    }
                    //ASK 처리
                    switch(task.mInfo.target.interaction.type) {
                        //ask와 interrupt의 차이는 ask는 거절 할 수 있지만 interrupt는 무조건 true리턴
                        case TASK_INTERACTION_TYPE.ASK:
                        case TASK_INTERACTION_TYPE.INTERRUPT: 
                        if(!SetReserveToTarget(target.Item2, task)) 
                            return SET_TASK_ERROR.FAILURE;
                        break;
                        default:
                        break;
                    }         
                    mTaskContext.Set(task, target.Item1, target.Item2, target.Item3, target.Item4);
                    if(task.mInfo.scene != string.Empty) {
                        mReservedScene = task.mInfo.scene;
                    }

                    switch(task.mInfo.target.type) {
                        case TASK_TARGET_TYPE.RESERVE_VEHICLE:
                        VehicleHandler.Instance.SetReserve(target.Item2, mUniqueId);
                        break; 
                        case TASK_TARGET_TYPE.GET_IN_VEHICLE:
                        return SET_TASK_ERROR.GET_IN;
                        case TASK_TARGET_TYPE.GET_OFF_VEHICLE:
                        return SET_TASK_ERROR.GET_OFF;
                        default:
                        break;
                    }
                    return SET_TASK_ERROR.SUCCESS;
                }
                private bool SetReserveToTarget(string targetActorId, FnTask task) {
                    var targetActor = ActorHandler.Instance.GetActor(targetActorId);
                    if(targetActor == null)
                        return false;

                    if(!targetActor.Loop_Reserved(this, task))
                        return false;                    
                    return true;                    
                }                         
                // Loop_Move -------------------------------------------------------------------------------
                public void Loop_Move() {
                    mLOOP_STATE = LOOP_STATE.MOVE; 
                    CallCallback(LOOP_STATE.MOVE);
                }
                public void Loop_Animation() {
                    mLOOP_STATE = LOOP_STATE.ANIMATION; 
                    CallCallback(LOOP_STATE.ANIMATION);
                }
                public bool Loop_Reserved(Actor from, FnTask fromTask) {
                    if(mLOOP_STATE != LOOP_STATE.READY) 
                        return false;                    

                    mTaskContext.reserveContext.fromActor = from;
                    mTaskContext.reserveContext.fromTask = fromTask;

                    mLOOP_STATE = LOOP_STATE.RESERVED; 
                    CallCallback(LOOP_STATE.RESERVED);
                    return true;
                }
                public bool Loop_LookAt() {
                    mLOOP_STATE = LOOP_STATE.LOOKAT; 
                    CallCallback(LOOP_STATE.LOOKAT);
                    return true;
                }
                public void Loop_Dialogue() {
                    mLOOP_STATE = LOOP_STATE.DIALOGUE; 
                    var task = GetCurrentTask();
                    if(task == null)
                        throw new Exception("DoTask failure. The current task must exist.");

                    if(task.mInfo.target.interaction.type == TASK_INTERACTION_TYPE.ASK || task.mInfo.target.interaction.type == TASK_INTERACTION_TYPE.INTERRUPT)  {                        
                        CallCallback(LOOP_STATE.DIALOGUE);
                    } else {
                        throw new Exception("Invalid interaction type." + task.mInfo.target.interaction.type);
                    }
                }
                // Loop Decide ---------------------------------------------------------------------
                public bool Loop_Decide() {
                    mLOOP_STATE = LOOP_STATE.DECIDE;                     
                    return Decide(GetTaskContext().reserveContext.fromActor, GetTaskContext().reserveContext.fromTask);
                }
                private bool Decide(Actor? asker, FnTask? task) {
                    if(asker == null || task == null) 
                        throw new Exception("Invalid asker or TaskId.");
                    if(mDecide != null)
                        return mDecide.Decide(this, asker, task);
                    return true;
                }
                // Loop SetTask ----------------------------------------------------------------------
                public SET_TASK_ERROR Loop_SetTask(string taskId) {
                    mLOOP_STATE = LOOP_STATE.SET_TASK; 
                    SET_TASK_ERROR err = SetCurrentTask(taskId);
                    switch(err) {
                        case SET_TASK_ERROR.SUCCESS:
                        CallCallback(LOOP_STATE.SET_TASK);
                        break;
                        case SET_TASK_ERROR.GET_IN:
                        mLOOP_STATE = LOOP_STATE.GET_IN_VEHICLE; 
                        CallCallback(LOOP_STATE.GET_IN_VEHICLE);
                        break;
                        case SET_TASK_ERROR.GET_OFF:
                        mLOOP_STATE = LOOP_STATE.GET_OFF_VEHICLE; 
                        CallCallback(LOOP_STATE.GET_OFF_VEHICLE);
                        break;
                        default:
                        mLOOP_STATE = LOOP_STATE.TASK_UI; 
                        break;
                    }
                    /*
                    if(err != SET_TASK_ERROR.SUCCESS) {
                        mLOOP_STATE = LOOP_STATE.TASK_UI; 
                        return err;
                    }
                    CallCallback(LOOP_STATE.SET_TASK);
                    */
                    return err;
                }
                // LOOP Auto do task
                public void Loop_AutoDoTask(string taskId) {
                    mLOOP_STATE = LOOP_STATE.AUTO_DO_TASK; 
                    SetCurrentTask(taskId);
                    DoTask(false);
                    CallCallback(LOOP_STATE.AUTO_DO_TASK);
                }
                // Do Task & Refusal ----------------------------------------------------------------------------------------------------
                public void Loop_DoTask() {
                    mLOOP_STATE = LOOP_STATE.DO_TASK; 
                    DoTask(false);
                    CallCallback(LOOP_STATE.DO_TASK);                    
                }
                public void Loop_GetInVehicle() {
                    mLOOP_STATE = LOOP_STATE.GET_IN_VEHICLE;
                    CallCallback(LOOP_STATE.GET_IN_VEHICLE);    
                }
                public void Loop_GetOffVehicle() {
                    mLOOP_STATE = LOOP_STATE.GET_OFF_VEHICLE;
                    CallCallback(LOOP_STATE.GET_OFF_VEHICLE);
                }
                public void Loop_Refusal() {
                    mLOOP_STATE = LOOP_STATE.REFUSAL; 
                    DoTask(true);
                    CallCallback(LOOP_STATE.REFUSAL);                    
                }               
                //ret DoTask, islevelup
                private void DoTask(bool isRefusal) {
                    var task = GetCurrentTask();
                    if(task == null || task.mTaskId == null)
                        throw new Exception("DoTask failure. The current task must exist.");
                    //accumulation                    
                    mQuestContext.IncreaseTaskCount(task.mTaskId);
                    //satisfaction                    
                    Dictionary<string, float> values = isRefusal? task.GetSatisfactionsRefusal() : task.GetSatisfactions();
                    string? from = null;                        
                    if(task.mInfo.type == TASK_TYPE.REACTION) {
                        if(mTaskContext.reserveContext.fromActor == null) throw new Exception("Invalid interaction from actor.");
                        from = mTaskContext.reserveContext.fromActor.mUniqueId;
                    }                            
                    else if(task.mInfo.target.interaction.type == TASK_INTERACTION_TYPE.ASK || task.mInfo.target.interaction.type == TASK_INTERACTION_TYPE.INTERRUPT) 
                        from = mTaskContext.target.objectName;

                    foreach(var p in values) {                        
                        Obtain(p.Key, p.Value, from);
                    }
                    if(!isRefusal) {
                        //item 랜덤 함수
                        bool isWin = false;
                        for(int i = 0; i < task.mInfo.items.Count; i++) {
                            ConfigTask_Item item = task.mInfo.items[i];
                            if(!mItemContext.CheckAvailable(item)) //시장에 풀린 수량 확인
                                continue;
                            int luckyNumber = mItemContext.mRandItem.Next(item.totalRange);
                            if(item.winRange > luckyNumber) {
                                //win
                                isWin = true;
                                mItemContext.mObtainItemList.Add(item);
                                AddInventory(item.itemId, item.quantity);
                            }
                        }
                        if(isWin)
                            CallCallback(LOOP_STATE.ITEM);
                    }
                    //아이템 소비
                    for(int i = 0; i < task.mInfo.materialItems.Count; i++) {
                        Config_Reward material = task.mInfo.materialItems[i];
                        if(!SpendMaterialItem(material.itemId, material.quantity))
                            throw new Exception("SpendMaterialItem Failure");
                    }
                    //integration
                    TaskHandler.Instance.Integration(this, task);
                }
                // --------------------------------------------------------------------------------------------------
                public void Loop_Levelup() {
                    mLOOP_STATE = LOOP_STATE.LEVELUP; 
                    mTaskContext.IncreaseTaskCounter();
                    //Levelup 처리                                               
                    if(CheckLevelUp()) {
                        var reward = LevelHandler.Instance.Get(mType, level);
                        if(reward != null && reward.next != null && reward.next.rewards != null) {
                            LevelUp(reward.next.rewards);                            
                        }
                        CallCallback(LOOP_STATE.LEVELUP);
                    } else {
                        Loop_Release();
                    } 
                }
                
                //콜백 없음
                public void Loop_Chain() {
                    mLOOP_STATE = LOOP_STATE.CHAIN; 
                    var task = GetCurrentTask();
                    if(task == null || task.mTaskId == null)
                        throw new Exception("DoTask failure. The current task must exist.");
                    
                    //chain
                    if(task.mInfo.chain != null && task.mInfo.chain != string.Empty) {     
                        mTaskContext.Release();                   
                        Loop_SetTask(task.mInfo.chain);                        
                    } else {
                        Loop_Levelup();                        
                    }                 
                }          
                public void Loop_Release() {
                    mLOOP_STATE = LOOP_STATE.RELEASE; 
                    mTaskContext.Release();
                    CallCallback(LOOP_STATE.RELEASE);                    
                }            
                // =====================================================================================================
                // callback -----------------------------------------------------------------------------------------------
                public void SetCallback(Callback fn) {
                    mCallback = fn;
                    Loop_Release();
                }
                public void SetDecideFn(DecideClass fn) {
                    mDecide = fn;
                }
                public void CallCallback(LOOP_STATE state) {
                    if(mCallback != null) {
                        mCallback(state, this);
                    }
                }      
                //----------------------------------------------------------------------------------------------------------
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
                public string GetTaskString(bool includeState = false) {
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
                            if(includeState)
                                sz += String.Format("{0}: {1}({2}%) ", s.title, p.Value, (int)(GetNormValue(mSatisfaction[p.Key]) * 100));
                            else 
                                sz += String.Format("{0}: {1} ", s.title, p.Value);
                        }                        
                    }
                    //sz += mTaskContext.target.ToString();
                    return sz;
                }
                //Satisfaction update ---------------------------------------------------------------------------------------------------------------------
                public void Discharge(string satisfactionId, float amount) {                    
                    ApplySatisfaction(satisfactionId, -amount, 0, null, true);
                    CallCallback(LOOP_STATE.DISCHARGE);
                }
                public void Obtain(string satisfactionId, float amount, string? from) {
                    ApplySatisfaction(satisfactionId, amount, 0, from);
                }
                // task ---------------------------------------------------------------------
                public TaskContext GetTaskContext() {
                    return mTaskContext;
                }
                public Actor GetAsker() {
                    if(mTaskContext.reserveContext.fromActor == null)
                        throw new Exception("Null Reserved Actor");
                    return mTaskContext.reserveContext.fromActor;
                }
                public Actor? GetTargetActor() {
                    if(mTaskContext.target.type == TASKCONTEXT_TARGET_TYPE.ACTOR)
                        return ActorHandler.Instance.GetActor(mTaskContext.target.objectName);
                    return null;
                }

                public string GetCurrentTaskId() {
                    var task = GetCurrentTask();
                    if(task == null)
                        return string.Empty;
                    return task.mTaskId;
                }
                public FnTask? GetCurrentTask() {
                    return mTaskContext.currentTask;
                }       
                public string GetCurrentTaskTitle() {
                    if(mTaskContext.currentTask != null) {
                        return mTaskContext.currentTask.mInfo.title;
                    }
                    return string.Empty;
                }
                // pets ------------------------------------------------------   
                public void SetPets() {
                    //set pets
                    for(int i =0; i < mInfo.pets.Count; i++) {
                        if(!mPetContext.AddPet(mInfo.pets[i]))
                            throw new Exception("Adding Pet Failure. " + mInfo.pets[i]);
                        Actor pet = mPetContext.GetPet(mInfo.pets[i]);
                        pet.SetMaster(this);
                    }
                }
                public Dictionary<string, Actor> GetPets() {
                    return mPetContext.pets;
                }
                public bool HasPet() {
                    return (mPetContext.GetPets().Count > 0) ? true : false;
                }
                public Actor? GetDoingTaskPet() {
                    return mPetContext.GetDoingTaskPet();
                }
                public void SetMaster(Actor actor) {
                    master = actor;
                }
                public Actor GetMaster() {
                    if(master == null)
                        throw new Exception("Null Master");
                    return master;
                }
                public double GetDistanceToMaster() {
                    if(!follower || master == null)
                        return -1;
                    return position.GetDistance(master.position);
                }
                public double GetDistanceToDoingPet() {
                    var pet = GetDoingTaskPet();
                    if(follower || pet == null)
                        return -1;
                    
                    return position.GetDistance(pet.position);
                }
                
                // position ------------------------------------------------------              
                public void SetPosition(float x, float y, float z) {
                    if(position == null) {
                        position = new Position(x, y, z);
                    } else {
                        position.x = x;
                        position.y = y;
                        position.z = z;
                    }
                }
                // satisfaction ----------------------------------------------------
                public Satisfaction? GetSatisfaction(string id) {
                    if(mSatisfaction.ContainsKey(id)) {
                        return mSatisfaction[id];
                    }
                    return null;        
                }
                public Dictionary<string, Satisfaction> GetSatisfactions() {
                    return mSatisfaction;
                }
                public void ApplySatisfaction(string satisfactionId, float amount, int measure, string? from, bool skipAccumulation = false) {
                    if(mSatisfaction.ContainsKey(satisfactionId) == false) {
                        throw new Exception("Invalid Satisfaction. " + satisfactionId + " " + mUniqueId);
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
                    //Max를 초과할 수 없다
                    value = MathF.Min(mSatisfaction[satisfactionId].Max - mSatisfaction[satisfactionId].Value, value);

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
                }               
                //행복지수 
                public float GetSatisfactionCoefficient() {
                    float sum = 0;
                    foreach(var p in mSatisfaction) {
                        float normVal = GetNormValue(p.Value.Value, p.Value.Min, p.Value.Max);
                        sum += normVal;
                    }
                    return sum / mSatisfaction.Count;
                }
                /*
                public string GetMyMinSatisfaction() {
                    float val = 0;
                    string key = string.Empty;
                    foreach(var p in mSatisfaction) {
                        float normVal = GetNormValue(p.Value.Value, p.Value.Min, p.Value.Max);
                        if(key == string.Empty || val > normVal) {
                            val = normVal;
                            key = p.Key;
                        }
                    }
                    return key;
                } 
                */              
                /*
                public float GetExpectedValue(FnTask fn, Actor actor) {
                    //1. satisfaction loop
                    //2. if check in fn then sum
                    //3. cal normalization
                    //4. get mean                      
                    float sum = 0;
                    float sumRefusal = 0;
                    var taskSatisfaction = fn.GetValues(actor);    
                    if(taskSatisfaction == null)
                        return float.MinValue;
                        
                    mTaskTempExpectValueList.Clear();
                    mTaskTempRefusalExpectValueList.Clear();
                    float val, normVal;

                    foreach(var p in mSatisfaction) {
                        //task reward
                        val = p.Value.Value;
                        if(taskSatisfaction.Item1.ContainsKey(p.Key)) {
                            val += taskSatisfaction.Item1[p.Key];
                        }
                        normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        mTaskTempExpectValueList.Add(normVal);
                        sum += normVal;

                        //refusal
                        val = p.Value.Value;
                        if(taskSatisfaction.Item2.ContainsKey(p.Key)) {
                            val += taskSatisfaction.Item2[p.Key];
                        }
                        normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        mTaskTempRefusalExpectValueList.Add(normVal);
                        sumRefusal += normVal;
                    }

                    //std
                    float avg = sum / mSatisfaction.Count;
                    float std = 0;
                    for(int i=0; i < mTaskTempExpectValueList.Count; i++) {
                        std += MathF.Pow(mTaskTempExpectValueList[i] - avg, 2);
                    }
                    std = MathF.Sqrt(std / mSatisfaction.Count);

                    //std refusal
                    float avgRefusal = sumRefusal / mSatisfaction.Count;
                    float stdRefusal = 0;
                    for(int i=0; i < mTaskTempRefusalExpectValueList.Count; i++) {
                        stdRefusal += MathF.Pow(mTaskTempRefusalExpectValueList[i] - avgRefusal, 2);
                    }
                    stdRefusal = MathF.Sqrt(stdRefusal / mSatisfaction.Count);

                    if(taskSatisfaction.Item2.Count == 0) {
                        //return avg / std;
                        return avg;
                    } else {
                        //기대값 
                        //(sum / mSatisfaction.Count) * 0.5f + (sumRefusal / mSatisfaction.Count) * 0.5f
                        //여기서 확률을 relation으로 계산해서 받아온다.
                
                        // relation에서 weight
                        var target = fn.GetTargetObject(actor);
                        if(target.Item1 != TASKCONTEXT_TARGET_TYPE.ACTOR) {
                            //error!
                            throw new Exception("Invalid target info. the target of [" + fn.mTaskTitle + "] must ACTOR. " + target.Item1.ToString());
                        }
                        float expectedWeight = GetExpectedWeight(target.Item2);
                        //float ret = ((avg / std) * expectedWeight) + ((avgRefusal / stdRefusal) * (1.0f - expectedWeight)); 
                        float ret = (avg * expectedWeight) + (avgRefusal * (1.0f - expectedWeight)); 
                        return ret;
                    }
                }*/
                public bool GetExpectedValue(FnTask fn, Actor actor, ref float preAvg, ref float preStd) {
                    //1. satisfaction loop
                    //2. if check in fn then sum
                    //3. cal normalization
                    //4. get mean                      
                    float sum = 0;
                    float sumRefusal = 0;
                    var taskSatisfaction = fn.GetValues(actor);    
                    if(taskSatisfaction == null)
                        return false;
                        
                    mTaskTempExpectValueList.Clear();
                    mTaskTempRefusalExpectValueList.Clear();
                    float val, normVal;

                    foreach(var p in mSatisfaction) {
                        //task reward
                        val = p.Value.Value;
                        if(taskSatisfaction.Item1.ContainsKey(p.Key)) {
                            val += taskSatisfaction.Item1[p.Key];
                        }
                        normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        mTaskTempExpectValueList.Add(normVal);
                        sum += normVal;

                        //refusal
                        val = p.Value.Value;
                        if(taskSatisfaction.Item2.ContainsKey(p.Key)) {
                            val += taskSatisfaction.Item2[p.Key];
                        }
                        normVal = GetNormValue(val, p.Value.Min, p.Value.Max);
                        mTaskTempRefusalExpectValueList.Add(normVal);
                        sumRefusal += normVal;
                    }

                    //std
                    float avg = sum / mSatisfaction.Count;
                    float std = 0;
                    for(int i=0; i < mTaskTempExpectValueList.Count; i++) {
                        std += MathF.Pow(mTaskTempExpectValueList[i] - avg, 2);
                    }
                    std = MathF.Sqrt(std / mSatisfaction.Count);

                    //std refusal
                    float avgRefusal = sumRefusal / mSatisfaction.Count;
                    float stdRefusal = 0;
                    for(int i=0; i < mTaskTempRefusalExpectValueList.Count; i++) {
                        stdRefusal += MathF.Pow(mTaskTempRefusalExpectValueList[i] - avgRefusal, 2);
                    }
                    stdRefusal = MathF.Sqrt(stdRefusal / mSatisfaction.Count);

                    if(taskSatisfaction.Item2.Count == 0) {
                        //if(actor.mUniqueId == "ACTOR1-2")
                        //    Debug.Log(string.Format("{0} {1} AVG:{2}, STD:{3}", actor.mUniqueId, fn.mTaskId, avg, std));
                        // 평균이 높은걸 선택, 평균이 같으면 편차가 작은걸 선택 
                        if(avg > preAvg || (avg == preAvg && std < preStd) ) {
                            preAvg = avg;
                            preStd = std;
                            return true;
                        }
                        return false;
                    } else {
                        //기대값 
                        //(sum / mSatisfaction.Count) * 0.5f + (sumRefusal / mSatisfaction.Count) * 0.5f
                        //여기서 확률을 relation으로 계산해서 받아온다.
                
                        // relation에서 weight
                        var target = fn.GetTargetObject(actor);
                        if(target.Item1 != TASKCONTEXT_TARGET_TYPE.ACTOR) {
                            //error!
                            throw new Exception("Invalid target info. the target of [" + fn.mTaskTitle + "] must ACTOR. " + target.Item1.ToString());
                        }
                        float expectedWeight = GetExpectedWeight(target.Item2);
                        //float ret = ((avg / std) * expectedWeight) + ((avgRefusal / stdRefusal) * (1.0f - expectedWeight)); 
                        float retAvg = (avg * expectedWeight) + (avgRefusal * (1.0f - expectedWeight)); 
                        float retStd = (std * expectedWeight) + (stdRefusal * (1.0f - expectedWeight)); 
                        if(retAvg > preAvg || (retAvg == preAvg && retStd < preStd) ) {
                            preAvg = retAvg;
                            preStd = retStd;
                            return true;
                        }
                        return false;
                    }
                }
                private float GetExpectedWeight(string targetAcrotId) {
                    if(mRelation.ContainsKey(targetAcrotId) == false)
                        return 0.5f;
                    return 0.5f;
                }
                private float GetNormValue(Satisfaction p) {
                    return GetNormValue(p.Value, p.Min, p.Max);
                }
                private float GetNormValue(float value, float min, float max) {   
                    float v = (value - min) / (max - min);
                    if(value > max)
                        return 1;
                    else if(v < 0) {
                        return v * 2;
                    }
                    else {
                        return v; 
                    }
                }
                /*
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
                }*/
                /*
                //return satisfaction id                
                public Tuple<string, float> GetMotivation()
                {                                                            
                    //1. get mean
                    //2. finding max(norm(value) - avg)
                    string satisfactionId = string.Empty;//mSatisfaction.First().Key;
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
                */
                private float GetMean() {
                    float sum = 0.0f;
                    foreach(var p in mSatisfaction) {
                        Satisfaction v = p.Value;
                        sum += GetNormValue(v.Value, v.Min, v.Max);
                    }
                    return sum / mSatisfaction.Count;
                }
                // Trigger-------------------------------------------------------------------------------------------------------------
                private bool CheckTrigger() {
                    //null이거나 NO_TRIGGER면 항상 true
                    if(mInfo.trigger == null)
                        return true;
                    switch(mInfo.trigger.type) {
                        case TRIGGER_TYPE.NO_TRIGGER:
                        return true;
                        case TRIGGER_TYPE.DISTANCE: {
                            float distance = float.Parse(mInfo.trigger.value);
                            var actors = ActorHandler.Instance.GetActors();                            
                            foreach(var actor in actors) {
                                if(actor.Key == mUniqueId)
                                    continue;
                                
                                if(position.GetDistance(actor.Value.position) <= distance)
                                    return true;
                            }
                        }
                        break;
                    }
                    return false;
                }                
                public string LookAround() {
                    //주변에 가장 먼저 보이는 actorid 리턴   
                    if(mInfo.trigger == null || mInfo.trigger.value == null || mInfo.trigger.value == string.Empty)
                        return string.Empty;
                    float distance = float.Parse(mInfo.trigger.value);
                    var actors = ActorHandler.Instance.GetActors();                            
                    foreach(var actor in actors) {
                        if(actor.Key == mUniqueId)
                            continue;
                        
                        if(position.GetDistance(actor.Value.position) <= distance)
                            return actor.Key;
                    }
                    return string.Empty;
                }
                public float GetDistance(string actorId) {
                    var target = ActorHandler.Instance.GetActor(actorId);   
                    if(target == null)
                        throw new Exception("Invalid actorId. " + actorId);
                    return (float)position.GetDistance(target.position);
                }

                // Level up-------------------------------------------------------------------------------------------------------------
                public float GetLevelUpProgress() {
                    float sum = 0;
                    int count = 0;
                    var info = LevelHandler.Instance.Get(mType, level);
                    if(info != null && info.next != null && info.next.threshold != null) {                        
                        foreach(Config_KV_SF t in info.next.threshold) {
                            if(t.key is null) {
                                return -1;
                            }

                            count++;

                            switch(t.key.ToUpper()) {
                                case "TASKCOUNTER":
                                sum += mTaskContext.taskCounter / t.value;
                                break;                                
                            }

                        }
                    }
                    return count == 0 ? 0: sum / count;
                }
                public bool CheckLevelUp() {
                    //check level up                   
                    var info = LevelHandler.Instance.Get(mType, level);
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
                    level++;
                    
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
                    bool ret = mQuestContext.questList.Remove(questId);
                    if(ret) {
                        CallCallback(LOOP_STATE.COMPLETE_QUEST);
                    }
                    return ret;
                }
                public double GetAccumulationSatisfaction(string satisfactionId) {
                    return mQuestContext.GetSatisfaction(satisfactionId);
                }
                // -------------------------------------------------------------------------------------------------------------  
                
                //Item----------------------------------------------------------------------------------------------------------
                public ItemContext GetItemContext() {
                    return mItemContext;
                }
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
                public int GetItemQuantityInInventory(string itemKey) {
                    if(mItemContext.inventory.ContainsKey(itemKey))
                        return mItemContext.inventory[itemKey];
                    return 0;
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
                //
                private bool SpendMaterialItem(string itemId, int quantity) {
                    if(!mItemContext.inventory.ContainsKey(itemId))
                        throw new Exception("Invalid itemd id. " + itemId);
                    
                    if(mItemContext.inventory[itemId] < quantity) {
                        return false;
                    }
                    mItemContext.inventory[itemId] -= quantity;
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

                    for(int i =0; i < list.Count; i++) {
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
                            mSatisfaction[p.satisfactionId].Value = MathF.Min(p.value, mSatisfaction[p.satisfactionId].Max);
                            break;
                            case ITEM_SATISFACTION_MEASURE.PERCENT:
                            mSatisfaction[p.satisfactionId].Value += MathF.Min((mSatisfaction[p.satisfactionId].Value * (p.value / 100)), mSatisfaction[p.satisfactionId].Max - mSatisfaction[p.satisfactionId].Value);
                            break;
                            case ITEM_SATISFACTION_MEASURE.INCREASE:
                            mSatisfaction[p.satisfactionId].Value += MathF.Min(p.value, mSatisfaction[p.satisfactionId].Max - mSatisfaction[p.satisfactionId].Value);
                            break;
                        }                                                
                    }

                    return true;
                }
            }
        }
    }
}
