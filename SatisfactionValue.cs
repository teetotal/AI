using ENGINE.GAMEPLAY.MOTIVATION;
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