using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class LevelHandler {     
                // actor type, ConfigLevel
                private Dictionary<int, ConfigLevel> mLevelInfo = new Dictionary<int, ConfigLevel>();
                // start level table
                // actor type, level
                private Dictionary<int, int> mStartLevelTable = new Dictionary<int, int>();
                private static readonly Lazy<LevelHandler> instance =
                        new Lazy<LevelHandler>(() => new LevelHandler());
                public static LevelHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private LevelHandler() { }
                public void Set(int type, ConfigLevel p) {
                    mLevelInfo[type] = p;
                    mStartLevelTable[type] = p.startLevel;
                }
                public int GetStartLevel(int type) {
                    if(mStartLevelTable.ContainsKey(type) == false) {
                        return -1;
                    }
                    return mStartLevelTable[type];
                }
                public ConfigLevel_Detail? Get(int type, int level) {
                    int startLevel = GetStartLevel(type);
                    if(startLevel == -1) {
                        return null;
                    }
                    int idx = level-startLevel;         
                    if(mLevelInfo.ContainsKey(type) && mLevelInfo[type].levels != null && mLevelInfo[type].levels.Count - 1 > idx) {                        
                        return mLevelInfo[type].levels[idx];
                    }
                    return null;                        
                }
            }
        }
    }
}