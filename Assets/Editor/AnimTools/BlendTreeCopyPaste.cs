using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;
namespace AnimTools{
    //  [https://forum.unity.com/threads/mecanim-blend-trees-copy-and-paste.180152/#post-5989277]
    public class BlendTreeCopyPaste:MonoBehaviour{
     const string workDir="Assets/Editor/AnimTools/BlendTreeCopyPaste/";
     const string filename="btcopy_";
     static int depth=0;
     static string treePath="";
     static string log="";
     static BlendTree useTree=null;
        public class Pair<T1,T2>{
         public T1 first;
         public T2 second;
            public Pair(T1 first,T2 second){
             this.first=first;
             this.second=second;
            }
        }
        static void MakeWorkDirIfItDoesntExist(){
         string path="Assets";
         var split=workDir.Split('/');
         for(int i=1;i<split.Length;i++){
          string p=split[i];
          string newpath=path+"/"+p;
          if(!AssetDatabase.IsValidFolder(newpath))
           AssetDatabase.CreateFolder(path,p);
          path=newpath;
         }
        }
        //===========================================================
        static void ClearConsole(){
         var logEntries=System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
         var clearMethod=logEntries.GetMethod("Clear",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public);
         clearMethod.Invoke(null,null);
        }
        //===========================================================
        public static bool IsBlendTree(ChildMotion motion){
         try{var treeType=(motion.motion as BlendTree).blendType;}
         catch{return false;}
         return true;
        }
        //===========================================================
        public static BlendTree GetBlendTreeFromSelection(){
         BlendTree bt=useTree==null?Selection.activeObject as BlendTree:useTree;
         if(bt==null)bt=(Selection.activeObject as AnimatorState).motion as BlendTree;
         return bt;
        }
        //===========================================================
        public static string GetLogPath(){
         return Application.dataPath.Substring(0,Application.dataPath.Length-"Assets".Length)+workDir+"log.txt";
        }
        //===========================================================
        [MenuItem("AnimTools/Blend Tree/Copy Tree")]
        static void CopyBlendTree(){
         int notCopied=0;
         //  Get the selected blendTree
         BlendTree bt=useTree==null?GetBlendTreeFromSelection():useTree;
         if(bt==null){
          Debug.LogError("BlendTreeCopy - Error: No selected blend tree");
          return;
         }
         //  Copy directory
         MakeWorkDirIfItDoesntExist();
         log="";
         depth=-1;
         treePath="btcopy_";
         CopyTreeRecursively(bt,0);
         //  Save log
         //ClearConsole();
         System.IO.File.WriteAllText(GetLogPath(),log);
         Debug.Log("BlendTree copied!"+(notCopied>0?" ("+notCopied.ToString()+" child blend trees not copied)":""));
        }
        //===========================================================
        public static void CopyTreeRecursively(BlendTree t,int iChild){
         string oldPath=treePath; 
         treePath+=depth.ToString()+","+iChild.ToString()+"_";
         //  Save 't's motions
         BlendTree tClone=Instantiate<BlendTree>(t);
         string fPath=workDir+filename+depth.ToString()+","+iChild.ToString()+".asset";
         AssetDatabase.CreateAsset(tClone,workDir+filename+depth.ToString()+","+iChild.ToString()+".asset");
         log+=fPath+"\n";
         //  Save children (recursively)
         depth++;
         for(int i=0;i<t.children.Length;i++)
          if(IsBlendTree(t.children[i]))
           CopyTreeRecursively(t.children[i].motion as BlendTree,i);
         depth--;
         treePath=oldPath;
        }
        //===========================================================
        [MenuItem("AnimTools/Blend Tree/Paste")]
        static void PasteBlendTree(){
         try{
          BlendTree bt=useTree==null?GetBlendTreeFromSelection():useTree;
          var lines=System.IO.File.ReadAllLines(GetLogPath());
          List<BlendTree>trees=new List<BlendTree>();
          for(int i=0;i<lines.Length;i++)
           trees.Add(AssetDatabase.LoadAssetAtPath<BlendTree>(lines[i]));
          for(int i=1;i<lines.Length;i++){
           string l=lines[i].Substring((workDir+filename).Length);
           l=l.Substring(0,l.Length-".asset".Length);
           if(l.Length==0)continue;
           Debug.Log(l);
           var split=l.Split(',');
           int a=int.Parse(split[0]);
           int b=int.Parse(split[1]);
           trees[a].children[b].motion=trees[i];
          }
          PasteBlendTreeSettings(bt,trees[0]);
          //ClearConsole();
          Debug.Log("BlendTree pasted!");
         }catch{
          Debug.LogError("BlendTree - Error while pasting!");
         }
        }
        public static void PasteBlendTreeSettings(BlendTree bt,BlendTree paste){
         bt.blendType             =paste.blendType;
         bt.minThreshold          =paste.minThreshold;
         bt.maxThreshold          =paste.maxThreshold;
         bt.useAutomaticThresholds=paste.useAutomaticThresholds;
         bt.hideFlags             =paste.hideFlags;
         bt.children              =paste.children.Clone()as ChildMotion[];
         bt.blendParameter        =paste.blendParameter;
         bt.blendParameterY       =paste.blendParameterY;
        }
    }
}