public class Actor {
    protected List<Satisfaction> mSatisfaction = new List<Satisfaction>();    
    public bool Init()
    {
        for(int i = 0; i < 5; i++) {                        
            var rand = new Random();            
            int max = rand.Next(90, 150);
            int min = rand.Next(1, max);
            int val = rand.Next(max);
            mSatisfaction.Add(new Satisfaction(i, min, max, val));
        }        
        return true;
    }
    public void Print() {
        for(int i=0; i < mSatisfaction.Count(); i++) {    
            Satisfaction s = mSatisfaction[i];
            System.Console.WriteLine("Seq = {0}\t Id = {1}\t Min = {2}\t Max = {3}\t Value = {4}\t V/Max = {5}\t V/Min = {6}", 
            i, s.Id, s.Min, s.Max, s.value, s.value / s.Max, s.value / s.Min);
        }
    }
    public Satisfaction GetSatisfaction(int idx) {
        return mSatisfaction[idx];
    }
    public int GetMotivation()
    {
        /*
        0. min check
        1. norm
        2. get mean
        3. finding max(value - avg)
        */        
        bool isMinList = true;
        List<int> list = CheckMinVal();
        if(list.Count() == 0) {
            for(int i =0; i < mSatisfaction.Count(); i++) {
                list.Add(i);
            }
            isMinList = false;
        }
        float mean = GetMean(list, isMinList);
        int idx = list[0];
        float minVal = mSatisfaction[list[0]].value;
        foreach(int i in list) {
            float v = (mSatisfaction[i].value / (isMinList ? mSatisfaction[i].Min : mSatisfaction[i].Max) ) - mean;            
            if(v < minVal) {
                minVal = v;
                idx = i;
            }
        }
        
        return idx;
    }
    private List<int> CheckMinVal() {
        List<int> ret = new List<int>();
        for(int i=0; i < mSatisfaction.Count(); i++) {
            if(mSatisfaction[i].value <= mSatisfaction[i].Min ) {
                ret.Add(i);
            }
        }
        return ret;
    }

    private float GetMean(List<int> list, bool isMinList) {
        float sum = 0.0f;
        foreach(int i in list) {
            sum += (mSatisfaction[i].value / (isMinList ? mSatisfaction[i].Min : mSatisfaction[i].Max));
        }

        return sum / mSatisfaction.Count();
    }
}