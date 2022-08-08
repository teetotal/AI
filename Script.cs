using System;
using System.Collections.Generic;
using System.Text;

#nullable enable
namespace ENGINE {
    namespace GAMEPLAY {
        namespace MOTIVATION {
            public class ScriptHandler { 
               private Dictionary<string, List<string>> mDict = new Dictionary<string, List<string>>();
               private Dictionary<string, List<string>> mDictRefusal = new Dictionary<string, List<string>>();
               const string pre = "<";
               const string post = ">";
               const string INVALID_SCRIPT = "...";
               const string INVALID_SCRIPT_REFUSAL = "-.-";
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
                public string GetScript(string taskId, Actor from, Actor? to = null) {
                    if(mDict.ContainsKey(taskId)) {
                        if(mDict[taskId].Count == 0) {
                            return INVALID_SCRIPT;
                        }
                        var rnd = new Random();
                        int idx = rnd.Next(mDict[taskId].Count);
                        return GetReplacedString(mDict[taskId][idx], from, to);
                    }
                    return INVALID_SCRIPT;
                }               
                public string GetScriptRefusal(string taskId, Actor from, Actor? to = null) {
                    if(mDictRefusal.ContainsKey(taskId)) {
                        if(mDictRefusal[taskId].Count == 0) {
                            return INVALID_SCRIPT;
                        }
                        var rnd = new Random();
                        int idx = rnd.Next(mDictRefusal[taskId].Count);
                        return GetReplacedString(mDictRefusal[taskId][idx], from, to);
                    }
                    return INVALID_SCRIPT_REFUSAL;
                }                            
                //pooling처리 해야함
                private string GetReplacedString(string sz, Actor from, Actor? to) {
                    StringBuilder sbFrom = new StringBuilder(from.mInfo.nickname, from.mInfo.nickname.Length + pre.Length + post.Length);                    
                    sbFrom.Insert(0, pre);
                    sbFrom.Append(post);

                    StringBuilder sb = new StringBuilder(sz, sz.Length + 64);
                    sb.Replace("\\n", "\n");
                    sb.Replace("{from}", sbFrom.ToString());

                    
                    if(to != null) {
                        StringBuilder sbTo = new StringBuilder(to.mInfo.nickname, to.mInfo.nickname.Length + pre.Length + post.Length);
                        sbTo.Insert(0, pre);
                        sbTo.Append(post);

                        sb.Replace("{to}", sbTo.ToString());
                    }
                    return sb.ToString();
                }
            }
        }
    }
}