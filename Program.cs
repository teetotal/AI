using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ENGINE.GAMEPLAY.MOTIVATION;
using ENGINE.GAMEPLAY;

#nullable enable
class MainClass {    
    static void Main() {                
        //var p = new Loop();
        var p = new GameControlInstance();
        string[] szConfigs = new string[] {
            File.ReadAllText("config/satisfactions.json"),
            File.ReadAllText("config/task.json"),
            File.ReadAllText("config/actors.json"),
            File.ReadAllText("config/item.json"),
            File.ReadAllText("config/level.json"),
            File.ReadAllText("config/quest.json"),
            File.ReadAllText("config/script.json"),
            File.ReadAllText("config/scenario.json"),
            File.ReadAllText("config/village.json")
        };
        if(p.Load(szConfigs[0], szConfigs[1], szConfigs[2], szConfigs[3], szConfigs[4], szConfigs[5], szConfigs[6], szConfigs[7], szConfigs[8])) {
            //Battle test
            //BattleTest battle = new BattleTest();
            //battle.Init();

            //p.MainLoop();
        } else {
            Console.WriteLine("Failure loading config");
        }
    }    
}

public class BattleTest {
    private Battle? mBattle;
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
        abilityForward.HP = 9;
        abilityForward.AttackStyle = BattleActorAbility.ATTACK_STYLE.MOVER;
        abilityForward.AttackPower = 1;
        abilityForward.AttackDistance = 1;        
        abilityForward.AttackAccuracy = 1;
        abilityForward.Sight = 1;        
        abilityForward.Speed = 1;
        abilityForward.MoveForward = 3f;
        abilityForward.MoveBack = 0;
        abilityForward.MoveSide = 0;

        BattleActorAbility abilityBack = new BattleActorAbility();
        abilityBack.HP = 9;
        abilityBack.AttackStyle = BattleActorAbility.ATTACK_STYLE.MOVER;
        abilityBack.AttackPower = 1;
        abilityBack.AttackDistance = 1;   
        abilityBack.AttackAccuracy = 1;     
        abilityBack.Sight = 1;        
        abilityBack.Speed = 1;
        abilityBack.MoveForward = 0;
        abilityBack.MoveBack = 1.5f;
        abilityBack.MoveSide = 0;

        BattleActorAbility abilitySide = new BattleActorAbility();
        abilitySide.HP = 9;
        abilitySide.AttackStyle = BattleActorAbility.ATTACK_STYLE.DEFENDER;
        abilitySide.AttackPower = 1;
        abilitySide.AttackDistance = 1;      
        abilitySide.AttackAccuracy = 1;  
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
            Thread.Sleep(1000 * 1);
            mBattle.Print();
            Dictionary<string, BattleActorAction> next = mBattle.Next();            
            foreach(var p in next) {
                switch(p.Value.Type) {
                    case BATTLE_ACTOR_ACTION_TYPE.MOVING:
                    mBattle.Occupy(p.Key);
                    break;
                    case BATTLE_ACTOR_ACTION_TYPE.ATTACKED:
                    mBattle.Attacked(p.Key, p.Value);
                    break;
                }
                    
                
            }
        }
    }
}

public class DialogueInstance {
    public Actor from, to;
    public FnTask fromTask;
    private bool result;
    public DialogueInstance(Actor from, Actor to, FnTask fromTask) {
        this.from = from;
        this.to = to;
        this.fromTask = fromTask;
        //미리 decide를 하고 animation 
        result = to.Loop_Decide();
    }
    public void Do() {
        if(result) {
            from.Loop_DoTask();
            to.Loop_AutoDoTask(fromTask.mInfo.target.interaction.taskId);
        } else {
            from.Loop_Refusal();
            to.Loop_Release();
        }
        
    }
}

