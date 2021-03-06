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
                public string? key { get; set; }
                public float value { get; set; }
            }            
            //-------------------------------------------------------------
            public class Config_Reward {
                public string? itemId { get; set; }
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
            public class ConfigItem_Detail {
                public string? name { get; set; }
                public string? desc { get; set; }                
                public string? category { get; set; }
                public string? type { get; set; }
                public int level { get; set; }
                public int cost { get; set; }
                public string? installationKey { get; set; }                
                public ConfigItem_Invoke? invoke { get; set; }
                public List<ConfigItem_Satisfaction>? satisfaction { get; set; }                
                public List<int[]>? draft { get; set; }
            }
            public class ConfigItem_Invoke {
                public int type { get; set; }
                public int expire { get; set; }
            }
            public class ConfigItem_Satisfaction {
                public string? satisfactionId { get; set; }
                public float min { get; set; }
                public float max { get; set; }
                public float value { get; set; }
                public ConfigItem_Satisfaction_Measure? measure { get; set; }

            }
            public class ConfigItem_Satisfaction_Measure {
                public int min { get; set; }
                public int max { get; set; }
                public int value { get; set; }
                
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
            public class ConfigSatisfaction_Define {
                public string title { get; set; } = string.Empty;
                public float discharge { get; set; }
                public int period { get; set; }
                public ConfigSatisfaction_Seed seed { get; set; } = new ConfigSatisfaction_Seed();
            }                       
        
            //Actors ------------------------------------------------------------------
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
                public List<float>? position { get; set; }
                public List<float>? rotation { get; set; }      
                public ConfigActor_Trigger? trigger { get; set; }          
                public List<ConfigActor_Satisfaction>? satisfactions { get; set; }
                public bool isFly { get; set; } = false;
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
                public int villageLevel { get; set; }  = -1;//해금되는 부락 레벨
                public string title { get; set; } = string.Empty;
                public string desc { get; set; } = string.Empty;
                //Task에 의한 보상은 고정값으로 하고, %로 보상하는건 아이템 같은걸로 하자.
                public ConfigTask_Target target { get; set; } = new ConfigTask_Target();
                public string animation { get; set; } = string.Empty;
                public int animationRepeatTime { get; set; }
                //동시 실행 최대 값
                public int maxRef { get; set; }
                public Dictionary<string, float> satisfactions { get; set; } = new Dictionary<string, float>();
                public Dictionary<string, float> satisfactionsRefusal { get; set; } = new Dictionary<string, float>();
            }         

            public class ConfigTask_Interaction {
                public TASK_INTERACTION_TYPE type { get; set; }
                public string? taskId { get; set; }
                
            }
            public class ConfigTask_Target {
                public TASK_TARGET_TYPE type { get; set; }
                public List<string>? value { get; set; }
                public ConfigTask_Interaction interaction { get; set; } = new ConfigTask_Interaction();
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
                public string? title { get; set; }
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
            
            //-----------------------------------------------------------------------------------
            public class Loader {
                public bool Load( string stringSatisfactions, 
                                  string stringTask,
                                  string stringActors, 
                                  string stringItem, 
                                  string stringLevel,
                                  string stringQuest,
                                  string stringScript,
                                  string stringScenario,
                                  string stringVillage ) {
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
                    
                    //actors
                    if(SetActor(stringActors) == false) {
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
                    //Village
                    if(SetVillage(stringVillage) == false) {
                        return false;
                    }

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
                        foreach(var s in p.Value.satisfactions) {
                            if(s.satisfactionId == null) return false;
                            a.SetSatisfaction(s.satisfactionId, s.min, s.max, s.value);
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
                                    string[] positionArr = task.target.value[0].Split(',');
                                    string[] lootAtArr = task.target.value[1].Split(',');
                                    task.target.position = new Position(float.Parse(positionArr[0]), float.Parse(positionArr[1]), float.Parse(positionArr[2]));
                                    task.target.lookAt = new Position(float.Parse(lootAtArr[0]), float.Parse(lootAtArr[1]), float.Parse(lootAtArr[2]));
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
            }
        }
    }
}