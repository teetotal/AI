using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                public string mTaskId { get; set; } = string.Empty;
                public string mTaskTitle { get; set; } = string.Empty;
                public string mTaskDesc { get; set; } = string.Empty;
                public ConfigTask_Detail mInfo { get; set; } = new ConfigTask_Detail();
                public abstract Tuple<Dictionary<string, float>, Dictionary<string, float>>? GetValues(Actor actor);
                public abstract Dictionary<string, float> GetSatisfactions(Actor actor);
                public abstract Dictionary<string, float> GetSatisfactionsRefusal(Actor actor);
                
                // isActor, id, position, lookat
                public abstract Tuple<Actor.TASKCONTEXT_TARGET_TYPE, string, Position?, Position?> GetTargetObject(Actor actor);
                public string GetAnimation() {
                    if(mInfo == null || mInfo.animation == null) {
                        return "";
                    }
                    return mInfo.animation;
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
                public Dictionary<string, FnTask> GetTasks(Actor actor) {
                    int actorType = actor.mType;
                    int level = actor.mLevel;
                    Dictionary<string, FnTask> ret = new Dictionary<string, FnTask>();
                    if(mDictByActorType.ContainsKey(actorType) == false)
                        return ret;

                    for(int i = 0; i < mDictByActorType[actorType].Count; i++) {
                        string taskId = mDictByActorType[actorType][i];
                        var info = mDict[taskId].mInfo;
                        if( ((info.level != null && info.level[0] <= level && info.level[1] >= level) || info.level == null) //check level
                            && info.type == TASK_TYPE.NORMAL // check type
                            && (info.maxRef == -1 || info.maxRef > GetRef(taskId)) //check ref count
                        ) {
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
            }
        }
    }
}