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
            }
        }
    }
}