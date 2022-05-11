namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                //public int mTaskId { get; set; }
                //public string? mTaskTitle { get; set; }
                public abstract Dictionary<int, float> GetValues();
                public abstract bool DoTask(Actor actor);
                protected virtual void ApplyValue(Actor actor) {
                    Dictionary<int, float> values = GetValues();
                    foreach(var p in values) {
                        actor.Obtain(p.Key, p.Value);
                    }
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