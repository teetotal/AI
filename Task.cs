namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class FnTask {
                public abstract List<SatisfactionValue> GetValues();
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
            }
        }
    }
}