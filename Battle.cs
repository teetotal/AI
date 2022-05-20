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
                private int mWidth, mHeight;
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
                //candidates of possible position
                public List<string> Sight(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    string currPos = GetActorPosition(actorId);
                    List<string> list = GetNearPostions(currPos, actor.mAbility.Sight);
                    //check occupied
                    List<string> ret = new List<string>();
                    foreach(string pos in list) {
                        var tile = GetBattleMapTile(pos);
                        if(tile != null && tile.state == BATTLEMAPTILE_STATE.EMPTY) {
                            ret.Add(pos);
                        }
                    }
                    ret.Add(currPos); //현재 위치 추가.
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

                    mBattleMap[position] = GetChangedTile(mBattleMap[position], mBattleMap[position].actorId, BATTLEMAPTILE_STATE.OCCUPIED);

                    return true;
                }
                
                public List<string> GetReadyActors() {
                    List<string> ret = new List<string>();
                    foreach(var p in mActorPosition) {
                        string actorId = p.Key;
                        string position = p.Value;
                        var tile = GetBattleMapTile(position);
                        if(tile != null) {
                            if(tile.state == BATTLEMAPTILE_STATE.OCCUPIED) {
                                ret.Add(actorId);
                            }
                        }
                    }
                    return ret;
                }
                public void Print() {
                    Console.WriteLine("-----------------------");
                    for(int y = 0; y < mHeight; y++) {
                        string sz = "";
                        for(int x = 0; x < mWidth; x ++) {
                            string pos = GetPositionString(x,y);
                            var tile = GetBattleMapTile(pos);
                            if(tile != null) {
                                sz += string.Format("[{0} - {1}]\t", pos, tile.actorId);
                            }                            
                        }
                        Console.WriteLine(sz);
                    }
                    Console.WriteLine("-----------------------");
                }
            }
            public class Battle {
                public BattleMap mMap;
                private BattleActorHandler mBattleActor = new BattleActorHandler();
                public Battle(int mapWidth, int mapHeight) {
                     mMap = new BattleMap(mapWidth, mapHeight);
                }
                //직관적인 이해를 위해 x,y의 위치를 바꿔서 저장한다.
                public bool Init(int[,] advantage1, int[,] advantage2) {
                    for(int y = 0; y < advantage1.GetLength(0); y++) {
                        for(int x = 0; x < advantage1.GetLength(1); x++) {
                            if(!mMap.AppendInitMapTile(x, y, advantage1[y,x], advantage2[y,x])) {
                                return false;
                            } 
                        }            
                    }   

                    return true;
                }
                public bool AppendActor(int x, int y, Actor actor, BATTLE_SIDE side, BattleActorAbility ability) {
                    mBattleActor.CreateBattleActor(side, actor, ability);
                    return mMap.AppendActor(x, y, actor.mUniqueId);
                }
                // actorid, to
                public Dictionary<string, string> Next() {
                    Dictionary<string, string> ret = new Dictionary<string, string>();                    
                    // 점령한 tile에 있는 Actor만 찾는다.                    
                    List<string> list = mMap.GetReadyActors();
                    foreach(string actorId in list) {
                        var actor = mBattleActor.GetBattleActor(actorId);
                        if(actor == null) {
                            continue;
                        }

                        string to = Act(actor);
                        if(to.Length > 0) {
                            ret.Add(actorId, to);
                        }
                        
                    }                    
                    return ret;
                }
                public string Act(BattleActor actor) {
                    string actorId = actor.mActor.mUniqueId;
                    List<string> list = mMap.Sight(actor);
                    if(list.Count() == 0) {
                        return "";
                    }
                    string maxPos = list[0];
                    string from = mMap.GetActorPosition(actorId);
                    float max = GetEstimation(actor, from, list[0]);

                    //같은 패턴 반복을 막기 위해 
                    var rnd = new Random();
                    var randomized = list.OrderBy(item => rnd.Next());
                    foreach(var to in randomized) {
                        float v = GetEstimation(actor, from, to); 
                        if(v > max) {
                            maxPos = to;
                            max = v;
                        }
                    }

                    mMap.MoveTo(actorId, maxPos);
                    return maxPos;
                } 
                public void Occupy(string actorId) {                    
                    string position = mMap.GetActorPosition(actorId);
                    if(IsValidPosition(position)) {
                        mMap.Occupy(actorId, position);                        
                    }                    
                }
                public bool Validate() {                    
                    return mMap.Validate();
                }
                private bool IsValidPosition(string position) {                    
                    if(position.Length == 0) return false;
                    return true;
                }    

                // ----------------------------------------------------------------------
                //이동시 이득 계산. 
                //adv - disadv + cost
                private float GetEstimation(BattleActor actor, string from, string to) {
                    var fromTile = mMap.GetBattleMapTile(from);
                    var toTile = mMap.GetBattleMapTile(to);

                    float ret = 0;
                    if(fromTile != null && toTile != null) {
                        switch(actor.mSide) {
                            case BATTLE_SIDE.HOME:
                            ret = toTile.advantage1 - fromTile.advantage1;
                            break;
                            case BATTLE_SIDE.AWAY:
                            ret = toTile.advantage2 - fromTile.advantage2;
                            break;
                        }
                    }

                    ret = ret + GetCost(actor, from, to, actor.mSide);
                    return ret;
                }
                private float GetCost(BattleActor actor, string from, string to, BATTLE_SIDE mySide) {
                    //주변의 공격
                    //공격 가능 등

                    //일단은 인접 공격만 계산
                    float cost = 0;
                    //돌격성 or 안정성. 상대 진영으로 달려든다.
                    cost += GetMoveCost(actor, from, to);
                    /*
                    List<string> list = mMap.GetNearPostions(to, 1);
                    foreach(string pos in list) {
                        BattleMapTile? tile = mMap.GetBattleMapTile(pos);
                        if(tile != null && tile.actorId.Length > 0) {
                            var targetActor = mBattleActor.GetBattleActor(tile.actorId);
                            if(targetActor != null) {

                                //공격성 or 수비성. 상대방이 보이면 달려든다.
                                if(actor.mSide != mySide) {
                                    //cost += 1.0f;
                                }
                            }
                        }
                    }
                    */
                    return cost;
                }     
                private float GetMoveCost(BattleActor actor, string from, string to) {
                    int[] fromInt = mMap.GetPositionInt(from);
                    int[] toInt = mMap.GetPositionInt(to);

                    float cost = 0;
                    
                    switch(actor.mSide) {
                        case BATTLE_SIDE.HOME:
                        {
                            if(actor.mAbility.MoveForward != 0) {
                                if(toInt[0] - fromInt[0] > 0) { //전방
                                    cost += actor.mAbility.MoveForward;
                                }
                            }
                            
                            if(actor.mAbility.MoveBack != 0) {
                                if(toInt[0] - fromInt[0] < 0) { //후방
                                    cost += actor.mAbility.MoveBack;
                                }
                            }
                        }
                        break;
                        case BATTLE_SIDE.AWAY:
                        {
                            if(actor.mAbility.MoveForward != 0) {
                                if(toInt[0] - fromInt[0] < 0) { //전방
                                    cost += actor.mAbility.MoveForward;
                                }
                            }
                            if(actor.mAbility.MoveBack != 0) {
                                if(toInt[0] - fromInt[0] > 0) { //후방
                                    cost += actor.mAbility.MoveBack;
                                }
                            }
                        }
                        break;
                    }
                    //라인 유지
                    if(actor.mAbility.MoveSide != 0) {
                        if(fromInt[0] == toInt[0] && fromInt[1] != toInt[1]) { 
                            cost += actor.mAbility.MoveSide;
                        }
                    }
                    //Console.WriteLine("{0} [{1}] > [{2}] : {3}", actor.mActor.mUniqueId,  from, to, cost);

                    return cost;
                }       
            }
        }        
    }
}