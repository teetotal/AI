using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                public string? mTaskId { get; set; }
                public string? mTaskTitle { get; set; }
                public string? mTaskDesc { get; set; }
                public ConfigTask_Detail? mInfo { get; set; }
                public abstract Dictionary<string, float>? GetValues(Actor actor);
                //public abstract bool DoTask(Actor actor);
                
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
                public Dictionary<string, FnTask> GetTasks() {
                    return mDict;
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