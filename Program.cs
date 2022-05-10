﻿using ENGINE.GAMEPLAY.MOTIVATION;


//discharge
DischargeHandler.Instance.SetScenario(0, 100, 1, 1);

string uniqueId = "test1";
string uniqueId2 = "test2";

int type = 1;
int type2 = 0;

Actor a = ActorHandler.Instance.AddActor(type, uniqueId);
for(int i = 0; i < 5; i++) {
    var rand = new Random();            
    int max = rand.Next(90, 150);
    int min = rand.Next(1, max);
    int val = rand.Next(max);
    a.SetSatisfaction(i + 100, min, max, val);
}

a.Print();

var actor = ActorHandler.Instance.GetActor(uniqueId);
if(actor == null) {
    Console.WriteLine("Invalid Actor unique id");
} else {    
    var s = a.GetSatisfaction(actor.GetMotivation());
    if(s == null) {
        Console.WriteLine("Invalid motivationId");
    }else {
        Console.WriteLine("Value = {0}, Id = {1}", s.Value, s.Id);
    }
    
}

while(true) {
    var actors = ActorHandler.Instance.GetActors(type);
    if(actors == null) {
        Console.WriteLine("Invalid Actor type");
    } else {
        foreach(var p in actors) {
            p.Value.Print();
        }
    }

    Int64 counter = DischargeHandler.Instance.Discharge(type);
    Console.WriteLine("Discharged {0}", counter);
}