using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class SoldierAbility {
                public float HP { get; set; }
                public int movingDistance { get; set; } //speed
                public float attackPower { get; set; }
                public float attackRange { get; set; } // 공격 범위
                public float teamwork { get; set; } // 같은편이 얼마 이상 있으면 그쪽으로 이동
                public float avoidance { get; set; } //회피력
            }
            public class SoldierInfo {
                public int id { get; set; }
                public string name { get; set; } = string.Empty;
                public Position position { get; set; } = new Position();
                public MOVING_TYPE movingType { get; set; }
                public SoldierAbility ability { get; set; } = new SoldierAbility();
                public bool isHome;
            }
            public class Tactic {
                public TACTIC_ATTACK attack { get; set; }
                public TACTIC_DEFENCE defence { get; set; }
            }
            public class Config {
                public Tactic tactic { get; set; } = new Tactic();
                public List<SoldierInfo> soldiers { get; set; } = new List<SoldierInfo>();
            }
            public class Loader {
                public Dictionary<string, Config>? Load(string path) {
                    var json = JsonConvert.DeserializeObject< Dictionary<string, Config> >(path);
                    return json;
                }
            }
        }
    }
}