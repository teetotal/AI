using System;
using System.Collections.Generic;

namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            
            public class FarmingHandler {
                //Farm 기본 정보
                private Dictionary<string, ConfigFarming_Detail> mDicFarmInfo = new Dictionary<string, ConfigFarming_Detail>();
                //씨앗과 아이템과 satisfaction과의 관계 정의 필요.

                private static readonly Lazy<FarmingHandler> instance =
                                    new Lazy<FarmingHandler>(() => new FarmingHandler());
                public static FarmingHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private FarmingHandler() { }
            }
        }
    }
}