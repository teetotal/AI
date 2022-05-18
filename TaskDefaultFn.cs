using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {            
            //단순 task
            //json으로 관리            
            public class TaskDefaultFn : FnTask {
                private ConfigTask_Detail mInfo;
                public TaskDefaultFn(ConfigTask_Detail info) {
                    this.mTaskId = info.id;
                    this.mTaskTitle = info.title;
                    this.mTaskDesc = info.desc;
                    this.mInfo = info;
                }               
                public override Dictionary<string, float> GetValues(string fromActorId) {
                    if( (mInfo.relation != null && mInfo.relation.target != null && FindRelationTarget(mInfo.relation.target, fromActorId) is null)
                        || mInfo.satisfactions is null) {
                        return new Dictionary<string, float>();
                    }

                    return mInfo.satisfactions;
                }
                public override bool DoTask(Actor actor)
                {                    
                    if(mInfo.relation != null && mInfo.relation.target != null && mInfo.relation.satisfactions != null) {
                        //apply to someone
                        var targetActorId = FindRelationTarget(mInfo.relation.target, actor.mUniqueId);
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
                private string? FindRelationTarget(List<string> conditions, string fromActorId) {
                    string actorId = "";
                    //type.target1.target2:condition
                    //2.Satisfaction.100:max
                    foreach(var sz in conditions) {
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
                            
                            float value = condition == "MAX"? 0: -1;
                            foreach(var actor in actors) {
                                //자신이면 skip
                                if(actor.Value.mUniqueId == fromActorId) {
                                    continue;
                                }
                                var satisfaction = actor.Value.GetSatisfaction(target2);
                                if(satisfaction != null) {
                                    float v = satisfaction.Value;
                                    switch(condition) {
                                        case "MAX":
                                        if(v > value) {
                                            value = v;
                                            actorId = actor.Value.mUniqueId;
                                        }
                                        break;
                                        case "MIN":
                                        if(v < value) {
                                            value = v;
                                            actorId = actor.Value.mUniqueId;
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
                    return null;
                }
            }
        }
    }
}