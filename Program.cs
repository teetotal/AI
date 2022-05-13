using ENGINE.GAMEPLAY;
using ENGINE.GAMEPLAY.MOTIVATION;

//task
//TaskHandler.Instance.Add(new Task_Steal());
//TaskHandler.Instance.Add(new Task_Hello());

var pLoader = new Loader();
if(!pLoader.Load("config/satisfactions.json", "config/actors.json", "config/level.json")) {
    Console.WriteLine("Failure Loading config");
}


//string uniqueId = "애정이";
int type = 1;

  

var actors = ActorHandler.Instance.GetActors(type);
while(actors != null) {      
    Int64 counter = Counter.Instance.Next();
    Console.WriteLine("Counter {0}", counter);

    ActorHandler.Instance.UpdateSatisfactionSum();
    ActorHandler.Instance.PrintSatisfactionSum(type);
    //happening
    var happeningList = HappeningHandler.Instance.GetHappeningCandidates(type);
    HappeningHandler.Instance.PrintCandidates(happeningList);
    foreach(var happening in happeningList) {
        if(HappeningHandler.Instance.Do(type, happening.Info.id) == true) {
            Console.WriteLine("{0} 발생", happening.Info.title);
        } else {
            Console.WriteLine("Failure Happening");
        }
    }

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
        bool isLevelUp = actor.checkLevelUp();
        Console.WriteLine("! {0} ({1}), levelup? {2}", task.mTaskTitle, task.mTaskDesc, isLevelUp);
        if(isLevelUp == true) {
            actor.LevelUp();
        }
        task.Print(actor.mUniqueId);       
        actor.Print();

        Console.WriteLine("====");
    }
    
    //Discharge
    DischargeHandler.Instance.Discharge(type);

    //Thread.Sleep(1000 * 3);
    /*
    Console.WriteLine("Input:");
    string input = Console.ReadLine();
    Console.WriteLine(input);
    */
}