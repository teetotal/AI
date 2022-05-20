using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class BattleMapTile {
                public int advantage { get; set; }
                public int disadvantage { get; set; }
            }
            public class Battle {
                private BattleMapTile[,] mBattleMap;
            }
        }        
    }
}