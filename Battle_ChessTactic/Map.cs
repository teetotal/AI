using System;
using System.Collections.Generic;
using System.Linq;
using ENGINE;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace BATTLE_CHESS_TACTIC {
            public class MapNode {
                public Position position;
                public bool isObstacle;
                public MapNode(Position position, bool isObstacle) {
                    this.position = position;
                    this.isObstacle = isObstacle;
                }
            }
            public class Map {
                private int width, height;
                private double maxDistance; // 가장 먼거리. weight 계산에 활용
                private List<MapNode> mMapNodes = new List<MapNode>();
                private List<MapNode> mObstacles = new List<MapNode>();
                private int positionIdIndex;
                public Map(int width, int height) {
                    this.width = width;
                    this.height = height;

                    this.maxDistance =  new Position(0, 0, 0).GetDistance(new Position(width, height, 0));

                    positionIdIndex = (int)Math.Pow(10, ((int)System.Math.Log10( Math.Max(width, height))) + 1);

                    for(int x = 0; x < width; x ++) {
                        for(int y = 0; y < height; y++) {
                            mMapNodes.Add(new MapNode(new Position(x, y, 0), false));
                        }
                    }
                }
                public int GetWidth() {
                    return width;
                }
                public int GetHeight() {
                    return height;
                }
                public double GetMaxDistance() {
                    return maxDistance;
                }
                public List<MapNode> GetObstacles() {
                    return mObstacles;
                }
                public void AddObstacle(int x, int y) {
                    for(int i = 0; i < mMapNodes.Count; i++) {
                        if(mMapNodes[i].position.x == x && mMapNodes[i].position.y == y) {
                            mMapNodes[i].isObstacle = true;
                            mObstacles.Add(mMapNodes[i]);
                            return;
                        }
                    }
                }
                public List<MapNode> GetAttackableList(Position position, float distance) {
                    var ret =   from node in mMapNodes
                                where position.GetDistance(node.position) <= distance
                                select node;
                    
                    return ret.ToList();
                }
                public List<MapNode> GetMovalbleList(Position position, MOVING_TYPE type, float distance) {
                    var ret = from node in mMapNodes
                                where position.GetDistance(node.position) <= distance && !position.IsEqual(node.position)
                                select node;

                    switch(type) {
                        case MOVING_TYPE.CROSS:
                        case MOVING_TYPE.OVER_CROSS:
                        ret = ret.Where(e=> e.position.x != position.x && e.position.y != position.y);
                        break;
                        default:
                        ret = ret.Where(e=> (e.position.x == position.x && e.position.y != position.y) || (e.position.x != position.x && e.position.y == position.y));
                        break;
                    } 
                    
                    return ret.ToList();
                }
                public int GetPositionId(Position position) {
                    return GetPositionId((int)position.x, (int)position.y);
                    //return (int)((positionIdIndex * position.x) + position.y);
                }
                public int GetPositionId(int x, int y) {
                    return (positionIdIndex * x) + y;
                }
                public Position GetPosition(int positionId) {
                    
                    return new Position((int)(positionId / positionIdIndex), positionId % positionIdIndex, 0);
                }
            }
        }
    }
}