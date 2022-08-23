using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class QuestHandler {
                const string QUEST_KEY_SATISFACTION = "SATISFACTION";
                //Actor type, Quest id, Quest list
                private Dictionary<int, Dictionary<string, ConfigQuest_Detail>> mDict = new Dictionary<int, Dictionary<string, ConfigQuest_Detail>>();
                //actor type별 top
                private Dictionary<int, int> mDictQuestTop = new Dictionary<int, int>();
                private static readonly Lazy<QuestHandler> instance =
                        new Lazy<QuestHandler>(() => new QuestHandler());
                public static QuestHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private QuestHandler() { }
                public void Add(int actorType, ConfigQuest quests) {
                    if(quests.quests == null) return;

                    //top
                    mDictQuestTop[actorType] = quests.top;

                    if(mDict.ContainsKey(actorType) == false) {
                        mDict[actorType] = new Dictionary<string, ConfigQuest_Detail>();
                    }

                    foreach(var q in quests.quests) {
                        if(q.id == null) continue;
                        mDict[actorType].Add(q.id, q);
                    }                    
                }
                public int GetTop(int actorType) {
                    if(mDictQuestTop.ContainsKey(actorType)) return mDictQuestTop[actorType];
                    return 0;
                }
                public ConfigQuest_Detail? GetQuestInfo(int actorType, string questId) {
                    if(mDict.ContainsKey(actorType) && mDict[actorType].ContainsKey(questId)) {
                        return mDict[actorType][questId];
                    }
                    return null;
                }
                //actor가 생성될때 받아가고, Actor가 저장될때 quest list도 저장되어야 한다.
                public List<string> GetQuestList(int actorType) {
                    List<string> ret = new List<string>();
                    if(mDict.ContainsKey(actorType) == true) {
                        foreach(var p in mDict[actorType]) {
                            if(p.Value.id != null) ret.Add(p.Value.id);
                        }
                    }                    
                    return ret;                
                }
                public float GetCompleteRate(Actor actor, string questId) {
                    float ret = 0.0f;
                    if(mDict.ContainsKey(actor.mType) && mDict[actor.mType].ContainsKey(questId)) {
                        ConfigQuest_Detail quest = mDict[actor.mType][questId];

                        if(quest.values != null) {

                            float sum = 0.0f;
                            int cnt = 0;
                            foreach(Config_KV_SF v in quest.values) {
                                if(v.key == string.Empty) 
                                    break;
                                string[] keys = v.key.Split(':');
                                switch(keys[0].ToUpper()) {
                                    case QUEST_KEY_SATISFACTION:
                                    {
                                        //QUEST_KEY_SATISFACTION:SatisfactionId
                                        string satisfactionId = keys[1];
                                        double p = actor.GetAccumulationSatisfaction(satisfactionId);
                                        sum += Math.Min(1.0f, (float)(p / v.value));
                                        cnt++;
                                    }
                                    break;
                                }
                            }
                            ret = sum / cnt;
                        }                        
                    }
                    return ret;
                }
                public bool Complete(Actor actor, string questId) {
                    if(GetCompleteRate(actor, questId) < 1) {
                        return false;
                    }
                    //보상 지급
                    if(mDict.ContainsKey(actor.mType) && mDict[actor.mType].ContainsKey(questId)) {
                        ConfigQuest_Detail quest = mDict[actor.mType][questId];
                        if(quest.rewards != null) {
                            foreach(var p in quest.rewards) {
                                if(p.itemId == null || actor.ReceiveItem(p.itemId, p.quantity) == false) {
                                    return false;
                                }                                
                            }
                            actor.RemoveQuest(questId);
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
    }
}