string uniqueId = "test1";
string uniqueId2 = "test2";

int type = 1;
int type2 = 0;

Actor a = ActorHandler.Instance.AddActor(type, uniqueId);
for(int i = 0; i < 5; i++) {
    var rand = new Random();            
    int max = rand.Next(90, 150);
    int min = rand.Next(1, max);
    int val = rand.Next(max);
    a.SetSatisfaction(i, min, max, val);
}

a.Print();

var actor = ActorHandler.Instance.GetActor(uniqueId);
if(actor == null) {
    Console.WriteLine("Invalid Actor unique id");
} else {
    int idx = actor.GetMotivation();
    Satisfaction s = a.GetSatisfaction(idx);
    Console.WriteLine("Value = {0}, Id = {1}", s.value, s.Id);
}


var actors = ActorHandler.Instance.GetActors(type);
if(actors == null) {
    Console.WriteLine("Invalid Actor type");
} else {
    foreach(var p in actors) {
        p.Value.Print();
    }
}