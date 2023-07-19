//  Paths
const rootPath="./WikiContent";
const dirRequest=new XMLHttpRequest();
function RequestDirectoryHierarchyRecursively(depth=0,onloadFunc=null){
  console.log("RequestDirectoryHierarchyRecursively");
  //  https://stackoverflow.com/questions/54857700/list-files-on-a-server-via-front-end-javascript
  dirRequest.open("GET",rootPath,true);
  dirRequest.onload=()=>{
    console.log("dirRequest.status:"+dirRequest.status);
    var dirRequestRespText=null;
    if(dirRequest.status>=200&&dirRequest.status<400){
      //  Succesful response
      dirRequestRespText=dirRequest.responseText;
      //console.log("dirRequestRespText:"+dirRequestRespText);
      if(onloadFunc!==null){
        onloadFunc(dirRequestRespText);
      }
    }
  };
  dirRequest.send(null);
}
console.log("hierarchy.js is alive now");
export const RequestDirectoryHierarchyRecursivelyExport=RequestDirectoryHierarchyRecursively;