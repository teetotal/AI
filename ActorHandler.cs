namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ActorHandler {
                private static readonly Lazy<ActorHandler> instance =
                    new Lazy<ActorHandler>(() => new ActorHandler());
                
                private Dictionary<string, Actor> mDict = new Dictionary<string, Actor>();
                private Dictionary<int, Dictionary<string, Actor>> mDictType = new Dictionary<int, Dictionary<string, Actor>>();
            
                public static ActorHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private ActorHandler() {
                }

                public Actor AddActor(int type, string uniqueId) {
                    Actor a = new Actor(type, uniqueId);
                    mDict.Add(uniqueId, a);
                    if(mDictType.ContainsKey(type) == false) {
                        mDictType[type] = new Dictionary<string, Actor>();
                    }
                    mDictType[type][uniqueId] = a;
                    return a;
                }

                public Actor? GetActor(string uniqueId) {
                    if(mDict.ContainsKey(uniqueId) == true) {
                        return mDict[uniqueId];
                    }
                    return null;
                }

                public Dictionary<string, Actor>? GetActors(int type) {
                    if(mDictType.ContainsKey(type) == true) {
                        return mDictType[type];
                    }
                    return null;
                }
            }
        }
    }
}
