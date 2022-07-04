using System;
using System.Collections.Generic;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ScenarioInfoHandler {
                public Dictionary<string, ConfigScenario_Detail> mInfo = new Dictionary<string, ConfigScenario_Detail>();
                private static readonly Lazy<ScenarioInfoHandler> instance =
                                    new Lazy<ScenarioInfoHandler>(() => new ScenarioInfoHandler());
                public static ScenarioInfoHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private ScenarioInfoHandler() { }

                public void Insert(string key, ConfigScenario_Detail val) {
                    mInfo.Add(key, val);
                }
            }
        }
    }
}