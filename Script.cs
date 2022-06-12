using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ScriptHandler {
                private string[] hello = new string[] {
                   "안녕 {to}",
                   "{to} 좋은 하루 보내",
                   "반가워 {to}",
                   "하이 {to} 널 보면 기분이가 좋아~",
                   "하잉 {to}",
                   "헤헷! 안녕! {to}"
                };
                private string[] helloAck = new string[] {
                   "응 {to} 너두",
                   "{to} 너두 좋은 하루 보내",
                   "인사 고마워 {to}",
                   "앗 {to} 상냥하기도 하지",
                   "하잉 {to}",
                   "안녕! {to}"
                };
               
               private Dictionary<string, List<string>> mDict = new Dictionary<string, List<string>>();
               private static readonly Lazy<ScriptHandler> instance =
                        new Lazy<ScriptHandler>(() => new ScriptHandler());
                public static ScriptHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private ScriptHandler() { }
                public bool Add(string key, List<string> list) {                   
                    if(mDict.ContainsKey(key)) return false;

                    mDict.Add(key, list);
                    return true;
                }                
                public string GetScript(string taskId, Actor from, Actor to) {
                    if(mDict.ContainsKey(taskId)) {
                        var rnd = new Random();
                        int idx = rnd.Next(mDict[taskId].Count);
                        return GetReplacedString(mDict[taskId][idx], from, to);
                    }
                    return "...";
                }               
                public string GetScript(string taskId, Actor from) {
                    if(mDict.ContainsKey(taskId)) {
                        var rnd = new Random();
                        int idx = rnd.Next(mDict[taskId].Count);
                        return GetReplacedString(mDict[taskId][idx], from, null);
                    }
                    return "...";
                }               
                private string GetReplacedString(string sz, Actor from, Actor? to) {
                    string ret = sz.Replace("{from}", from.mUniqueId);
                    if(to != null) {
                        ret = ret.Replace("{to}", to.mUniqueId);
                    }
                    return ret;
                }
            }
        }
    }
}