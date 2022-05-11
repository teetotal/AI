using ENGINE.GAMEPLAY.MOTIVATION;

//task
//TaskHandler.Instance.Add(new Task_Steal());
//TaskHandler.Instance.Add(new Task_Hello());

var pLoader = new Loader();
if(!pLoader.Load("config/satisfactions.json", "config/actors.json")) {
    Console.WriteLine("Failure Loading config");
}


string uniqueId = "애정이";
string uniqueId2 = "test2";

int type = 1;
int type2 = 0;

  
bool run = true;
var actors = ActorHandler.Instance.GetActors(type);
if(actors == null) {
    Console.WriteLine("Invalid Actor type");
    run = false;
} 

while(run) {      
    Console.WriteLine("---------------------------------------------------------");
    foreach(var p in actors) {            
        var actor = p.Value;        
        //Check Motivation
        var motivation = actor.GetMotivation();
        var s = actor.GetSatisfaction(motivation.Item1);
        if(s == null) {
            Console.WriteLine("Invalid motivationId");
        }else {
            Console.WriteLine("> {0} 만족도 ({1}) Motivation {2}({3})", actor.mUniqueId, motivation.Item2, SatisfactionDefine.Instance.Get(s.SatisfactionId).title, s.Value );
        }

        //Task 
        int taskid = actor.GetTaskId();
        var task = TaskHandler.Instance.GetTask(taskid);                 
        task.DoTask(actor);
        Console.WriteLine("! {0} ({1})", task.mTaskTitle, task.mTaskDesc);
        task.Print();       
        actor.Print();

        Console.WriteLine("====");
    }
    
    //Discharge
    Int64 counter = DischargeHandler.Instance.Discharge(type);
    Console.WriteLine("Discharged {0}", counter);
    //Thread.Sleep(1000 * 3);
    /*
    Console.WriteLine("Input:");
    string input = Console.ReadLine();
    Console.WriteLine(input);
    */
}