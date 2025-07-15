const path=require('path');
const fs=require('fs');
//  get hierarchy
const public={
  wikiContentDirectoryHierarchy:{},
};
function BuildDirectoryHierarchyArrayRecursively(){
  var wikiContentPath=path.join(__dirname,'..','WikiContent');
  console.log("RequestDirectoryHierarchyRecursively for "+wikiContentPath);
  for(let key in public.wikiContentDirectoryHierarchy){
    if(Object.hasOwn(public.wikiContentDirectoryHierarchy,key)){
      delete public.wikiContentDirectoryHierarchy[key];
    }
  }  
  TraverseItemsRecursively(wikiContentPath,0,public.wikiContentDirectoryHierarchy);
  function TraverseItemsRecursively(hierarchySubdirPath,hierarchySubdirDepth,hierarchy){
    console.log('hierarchySubdirDepth:',hierarchySubdirDepth);
    const items=fs.readdirSync(hierarchySubdirPath);
    if(items.length>0){
      var folders=[];
      console.log('hierarchySubdirPath:',hierarchySubdirPath);
      items.forEach(item=>{
        const itemPath=path.join(hierarchySubdirPath,item);
        const itemStat=fs.statSync(itemPath);
        if(itemStat.isDirectory()){
          console.log(`pasta:${item}`);
          folders.push(itemPath);
        }else if(itemStat.isFile()){
          console.log(`arquivo:${item}`);
          hierarchy[itemPath]={};
        }
      });
      hierarchySubdirDepth++;
      folders.forEach(folder=>{
        hierarchy[folder]={};
        TraverseItemsRecursively(folder,hierarchySubdirDepth,hierarchy[folder]);
      });
      return;
    }else{
      console.log('path items:none');
    }
    return;
  }
}
function DoForEachRecursivelyAndCreateIndex(hierarchyObj,hierarchyDepth){
  for(let key in hierarchyObj){
    if(typeof key==='string'){
      const itemPath=key;
      try{
        if(fs.existsSync(itemPath)){
          const itemStat=fs.statSync(itemPath);
          if(itemStat.isFile()){
            console.log('hierarchyDepth:'+hierarchyDepth+':file:'+itemPath);
            continue;
          }
        }
      }catch(error){
        console.error(`erro com ${itemPath}:`,error.message);
      }
    }    
  }
  hierarchyDepth++;
  for(let key in hierarchyObj){
    //console.log("key:"+key);
    if(typeof key==='string'){
      const itemPath=key;
      try{
        if(fs.existsSync(itemPath)){
          const itemStat=fs.statSync(itemPath);
          if(itemStat.isDirectory()){
            console.log('hierarchyDepth:'+hierarchyDepth+':folder:'+itemPath);
            DoForEachRecursivelyAndCreateIndex(hierarchyObj[key],hierarchyDepth);
            continue;
          }
        }
      }catch(error){
        console.error(`erro com ${itemPath}:`,error.message);
      }
    }    
  }
  //       var contentTxtFile=null;



//         if(key=="files"){
//           console.log("ParseMenuText for files");
//           for(var key2 in hierarchyObj[key]){
//             var fileInHierarchy=hierarchyObj[key][key2];
//             console.log("ParseMenuText:fileInHierarchy:"+fileInHierarchy);
//             if(contentTxtFile==null&&fileInHierarchy.endsWith("Content.txt")){
//               contentTxtFile=fileInHierarchy;
//               continue;
//             }
//           }
//           continue;
//         }
//       }
//       if(hierarchyPath!=null){
//         hierarchyParentElement=CreateMenuDOMelement(menuObj,hierarchyPath,0,hierarchyParentElement,contentTxtFile);
//       }
//       for(var key in hierarchyObj){
//         if(key=="files"){
//           continue;
//         }
//         if(typeof key=="string"&&key.endsWith("/")){
//           console.log("ParseMenuText:doing parse:next key:"+key);
//           DoForEachRecursively(hierarchyObj[key],key,hierarchyParentElement);
//           continue;
//         }
}
//     console.log("ParseMenuText:do parsing now!");
//     DoForEachRecursively(hierarchy,null,null);
module.exports={
  public,
  BuildDirectoryHierarchyArrayRecursively,
  DoForEachRecursivelyAndCreateIndex,
};
console.log("hierarchy.js is alive");
//   hierarchy[path]={};
//   hierarchy[path]["files"]={};
//   //  https://stackoverflow.com/questions/54857700/list-files-on-a-server-via-front-end-javascript
//   const dirRequest=new XMLHttpRequest();
//   dirRequest.open("GET",path,true);
//   dirRequest.responseType='text';
//   dirRequest.onload=()=>{
//     console.log("dirRequest.status:"+dirRequest.status);
//     var newRequestsCount=0;
//     var dirRequestRespText=null;
//     if(dirRequest.status>=200&&dirRequest.status<400){
//       console.log("depth:"+depth);
//       //  Succesful response
//       dirRequestRespText=dirRequest.responseText;
//       //console.log("dirRequestRespText:"+dirRequestRespText);
//       var hierarchyPathSubdirIndex=0;
//        var hierarchyPathFileIndex=0;
//       var index=-1;
//       while((index=dirRequestRespText.indexOf("href=\"",index+1))>0){
//         var subdirStart=index+6;
//         var subdirEnd=dirRequestRespText.indexOf("\"",subdirStart);
//         var subdir=dirRequestRespText.substring(subdirStart,subdirEnd);
//         console.log("index:"+index+";"+subdir);
//         if(subdir=="../"){
//           console.log("ignore ../");
//           continue;
//         }
//         if(!subdir.endsWith("/")){
//           console.log("ignore file for dir request but add it to files array");
//           hierarchy[path]["files"][hierarchyPathFileIndex]=subdir;
//           hierarchyPathFileIndex++;
//           continue;
//         }
//         hierarchy[path][hierarchyPathSubdirIndex]=subdir;
//         hierarchyPathSubdirIndex++;
//       }
//       const pathsToRequest=[];
//       var pathsToRequestIndex=0;
//       for(var key in hierarchy[path]){
//         if(key=="files"){
//           console.log("ignore RequestDirectoryHierarchyRecursively for files");
//           continue;
//         }
//         console.log("hierarchy["+path+"]["+key+"]:"+hierarchy[path][key]);
//         pathsToRequest[pathsToRequestIndex]=hierarchy[path][key];
//         pathsToRequestIndex++;
//       }
//       pathsToRequest.forEach((value,index,array)=>{
//         RequestDirectoryHierarchyRecursively(value,hierarchy[path],depth+1,onloadFunc);
//         newRequestsCount++;
//       });
//     }
//     if(onloadFunc!==null){
//       onloadFunc(newRequestsCount);
//     }
//   };
//   dirRequest.send(null);