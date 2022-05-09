public class Satisfaction {
    public Satisfaction(int id, float min, float max, float defaultVal) {
        this.Id = id;        
        this.Min = min;
        this.Max = max;
        this.value = defaultVal;
    }
    public int Id{ get; set; }
    public float Min { get; set; }
    public float Max { get; set; }
    public float value { get; set; }
}