using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                public string mTaskId { get; set; } = string.Empty;
                public int mActorType;
                public string mTaskTitle { get; set; } = string.Empty;
                public string mTaskDesc { get; set; } = string.Empty;
                //for TextMeshPro
                public string mTaskString { get; set; } = string.Empty;
                public abstract void SetTaskString();
                public ConfigTask_Detail mInfo { get; set; } = new ConfigTask_Detail();
                public abstract Tuple<Dictionary<string, float>, Dictionary<string, float>>? GetValues(Actor actor);
                public abstract Dictionary<string, float> GetSatisfactions();
                public abstract Dictionary<string, float> GetSatisfactionsRefusal();
                
                // isActor, id, position, lookat
                public abstract Tuple<Actor.TASKCONTEXT_TARGET_TYPE, string, Position?, Position?> GetTargetObject(Actor actor);
                public string GetAnimation() {
                    if(mInfo == null || mInfo.animation == null) {
                        return "";
                    }
                    return mInfo.animation;
                }  
                protected List<KeyValuePair<string, float>> GetResources(Dictionary<string, string> satisfactions) {
                    //pooling 구현해야함.
                    List<KeyValuePair<string, float>> resources = new List<KeyValuePair<string, float>>();
                    foreach(var p in satisfactions) {
                        if(CheckImmutableSatisfaction(p.Value) && SatisfactionDefine.Instance.Get(p.Key).resource && p.Value[0] != '-') {
                            resources.Add(new KeyValuePair<string, float>(p.Key, float.Parse(p.Value)));
                        } 
                    }
                    return resources;
                }
                protected bool CheckImmutableSatisfaction(string satisfactionVal) {
                    switch(satisfactionVal[0]) {
                        case '$':
                        return false;
                        default:
                        return true;
                    }
                }
                protected float GetMutableSatisfactionValue(List<KeyValuePair<string, float>> resources, string valFormat) {
                    float sum = 0;
                    for(int i = 0; i < resources.Count; i++) {
                        sum += GetMutableValue(resources[i].Key, valFormat) * resources[i].Value;
                    }
                    return sum;
                    
                }       
                private float GetMutableValue(string resourceId, string valFormat) {
                    string[] arr = valFormat.Split('|');
                    switch(arr[0]) {
                        case "$": //market price
                        {
                            float markPrice = SatisfactionMarketPrice.Instance.GetMarketPrice(resourceId);
                            switch(arr[2]) {
                                case "-%":
                                return -markPrice * float.Parse(arr[1]);
                                case "+%":
                                return markPrice * float.Parse(arr[1]);
                                default:
                                break;
                            }
                        }
                        return -1;
                        default:
                        return -1;
                    }
                }       
            }
            public class TaskHandler {
                private Dictionary<string, FnTask> mDict = new Dictionary<string, FnTask>();
                //Actor type, taskid
                private Dictionary<int, List<string>> mDictByActorType = new Dictionary<int, List<string>>();
                //reference count
                private Dictionary<string, UInt32> mDicRefCount = new Dictionary<string, uint>();
                private static readonly Lazy<TaskHandler> instance =
                        new Lazy<TaskHandler>(() => new TaskHandler());
                public static TaskHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private TaskHandler() { }
                public bool Add(int actorType, FnTask task) {
                    if(task.mTaskId == null) 
                        return false;
                    mDict.Add(task.mTaskId, task);
                    if(mDictByActorType.ContainsKey(actorType) == false) {
                        mDictByActorType[actorType] = new List<string>();
                    }
                    mDictByActorType[actorType].Add(task.mTaskId);
                    return true;
                }
                public Dictionary<string, FnTask> GetTasks(Actor actor, bool checkRef = true) {
                    Dictionary<string, FnTask> ret = new Dictionary<string, FnTask>();
                    int actorType = actor.mType;
                    int level = actor.level;
                    if(mDictByActorType.ContainsKey(actorType) == false)
                        return ret;

                    for(int i = 0; i < mDictByActorType[actorType].Count; i++) {
                        string taskId = mDictByActorType[actorType][i];
                        var info = mDict[taskId].mInfo;
                        if(actor.mInfo.village != string.Empty && info.villageLevel != -1) {
                            if(info.villageLevel > ActorHandler.Instance.GetVillageLevel(actor.mInfo.village))
                                continue;
                        }
                        
                        if( ((info.level != null && info.level[0] <= level && info.level[1] >= level) || info.level == null) //check level
                            && info.type == TASK_TYPE.NORMAL // check type
                        ) {
                            if(checkRef && info.maxRef != -1) {//check ref count
                                if(info.maxRef < GetRef(taskId)) {
                                    continue;
                                }
                            } 
                            //actor가 가지고 있는 satisfaction만 추가
                            foreach(var satisfaction in info.satisfactions) {
                                if(actor.GetSatisfaction(satisfaction.Key) == null) continue;
                            }                            
                            ret.Add(taskId, mDict[taskId]);
                        }                          
                    }
                    
                    return ret;
                }
                public FnTask? GetTask(string? taskId) {
                    if(taskId == null) {
                        return null;
                    }
                    if(mDict.ContainsKey(taskId))
                        return mDict[taskId];
                    return null;
                }
                public UInt32 IncreaseRef(string taskId) {
                    if(mDicRefCount.ContainsKey(taskId) == false) {
                        mDicRefCount.Add(taskId, 1);
                    } else {
                        mDicRefCount[taskId] ++;
                    }
                    return mDicRefCount[taskId];
                }
                public UInt32 ReleaseRef(string taskId) {
                    if(mDicRefCount.ContainsKey(taskId) == false) {
                        throw new Exception("taskId ref didn't alloc. " + taskId);
                    } else {
                        mDicRefCount[taskId] --;
                    }
                    return mDicRefCount[taskId];
                }
                public UInt32 GetRef(string taskId) {
                    if(mDicRefCount.ContainsKey(taskId) == false) {
                        return 0;
                    } else {
                        return mDicRefCount[taskId];
                    }
                }
                public bool CheckRef(FnTask task) {
                    if(task.mInfo.maxRef == -1 || GetRef(task.mTaskId) < task.mInfo.maxRef)
                        return true;
                    return false;
                }
                public bool CheckSatisfaction(Actor actor, FnTask task) {
                    foreach(var s in task.GetSatisfactions()) {
                        if(s.Value < 0) {
                            var myS = actor.GetSatisfaction(s.Key);
                            if(myS == null || myS.Value + s.Value < 0)
                                return false;
                        }
                    }
                    return true;
                }
            }
        }
    }
}