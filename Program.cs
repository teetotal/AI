using ENGINE.GAMEPLAY.MOTIVATION;

Dictionary<int, SatisfactionValue> fnTable = new Dictionary<int, SatisfactionValue>();
fnTable.Add(100, new SV1());
fnTable.Add(101, new SV1());
fnTable.Add(110, new SV1());
fnTable.Add(120, new SV1());
fnTable.Add(130, new SV1());

//task
TaskHandler.Instance.Add(new Task1());

var pLoader = new Loader();
pLoader.Load("config/satisfactions.json", "config/actors.json", fnTable);


string uniqueId = "애정이";
string uniqueId2 = "test2";

int type = 1;
int type2 = 0;

var actor = ActorHandler.Instance.GetActor(uniqueId);
if(actor == null) {
    Console.WriteLine("Invalid Actor unique id");
} else {    
    actor.Print();
    var s = actor.GetSatisfaction(actor.GetMotivation());
    if(s == null) {
        Console.WriteLine("Invalid motivationId");
    }else {
        Console.WriteLine("Value = {0}, SatisfactionId = {1}", s.Value, s.SatisfactionId);
    }
    
}

while(true) {    

    var actors = ActorHandler.Instance.GetActors(type);
    if(actors == null) {
        Console.WriteLine("Invalid Actor type");
    } else {
        foreach(var p in actors) {
            int taskId = p.Value.GetTaskId();
            SatisfactionTable.Instance.ApplySatisfaction(1, p.Value.mUniqueId);
            p.Value.Print();
        }
    }

    Int64 counter = DischargeHandler.Instance.Discharge(type);
    Console.WriteLine("Discharged {0}", counter);
    Thread.Sleep(1000 * 3);
    /*
    Console.WriteLine("Input:");
    string input = Console.ReadLine();
    Console.WriteLine(input);
    */
}