using System.Text.Json;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ConfigSatisfaction_Define {
                public string? title { get; set; }
                public float discharge { get; set; }
                public int period { get; set; }
            }
            public class ConfigSatisfaction_Obtains_Defaults {
                public int id { get; set; }
                public float amount { get; set; }
            }
            public class ConfigSatisfaction_Obtains {
                public string? title { get; set; }
                public List<ConfigSatisfaction_Obtains_Defaults> defaults { get; set; }
            }
            public class ConfigSatisfaction {
                public Dictionary<string, ConfigSatisfaction_Define>? define { get; set; }
                public Dictionary<string, ConfigSatisfaction_Obtains>? obtains { get; set; }                
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
            public class ConfigActors {
                
            }
            public class Loader {
                public bool Load(string pathSatisfactions, string pathActors, Dictionary<int, SatisfactionValue> fnTable) {
                    string jsonString = File.ReadAllText(pathSatisfactions);
                    var sf = JsonSerializer.Deserialize<ConfigSatisfaction>(jsonString);

                    if(sf == null || sf.define == null || sf.obtains == null) {
                        return false;
                    }

                    //discharge
                    foreach(var p in sf.define) {          
                        int satisfactionId = int.Parse(p.Key);              
                        DischargeHandler.Instance.Add(satisfactionId, p.Value.discharge, p.Value.period);
                    }
                    
                    //satisfaction table
                    foreach(var p in sf.obtains) {
                        int satisfactionTableId = int.Parse(p.Key);
                        
                        foreach(var pDefaults in sf.obtains[p.Key].defaults) {
                            var fn = fnTable[pDefaults.id];
                            fn.SatisfactionId = pDefaults.id;                            
                            SatisfactionTable.Instance.SetSatisfactionTable(satisfactionTableId, fn);
                        }                        
                    }          

                    //Actor     
                    jsonString = File.ReadAllText(pathActors);
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
            }
        }
    }
}