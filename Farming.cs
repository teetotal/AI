using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class FarmingHandler {
                public delegate void FnPlant(string farmId, string fieldId, string seedId);
                public delegate void FnComplete(string farmId, string fieldId, string seedId);
                private FnPlant? mFnPlant = null;
                private FnComplete? mFnComplete = null;
                //village, info
                private Dictionary<string, List<ConfigFarming_Detail>> mDicFarmInfoByVillage = new Dictionary<string, List<ConfigFarming_Detail>>();
                //farm id, info
                private Dictionary<string, ConfigFarming_Detail> mDicFarm = new Dictionary<string, ConfigFarming_Detail>();
                private Dictionary<string, ConfigFarming_Seed> mDicSeed = new Dictionary<string, ConfigFarming_Seed>();
                private static readonly Lazy<FarmingHandler> instance =
                                    new Lazy<FarmingHandler>(() => new FarmingHandler());
                public static FarmingHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private FarmingHandler() { }
                public void Update(string village) {
                    var list = GetFarms(village);
                    if(list == null)
                        return;
                    
                    long now = CounterHandler.Instance.GetCount();
                    for(int i = 0; i < list.Count; i++) {
                        for(int j = 0; j < list[i].fields.Count; j++) {
                            if(list[i].fields[j].seedId != string.Empty) {
                                var field = list[i].fields[j];
                                if(GetRemainCount(field, now) <= 0) {
                                    //callback
                                    if(mFnComplete != null) {
                                        mFnComplete(list[i].farmId, field.fieldId, field.seedId);
                                    }
                                }
                            }
                        }
                    }
                }
                private int GetRemainCount(ConfigFarming_Field field, long now) {
                    ConfigFarming_Seed seed = mDicSeed[field.seedId];
                    return (seed.duration - (field.cares * seed.careValue)) - (int)(now - field.startCount);
                }
                public void Set(Dictionary<string, List<ConfigFarming_Detail>> p) {
                    mDicFarmInfoByVillage = p;
                    foreach(var f in mDicFarmInfoByVillage) {
                        for(int i = 0; i < f.Value.Count; i++) {
                            ConfigFarming_Detail farm = f.Value[i];
                            mDicFarm.Add(farm.farmId, farm);
                        }
                    }
                }
                public void SetSeed(Dictionary<string, ConfigFarming_Seed> p) {
                    mDicSeed = p;
                }
                public void SetCallback(FnPlant fnPlant, FnComplete fnComplete) {
                    mFnPlant = fnPlant;
                    mFnComplete = fnComplete;
                }
                public List<ConfigFarming_Detail>? GetFarms(string village) {
                    if(!mDicFarmInfoByVillage.ContainsKey(village))
                        return null;
                    return mDicFarmInfoByVillage[village];
                }
               
                private bool GetTillage(string farmId) {
                    if(!mDicFarm.ContainsKey(farmId)) {
                        throw new Exception("Invalid farm id. " + farmId);
                    }
                  
                    return mDicFarm[farmId].tillage;
                }
                private int GetIdleFieldCount(string farmId) {
                    int ret = 0;
                    for(int i = 0; i < mDicFarm[farmId].fields.Count; i++) {
                        if(mDicFarm[farmId].fields[i].seedId == string.Empty)
                            ret++;
                    }
                    return ret;
                }
                private bool Plant(string farmId, string seedId) {
                    for(int i = 0; i < mDicFarm[farmId].fields.Count; i++) {
                        if(mDicFarm[farmId].fields[i].seedId == string.Empty) {
                            mDicFarm[farmId].fields[i].seedId = seedId;
                            mDicFarm[farmId].fields[i].startCount = CounterHandler.Instance.GetCount();
                            //callback
                            if(mFnPlant != null) {
                                mFnPlant(farmId, mDicFarm[farmId].fields[i].fieldId, seedId);
                            }
                            return true;
                        }
                    }
                    return false;
                }
                public bool Check(Actor actor, FnTask task) {
                    string[] arr = task.mInfo.integration.Split(':');
                    string type = arr[0];
                    string farmId = arr[1];
                    string action = arr[2];

                    return Check(actor, task, arr, farmId, action);
                }
                private bool Check(Actor actor, FnTask task, string[] arr, string farmId, string action) {
                    switch(action) {
                        case "TILLAGE": {
                            if(GetTillage(farmId))
                                return false;
                            return true;
                        }
                        case "PLANT": {
                            if(!GetTillage(farmId))
                                return false;
                            if(GetIdleFieldCount(farmId) == 0)
                                return false;
                            return true;
                        }
                        default:
                        return true;
                    }
                }
                public bool Integration(Actor actor, FnTask task) {
                    string[] arr = task.mInfo.integration.Split(':');
                    string type = arr[0];
                    string farmId = arr[1];
                    string action = arr[2];

                    if(!Check(actor, task, arr, farmId, action))
                        return false;
                    return Integration(actor, task, arr, farmId, action);
                }
                private bool Integration(Actor actor, FnTask task, string[] arr, string farmId, string action) {
                    switch(action) {
                        case "TILLAGE": {                            
                            mDicFarm[farmId].tillage = true;
                            break;
                        }
                        case "PLANT": {
                            return Plant(farmId, arr[3]);
                        }
                        default:
                        break;
                    }
                    return true;
                }
            }
        }
    }
}