public class ActorInstance {
    public Actor mActor;
    public ActorInstance(Actor actor) {
        mActor = actor;
        mActor.SetCallback(Callback);
    }
    public void Callback(Actor.LOOP_STATE state, Actor actor) {
        Thread.Sleep(100);
        Console.WriteLine("{0}({1}) - {2}", actor.mUniqueId, actor.mInfo.nickname, state);
        switch(state) {
            case Actor.LOOP_STATE.INVALID:
            break;
            case Actor.LOOP_STATE.READY:
            break;            
            case Actor.LOOP_STATE.TASK_UI:
            break;
            case Actor.LOOP_STATE.TAKE_TASK:
            Console.WriteLine("- {0}", actor.GetCurrentTaskTitle());
            actor.Loop_Move();
            break;
            case Actor.LOOP_STATE.MOVE:
            //이동 처리
            //도착하면 
            switch(actor.GetCurrentTask().mInfo.target.interaction.type) {
                case TASK_INTERACTION_TYPE.ASK:
                case TASK_INTERACTION_TYPE.INTERRUPT:
                actor.Loop_Dialogue();
                return;
                default:
                actor.Loop_Animation();
                return;
            }                        
            case Actor.LOOP_STATE.ANIMATION:
            actor.Loop_DoTask();
            break;
            case Actor.LOOP_STATE.RESERVED:
            actor.Loop_LookAt();
            break;
            case Actor.LOOP_STATE.LOOKAT:
            //쳐다 보기 설정
            //actor.GetTaskContext().interactionFromActor;

            //Decide가 호출될때 까지 처다만 본다.
            break;
            case Actor.LOOP_STATE.DIALOGUE:
            //Dialogue로 handover
            DialogueInstance p = new DialogueInstance(actor, actor.GetTaskContext().GetTargetActor(), actor.GetCurrentTask());       
            p.Do();     
            break;            
            case Actor.LOOP_STATE.SET_TASK:
            Console.WriteLine("- {0}", actor.GetCurrentTaskTitle());
            actor.Loop_Move();
            break;
            case Actor.LOOP_STATE.DO_TASK:
            actor.Loop_Levelup();
            break;
            case Actor.LOOP_STATE.AUTO_DO_TASK:
            Console.WriteLine("- {0}", actor.GetCurrentTaskTitle());
            actor.Loop_Levelup();
            break;
            case Actor.LOOP_STATE.LEVELUP:
            //levelup 모션 처리
            actor.Loop_Chain();
            break;
            case Actor.LOOP_STATE.REFUSAL:
            actor.Loop_Chain();
            break;            
            case Actor.LOOP_STATE.RELEASE:
            actor.Loop_Ready();
            break;
            case Actor.LOOP_STATE.DISCHARGE:
            //update UI
            break;
            case Actor.LOOP_STATE.TAX_COLLECTION:
            Console.WriteLine("Tax Collection");
            break;
        }
    }
}

public class GameControlInstance {
    public Dictionary<string, ActorInstance> mDictActor = new Dictionary<string, ActorInstance>();
    private int ManagedInterval = 3;
    private void VillageLevelUpCallback(string villageId, int level) {
        ConfigVillage_Detail info = ActorHandler.Instance.GetVillageInfo(villageId);
        Console.WriteLine("---------\nLevel Up Village ({0}) lv.{1}\n---------", info.name, level);
    }
    public bool Load(   string jsonSatisfaction, 
                        string jsonTask, 
                        string jsonActor, 
                        string jsonItem, 
                        string jsonLevel, 
                        string jsonQuest, 
                        string jsonScript, 
                        string jsonScenario,
                        string jsonVillage) {
        var pLoader = new Loader();
        if(!pLoader.Load(jsonSatisfaction, jsonTask, jsonActor, jsonItem, jsonLevel, jsonQuest, jsonScript, jsonScenario, jsonVillage)) {
            Console.WriteLine("Failure Loading config");
            return false;
        }
        //!!반드시 해줘야 pet과 tax, Village level up 이 됨
        ActorHandler.Instance.PostInit(VillageLevelUpCallback);
        
        DecideAlwaysTrue decide = new DecideAlwaysTrue();
        foreach(var p in ActorHandler.Instance.GetActors()) {
            Actor actor = p.Value;            
            var instance = new ActorInstance(actor);
            mDictActor.Add(p.Key, instance);           
        }

        Thread myThread = new Thread(new ParameterizedThreadStart(ThreadFn)); 
        myThread.Start(this); 

        return true;
    }
    public void Do(ActorInstance actor, long counter) {     
        if(!actor.mActor.IsAutoTakeable())
            return;   
        
        //counter 확인
        if(counter - actor.mActor.GetTaskContext().lastCount < ManagedInterval)
            return;
        
        // UI & Auto확인

        //take task
        if(!actor.mActor.Loop_TakeTask()) {
            actor.mActor.Loop_Ready();
        }

    }

