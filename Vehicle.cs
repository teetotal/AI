using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class VehicleHandler {
                public delegate bool FnHangAround(string vehicleId, string position, string rotation);  
                private FnHangAround? mFnHangAround = null;
                private string mCurrentVillage = string.Empty;
                private Dictionary<string, ConfigVehicle_Detail> mDicAll = new Dictionary<string, ConfigVehicle_Detail>();
                private Dictionary<string, string> mDicReserve = new Dictionary<string, string>(); //예약차량
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
                    mDicAll = info;
                    foreach(var p in mDicAll) {
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
                    return mTempList;
                }
                public ConfigVehicle_Detail? GetOne(string type, string actorId) {
                    if(!mDicType.ContainsKey(type))
                        throw new Exception("Invalid Vehicle Type. " + type);
                    
                    //reserve
                    if(actorId != string.Empty) {
                        if(mDicReserve.ContainsKey(actorId)) {
                            return mDicAll[mDicReserve[actorId]];
                        }
                    }

                    for(int i = 0; i < mTempList.Count; i++) {
                        if(mTempList[i].state == VEHICLE_STATE.IDLE)
                            return mTempList[i];
                    }
                    
                    return null;
                }
                public void SetVillage(string village) {
                    mCurrentVillage = village;
                    mTempList.Clear();
                    foreach(var p in mDicAll) {
                        if(p.Value.village == village) {
                            //Scene 교체됐을때 일단 리셋 시켜버림. 나중에 고민
                            p.Value.state = VEHICLE_STATE.IDLE;
                            p.Value.last = 0;
                            mTempList.Add(p.Value);
                        }
                    }  
                    mDicReserve.Clear();              
                }
                public void Leave(string vehicleId, string actorId) {
                    Arrive(vehicleId);
                    mDicReserve.Remove(actorId);
                } 
                public void Arrive(string vehicleId) {
                    mDicAll[vehicleId].state = VEHICLE_STATE.IDLE;
                    mDicAll[vehicleId].last = CounterHandler.Instance.GetCount();
                }
                public void SetMoving(string vehicleId) {
                    mDicAll[vehicleId].state = VEHICLE_STATE.MOVING;
                    mDicAll[vehicleId].last = CounterHandler.Instance.GetCount();
                }
                public void SetReserve(string vehicleId, string actorId) {
                    mDicAll[vehicleId].state = VEHICLE_STATE.RESERVED;
                    mDicReserve.Add(actorId, vehicleId);
                }
                public void CancelReserve(string actorId) {
                    string vehicleId = mDicReserve[actorId];
                    mDicAll[vehicleId].state = VEHICLE_STATE.IDLE;
                    mDicReserve.Remove(actorId);
                }
                public void Update() {
                    long now = CounterHandler.Instance.GetCount();
                    if(mLastUpdated == now)
                        return;
                    
                    mLastUpdated = now;

                    for(int i = 0; i < mTempList.Count; i++) {
                        ConfigVehicle_Detail veh = mTempList[i];
                        if(veh.state == VEHICLE_STATE.IDLE && now - veh.last > veh.waiting) {
                            if(mFnHangAround != null) {
                                int idx = mRand.Next(veh.positions.Count);
                                mFnHangAround(veh.vehicleId, veh.positions[idx].position, veh.positions[idx].rotation);
                            }
                        }
                    }
                }
            }
        }
    }
}