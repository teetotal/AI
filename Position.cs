using System;

#nullable enable
namespace ENGINE {
    public class Position {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public Position() { }
        public Position(float x, float y, float z) {
            Set(x, y, z);
        }
        public void Set(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public double GetDistance(Position to) {
            return Math.Sqrt(Math.Pow(to.x - x, 2) + Math.Pow(to.y - y, 2) + Math.Pow(to.z - z, 2));
        }
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", x, y, z);
        }
    }  
}