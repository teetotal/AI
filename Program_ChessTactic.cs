using System;
using System.Collections.Generic;
using System.Threading;
using ENGINE.GAMEPLAY.BATTLE_CHESS_TACTIC;
class ChessTacticMain {
    static void Main() {      
        ChessTacticSample p = new ChessTacticSample();
        p.Run();
    }
}

public class ChessTacticSample {
    public void Run() {
        Map m = CreateMap();
        Battle battle = new Battle(m, CreateSolidiers(true, m), CreateSolidiers(false, m), CreateTactic(true), CreateTactic(false));
        
        while(!battle.IsFinish()) {
            List<Rating> ret = battle.Update();
            Thread.Sleep(1000);
            // action.
            // 모든 action은 한 스텝에 모두.
            // 시간이 고정되는 개념 
            for(int i = 0; i < ret.Count; i++) {
                Rating rating = ret[i];
                Console.WriteLine("{0} {1} {2} {3}", rating.soldierId, rating.isHome, rating.type, rating.targetId);
            }

            //반영
            for(int i = 0; i < ret.Count; i++) {
                battle.Action(ret[i]);
            }
        }
        //종료 처리
    }
    private Map CreateMap() {
        Map m = new Map(10, 10);
        m.AddObstacle(1,1);
        m.AddObstacle(2,2);
        m.AddObstacle(3,1);

        return m;
    }
    private List<Soldier> CreateSolidiers(bool isHome, Map map) {
        List<Soldier> list = new List<Soldier>();
        if(isHome) {
            SoldierAbility ability = new SoldierAbility();
            ability.distance = 3;
            Soldier soldier = new Soldier(true, 0, MOVING_TYPE.STRAIGHT, ability, new ENGINE.Position(2, 0, 0), map);
            list.Add(soldier);
        }
        return list;
    }
    private Tactic CreateTactic(bool isHome) {
        Tactic tactic = new Tactic();
        return tactic;
    }
}