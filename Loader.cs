using System;
using System.Collections.Generic;
using System.IO;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            using Newtonsoft.Json;
            //공통           
            public class Config_KV_SF {
                public string key { get; set; } = string.Empty;
                public float value { get; set; }
            }            
            //-------------------------------------------------------------
            public class Config_Reward {
                public string itemId { get; set; } = string.Empty;
                public int quantity { get; set; }
            }
            /*
            Item
            {
                "item1": {
                    "name": "무기1",
                    "desc": "",
                    "category": "무기", //카테고리
                    "type": "공격", //서브 카테고리 
                    "level": 1,
                    "cost": 10, //강화 재료로 쓰일경우 가치
                    "installationKey": "weapon:attack:hand", //장착 위치에 대한 정보
                    "invoke": {
                        "type": 1, //발동타입, 0 받자마자 발동, 1 가방에 저장
                        "expire": 0 // 발동되고 얼마동안 유지되나 0 영원히
                    },
                    "satisfaction": [
                        { //모든 능력치는 여기에 정의
                            "satisfactionId": "101",
                            "min": 10,
                            "max": 10,
                            "value": 10,
                            "measure": {
                                "min": 0, //0 절대값, 1 percent
                                "max": 0,
                                "value": 0
                            }                            
                        }
                    ],                    
                    "draft": [ //강화 단계별 비용(min,max) 정의
                        [50, 100],
                        [150, 200],
                        [250, 300]
                    ] 
                }
            }
            */            
            //Item ---------------------------------------------------------------    
            public enum ITEM_CATEGORY : int {
                SYSTEM = -1, //level up 같은 시스템 보상용 아이템
                SATISFACTION_ONLY = 0,
                WEAPON,
                VEHICLE,
                FARMING,
                COOKING
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
            public class ConfigItem_Detail {
                public string name { get; set; } = string.Empty;
                public string desc { get; set; } = string.Empty;                
                public ITEM_CATEGORY category { get; set; } = ITEM_CATEGORY.SYSTEM;
                public string type { get; set; } = string.Empty;
                public int level { get; set; }
                public int cost { get; set; }
                public string installationKey { get; set; } = string.Empty;          
                public ConfigItem_Invoke? invoke { get; set; }
                public List<ConfigItem_Satisfaction>? satisfaction { get; set; }                
                public List<int[]>? draft { get; set; }
                public bool purchasable { get; set; } = false;
                public float price { get; set; }
            }
            public class ConfigItem_Invoke {
                public ITEM_INVOKE_TYPE type { get; set; }
                public int expire { get; set; }
            }
            public class ConfigItem_Satisfaction {
                public string satisfactionId { get; set; } = string.Empty;
                public float min { get; set; }
                public float max { get; set; }
                public float value { get; set; }
                public ConfigItem_Satisfaction_Measure? measure { get; set; }

            }
            public class ConfigItem_Satisfaction_Measure {
                public ITEM_SATISFACTION_MEASURE min { get; set; }
                public ITEM_SATISFACTION_MEASURE max { get; set; }
                public ITEM_SATISFACTION_MEASURE value { get; set; }
                
            }
            //happening -----------------------------------------------------------
            public class ConfigSatisfaction_Happening {
                public List<int>? types { get; set; } //대상 actor type
                public int id { get; set; } //happening id
                public string? title { get; set; }
                public string? desc { get; set; }
                public float range1 { get; set; }
                public float range2 { get; set; }
                public float amount { get; set; }
                public int measure { get; set; } //단위 0: 절대값, 1: percent
            }
            //Satisfaction ---------------------------------------------------------
            public class ConfigSatisfaction_Seed {
                public int min { get; set; }
                public int max { get; set; }
                public int random { get; set; }
            }
            //자원의 경우 시장 가격 책정 정보
            public class ConfigSatisfaction_MarketPrice {
                public float gradient { get; set; } //수요곡선 기울기
                public float bias { get; set; } // 수요곡선 절편
                public float maxQuantity { get; set; } //시장에 풀릴 수 있는 최대 수량
            }
            public enum SATISFACTION_TYPE {
                SATISFACTION = 0,
                RESOURCE,
                CURRENCY
            }
            public class ConfigSatisfaction_Define {
                public string satisfactionId { get; set; } = string.Empty;
                public string title { get; set; } = string.Empty;
                public SATISFACTION_TYPE type { get; set; }
                public float discharge { get; set; }
                public int period { get; set; }
                public float defaultPrice { get; set; } = -1;
                public string desc { get; set; } = string.Empty;
                /*
                public ConfigSatisfaction_Seed? seed { get; set; }
                public ConfigSatisfaction_MarketPrice? marketPrice { get; set; }
                */
            }                       
        
            //Actors ------------------------------------------------------------------
            public class ConfigActor_Inventory {
                public string itemId { get; set; } = string.Empty;
                public int quantity { get; set; }
                public bool installation { get; set; }
            }
            public class ConfigActor_Satisfaction {
                public string? satisfactionId { get; set; }
                public float min { get; set; }
                public float max { get; set; }
                public float value { get; set; }
            }      
            public class ConfigActor_Detail {
                public bool enable { get; set; }
                public string village { get; set; } = string.Empty;
                public bool follower { get; set; }
                public int type { get; set; }
                public string nickname {get; set; } = string.Empty;
                public List<string> pets { get; set; } = new List<string>();
                public int level { get; set; }
                public string? prefab { get; set; }
                public List<float> position { get; set; } = new List<float>();
                public List<float> rotation { get; set; } = new List<float>();      
                public ConfigActor_Trigger? trigger { get; set; }          
                public List<ConfigActor_Satisfaction> satisfactions { get; set; } = new List<ConfigActor_Satisfaction>();
                public List<ConfigActor_Inventory> inventory { get; set; } = new List<ConfigActor_Inventory>();
                public bool isFly { get; set; } = false;
                public int laziness;
            }
            public enum TRIGGER_TYPE {
                NO_TRIGGER,
                DISTANCE
            }
            public class ConfigActor_Trigger {
                public TRIGGER_TYPE type { get; set; }
                public string value { get; set; } = String.Empty;
            }
            //Task ---------------------------------------------------------------         
            public enum TASK_TYPE : int {
                NORMAL = 0,
                REACTION,
                CHAIN,
            }  
            public enum TASK_TARGET_TYPE : int {
                NON_TARGET = 0,
                OBJECT,
                ACTOR,
                ACTOR_CONDITION,
                ACTOR_FROM, //interaction을 건 상대
                POSITION, //좌표
                FLY, //공중 좌표
                RESERVE_VEHICLE,
                GET_IN_VEHICLE,
                GET_OFF_VEHICLE
            }
            public enum TASK_INTERACTION_TYPE : int {
                NO_INTERACTION = 0,
                ASK,
                INTERRUPT
            }
            public class ConfigTask_Detail {
                public string id { get; set; } = string.Empty;// task 고유 id
                public string chain { get; set; } = string.Empty;// task 고유 id
                public TASK_TYPE type { get; set; }
                public List<int> level { get; set; } = new List<int>(); //사용가능한 Actor 최소 레벨, 최대 레벨
                public string village { get; set; } = string.Empty;
                public int villageLevel { get; set; }  = -1;//해금되는 부락 레벨
                public string title { get; set; } = string.Empty;
                public string desc { get; set; } = string.Empty;
                //Task에 의한 보상은 고정값으로 하고, %로 보상하는건 아이템 같은걸로 하자.
                public ConfigTask_Target target { get; set; } = new ConfigTask_Target();
                public string animation { get; set; } = string.Empty;
                public int animationRepeatTime { get; set; }
                //동시 실행 최대 값
                public int maxRef { get; set; }
                public Dictionary<string, string> satisfactions { get; set; } = new Dictionary<string, string>();
                public Dictionary<string, string> satisfactionsRefusal { get; set; } = new Dictionary<string, string>();
                public List<ConfigTask_Item> items { get; set; } = new List<ConfigTask_Item>();
                public List<Config_Reward> materialItems { get; set; } = new List<Config_Reward>();
                public string integration { get; set; } = string.Empty;
                public string scene { get; set; } = string.Empty;
            }        
            public class ConfigTask_Item {
                public string itemId { get; set; } = string.Empty;
                public int quantity { get; set; }
                public int winRange { get; set; }
                public int totalRange { get; set; }
            } 
            public class ConfigTask_Interaction {
                public TASK_INTERACTION_TYPE type { get; set; }
                public string? taskId { get; set; }
                
            }
            public class ConfigTask_Target {
                public TASK_TARGET_TYPE type { get; set; }
                public List<string>? value { get; set; }
                public ConfigTask_Interaction interaction { get; set; } = new ConfigTask_Interaction();
                //Parsing에서 처리
                public bool isAssignedPosition = false;
                public Position position = new Position(-1, -1, -1);
                public Position lookAt = new Position(-1, -1, -1);
            }

            //Level ---------------------------------------------------------------
            public class ConfigLevel {
                public int startLevel { get; set; }
                public List<ConfigLevel_Detail>? levels { get; set; }
            }
            public class ConfigLevel_Detail {
                public int level { get; set; }
                public string title { get; set; } = string.Empty;
                public ConfigLevel_Next? next { get; set; }
            }  
            public class ConfigLevel_Next {
                public List<Config_KV_SF>? threshold { get; set; }
                public List<Config_Reward>? rewards { get; set; }                
            }            
            //Quest ----------------------------------------------------------------------
            public class ConfigQuest {
                public int top { get; set; } //화면에 몇개씩 노출시킬것인가
                public List<ConfigQuest_Detail>? quests { get; set; }
            }
            public class ConfigQuest_Detail {
                public string? id { get; set; }
                public string? title { get; set; }
                public string? desc { get; set; }
                public List<Config_KV_SF>? values { get; set; }     
                public List<Config_Reward>? rewards  { get; set; }     
            }
            //Script ----------------------------------------------------------------------
            public class ConfigScript {
                public Dictionary<string, List<string>>? refusal { get; set; }
                public Dictionary<string, List<string>>? scripts { get; set; }
            }
            //Scenario ----------------------------------------------------------------------
            public enum SCENARIO_NODE_TYPE {
                FROM_STOP = 0,
                FROM_SAY,
                FROM_ANIMATION,
                FROM_SAY_ANIMATION,
                FROM_REACTION,   
                TO_STOP = 10,
                TO_FEEDBACK = 11,
                TO_DECIDE,
                TO_RELEASE,
            }
            public class ConfigScenario_Node {
                public int time { get; set; }
                public SCENARIO_NODE_TYPE type { get; set; }
            }
            public class ConfigScenario_Detail {
                public List<ConfigScenario_Node>? from { get; set; }
                public List<ConfigScenario_Node>? to { get; set; }
            }
            // Village --------------------------------------------------------------------------
            public class ConfigVillage_Level_Threshold {
                public int level { get; set; }
                public Dictionary<string, int> satisfactions { get; set; } = new Dictionary<string, int>();
            }
            public class ConfigVillage_Level {
                public int current { get; set; }
                //level, satisfaction id, amount
                public Dictionary<string, Dictionary<string, int>> threshold { get; set; } = new Dictionary<string, Dictionary<string, int>>();
            }
            public class ConfigVillage_Detail {
                public string name { get; set; } = string.Empty;
                public string desc { get; set; } = string.Empty;
                public int collectionDuration { get; set; }
                public Dictionary<string, float> finances { get; set; } = new Dictionary<string, float>();
                public Dictionary<string, float> tax { get; set; } = new Dictionary<string, float>();
                public ConfigVillage_Level level { get; set; } = new ConfigVillage_Level();
            }
            // Vehicle --------------------------------------------------------------------------
            public class ConfigVehicle_Position {
                public string position { get; set; } = string.Empty;
                public string rotation { get; set; } = string.Empty;
            }
            public class ConfigVehicle_Detail {
                public string type { get; set; } = string.Empty;
                public string vehicleId { get; set; } = string.Empty;
                public bool ownable { get; set; }
                public string name { get; set; } = string.Empty;
                public float speed { get; set; }
                public float acceleration { get; set; }
                public int waiting { get; set; }
                public string prefab { get; set; } = string.Empty;
                public string owner { get; set; } = string.Empty;
                public string village { get; set; } = string.Empty;
                public List<ConfigVehicle_Position> positions { get; set; } = new List<ConfigVehicle_Position>();
            }
            //Farming --------------------------------------------------------------------------------------------
            public class ConfigFarming_Field {
                public string fieldId { get; set; } = string.Empty;
                public string seedId { get; set; } = string.Empty;
                public long startCount { get; set; }
                public int cares { get; set; }
                public bool complete { get; set; } = false;
                public void Reset() {
                    seedId = string.Empty;
                    cares = 0;
                    complete = false;
                }
            }
            public class ConfigFarming_Detail {
                public string farmId { get; set; } = string.Empty;
                public string name { get; set; } = string.Empty; //농장명
                public float capacity { get; set; } //생산력
                public string prefab { get; set; } = string.Empty;
                public string position { get; set; } = string.Empty;
                public List<ConfigFarming_Field> fields { get; set; } = new List<ConfigFarming_Field>();
                public bool tillage { get; set; } = false; //밭갈이 완료 상태 여부
            }
            //Seed ------
            public class ConfigFarming_Seed {
                public string name { get; set; } = string.Empty;
                public int duration { get; set; }
                public int careValue { get; set; }
                public int maxCare { get; set; }
                public string prefabPlant {get; set; } = string.Empty;
                public string prefabIngredient { get; set; } = string.Empty;
                public Config_Reward harvest { get; set; } = new Config_Reward();
            }
            //Stock Market ----------------------------------------------------------------------
            public class ConfigStockMarket {
                public int updateInterval { get; set; }
                public int capacity { get; set; }
                public int customers { get; set; }
                public int moneyMin { get; set; }
                public int moneyMax { get; set; }
                public int defaultQuantity { get; set; }
                public float fee { get; set; }
                public string currencyId { get; set; } = string.Empty;
            }
            //-----------------------------------------------------------------------------------
            public class Loader {
                public bool mInitialized = false;
                private static readonly Lazy<Loader> instance =
                        new Lazy<Loader>(() => new Loader());
                public static Loader Instance {
                    get {
                        return instance.Value;
                    }
                }
                private Loader() { }
                public bool Load( string stringSatisfactions, 
                                  string stringTask,
                                  string stringActors, 
                                  string stringItem, 
                                  string stringLevel,
                                  string stringQuest,
                                  string stringScript,
                                  string stringScenario,
                                  string stringVillage,
                                  string stringL10n,
                                  string stringVehicle,
                                  string stringFarming,
                                  string stringSeed,
                                  string stringStockMarket ) 
                {
                    if(mInitialized)
                        return true;
                            
                    //string jsonString = File.ReadAllText(pathSatisfactions);
                    string jsonString = stringSatisfactions;
                    
                    var sf = JsonConvert.DeserializeObject<Dictionary<string, ConfigSatisfaction_Define>>(jsonString);                                                   
                    if(sf == null) {
                        return false;
                    }                    
                    // define & discharge
                    foreach(var p in sf) {          
                        string satisfactionId = p.Key;
                        DischargeHandler.Instance.Add(satisfactionId, p.Value.discharge, p.Value.period);
                        SatisfactionDefine.Instance.Add(satisfactionId, p.Value);
                    }
                    //stock market
                    var sm = JsonConvert.DeserializeObject<ConfigStockMarket>(stringStockMarket);
                    if(sm == null)
                        return false;

                    StockMarketHandler.Instance.Init(sm);

                    //Village
                    if(SetVillage(stringVillage) == false) {
                        return false;
                    }
                    //Farming
                    if(SetFarming(stringFarming) == false) {
                        return false;
                    }
                    //Seed
                    if(SetSeed(stringSeed) == false) {
                        return false;
                    }
                    //default task
                    if(SetTask(stringTask) == false) {
                        return false;
                    }
                    if(SetQuest(stringQuest) == false)  {
                        return false;
                    }

                    // level
                    if(SetLevel(stringLevel) == false) {
                        return false;
                    }

                    //Item
                     if(SetItem(stringItem) == false) {
                        return false;
                    }
                    //Script
                    if(SetScript(stringScript) == false) {
                        return false;
                    }     
                    //Scenario
                    if(SetScenario(stringScenario) == false) {
                        return false;
                    }     
                    //L10n
                    if(!SetL10n(stringL10n))
                        return false;
                    //Vehicle
                    if(!SetVehicle(stringVehicle))
                        return false;
                    //actors
                    if(SetActor(stringActors) == false) {
                        return false;
                    }        

                    mInitialized = true;

                    return true;
                }    
                // Set Village
                private bool SetVillage(string sz) {
                    var j = JsonConvert.DeserializeObject<Dictionary<string, ConfigVillage_Detail>>(sz); 
                    if(j == null)
                        return false;
                    ActorHandler.Instance.SetVillageInfo(j);
                    return true;
                }
                // Set Scenario   
                private bool SetScenario(string sz) {
                    var j = JsonConvert.DeserializeObject<Dictionary<string, ConfigScenario_Detail>>(sz);  
                    if(j == null)
                        return false;
                    foreach(var p in j) {
                        string key = p.Key;
                        ConfigScenario_Detail detail = p.Value;
                        ScenarioInfoHandler.Instance.Insert(key, detail);
                    }
                    return true;
                }
                // Set Script
                private bool SetScript(string sz) {
                    string jsonString = sz; 
                    var j = JsonConvert.DeserializeObject<ConfigScript>(jsonString);  
                    if(j == null || j.refusal == null || j.scripts == null) {
                        return false;
                    }
                    foreach(var p in j.refusal) {
                        ScriptHandler.Instance.AddRefusal(p.Key, p.Value);
                    }
                    foreach(var p in j.scripts) {
                        ScriptHandler.Instance.Add(p.Key, p.Value);
                    }
                    return true;
                }
                // Set Actor
                private bool SetActor(string sz) {
                    //Actor     
                    string jsonString = sz; 
                    var actors = JsonConvert.DeserializeObject< Dictionary<string, ConfigActor_Detail> >(jsonString);  
                    if(actors == null) {
                        return false;
                    }

                    foreach(var p in actors) {
                        if(!p.Value.enable) continue;
                        //나중에 진행한 quest도 읽어와서 넣어줘야 함
                        Actor a = ActorHandler.Instance.AddActor(p.Key, p.Value, null);
                        if(p.Value.satisfactions == null) {
                            return false;
                        }
                    }
                    return true;
                }
                // Set Task
                private bool SetTask(string sz) {
                    var taskData = JsonConvert.DeserializeObject< Dictionary<string, List<ConfigTask_Detail>> >(sz); 
                    if(taskData == null)
                        return false;
                    foreach(var pTask in taskData) {
                        int actorType = int.Parse(pTask.Key);
                        List<ConfigTask_Detail>? tasks = pTask.Value;
                        if(tasks == null) continue;
                        for(int i=0; i < tasks.Count; i++) {
                            var task = tasks[i];
                            if(task == null || task.target == null || task.id.Length == 0) {
                                return false;
                            }
                            switch(task.target.type) {
                                case TASK_TARGET_TYPE.POSITION:
                                case TASK_TARGET_TYPE.FLY:
                                {
                                    if(task.target.value == null) {
                                        throw new Exception("task.target.value must exist");
                                    }
                                    //좌표가 아닌 경우는 value[0]을 보고 찾아가면 된다.
                                    if(task.target.value.Count == 2) {
                                        task.target.isAssignedPosition = true;
                                        string[] positionArr = task.target.value[0].Split(',');
                                        string[] lootAtArr = task.target.value[1].Split(',');
                                        task.target.position = new Position(float.Parse(positionArr[0]), float.Parse(positionArr[1]), float.Parse(positionArr[2]));
                                        task.target.lookAt = new Position(float.Parse(lootAtArr[0]), float.Parse(lootAtArr[1]), float.Parse(lootAtArr[2]));
                                    }
                                }                                
                                break;
                            }
                            TaskDefaultFn fn = new TaskDefaultFn(task);
                            fn.mActorType = actorType;
                            TaskHandler.Instance.Add(actorType, fn);
                        }
                    }
                    return true;
                }
                private bool SetQuest(string sz) {                         
                    string jsonString = sz; //File.ReadAllText(path);
                    var j = JsonConvert.DeserializeObject< Dictionary<string, ConfigQuest> >(jsonString);  
                    if(j == null) {
                        return false;
                    }

                    foreach(var p in j) {
                        int type = int.Parse(p.Key);                        
                        QuestHandler.Instance.Add(type, p.Value);
                    }
                    return true;
                }

                // Set Level
                private bool SetLevel(string sz) {
                    string jsonString = sz; //File.ReadAllText(path);
                    var j = JsonConvert.DeserializeObject< Dictionary<string, ConfigLevel> >(jsonString);  
                    if(j == null) {
                        return false;
                    }

                    foreach(var p in j) {
                        int type = int.Parse(p.Key);                        
                        LevelHandler.Instance.Set(type, p.Value);
                    }
                    return true;
                }
                // Set Item
                private bool SetItem(string sz) {
                    string jsonString = sz; //File.ReadAllText(path);
                    var j = JsonConvert.DeserializeObject< Dictionary<string, ConfigItem_Detail> >(jsonString);  
                    if(j == null) {
                        return false;
                    }

                    ItemHandler.Instance.Set(j);

                    return true;
                }
                private bool SetL10n(string sz) {
                    var j = JsonConvert.DeserializeObject< Dictionary<string, string> >(sz);  
                    if(j == null) {
                        return false;
                    }
                    L10nHandler.Instance.Set(j);
                    
                    return true;
                }   
                private bool SetVehicle(string sz) {
                    var j = JsonConvert.DeserializeObject< Dictionary<string, ConfigVehicle_Detail> >(sz);  
                    if(j == null) {
                        return false;
                    }
                    VehicleHandler.Instance.Set(j);
                    
                    return true;
                }     
                private bool SetFarming(string sz) {
                    var j = JsonConvert.DeserializeObject< Dictionary<string, List<ConfigFarming_Detail>> >(sz);  
                    if(j == null) {
                        return false;
                    }
                    FarmingHandler.Instance.Set(j);
                    
                    return true;
                }       
                private bool SetSeed(string sz) {
                    var j = JsonConvert.DeserializeObject< Dictionary<string, ConfigFarming_Seed> >(sz);  
                    if(j == null) {
                        return false;
                    }
                    FarmingHandler.Instance.SetSeed(j);
                    
                    return true;
                    
                } 
            }
        }
    }
}