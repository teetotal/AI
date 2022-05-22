﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ENGINE.GAMEPLAY.MOTIVATION;
using ENGINE.GAMEPLAY;

#nullable enable
class MainClass {    
    static void Main() {        
        
        var p = new Loop();
        if(p.Load("config/satisfactions.json", "config/actors.json", "config/item.json", "config/level.json", "config/quest.json")) {
            //Battle test
            BattleTest battle = new BattleTest();
            battle.Init();

            //p.MainLoop();
        } else {
            Console.WriteLine("Failure loading config");
        }
    }    
}

public class BattleTest {
    private Battle mBattle;
    public void Init() {        
        int[,] mapAdv1 =
        {
            {2,      2,      2,       2,      2,    2,  2,  2},
            {2,      2,      2,       2,      2,    2,  2,  2},
            {3,      2,      2,       2,      2,    2,  2,  3},
            {2,      2,      2,       2,      2,    2,  2,  2},
            {2,      2,      2,       2,      2,    2,  2,  2}
        };
        /*
        int[,] mapAdv2 =
        {
            {7,      8,      7,       6,      5,    2},
            {8,      8,      7,       6,      5,    3},
            {9,      8,      7,       6,      5,    4},
            {8,      8,      7,       6,      5,    3},
            {7,      8,      7,       6,      5,    2}
        };
        */

        mBattle = new Battle(mapAdv1.GetLength(1), mapAdv1.GetLength(0));
        if(!mBattle.Init(mapAdv1, mapAdv1)) {            
            return;
        }

        var pActorHomeF = ActorHandler.Instance.GetActor("hf");
        var pActorHomeB = ActorHandler.Instance.GetActor("hb");
        var pActorHomeS = ActorHandler.Instance.GetActor("hs");

        var pActorAwayF = ActorHandler.Instance.GetActor("af");
        var pActorAwayB = ActorHandler.Instance.GetActor("ab");
        var pActorAwayS = ActorHandler.Instance.GetActor("as");
        
        if( pActorHomeF == null || pActorHomeB == null || pActorHomeS == null || 
            pActorAwayF == null || pActorAwayB == null || pActorAwayS == null) 
        {
            return;
        }
        BattleActorAbility abilityForward = new BattleActorAbility();
        abilityForward.Sight = 1;        
        abilityForward.Speed = 1;
        abilityForward.MoveForward = 3f;
        abilityForward.MoveBack = 0;
        abilityForward.MoveSide = 0;

        BattleActorAbility abilityBack = new BattleActorAbility();
        abilityBack.Sight = 1;        
        abilityBack.Speed = 1;
        abilityBack.MoveForward = 0;
        abilityBack.MoveBack = 1.5f;
        abilityBack.MoveSide = 0;

        BattleActorAbility abilitySide = new BattleActorAbility();
        abilitySide.Sight = 1;        
        abilitySide.Speed = 1;
        abilitySide.MoveForward = 0;
        abilitySide.MoveBack = 0;
        abilitySide.MoveSide = 1.5f;

        mBattle.AppendActor(0, 2, pActorHomeF, BATTLE_SIDE.HOME, abilityForward);
        mBattle.AppendActor(3, 0, pActorHomeB, BATTLE_SIDE.HOME, abilityBack);
        mBattle.AppendActor(2, 2, pActorHomeS, BATTLE_SIDE.HOME, abilitySide);

        mBattle.AppendActor(7, 2, pActorAwayF, BATTLE_SIDE.AWAY, abilityForward);
        mBattle.AppendActor(4, 4, pActorAwayB, BATTLE_SIDE.AWAY, abilityBack);
        mBattle.AppendActor(4, 3, pActorAwayS, BATTLE_SIDE.AWAY, abilitySide);
        if(!mBattle.Validate()) {
            Console.WriteLine("Invalid");
            return;
        }

        while(true) {
            mBattle.mMap.Print();
            Dictionary<string, string[]> next = mBattle.Next();
            foreach(var p in next) {
                mBattle.Occupy(p.Key);
            }
        }
    }
}



public class Loop {
    int type = 1;    
    public void MainLoop() {
        var actors = ActorHandler.Instance.GetActors(type);
        while(actors != null) {      
            Thread.Sleep(1000 * 1);
            PrintLine();
            Console.Write("0(System) 1(Next) 2(Task) 3(Inquiry) 4(Inquiry All) 5(Inventory) q(Quit): ");
            //var input = Console.ReadLine();
            string input  = "2";
            Console.WriteLine();
            PrintLine();
            if(input != null) {
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
                    case 5:
                    InventoryAll(actors);
                    break;
                }
            }
        }
    }
    public bool Load(string pathSatisfaction, string pathActor, string pathItem, string pathLevel, string pathQuest) {
        var pLoader = new Loader();
        if(!pLoader.Load(pathSatisfaction, pathActor, pathItem, pathLevel, pathQuest)) {
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
    private void InventoryAll(Dictionary<string, Actor> actors) {
        foreach(var actor in actors) {
            Console.WriteLine("{0} {1}", actor.Key, actor.Value.PrintInventory());
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
            string? taskid = actor.GetTaskId();
            if(taskid == null) continue;             
            var task = TaskHandler.Instance.GetTask(taskid);                 
            if(task == null) continue;   
            task.DoTask(actor);

            //levelup
            bool isLevelUp = actor.checkLevelUp();
            Console.WriteLine("> {0}: {1} ({2}), {3}", actor.mUniqueId, task.mTaskTitle, task.mTaskDesc, task.GetPrintString(actor.mUniqueId));            
            if(isLevelUp == true) {
                var reward = LevelHandler.Instance.Get(actor.mType, actor.mLevel);
                if(reward != null && reward.next != null && reward.next.rewards != null) {
                    actor.LevelUp(reward.next.rewards);
                    Console.WriteLine("Level up!! {0}", actor.mLevel);
                    foreach(var item in reward.next.rewards) {
                        if(item.itemId != null)
                            Console.WriteLine("> Reward {0}", ItemHandler.Instance.GetPrintString(item.itemId));
                    }
                    
                }
            }

            //quest
            string completeQuestId = "";
            List<string> quests = actor.GetQuest();
            foreach(string questId  in quests) {
                ConfigQuest_Detail? quest = QuestHandler.Instance.GetQuestInfo(actor.mType, questId);
                if(quest == null) continue;
                float complete = QuestHandler.Instance.GetCompleteRate(actor, questId);
                Console.WriteLine("Quest> {0} ({1}) {2}%", quest.title, quest.desc, complete * 100);
                if(complete >= 1 && completeQuestId.Length == 0) {
                    //완료 처리
                    completeQuestId = questId;
                }
            }

            if(completeQuestId.Length > 0) {
                bool ret = QuestHandler.Instance.Complete(actor, completeQuestId);                
                ConfigQuest_Detail? quest = QuestHandler.Instance.GetQuestInfo(actor.mType, completeQuestId);
                if(quest == null) continue;
                Console.WriteLine("Quest Complete> {0} ({1}) {2}", quest.title, quest.desc, ret);
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