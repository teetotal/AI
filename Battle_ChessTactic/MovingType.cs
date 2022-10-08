using System;
using System.Collections.Generic;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public enum MOVING_TYPE {
                STRAIGHT = 0,     //직선 이동. 장애물 뚫지 못하는 공격
                CROSS,            //사선 이동. 장애물 뚫지 못하는 공격
                OVER_STRAIGHT,    //직선 이동. 포병
                OVER_CROSS,       //사선 이동. 포병
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