    private static void ThreadFn(object? instance) {
        if(instance == null)
            return;

        GameControlInstance pInstance = (GameControlInstance)instance;
        int type = 1;
        while(true) {
            //Thread.Sleep(1000 * 1);
            Console.WriteLine("Thread -----------------------------------------");
            long counter = CounterHandler.Instance.Next();
            
            //Discharge
            DischargeHandler.Instance.Discharge(type);            
            ActorHandler.Instance.TaxCollection();

            foreach(var actor in pInstance.mDictActor) {
                pInstance.Do(actor.Value, counter);
            }
        }
    }
}
/*
public class Loop {
    int type = 1;    
    Queue<KeyValuePair<Actor.LOOP_STATE, Actor>> mLoopQueue = new Queue<KeyValuePair<Actor.LOOP_STATE, Actor>>();
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
                    {
                        Next();
                        foreach(var p in ActorHandler.Instance.GetActors()) {
                            Actor actor = p.Value;
                            if(actor.GetState() == Actor.STATE.READY && actor.TakeTask() == false) {                    
                                Console.WriteLine(p.Key + " Take Task Failure");
                                continue;            
                            }
                        }
                    }
                    //Next();
                    //TakeTask(actors);
                    //DoActor(actors);                    
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
    public bool Load(string jsonSatisfaction, string jsonTask, string jsonActor, string jsonItem, string jsonLevel, string jsonQuest, string jsonScript, string jsonScenario) {
        var pLoader = new Loader();
        if(!pLoader.Load(jsonSatisfaction, jsonTask, jsonActor, jsonItem, jsonLevel, jsonQuest, jsonScript, jsonScenario)) {
            Console.WriteLine("Failure Loading config");
            return false;
        }
        DecideAlwaysTrue decide = new DecideAlwaysTrue();
        foreach(var p in ActorHandler.Instance.GetActors()) {
            Actor actor = p.Value;
            actor.SetCallback(Callback);
            actor.SetDecideFn(decide);
        }
        return true;
    }
    private void PrintLine() {
        Console.WriteLine("---------------------------------------------------------");
    }
    //Loop -------------------------------------------------------------------------------
    public void Callback(Actor.LOOP_STATE state, Actor actor) {
        switch(state) {
            default:
            mLoopQueue.Enqueue(new KeyValuePair<Actor.LOOP_STATE, Actor>(state, actor));
            break;
        }
    }
    private void Dequeue() {
        if(mLoopQueue.Count == 0)
            return;
        var node = mLoopQueue.Dequeue();

    }
    //-------------------------------------------------------------------------------
    public void Callback(Actor.CALLBACK_TYPE type, string actorId) {
        //Console.WriteLine(actorId + " " + type.ToString());
        var actor = ActorHandler.Instance.GetActor(actorId);
        if(actor == null)
            return;

        switch(type) {            
            case Actor.CALLBACK_TYPE.TAKE_TASK:
            {
                Console.WriteLine(string.Format("{0} 가장 부족한 능력 {1}", actor.mUniqueId, actor.GetMyMinSatisfaction()));
                DoActor(actor, !actor.DoTaskBefore());
            }
            break;
            case Actor.CALLBACK_TYPE.ASKED:
            break;
            case Actor.CALLBACK_TYPE.SET_READY:
            break;
            case Actor.CALLBACK_TYPE.DO_TASK:
            break;
            case Actor.CALLBACK_TYPE.RESERVE:
            break;
            case Actor.CALLBACK_TYPE.RESERVED:
            break;
            case Actor.CALLBACK_TYPE.ASK:
            break;
            case Actor.CALLBACK_TYPE.INTERRUPT:
            break;
            case Actor.CALLBACK_TYPE.INTERRUPTED:
            break;
            case Actor.CALLBACK_TYPE.REFUSAL:
            actor.GetTaskContext().ReleaseAck();
            break;
            case Actor.CALLBACK_TYPE.LEVELUP:
            Console.WriteLine("Level up!! {0}", actor.mLevel);
            break;
            case Actor.CALLBACK_TYPE.DISCHARGE:
            Console.WriteLine("{0} DISCHARGE", actor.mUniqueId);
            break;
        }
    }
    //---------------------------------------------------------------------------
    private void System() {
        Console.Write("s(SatisfactionSum) c(Counter): ");
        var input = Console.ReadLine();
        switch(input) {
            case "s":
            ActorHandler.Instance.PrintSatisfactionSum(type);
            break;
            case "c":
            Console.WriteLine("Counter {0}", CounterHandler.Instance.GetCount());
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
    private bool DoActor(Actor actor, bool isRefusal) {
                    
        if(actor.GetState() != Actor.STATE.TASKED)
            return false;
        //Task 
        FnTask? task = actor.GetCurrentTask();
        if(task == null) 
            return false;
        string pre = "-";
        switch(task.mInfo.target.interaction.type) {
            case TASK_INTERACTION_TYPE.ASK:
            pre = "!";
            break;
        }
        if(task.mInfo.type == TASK_TYPE.REACTION) {
            pre = ">";
        }
        Console.WriteLine(pre + " {0}: {1} ({2}), {3} refusal({4}) ref({5}), {6} : {7}", 
            actor.mUniqueId, task.mTaskTitle, task.mTaskDesc, actor.GetTaskString(), isRefusal, TaskHandler.Instance.GetRef(task.mTaskId), 
            ScriptHandler.Instance.GetScript(task.mTaskId, actor, actor.GetTaskContext().GetTargetActor()), actor.GetLevelUpProgress());
        actor.DoTask(isRefusal);
        

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
            if(quest == null) throw new Exception("Invalid Quest." + completeQuestId);
            Console.WriteLine("Quest Complete> {0} ({1}) {2}", quest.title, quest.desc, ret);
        }
        
        return true;
    }
    public void Next() {
        Int64 counter = CounterHandler.Instance.Next();
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
*/