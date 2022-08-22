using System;

namespace ENGINE {
    public class Singleton<T> where T : new() {
        protected static readonly Lazy<T> instance =
                            new Lazy<T>(() => new T());
        public static T Instance {
            get {
                return instance.Value;
            }
        }
    }
}