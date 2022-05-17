using System;
using System.Collections.Generic;
using System.IO;
//using System.Text.Json;
using System.Text.Json.Serialization;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            //공통           
            public class Config_Satisfaction {
                public string? satisfactionId { get; set; }
                public float min { get; set; }
                public float max { get; set; }
                public float value { get; set; }
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
            //Define
            public class ConfigSatisfaction_Define {
                public string? title { get; set; }
                public float discharge { get; set; }
                public int period { get; set; }
                public List<ConfigSatisfaction_Happening>? happening { get; set; }
            }                       
            public class ConfigSatisfaction {
                public Dictionary<string, ConfigSatisfaction_Define>? define { get; set; }
                public List<ConfigTask_Detail>? tasks { get; set; }
            }
            //Actors            
            public class ConfigActors_Detail {
                public int type { get; set; }
                public int level { get; set; }
                public List<Config_Satisfaction>? satisfactions { get; set; }
            }
            //Task ---------------------------------------------------------------           
            public class ConfigTask_Detail {
                public int level1 { get; set; } //사용가능한 Actor 최소 레벨
                public int level2 { get; set; } //사용가능한 Actor 최대 레벨
                public string? title { get; set; }
                public string? desc { get; set; }
                //Task에 의한 보상은 고정값으로 하고, %로 보상하는건 아이템 같은걸로 하자.
                public Dictionary<string, float>? satisfactions { get; set; }
                public ConfigTask_Relation? relation { get; set; }
            }         

            public class ConfigTask_Relation {
                public List<string>? target { get; set; }
                public Dictionary<string, float>? satisfactions { get; set; }
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
                public List<ConfigLevel_Threshold>? threshold { get; set; }
                public List<ConfigLevel_Rewards>? rewards { get; set; }                
            }
            public class ConfigLevel_Threshold {
                public string? key { get; set; }
                public float value { get; set; }
            }
            public class ConfigLevel_Rewards{
                public string? itemId { get; set; }
                public int quantity { get; set; }
            }
            // ----------------------------------------------------------------------
            public class Loader {
                public bool Load(string pathSatisfactions, string pathActors, string pathItem, string pathLevel) {
                    string jsonString = File.ReadAllText(pathSatisfactions);
                    var sf = JsonSerializer.Deserialize<ConfigSatisfaction>(jsonString);

                    if(sf == null || sf.define == null || sf.tasks == null) {
                        return false;
                    }                    

                    // define & discharge & happening
                    foreach(var p in sf.define) {          
                        string satisfactionId = p.Key;
                        DischargeHandler.Instance.Add(satisfactionId, p.Value.discharge, p.Value.period);
                        SatisfactionDefine.Instance.Add(satisfactionId, p.Value);
                        //happening
                        if(p.Value.happening != null) {
                            foreach(var happening in p.Value.happening) {
                                if(happening.types == null) {
                                    return false;
                                }
                                foreach(var type in happening.types) {
                                    HappeningHandler.Instance.Add(type, satisfactionId, happening);
                                }                                
                            }
                        }
                    }

                    //default task
                    if(SetTask(sf.tasks) == false) {
                        return false;
                    }

                    // level
                    if(SetLevel(pathLevel) == false) {
                        return false;
                    }

                    //Item
                     if(SetItem(pathItem) == false) {
                        return false;
                    }
                    
                    //actors
                    if(SetActor(pathActors) == false) {
                        return false;
                    }

                    return true;
                }
                // Set Actor
                private bool SetActor(string pathActors) {
                    //Actor     
                    string jsonString = File.ReadAllText(pathActors);
                    var actors = JsonSerializer.Deserialize< Dictionary<string, ConfigActors_Detail> >(jsonString);  
                    if(actors == null) {
                        return false;
                    }

                    foreach(var p in actors) {
                        Actor a = ActorHandler.Instance.AddActor(p.Value.type, p.Key, p.Value.level);
                        if(p.Value.satisfactions == null) {
                            return false;
                        }
                        foreach(var s in p.Value.satisfactions) {
                            a.SetSatisfaction(s.satisfactionId, s.min, s.max, s.value);
                        }
                    }
                    return true;
                }

                // Set Task
                private bool SetTask(List<ConfigTask_Detail> tasks) {
                    foreach(var p in tasks) {
                        if(p is null) {
                            return false;
                        }
                        TaskDefaultFn fn = new TaskDefaultFn(p);                        
                        TaskHandler.Instance.Add(fn);
                    }
                    return true;
                }

                // Set Level
                private bool SetLevel(string path) {
                    //Actor     
                    string jsonString = File.ReadAllText(path);
                    var j = JsonSerializer.Deserialize< Dictionary<string, ConfigLevel> >(jsonString);  
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
                private bool SetItem(string path) {
                    string jsonString = File.ReadAllText(path);
                    var j = JsonSerializer.Deserialize< Dictionary<string, ConfigItem_Detail> >(jsonString);  
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