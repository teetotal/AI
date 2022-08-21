using System;
using System.Collections.Generic;
using System.Text;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {            
            //단순 task
            //json으로 관리            
            public class TaskDefaultFn : FnTask {        
                private Random mRandom = new Random();        
                private const float DISTANCE_MAX = 40;
                private string template = "<size=100%>{title}</size>\n<size=80%>{desc} <color=#E4CA44>{satisfaction}</color></size>";
                private Dictionary<string, float> mSatisfaction = new Dictionary<string, float>();
                private Dictionary<string, float> mSatisfactionRefusal = new Dictionary<string, float>();
                private const string VEHICLE = "VEHICLE";
                private const char TARGET_DELIMETER = ':';
                private StringBuilder mSzBuilder = new StringBuilder();

                public TaskDefaultFn(ConfigTask_Detail info) {
                    this.mTaskId = info.id;
                    this.mTaskTitle = info.title;
                    this.mTaskDesc = info.desc;
                    this.mInfo = info;
                    //SetTaskString();
                }   
                public override void SetTaskString() {
                    this.mTaskString = this.template.Replace("{title}", this.mTaskTitle);
                    this.mTaskString = this.mTaskString.Replace("{desc}", this.mTaskDesc);
                    string satisfaction = String.Empty;
                    foreach(var s in GetSatisfactions()) {
                        satisfaction += SatisfactionDefine.Instance.GetTitle(s.Key);
                        satisfaction += " ";
                        if(s.Value > 0) {
                            satisfaction += "+";
                        }
                        satisfaction += s.Value.ToString();
                        satisfaction += " ";
                    }
                    this.mTaskString = this.mTaskString.Replace("{satisfaction}", satisfaction);
                }  
                private string? ReplaceTargetName(string actorId) {
                    if(mInfo.target.value == null)
                        throw new Exception("mInfo.target.value must be not null");
                    string target = mInfo.target.value[mRandom.Next(0, mInfo.target.value.Count)];
                    string[] szArr  = target.Split(TARGET_DELIMETER);
                    string sz = szArr[0];
                    string[] arr = sz.Split('.');
                    if(arr.Length == 1)
                        return sz;
                    switch(arr[0]) {
                        case VEHICLE: {
                            var veh = VehicleHandler.Instance.GetOne(arr[1], actorId);
                            if(veh == null)
                                return null;
                            if(szArr.Length == 2) {
                                mSzBuilder.Clear();
                                mSzBuilder.Append(veh.vehicleId);
                                mSzBuilder.Append(TARGET_DELIMETER);
                                mSzBuilder.Append(szArr[1]);
                                return mSzBuilder.ToString();
                            } else {
                                return veh.vehicleId;
                            }
                        }
                        default:
                            return sz;
                    }
                }
                public override Tuple<Actor.TASKCONTEXT_TARGET_TYPE, string, Position?, Position?> GetTargetObject(Actor actor) {
                    Actor.TASKCONTEXT_TARGET_TYPE type = Actor.TASKCONTEXT_TARGET_TYPE.INVALID;
                    string? targetValue = (mInfo.target.value == null || mInfo.target.type == TASK_TARGET_TYPE.NON_TARGET) ? string.Empty : ReplaceTargetName(actor.mUniqueId);   
                    if(targetValue == null)
                        return new Tuple<Actor.TASKCONTEXT_TARGET_TYPE, string, Position?, Position?>(Actor.TASKCONTEXT_TARGET_TYPE.INVALID, string.Empty, null, null);  

                    Position? position = null;
                    Position? lootAt = null;

                    switch(mInfo.target.type) {
                        case TASK_TARGET_TYPE.NON_TARGET:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.NON_TARGET;
                        break;
                        case TASK_TARGET_TYPE.ACTOR:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.ACTOR;
                        break;
                        case TASK_TARGET_TYPE.OBJECT:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.OBJECT;
                        break;
                        case TASK_TARGET_TYPE.ACTOR_CONDITION: {
                            targetValue = FindRelationTarget(actor);
                            if(targetValue == string.Empty)
                                type = Actor.TASKCONTEXT_TARGET_TYPE.INVALID;
                            else
                                type = Actor.TASKCONTEXT_TARGET_TYPE.ACTOR;
                        }
                        break;
                        case TASK_TARGET_TYPE.ACTOR_FROM:
                        Actor.TaskContext context = actor.GetTaskContext();
                        if(context.reserveContext.fromActor == null)
                            targetValue = string.Empty;    
                        else 
                            targetValue = context.reserveContext.fromActor.mUniqueId;
                        type = Actor.TASKCONTEXT_TARGET_TYPE.ACTOR;
                        break;
                        case TASK_TARGET_TYPE.POSITION:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.POSITION;
                        if(mInfo.target.isAssignedPosition) {
                            position = mInfo.target.position;
                            lootAt = mInfo.target.lookAt;
                        }
                        break;
                        case TASK_TARGET_TYPE.FLY:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.FLY;
                        if(mInfo.target.isAssignedPosition) {
                            position = mInfo.target.position;
                            lootAt = mInfo.target.lookAt;
                        }
                        break;
                        case TASK_TARGET_TYPE.RESERVE_VEHICLE:
                        case TASK_TARGET_TYPE.GET_IN_VEHICLE:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.OBJECT;
                        break;
                        case TASK_TARGET_TYPE.GET_OFF_VEHICLE:
                        type = Actor.TASKCONTEXT_TARGET_TYPE.NON_TARGET;
                        break;
                    }
                    return new Tuple<Actor.TASKCONTEXT_TARGET_TYPE, string, Position?, Position?>(type, targetValue, position, lootAt);
                }          
                public override Tuple<Dictionary<string, float>, Dictionary<string, float>>? GetValues(Actor actor) {
                    switch(mInfo.target.type) {
                        case TASK_TARGET_TYPE.ACTOR_CONDITION:
                        if(FindRelationTarget(actor).Length == 0)
                            return null;
                        break;
                        default:
                        break;
                    }
                    //chain 처리
                    FnTask? p = this;
                    Dictionary<string, float> satisfaction = new Dictionary<string, float>();
                    Dictionary<string, float> refusal = new Dictionary<string, float>();

                    while(p != null) {
                        var s = p.GetSatisfactions();
                        foreach(var kv in s) {
                            if(satisfaction.ContainsKey(kv.Key))
                                satisfaction[kv.Key] += kv.Value;
                            else
                                satisfaction[kv.Key] = kv.Value;
                        }
                        var r = p.GetSatisfactionsRefusal();
                        foreach(var kv in r) {
                            if(refusal.ContainsKey(kv.Key))
                                refusal[kv.Key] += kv.Value;
                            else
                                refusal[kv.Key] = kv.Value;
                        }
                        if(p.mInfo.chain != string.Empty)
                            p = TaskHandler.Instance.GetTask(p.mInfo.chain);
                        else
                            p = null;
                    }
                    return new Tuple<Dictionary<string, float>, Dictionary<string, float>>(satisfaction, refusal);
                }
                //시세에 따라 달라지는 문제는 여기서 처리한다.
                public override Dictionary<string, float> GetSatisfactions() {
                    mSatisfaction.Clear();
                    if(mInfo != null && mInfo.satisfactions != null) {
                        var resources = GetResources(mInfo.satisfactions);
                        foreach(var p in mInfo.satisfactions) {
                            if(CheckImmutableSatisfaction(p.Value)) {
                                mSatisfaction.Add(p.Key, float.Parse(p.Value));
                            } else {
                                mSatisfaction.Add(p.Key, GetMutableSatisfactionValue(resources, p.Value));
                            }
                        }
                    }
                    return mSatisfaction;
                }
                public override Dictionary<string, float> GetSatisfactionsRefusal() {
                    mSatisfactionRefusal.Clear();
                    if(mInfo != null && mInfo.satisfactionsRefusal != null) {
                        var resources = GetResources(mInfo.satisfactions);
                        foreach(var p in mInfo.satisfactionsRefusal) {
                            if(CheckImmutableSatisfaction(p.Value)) {
                                mSatisfactionRefusal.Add(p.Key, float.Parse(p.Value));
                            } else {
                                mSatisfactionRefusal.Add(p.Key, GetMutableSatisfactionValue(resources, p.Value));
                            }
                        }
                    }
                    return mSatisfactionRefusal;
                }
                /*
                public override bool DoTask(Actor actor)
                {                    
                    if(mInfo != null && mInfo.relation != null && mInfo.relation.target != null && mInfo.relation.satisfactions != null) {
                        //apply to someone
                        var targetActorId = FindRelationTarget(actor);
                        if(targetActorId is null) {
                            return false;
                        }
                        var targetActor = ActorHandler.Instance.GetActor(targetActorId);
                        if(targetActor is null) {
                            return false;
                        }
                        foreach(var p in mInfo.relation.satisfactions) {
                            targetActor.ApplySatisfaction(p.Key, p.Value, 0, actor.mUniqueId);
                        }
                    }
                    //Quest 계산
                    if(mTaskId != null) {
                        actor.DoTask(mTaskId);                        
                    }                    
                    this.ApplyValue(actor);
                    return true;
                    
                }
                */
                
                private string FindRelationTarget(Actor actor) {
                    if(mInfo.target.value == null || mInfo.target.value.Count == 0 || mInfo.target.type != TASK_TARGET_TYPE.ACTOR_CONDITION) {
                        return string.Empty;
                    }
                    List<string> conditions = mInfo.target.value;
                    string fromActorId = actor.mUniqueId;                    
                    string actorId = string.Empty;
                    //type.target1.target2:condition
                    //2.Satisfaction.100:max
                    for(int i = 0; i < conditions.Count; i++)
                    {
                        string sz = conditions[i];
                        int type;                        
                        string condition;
                        //표현식을 확장할때 고치자. 일단은 고정된 형태만
                        string[] arr = sz.Split(':');
                        condition = arr[1].ToUpper();
                        string[] arr2 = arr[0].Split('.');
                        type = int.Parse(arr2[0]);
                        string target1 = arr2[1]; 
                        string target2 = arr2[2];
                        if(target1.ToUpper() == "SATISFACTION") {
                            var actors = ActorHandler.Instance.GetActors(type);
                            if(actors is null) {
                                continue;
                            }

                            float value = condition == "MAX"? -1: float.MaxValue;
                            foreach(var p in actors) {
                                //자신이면 skip
                                if(p.Value.mUniqueId == fromActorId) {
                                    continue;
                                }
                                //reserved가 아니고, 허용 가능한 범위 안에 있고 현재 Ready이거나 Tasking인 상태 actor                                
                                if(actor.position == null || p.Value.position == null) {
                                    continue;
                                }
                                double distance = actor.position.GetDistance(p.Value.position);
                                if(distance > DISTANCE_MAX || p.Value.GetState() != Actor.LOOP_STATE.READY) {
                                    continue;
                                }
                                var satisfaction = p.Value.GetSatisfaction(target2);
                                if(satisfaction != null) {
                                    float v = satisfaction.Value;
                                    switch(condition) {
                                        case "MAX":
                                        if(v > value) {
                                            value = v;
                                            actorId = p.Value.mUniqueId;
                                        }
                                        break;
                                        case "MIN":
                                        if(v < value) {
                                            value = v;
                                            actorId = p.Value.mUniqueId;
                                        }
                                        break;
                                    }                                    
                                }
                            }    
                            //찾았으면 리턴. 없으면 다음 조건으로 그래도 없으면 null
                            if(actorId.Length > 0) {
                                return actorId;
                            }
                        }                        
                    }
                    return string.Empty;
                }
            }
        }
    }
}