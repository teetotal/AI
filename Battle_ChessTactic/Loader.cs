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
                public float attackPower { get; set; } // 최대 damage
                public float attackPowerMin { get; set; } //빗맞았을때 damage
                public float attackAccuracy { get; set; } //공격 정확도
                public float attackRange { get; set; } // 공격 범위
                public float teamwork { get; set; } // 같은편이 얼마 이상 있으면 그쪽으로 이동
                public float avoidance { get; set; } //회피 기준 HP rate
                //public float retreat { get; set; } // 후퇴력. 회피할때 적으로 부터 멀리 떨어지는 가중치 
            }
            public class SoliderItem {
                public int firstAid { get; set; } //구급약
                public float firstAidEffect { get; set; } // 구급약 효과
            } 
            public class ChessTactic_SoldierInfo {
                public int side;
                public int id { get; set; }
                public string name { get; set; } = string.Empty;
                public Position position { get; set; } = new Position();
                public MOVING_TYPE movingType { get; set; }
                public SoldierAbility ability { get; set; } = new SoldierAbility();
                public SoliderItem item { get; set; } = new SoliderItem();
                public float nextFirstAidHP = 0.5f;
                public float nextAvoidHP;
            }
            public class ChessTactic_Info {
                public string name { get; set; } = string.Empty;
                public TACTIC_ATTACK attackTactic { get; set; }
                public TACTIC_DEFENCE defenceTactic { get; set; }
            }
            public class ChessTactic {
                public ChessTactic_Info info { get; set; } = new ChessTactic_Info();
                public List<ChessTactic_SoldierInfo> soldiers { get; set; } = new List<ChessTactic_SoldierInfo>();
            }
            public class Loader {
                public Dictionary<string, ChessTactic>? Load(string path) {
                    var json = JsonConvert.DeserializeObject< Dictionary<string, ChessTactic> >(path);
                    return json;
                }
            }
        }
    }
}