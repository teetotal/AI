using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ScriptHandler {
               private Dictionary<string, List<string>> mDict = new Dictionary<string, List<string>>();
               private Dictionary<string, List<string>> mDictRefusal = new Dictionary<string, List<string>>();
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
                public bool AddRefusal(string key, List<string> list) {                   
                    if(mDictRefusal.ContainsKey(key)) return false;

                    mDictRefusal.Add(key, list);
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
                public string GetScriptRefusal(string taskId, Actor from, Actor to) {
                    if(mDictRefusal.ContainsKey(taskId)) {
                        var rnd = new Random();
                        int idx = rnd.Next(mDictRefusal[taskId].Count);
                        return GetReplacedString(mDictRefusal[taskId][idx], from, to);
                    }
                    return "-.-";
                }               
                public string GetScriptRefusal(string taskId, Actor from) {
                    if(mDictRefusal.ContainsKey(taskId)) {
                        var rnd = new Random();
                        int idx = rnd.Next(mDictRefusal[taskId].Count);
                        return GetReplacedString(mDictRefusal[taskId][idx], from, null);
                    }
                    return "-.-";
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