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
        Battle battle = new Battle();
        
        while(!battle.IsFinish()) {
            List<Rating> ret = battle.Update();
            Thread.Sleep(1000);
            // action.
            // 모든 action은 한 스텝에 모두.
            // 시간이 고정되는 개념 
            for(int i = 0; i < ret.Count; i++) {
                battle.Action(ret[i]);
            }
        }
        //종료 처리
    }
}