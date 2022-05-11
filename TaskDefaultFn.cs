namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            /*
            단순 task
            json으로 관리
            */
            public class TaskDefaultFn : FnTask {
                Dictionary<int, float> mValues = new Dictionary<int, float>();
                public TaskDefaultFn(string title, string desc) {
                    this.mTaskTitle = title;
                    this.mTaskDesc = desc;
                }
                public void AddValue(int satisfactionId, float value) {
                    mValues.Add(satisfactionId, value);
                }
                public override Dictionary<int, float> GetValues()
                {
                    return mValues;
                }
                public override bool DoTask(Actor actor)
                {
                    this.ApplyValue(actor);
                    return true;
                }
            }
        }
    }
}