using ENGINE.GAMEPLAY;
using ENGINE.GAMEPLAY.MOTIVATION;
using System;

var p = new Loop();
if(p.Load("config/satisfactions.json", "config/actors.json", "config/item.json", "config/level.json")) {
    p.MainLoop();
} else {
    Console.WriteLine("Failure loading config");
}


public class Loop {
    int type = 1;
    public void MainLoop() {
        var actors = ActorHandler.Instance.GetActors(type);
        while(actors != null) {      
            Thread.Sleep(1000 * 1);
            PrintLine();
            Console.Write("0(System) 1(Next) 2(Task) 3(Inquiry) 4(Inquiry All) q(Quit): ");
            var input = Console.ReadLine();
            PrintLine();
            if(input is not null) {
                if(input == "q") {
                    return;
                }
                switch(int.Parse(input)) {
                    case 0:
                    System();
                    break;
                    case 1:
                    Next();
                    break;
                    case 2:
                    DoActor(actors);
                    Next();
                    break;
                    case 3:
                    Inquiry(actors);
                    break;
                    case 4:
                    InquiryAll(actors);
                    break;
                }
            }
        }
    }
    public bool Load(string pathSatisfaction, string pathActor, string pathItem, string pathLevel) {
        var pLoader = new Loader();
        if(!pLoader.Load(pathSatisfaction, pathActor, pathItem, pathLevel)) {
            Console.WriteLine("Failure Loading config");
            return false;
        }
        return true;
    }
    private void PrintLine() {
        Console.WriteLine("---------------------------------------------------------");
    }
    private void System() {
        Console.Write("s(SatisfactionSum) c(Counter): ");
        var input = Console.ReadLine();
        switch(input) {
            case "s":
            ActorHandler.Instance.PrintSatisfactionSum(type);
            break;
            case "c":
            Console.WriteLine("Counter {0}", Counter.Instance.GetCount());
            break;
        }
    }
    private void InquiryAll(Dictionary<string, Actor> actors) {
        foreach(var actor in actors) {
            InquiryActor(actor.Key);
        }
    }
    private void Inquiry(Dictionary<string, Actor> actors) {
        List<string> list = new List<string>();
        string sz = "";
        int n = 0;
        foreach(var actor in actors) {
            list.Add(actor.Key);
            sz += String.Format("{0}({1}) ", n++, actor.Key);
        }
        while(true) {
            PrintLine();
            Console.Write(sz + " q(Quit): ");
            var input = Console.ReadLine();
            
            if(input is null) {
                return;
            }
            if(input == "q") {
                return;
            }
            int idx = int.Parse(input);
            InquiryActor(list[idx]);
        }
    }
    private void InquiryActor(string actorId) {
        //Check Motivation
        var actor = ActorHandler.Instance.GetActor(actorId);
        if(actor is null) {
            return;
        }
        Console.WriteLine("====== {0} Lv.{1} ======", actor.mUniqueId, actor.mLevel);

        actor.Print();
        var motivation = actor.GetMotivation();
        var s = actor.GetSatisfaction(motivation.Item1);
        if(s == null) {
            Console.WriteLine("Invalid motivationId");
        }else {
            Console.WriteLine("> {0} 만족도 ({1}) 동기 ({2})", actor.mUniqueId, motivation.Item2, SatisfactionDefine.Instance.GetTitle(s.SatisfactionId));
        }
    }
    private void DoActor(Dictionary<string, Actor> actors) {
        foreach(var p in actors) {            
            var actor = p.Value;        
            //Task 
            int taskid = actor.GetTaskId();
            var task = TaskHandler.Instance.GetTask(taskid);                 
            task.DoTask(actor);
            bool isLevelUp = actor.checkLevelUp();
            Console.WriteLine("> {0}: {1} ({2}), {3}", actor.mUniqueId, task.mTaskTitle, task.mTaskDesc, task.GetPrintString(actor.mUniqueId));
            if(isLevelUp == true) {
                actor.LevelUp();
                Console.WriteLine("Level up!! {0}", actor.mLevel);
            }  
        }
    }
    public void Next() {
        Int64 counter = Counter.Instance.Next();
        //Discharge
        DischargeHandler.Instance.Discharge(type);
        ActorHandler.Instance.UpdateSatisfactionSum();
        
        //happening
        var happeningList = HappeningHandler.Instance.GetHappeningCandidates(type);
        //HappeningHandler.Instance.PrintCandidates(happeningList);
        foreach(var happening in happeningList) {
            if(happening.Info is null) {
                continue;
            }
            if(HappeningHandler.Instance.Do(type, happening.Info.id) == true) {
                Console.WriteLine("{0} 발생", happening.Info.title);
                break;
            } else {
                Console.WriteLine("Failure Happening");
            }
        }
    }
}