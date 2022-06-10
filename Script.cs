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
                   "하이 널 보면 기분이가 좋아~",
                   "하잉 {to}",
                   "헤헷! 안녕! {to}"
                };
                private string[] helloAck = new string[] {
                   "응 너두",
                   "{to} 너두 좋은 하루 보내",
                   "인사 고마워 {to}",
                   "앗 {to} 상냥하기도 하지",
                   "나두 하잉",
                   "안녕!"
                };
               
               private Dictionary<string, List<string>> mDict = new Dictionary<string, List<string>>();
               private Dictionary<string, List<string>> mDictAck = new Dictionary<string, List<string>>();
               private static readonly Lazy<ScriptHandler> instance =
                        new Lazy<ScriptHandler>(() => new ScriptHandler());
                public static ScriptHandler Instance {
                    get {
                        return instance.Value;
                    }
                }
                private ScriptHandler() { 
                    Add("hello", hello);
                    AddAck("hello_ack", helloAck);
                }
                private void Add(string key, string[] arr) {
                    List<string> list = new List<string>();
                    foreach(var sz in arr) {
                        list.Add(sz);
                    }
                    mDict.Add(key, list);
                }
                private void AddAck(string key, string[] arr) {
                    List<string> list = new List<string>();
                    foreach(var sz in arr) {
                        list.Add(sz);
                    }
                    mDictAck.Add(key, list);
                }
                public string GetScript(string taskId, string from, string to) {
                    if(mDict.ContainsKey(taskId)) {
                        var rnd = new Random();
                        int idx = rnd.Next(mDict[taskId].Count);
                        return GetReplacedString(mDict[taskId][idx], from, to);
                    }
                    return "...";
                }
                public string GetScriptAck(string taskId, string from, string to) {
                    if(mDictAck.ContainsKey(taskId)) {
                        var rnd = new Random();
                        int idx = rnd.Next(mDictAck[taskId].Count);
                        return GetReplacedString(mDictAck[taskId][idx], from, to);
                    }
                    return "??";
                }
                private string GetReplacedString(string sz, string from, string to) {
                    return sz.Replace("{from}", from).Replace("{to}", to);
                }
            }
        }
    }
}