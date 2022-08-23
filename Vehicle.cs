using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class VehicleHandler {
                private enum STATE {
                    IDLE,
                    MOVING,
                    RESERVED
                }
                public delegate bool FnHangAround(string vehicleId, string position, string rotation);  
                private FnHangAround? mFnHangAround = null;
                private string mCurrentVillage = string.Empty;

                //아래 3개 합쳐야 함
                private Dictionary<string, ConfigVehicle_Detail> mDic = new Dictionary<string, ConfigVehicle_Detail>();
                private Dictionary<string, STATE> mDicState = new Dictionary<string, STATE>();
                private Dictionary<string, long> mDicLastTime = new Dictionary<string, long>();//마지막 counter


                private Dictionary<string, string> mDicReseve = new Dictionary<string, string>(); //예약차량
                
                private Dictionary<string, List<string>> mDicType = new Dictionary<string, List<string>>();
                private List<ConfigVehicle_Detail> mTempList = new List<ConfigVehicle_Detail>();
                public Random mRand= new Random();
                private long mLastUpdated = 0;
                private static readonly Lazy<VehicleHandler> instance =
                        new Lazy<VehicleHandler>(() => new VehicleHandler());
                public static VehicleHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private VehicleHandler() { }
                public void Set(Dictionary<string, ConfigVehicle_Detail> info) {
                    mDic = info;
                    foreach(var p in mDic) {

                        mDicState.Add(p.Key, STATE.IDLE);
                        mDicLastTime.Add(p.Key, 0);

                        if(!mDicType.ContainsKey(p.Value.type)) {
                            mDicType[p.Value.type] = new List<string>();
                        }
                        mDicType[p.Value.type].Add(p.Key);
                    }
                }
                public void Init(FnHangAround fn, string village) {
                    mFnHangAround = fn;
                    SetVillage(village);
                }
                public List<ConfigVehicle_Detail> GetAll(string village) {
                    mTempList.Clear();
                    foreach(var p in mDic) {
                        if(p.Value.village == village) {
                            mTempList.Add(p.Value);
                        }
                    }
                    return mTempList;
                }
                public ConfigVehicle_Detail? GetOne(string type, string actorId) {
                    if(!mDicType.ContainsKey(type))
                        throw new Exception("Invalid Vehicle Type. " + type);
                    
                    //reserve
                    if(actorId != string.Empty) {
                        if(mDicReseve.ContainsKey(actorId)) {
                            return mDic[mDicReseve[actorId]];
                        }
                    }
                    
                    foreach(var vehicleId in mDicType[type]) {
                        if(mDicState[vehicleId] == STATE.IDLE)
                            return mDic[vehicleId];
                    }
                    return null;
                }
                public void SetVillage(string village) {
                    mCurrentVillage = village;
                }
                public void Leave(string vehicleId, string actorId) {
                    Arrive(vehicleId);
                    mDicReseve.Remove(actorId);
                } 
                public void Arrive(string vehicleId) {
                    mDicState[vehicleId] = STATE.IDLE;
                    mDicLastTime[vehicleId] = CounterHandler.Instance.GetCount();
                }
                public void SetMoving(string vehicleId) {
                    mDicState[vehicleId] = STATE.MOVING;
                    mDicLastTime[vehicleId] = CounterHandler.Instance.GetCount();
                }
                public void SetReserve(string vehicleId, string actorId) {
                    mDicState[vehicleId] = STATE.RESERVED;
                    mDicReseve.Add(actorId, vehicleId);
                }
                public void Update() {
                    long now = CounterHandler.Instance.GetCount();
                    if(mLastUpdated == now)
                        return;
                    
                    mLastUpdated = now;

                    foreach(var p in mDic) {
                        if(p.Value.village != mCurrentVillage)
                            continue;
                        if(mDicState[p.Key] == STATE.IDLE && now - mDicLastTime[p.Key] > mDic[p.Key].waiting) {
                            if(mFnHangAround != null) {
                                ConfigVehicle_Detail info = mDic[p.Key];
                                int idx = mRand.Next(info.positions.Count);
    
                                if(mFnHangAround(p.Key, info.positions[idx].position, info.positions[idx].rotation)) {
                                    mDicState[p.Key] = STATE.MOVING;
                                    mDicLastTime[p.Key] = now;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}