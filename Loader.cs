using System.Text.Json;

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
            //happening
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
                public int itemId { get; set; }
                public int quantity { get; set; }
            }
            // ----------------------------------------------------------------------
            public class Loader {
                public bool Load(string pathSatisfactions, string pathActors, string pathLevel) {
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
            }
        }
    }
}