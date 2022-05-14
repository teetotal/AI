using ENGINE.GAMEPLAY;
using ENGINE.GAMEPLAY.MOTIVATION;
using System;

var p = new Loop();
p.Load();
p.MainLoop();

public class Loop {
    int type = 1;
    public void MainLoop() {
        var actors = ActorHandler.Instance.GetActors(type);
        while(actors != null) {      
            Console.WriteLine("---------------------------------------------------------");
            foreach(var p in actors) {            
                var actor = p.Value;        
                //Check Motivation
                var motivation = actor.GetMotivation();
                var s = actor.GetSatisfaction(motivation.Item1);
                if(s == null) {
                    Console.WriteLine("Invalid motivationId");
                }else {
                    Console.WriteLine("> {0} 만족도 ({1}) Motivation {2}({3})", actor.mUniqueId, motivation.Item2, SatisfactionDefine.Instance.GetTitle(s.SatisfactionId), s.Value );
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

            Thread.Sleep(1000 * 1);
            
            Console.WriteLine("Input:");
            string input = Console.ReadLine();
            Console.WriteLine(input);
            
        }
    }
    public bool Load() {
        var pLoader = new Loader();
        if(!pLoader.Load("config/satisfactions.json", "config/actors.json", "config/level.json")) {
            Console.WriteLine("Failure Loading config");
            return false;
        }
        return true;
    }
    public void Next() {
        Int64 counter = Counter.Instance.Next();
        Console.WriteLine("Counter {0}", counter);
        //Discharge
        DischargeHandler.Instance.Discharge(type);

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
    }
}




//string uniqueId = "애정이";

