using System;
using System.Collections.Generic;

namespace ENGINE {
    namespace GAMEPLAY {
        //게임 전체 카운터
        public class Counter {
            private Int64 mCounter { get; set; }
            private static readonly Lazy<Counter> instance =
                        new Lazy<Counter>(() => new Counter());
            public static Counter Instance {
                get {
                    return instance.Value;
                }
            }
            private Counter() { }

            public void SetCounter(Int64 count) {
                mCounter = count;
            }
            public Int64 GetCount() {
                return mCounter;
            }
            public Int64 Next() {
                mCounter ++;
                return mCounter;
            }
        }
    }
}