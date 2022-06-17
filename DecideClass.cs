using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public abstract class DecideClass {
                public abstract bool Decide(Actor actor, Actor askerActor, FnTask task);
            }

            public class DecideAlwaysTrue : DecideClass {
                public override bool Decide(Actor actor, Actor askerActor, FnTask task) {
                    return true;
                }
            }
            public class DecideAlwaysFalse : DecideClass {
                public override bool Decide(Actor actor, Actor askerActor, FnTask task) {
                    return false;
                }
            }
        }
    }
}