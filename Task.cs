namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                //public int mTaskId { get; set; }
                public string? mTaskTitle { get; set; }
                public string? mTaskDesc { get; set; }
                public abstract Dictionary<string, float> GetValues(string fromActorId);
                public abstract bool DoTask(Actor actor);
                public void Print(string fromActorId) {
                    var values = GetValues(fromActorId);
                    string sz = "";
                    foreach(var p in values) {
                        var s = SatisfactionDefine.Instance.Get(p.Key);
                        if(s == null) {
                            Console.WriteLine("Invalid SatisfactionDefine id");
                        } else {
                            sz += String.Format("{0}({1}) ", s.title, p.Value );                            
                        }                        
                    }
                    Console.WriteLine(sz);
                }
                protected virtual void ApplyValue(Actor actor) {
                    Dictionary<string, float> values = GetValues(actor.mUniqueId);
                    //apply to self
                    foreach(var p in values) {
                        actor.Obtain(p.Key, p.Value);                        
                    }
                    actor.mTaskCounter++;
                }                
            }
            public class TaskHandler {
                private List<FnTask> mList = new List<FnTask>();
                private static readonly Lazy<TaskHandler> instance =
                        new Lazy<TaskHandler>(() => new TaskHandler());
                public static TaskHandler Instance {
                    get {
                        return instance.Value;
                    }
                }

                private TaskHandler() { }
                public void Add(FnTask task) {
                    mList.Add(task);
                }
                public List<FnTask> GetTasks() {
                    return mList;
                }
                public FnTask GetTask(int idx) {
                    return mList[idx];
                }
            }
        }
    }
}