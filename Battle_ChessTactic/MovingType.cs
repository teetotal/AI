using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public enum MOVING_TYPE {
                STRAIGHT,   //직선 공격수
                OVER,       // 사선 공격수
                CROSS,      // 포병
                FORWARD     //오직 전방으로만 가는 방어진 역할
            }
        }
    }
}