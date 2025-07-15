module.exports={
  directoryHierarchy,
  BuildDirectoryHierarchyArrayRecursively,
};
const path=require('path');
const fs=require('fs');
//  get hierarchy
var directoryHierarchy=[];
function BuildDirectoryHierarchyArrayRecursively(){
  var wikiContentPath=path.join(__dirname,'..','WikiContent');
  console.log("RequestDirectoryHierarchyRecursively for "+wikiContentPath);
  TraverseItemsRecursively(wikiContentPath,0);
  function TraverseItemsRecursively(hierarchySubdirPath,hierarchySubdirDepth){
    hierarchySubdirDepth++;
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
        }
      });
      folders.forEach(folder=>{
        TraverseItemsRecursively(folder,hierarchySubdirDepth);
      });
      return;
    }else{
      console.log('path items:none');
    }
    return;
  }
}
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