namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class Satisfaction {
                public Satisfaction(int id, float min, float max, float value) {
                    this.Id = id;        
                    this.Min = min;
                    this.Max = max;
                    this.Value = value;
                }
                public int Id{ get; set; }
                public float Min { get; set; }
                public float Max { get; set; }
                public float Value { get; set; }
            }
        }
    }
}