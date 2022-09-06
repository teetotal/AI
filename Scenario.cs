using System;
using System.Collections.Generic;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ScenarioInfoHandler {
                public Dictionary<string, List<ConfigScenario_Node>> mInfo;// = new Dictionary<string, List<ConfigScenario_Node>>();
                private static readonly Lazy<ScenarioInfoHandler> instance =
                                    new Lazy<ScenarioInfoHandler>(() => new ScenarioInfoHandler());
                public static ScenarioInfoHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private ScenarioInfoHandler() { }

                public void Init(Dictionary<string, List<ConfigScenario_Node>> p) {
                    mInfo = p;
                }
            }
        }
    }
}