using System.Text.Json;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ConfigSatisfaction_Define {
                public string? title { get; set; }
                public float discharge { get; set; }
                public float period { get; set; }
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
            public class Loader {
                public bool Load(string jsonPath, Dictionary<int, SatisfactionValue> fnTable) {
                    string jsonString = File.ReadAllText(jsonPath);
                    var sf = JsonSerializer.Deserialize<ConfigSatisfaction>(jsonString);

                    if(sf == null || sf.define == null || sf.obtains == null) {
                        return false;
                    }

                    //discharge
                    /*
                    discharge를 한번에 같이 해야하는 경우가 있는가?
                    */
                    foreach(var p in sf.define) {                        
                        DischargeHandler.Instance.SetScenario(0, 100, 1, 1);
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

                    return true;
                }
            }
        }
    }
}