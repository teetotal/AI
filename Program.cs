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
    //static void Main() { Start(); }
    static void Start() {                
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
            File.ReadAllText("config/village.json"),
            File.ReadAllText("config/l10n.json"),
            File.ReadAllText("config/vehicle.json"),
            File.ReadAllText("config/farming.json"),
            File.ReadAllText("config/seed.json"),
            File.ReadAllText("config/stockmarket.json")
        };
        if(p.Load(  szConfigs[0], 
                    szConfigs[1], 
                    szConfigs[2], 
                    szConfigs[3], 
                    szConfigs[4], 
                    szConfigs[5], 
                    szConfigs[6], 
                    szConfigs[7], 
                    szConfigs[8], 
                    szConfigs[9],
                    szConfigs[10],
                    szConfigs[11],
                    szConfigs[12],
                    szConfigs[13]
                    )) {
            //Battle test
            //BattleTest battle = new BattleTest();
            //battle.Init();

            //p.MainLoop();
        } else {
            Console.WriteLine("Failure loading config");
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
                        string jsonVillage,
                        string jsonL10n,
                        string jsonVehicle,
                        string jsonFarming,
                        string jsonSeed,
                        string jsonStockMarket) {
        if(!Loader.Instance.Load(jsonSatisfaction, jsonTask, jsonActor, jsonItem, jsonLevel, jsonQuest, jsonScript, jsonScenario, jsonVillage, jsonL10n, jsonVehicle, jsonFarming, jsonSeed, jsonStockMarket)) {
            Console.WriteLine("Failure Loading config");
            return false;
        }
        
        while(true) {
            StockMarketHandler.Instance.Update();
            Thread.Sleep(100);
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
            Thread.Sleep(100 * 1);
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