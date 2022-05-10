using ENGINE.GAMEPLAY.MOTIVATION;
public class SV1: SatisfactionValue {
    public override float GetValue() {
        return 0.1f;
    }
} 

public class Task1: FnTask {
    public override List<SatisfactionValue> GetValues() {
        return new List<SatisfactionValue>();
    }
}