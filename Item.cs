using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ItemHandler {
                private Dictionary<string, ConfigItem_Detail>? mItemInfo = null;
                private static readonly Lazy<ItemHandler> instance =
                        new Lazy<ItemHandler>(() => new ItemHandler());
                public static ItemHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private ItemHandler() { }
                public void Set(Dictionary<string, ConfigItem_Detail> p) {
                    mItemInfo = p;
                }
                public ConfigItem_Detail? GetItemInfo(string key) {
                    if(mItemInfo is null || mItemInfo.ContainsKey(key) == false) {
                        return null;
                    }
                    return mItemInfo[key];
                }
                public string GetPrintString(string key) {
                    var info = GetItemInfo(key);
                    string sz = "";
                    if(info != null && info.satisfaction != null) {
                        sz += String.Format("{0}", info.name);
                        foreach(var p in info.satisfaction) {
                            if(p.satisfactionId != null) {
                                sz += String.Format(" - {0}({1})", SatisfactionDefine.Instance.GetTitle(p.satisfactionId), p.value);
                            }                            
                        }                        
                    }
                    return sz;
                }
            }
        }
    }
}