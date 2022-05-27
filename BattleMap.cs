using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public enum BATTLEMAPTILE_STATE {
                EMPTY,                    
                OCCUPIED,
                APPROCHING
            }
            public class BattleMapTile {                
                public int advantage1 { get; set; }
                public int advantage2 { get; set; }
                public string actorId { get; set; }
                public BATTLEMAPTILE_STATE state { get; set; }
                public BattleMapTile(string actorId, int advantage1, int advantage2, BATTLEMAPTILE_STATE state) {
                    this.actorId =actorId;
                    this.advantage1 = advantage1;
                    this.advantage2 = advantage2;
                    this.state = state;
                }                
                public BattleMapTile(int advantage1, int advantage2, BATTLEMAPTILE_STATE state) {
                    this.actorId = BattleMap.ACTORID_EMPTY;
                    this.advantage1 = advantage1;
                    this.advantage2 = advantage2;
                    this.state = state;
                }                
            }
            // BattleMap ----------------------------------------------------------------------------------------------------------
            public class BattleMap {
                public const string ACTORID_EMPTY = "";
                private int[,] mAdvantage1, mAdvantage2;
                public int mWidth, mHeight;
                //위치별 속성 + actor정보
                private Dictionary<string, BattleMapTile> mBattleMap = new Dictionary<string, BattleMapTile>();
                //Actor별 어느 위치에 있는지 표시
                // actorid, position
                private Dictionary<string, string> mActorPosition = new Dictionary<string, string>();                
                //현재 위치에서 이동 가능한 다음 목표지점 후보 리스트
                public BattleMap(int width, int height) {
                    this.mWidth = width;
                    this.mHeight = height;

                    mAdvantage1 = new int[width, height];
                    mAdvantage2 = new int[width, height];
                }
                public void RemoveActor(string actorId) {
                    if(mActorPosition.ContainsKey(actorId)) {
                        string pos = mActorPosition[actorId];
                        if(mBattleMap.ContainsKey(pos)) {
                            mBattleMap[pos] =  new BattleMapTile(ACTORID_EMPTY, mBattleMap[pos].advantage1, mBattleMap[pos].advantage2, BATTLEMAPTILE_STATE.EMPTY);
                        }
                        mActorPosition.Remove(actorId);
                    }
                }
                public string GetPositionString(int x, int y) {
                    return string.Format("{0},{1}", x, y);
                }
                public int[] GetPositionInt(string position) {
                    string[] arr = position.Split(',');
                    return new int[] {int.Parse(arr[0]), int.Parse(arr[1])};
                }
                public bool AppendInitMapTile(int x, int y, int advantage1, int advantage2) {
                    string position = GetPositionString(x, y);
                    if(mBattleMap.ContainsKey(position)) return false;
                    mAdvantage1[x,y] = advantage1;
                    mAdvantage2[x,y] = advantage2;
                    
                    BattleMapTile tile = new BattleMapTile(advantage1, advantage2, BATTLEMAPTILE_STATE.EMPTY);
                    mBattleMap.Add(position, tile);
                    return true;
                }
                public bool AppendActor(int x, int y, string actorId) {
                    string position = GetPositionString(x, y);
                    if(mActorPosition.ContainsKey(actorId) || mBattleMap.ContainsKey(position) == false) {
                        return false;
                    } 
                    mActorPosition.Add(actorId, position);                    
                    mBattleMap[position] = GetChangedTile(mBattleMap[position], actorId, BATTLEMAPTILE_STATE.OCCUPIED);

                    return true;
                }
                //설정상 오류가 없는지 확인하는 함수
                public bool Validate() {
                    //actorID 기준으로 점검
                    foreach(var p in mActorPosition) {
                        string position = p.Value;
                        if(mBattleMap.ContainsKey(position) == false || mBattleMap[position].actorId != p.Key) {
                            return false;
                        }                        
                    }
                    //position 기준으로 점검
                    foreach(var p in mBattleMap) {
                        string position = p.Key;
                        string actorId = p.Value.actorId;
                        if(actorId != ACTORID_EMPTY) {
                            if(mActorPosition.ContainsKey(actorId) && mActorPosition[actorId] != position) {
                                return false;
                            }                        
                        }                        
                    }

                    return true;
                }
                public int[] GetActorPositionInt(string actorId) {
                    string pos = GetActorPosition(actorId);
                    if(pos.Length == 0) 
                        return new int[] {-1, -1};
                    return GetPositionInt(pos);
                }
                public string GetActorPosition(string actorId) {
                    if(mActorPosition.ContainsKey(actorId) == false) 
                        return "";

                    return mActorPosition[actorId];
                }
                public Dictionary<string, string> GetActorPositions() {
                    return mActorPosition;
                }
                public BattleMapTile? GetBattleMapTile(string position) {
                    if(mBattleMap.ContainsKey(position) == false) {
                        return null;
                    }

                    return mBattleMap[position];
                }
                public bool Exist(string pos) {
                    if(mBattleMap.ContainsKey(pos)) {
                        return true;
                    }
                    return false;
                }
                /*
                public string Act(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    List<string> list = Sight(actor);
                    if(list.Count() == 0) {
                        return "";
                    }
                    int idx = 0;
                    string from = mActorPosition[actorId];
                    float max = GetEstimation(actor, from, list[idx]);

                    for(int i = 0; i < list.Count(); i++) {
                        string position = list[i];
                        float v = GetEstimation(actor, from, position);

                        if(v > max) {
                            idx = i;
                            max = v;
                        }
                    }
                    MoveTo(actorId, list[idx]);
                    return list[idx];
                }
                */
                
                //주변 공격 대상 찾기
                public List<string> LookOut(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    string currPos = GetActorPosition(actorId);
                    List<string> list = GetNearPostions(currPos, actor.mAbility.AttackDistance);
                    //check occupied
                    List<string> ret = new List<string>();
                    foreach(string pos in list) {
                        var tile = GetBattleMapTile(pos);
                        if(tile != null && tile.state == BATTLEMAPTILE_STATE.OCCUPIED) {                            
                            ret.Add(pos);
                        }
                    }
                    return ret;
                }
                //인접한 공간 찾기
                public List<string> GetNearPositionsByState(string position, BATTLEMAPTILE_STATE state, int sight = 1) {
                    List<string> list = GetNearPostions(position, sight);
                    //check occupied
                    List<string> ret = new List<string>();
                    foreach(string pos in list) {
                        var tile = GetBattleMapTile(pos);
                        if(tile != null && tile.state == state) {                            
                            ret.Add(pos);
                        }
                    }
                    return ret;
                }

                public List<string> GetNearPostions(string position, int sight) {
                    int[] pos = GetPositionInt(position);
                    List<string> ret = new List<string>(); 
                    if(pos[0] == -1 && pos[1] == -1) {
                        return ret;
                    }
                    
                    int width = mWidth -1;
                    int height = mHeight -1;
                    
                    for(int n = 1; n <= sight; n++) {
                        //상하좌우 + 대각선4
                        for(int i = 0; i < 8; i++) {
                            int x = pos[0];
                            int y = pos[1];

                            switch(i) {
                                case 0: //+x  
                                x = Math.Min(width, x+n);
                                break;
                                case 1: //-x
                                x = Math.Max(0, x-n);
                                break;
                                case 2: //+y
                                y = Math.Min(height, y+n);
                                break;
                                case 3: //-y
                                y = Math.Max(0, y-n);
                                break;
                                case 4: //-x +y
                                x = Math.Max(0, x-n);
                                y = Math.Min(height, y+n);
                                break;
                                case 5: //+x +y
                                x = Math.Min(width, x+n);
                                y = Math.Min(height, y+n);
                                break;
                                case 6: //-x -y
                                x = Math.Max(0, x-n);
                                y = Math.Max(0, y-n);
                                break;
                                case 7: //+x -y
                                x = Math.Min(width, x+n);
                                y = Math.Max(0, y-n);
                                break;
                            }
                            if(pos[0] == x && pos[1] == y) continue;
                            ret.Add(GetPositionString(x, y));
                        }
                    }
                    return ret;
                }
                public BattleMapTile GetChangedTile(BattleMapTile origin, string actorId, BATTLEMAPTILE_STATE state) {
                    return new BattleMapTile(actorId, origin.advantage1, origin.advantage2, state);
                }
                //actor를 어디론가 이동. 그 자리에 누군가 있으면 실패
                public bool MoveTo(string actorId, string to) {
                    if(mBattleMap.ContainsKey(to) == false || mBattleMap[to].actorId.Length > 0 )
                        return false;

                    //원래 있던 자리를 비우고
                    string fromPosition = mActorPosition[actorId];
                    BattleMapTile fromTile =  mBattleMap[fromPosition];
                    mBattleMap[fromPosition] = GetChangedTile(mBattleMap[fromPosition], ACTORID_EMPTY, BATTLEMAPTILE_STATE.EMPTY);

                    //새로 가는 자리를 채운다.
                    mBattleMap[to] = GetChangedTile(mBattleMap[to], actorId, BATTLEMAPTILE_STATE.APPROCHING);
                    mActorPosition[actorId] = to;
                    return true;
                }
                //영역 차지
                public bool Occupy(string actorId, string position) {
                    if( mBattleMap.ContainsKey(position) == false || 
                        mActorPosition.ContainsKey(actorId) == false || 
                        mBattleMap[position].actorId != actorId || 
                        position != mActorPosition[actorId]) {
                        return false;
                    }
                    if(mBattleMap[position].state != BATTLEMAPTILE_STATE.OCCUPIED) 
                        mBattleMap[position] = GetChangedTile(mBattleMap[position], mBattleMap[position].actorId, BATTLEMAPTILE_STATE.OCCUPIED);

                    return true;
                }
            }
        }        
    }
}