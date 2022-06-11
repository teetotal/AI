using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                public string mTaskId { get; set; } = "";
                public string mTaskTitle { get; set; } = "";
                public string mTaskDesc { get; set; } = "";
                public ConfigTask_Detail mInfo { get; set; } = new ConfigTask_Detail();
                public abstract Dictionary<string, float>? GetValues(Actor actor);
                public abstract Dictionary<string, float> GetSatisfactions(Actor actor);
                
                // isActor, id
                public abstract Tuple<bool, string> GetTargetObject(Actor actor);
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
                public Dictionary<string, FnTask> GetTasks(int actorType, int level) {
                    Dictionary<string, FnTask> ret = new Dictionary<string, FnTask>();
                    if(mDictByActorType.ContainsKey(actorType) == false)
                        return ret;

                    for(int i = 0; i < mDictByActorType[actorType].Count; i++) {
                        string taskId = mDictByActorType[actorType][i];
                        var info = mDict[taskId].mInfo;
                        if( ((info.level != null && info.level[0] <= level && info.level[1] >= level) || info.level == null) //check level
                            && info.type == TASK_TYPE.NORMAL // check type
                        ) {
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
            }
        }
    }
}