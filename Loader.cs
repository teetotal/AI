using System.Text.Json;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            //공통
            public class Config_Id_Amount {
                public int id { get; set; }
                public float amount { get; set; }
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
                public List<ConfigTaskDetail>? tasks { get; set; }
            }
            //Actors
            public class ConfigActors_Satisfaction {
                public int satisfactionId { get; set; }
                public float min { get; set; }
                public float max { get; set; }
                public float value { get; set; }
            }
            public class ConfigActors_Detail {
                public int type { get; set; }
                public List<ConfigActors_Satisfaction>? satisfactions { get; set; }
            }
           
            public class ConfigTaskDetail {
                public string? title { get; set; }
                public string? desc { get; set; }
                public List<Config_Id_Amount>? satisfactions { get; set; }
            }           
            public class Loader {
                public bool Load(string pathSatisfactions, string pathActors) {
                    string jsonString = File.ReadAllText(pathSatisfactions);
                    var sf = JsonSerializer.Deserialize<ConfigSatisfaction>(jsonString);

                    if(sf == null || sf.define == null || sf.tasks == null) {
                        return false;
                    }                    

                    // define & discharge & happening
                    foreach(var p in sf.define) {          
                        int satisfactionId = int.Parse(p.Key);              
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
                        Actor a = ActorHandler.Instance.AddActor(p.Value.type, p.Key);
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
                private bool SetTask(List<ConfigTaskDetail> tasks) {
                    foreach(var p in tasks) {
                        if(p.title == null || p.desc == null || p.satisfactions == null) {
                            return false;
                        }
                        TaskDefaultFn fn = new TaskDefaultFn(p.title, p.desc);
                        foreach(var d in p.satisfactions) {
                            fn.AddValue(d.id, d.amount);
                        }
                        TaskHandler.Instance.Add(fn);
                    }
                    return true;
                }
            }
        }
    }
}