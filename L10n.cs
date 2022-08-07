using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class L10nHandler {
                private Dictionary<string, string> mDic = new Dictionary<string, string>();
                private static readonly Lazy<L10nHandler> instance =
                        new Lazy<L10nHandler>(() => new L10nHandler());
                public static L10nHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private L10nHandler() { }
                public void Set(Dictionary<string, string> p) {
                    mDic = p;
                }
                public string Get(string key) {
                    if(!mDic.ContainsKey(key))
                        throw new Exception("Invalid L10n key. " + key);
                    
                    return mDic[key];
                }
            }
        }
    }
}