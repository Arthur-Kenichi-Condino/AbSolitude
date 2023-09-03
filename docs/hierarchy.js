export const RequestDirectoryHierarchyRecursivelyExport=RequestDirectoryHierarchyRecursively;
//  Get hierarchy
function RequestDirectoryHierarchyRecursively(path,hierarchy,depth=0,onloadFunc=null){
  console.log("RequestDirectoryHierarchyRecursively for "+path);
  hierarchy[path]={};
  hierarchy[path]["files"]={};
  //  https://stackoverflow.com/questions/54857700/list-files-on-a-server-via-front-end-javascript
  const dirRequest=new XMLHttpRequest();
  dirRequest.open("GET",path,true);
  dirRequest.responseType='text';
  dirRequest.onload=()=>{
    console.log("dirRequest.status:"+dirRequest.status);
    var newRequestsCount=0;
    var dirRequestRespText=null;
    if(dirRequest.status>=200&&dirRequest.status<400){
      console.log("depth:"+depth);
      //  Succesful response
      dirRequestRespText=dirRequest.responseText;
      //console.log("dirRequestRespText:"+dirRequestRespText);
      var hierarchyPathSubdirIndex=0;
       var hierarchyPathFileIndex=0;
      var index=-1;
      while((index=dirRequestRespText.indexOf("href=\"",index+1))>0){
        var subdirStart=index+6;
        var subdirEnd=dirRequestRespText.indexOf("\"",subdirStart);
        var subdir=dirRequestRespText.substring(subdirStart,subdirEnd);
        console.log("index:"+index+";"+subdir);
        if(subdir=="../"){
          console.log("ignore ../");
          continue;
        }
        if(!subdir.endsWith("/")){
          console.log("ignore file for dir request but add it to files array");
          hierarchy[path]["files"][hierarchyPathFileIndex]=subdir;
          hierarchyPathFileIndex++;
          continue;
        }
        hierarchy[path][hierarchyPathSubdirIndex]=subdir;
        hierarchyPathSubdirIndex++;
      }
      const pathsToRequest=[];
      var pathsToRequestIndex=0;
      for(var key in hierarchy[path]){
        if(key=="files"){
          console.log("ignore RequestDirectoryHierarchyRecursively for files");
          continue;
        }
        console.log("hierarchy["+path+"]["+key+"]:"+hierarchy[path][key]);
        pathsToRequest[pathsToRequestIndex]=hierarchy[path][key];
        pathsToRequestIndex++;
      }
      pathsToRequest.forEach((value,index,array)=>{
        RequestDirectoryHierarchyRecursively(value,hierarchy[path],depth+1,onloadFunc);
        newRequestsCount++;
      });
    }
    if(onloadFunc!==null){
      onloadFunc(newRequestsCount);
    }
  };
  dirRequest.send(null);
}
console.log("hierarchy.js is alive now");