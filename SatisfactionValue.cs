/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
*/
/*

public class Task_Steal: FnTask {
    public enum SATISFACTION_CODE : int {
        HUNGRY = 100,
        GOLD,
        SECURITY = 110,
        FELLOWSHIP = 120,
        AFFECTION = 130
    }
    public override Dictionary<string, float> GetValues(string fromActorId) {
        var d = new Dictionary<string, float>();
        d.Add( SATISFACTION_CODE.HUNGRY.ToString(), 20 );
        d.Add( SATISFACTION_CODE.SECURITY.ToString(), -20 );
        d.Add( SATISFACTION_CODE.FELLOWSHIP.ToString(), -20 );

        return d;
    }
    public override bool DoTask(Actor actor)
    {
        this.ApplyValue(actor);
        return true;
    }
}
*/