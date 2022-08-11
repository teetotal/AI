using System;
using System.Collections.Generic;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class VehicleHandler {
                public delegate void FnHangAround(string vehicleId, string position, string rotation);  
                private FnHangAround? mFnHangAround = null;
                private Dictionary<string, ConfigVehicle_Detail> mDic = new Dictionary<string, ConfigVehicle_Detail>();
                private Dictionary<string, bool> mDicMoving = new Dictionary<string, bool>(); //이동 여부
                private Dictionary<string, long> mDicLastTime = new Dictionary<string, long>();//마지막 counter
                private Dictionary<string, List<string>> mDicType = new Dictionary<string, List<string>>();
                public Random mRand= new Random();
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

                        mDicMoving.Add(p.Key, false);
                        mDicLastTime.Add(p.Key, 0);

                        if(!mDicType.ContainsKey(p.Value.type)) {
                            mDicType[p.Value.type] = new List<string>();
                        }
                        mDicType[p.Value.type].Add(p.Key);
                    }
                }
                public Dictionary<string, ConfigVehicle_Detail> GetAll() {
                    return mDic;
                }
                public ConfigVehicle_Detail? GetOne(string type) {
                    if(!mDicType.ContainsKey(type))
                        throw new Exception("Invalid Vehicle Type. " + type);
                    
                    foreach(var vehicleId in mDicType[type]) {
                        if(!mDicMoving[vehicleId])
                            return mDic[vehicleId];
                    }
                    return null;
                }
                public void SetFnHangAround(FnHangAround fn) {
                    mFnHangAround = fn;
                }
                public void GetIn(string vehicleId) {
                    mDicMoving[vehicleId] = true;
                    mDicLastTime[vehicleId] = CounterHandler.Instance.GetCount();
                }
                public void GetOff(string vehicleId) {
                    mDicMoving[vehicleId] = false;
                    mDicLastTime[vehicleId] = CounterHandler.Instance.GetCount();
                }   
                public void Update() {
                    long now = CounterHandler.Instance.GetCount();
                    foreach(var p in mDic) {
                        if(!mDicMoving[p.Key] && now - mDicLastTime[p.Key] > mDic[p.Key].waiting) {
                            if(mFnHangAround != null) {
                                ConfigVehicle_Detail info = mDic[p.Key];
                                int idx = mRand.Next(info.positions.Count);
                                mDicMoving[p.Key] = true;
                                mDicLastTime[p.Key] = now;
                                mFnHangAround(p.Key, info.positions[idx].position, info.positions[idx].rotation);
                            }
                        }
                    }
                }
            }
        }
    }
}