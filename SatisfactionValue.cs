using ENGINE.GAMEPLAY.MOTIVATION;
public class SV1: SatisfactionValue {
    public override float GetValue() {
        return 0.1f;
    }
} 
public enum SATISFACTION_CODE : int {
    HUNGRY = 100,
    GOLD,
    SECURITY = 110,
    FELLOWSHIP = 120,
    AFFECTION = 130
}
/*
도둑질
*/
public class Task_Steal: FnTask {
    public override Dictionary<int, float> GetValues() {
        var d = new Dictionary<int, float>();
        d.Add( (int)SATISFACTION_CODE.HUNGRY, 20 );
        d.Add( (int)SATISFACTION_CODE.SECURITY, -20 );
        d.Add( (int)SATISFACTION_CODE.FELLOWSHIP, -20 );

        return d;
    }
    public override bool DoTask(Actor actor)
    {
        this.ApplyValue(actor);
        return true;
    }
}

/*
인사
*/
public class Task_Hello: FnTask {
    public override Dictionary<int, float> GetValues() {
        var d = new Dictionary<int, float>();        
        d.Add( (int)SATISFACTION_CODE.FELLOWSHIP, 3 );

        return d;
    }
    public override bool DoTask(Actor actor)
    {
        this.ApplyValue(actor);
        return true;
    }
}