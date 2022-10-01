using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public enum MOVING_TYPE {
                STRAIGHT,   //직선 공격수
                OVER,       // 포병
                CROSS,      // 사선 공격수
                FORWARD     //오직 전방으로만 가는 방어진 역할
            }

            public enum TACTIC_ATTACK {
                MAX
            }
            public enum TACTIC_DEFENCE {
                MAX
            }
        }
    }
}