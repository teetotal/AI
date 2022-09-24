using System;
using System.Collections.Generic;
using System.Threading;
using ENGINE;
using ENGINE.GAMEPLAY.BATTLE_CHESS_TACTIC;
class ChessTacticMain {
    static void Main() {      
        ChessTacticSample p = new ChessTacticSample();
        p.Run();
    }
}

public class ChessTacticSample {
    Battle mBattle;
    public void Run() {
        Map m = CreateMap();
        mBattle = new Battle(m, CreateSolidiers(true, m), CreateSolidiers(false, m), CreateTactic(true), CreateTactic(false));
        
        while(!mBattle.IsFinish()) {
            List<Rating> ret = mBattle.Update();
            Thread.Sleep(1000);
            // action.
            // 모든 action은 한 스텝에 모두.
            // 시간이 고정되는 개념 
            Console.WriteLine("------------------------------------------------");
            Print(m, ret);

            //반영
            for(int i = 0; i < ret.Count; i++) {
                mBattle.Action(ret[i]);
            }
        }
        //종료 처리
    }
    private void Print(Map map, List<Rating> ret) {
        var obstacles = map.GetObstacles();

        for(int y =0; y < map.GetHeight(); y++) {
            string sz = y.ToString() + "\t";
            for(int x = 0; x < map.GetWidth(); x++) {
                bool isObstacle = false;
                for(int i = 0; i < obstacles.Count; i++) {
                    if(x == obstacles[i].position.x && y == obstacles[i].position.y) {
                        sz += "X\t";
                        isObstacle = true;
                        continue;
                    }
                }
                if(isObstacle)
                    continue;

                bool assigned = false;
                for(int i = 0; i < ret.Count; i++) {
                    Rating rating = ret[i];
                    //Console.WriteLine("{0} {1} {2} {3}", rating.soldierId, rating.isHome, rating.type, rating.targetId);
                    Soldier soldier = mBattle.GetSoldier(rating);
                    if(soldier.GetPosition().x == x && soldier.GetPosition().y == y) {
                        if(rating.isHome)
                            sz += "H\t";
                        else
                            sz += "A\t";
                        assigned = true;
                        break;
                    }
                }
                
                if(!assigned) 
                    sz += "-\t";
            }
            Console.WriteLine(sz);
        }
    }
    private Map CreateMap() {
        Map m = new Map(5, 15);
        m.AddObstacle(1,2);
        m.AddObstacle(0,6);
        m.AddObstacle(1,1);
        m.AddObstacle(2,2);
        m.AddObstacle(4,10);
        

        return m;
    }
    private List<Soldier> CreateSolidiers(bool isHome, Map map) {
        List<Soldier> list = new List<Soldier>();
        if(isHome) {
            SoldierAbility ability = new SoldierAbility();
            ability.distance = 2;
            ability.attackRange = 3;
            Soldier soldier = new Soldier(isHome, 0, MOVING_TYPE.CROSS, ability, new ENGINE.Position(2, 0, 0), map);
            list.Add(soldier);
        } else {
            SoldierAbility ability = new SoldierAbility();
            ability.distance = 2;
            ability.attackRange = 3;
            Soldier soldier = new Soldier(isHome, 0, MOVING_TYPE.STRAIGHT, ability, new ENGINE.Position(2, 14, 0), map);
            list.Add(soldier);
        }
        return list;
    }
    private Tactic CreateTactic(bool isHome) {
        Tactic tactic = new Tactic();
        return tactic;
    }
}