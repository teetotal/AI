using System;
using System.Collections.Generic;
using System.Linq;

namespace ENGINE {
    namespace GAMEPLAY {                
        public class Counter {
            private Int64 mCounter { get; set; }
            public void SetCounter(Int64 count) {
                mCounter = count;
            }
            public Int64 GetCount() {
                return mCounter;
            }
            public Int64 GetNextCount() {
                return mCounter + 1;
            }
            public Int64 Next() {
                mCounter ++;
                return mCounter;
            }
        }
        public class CounterHandler {
            private Counter mCounter = new Counter();
            
            private static readonly Lazy<CounterHandler> instance =
                        new Lazy<CounterHandler>(() => new CounterHandler());
            public static CounterHandler Instance {
                get {
                    return instance.Value;
                }
            }
            public CounterHandler() { }
            public void SetCounter(Int64 count) {
                mCounter.SetCounter(count);
            }
            public Int64 GetCount() {
                return mCounter.GetCount();
            }
            public Int64 Next() {                
                return mCounter.Next();
            }

            
        }
    }
}