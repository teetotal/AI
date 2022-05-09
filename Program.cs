Actor a = new Actor();
a.Init();
a.Print();


int idx = a.GetMotivation();
Satisfaction s = a.GetSatisfaction(idx);
Console.WriteLine("Value = {0}, Id = {1}", s.value, s.Id);