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
                public string GetPrintString(Actor actor) {
                    var values = GetValues(actor);
                    string sz = "";
                    if(values == null) return sz;
                    foreach(var p in values) {
                        var s = SatisfactionDefine.Instance.Get(p.Key);
                        if(s == null) {
                            Console.WriteLine("Invalid SatisfactionDefine id");
                        } else {
                            sz += String.Format("{0}({1}) ", s.title, p.Value );                            
                        }                        
                    }
                    return sz;
                }
                public void Print(Actor actor) {
                    
                    Console.WriteLine(GetPrintString(actor));
                }
                
            }
            public class TaskHandler {
                private Dictionary<string, FnTask> mDict = new Dictionary<string, FnTask>();
                private static readonly Lazy<TaskHandler> instance =
                        new Lazy<TaskHandler>(() => new TaskHandler());
                public static TaskHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private TaskHandler() { }
                public bool Add(FnTask task) {
                    if(task.mTaskId == null) 
                        return false;
                    mDict.Add(task.mTaskId, task);
                    return true;
                }
                public Dictionary<string, FnTask> GetTasks(int level) {
                    Dictionary<string, FnTask> ret = new Dictionary<string, FnTask>();
                    foreach(var p in mDict) {
                        if( p.Value.mInfo != null && p.Value.mInfo.level != null)
                        {
                            var info = p.Value.mInfo;
                            if( ((info.level[0] <= level && info.level[1] >= level) || info.level == null) //check level
                                && info.type == TASK_TYPE.NORMAL // check type
                            ) {
                                ret.Add(p.Key, p.Value);
                            }
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