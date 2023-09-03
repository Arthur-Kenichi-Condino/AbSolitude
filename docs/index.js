const rootPath=".\\WikiContent/";
const hierarchy={};
import{RequestDirectoryHierarchyRecursivelyExport as RequestDirectoryHierarchyRecursively}from"./hierarchy.js"
//  create DOM element object
function CreateDOMobject(parent,child,className){
  this.parent=parent;
  this.child=child;
  this.className=className;
}
const menuObj=new CreateDOMobject(".wiki_menu","div","menu_item");
RequestDirectoryHierarchyRecursively(rootPath,hierarchy,0,ParseMenuText);
var sentRequestsCount=1;
function ParseMenuText(newRequestsCount){
  sentRequestsCount+=newRequestsCount;
  sentRequestsCount--;
  console.log("ParseMenuText:sentRequestsCount:"+sentRequestsCount);
  if(sentRequestsCount<=0){
    function DoForEachRecursively(hierarchyObj,hierarchyPath,hierarchyParentElement){
      var contentTxtFile=null;
      for(var key in hierarchyObj){
        if(key=="files"){
          console.log("ParseMenuText for files");
          for(var key2 in hierarchyObj[key]){
            var fileInHierarchy=hierarchyObj[key][key2];
            console.log("ParseMenuText:fileInHierarchy:"+fileInHierarchy);
            if(contentTxtFile==null&&fileInHierarchy.endsWith("Content.txt")){
              contentTxtFile=fileInHierarchy;
              continue;
            }
          }
          continue;
        }
      }
      if(hierarchyPath!=null){
        hierarchyParentElement=CreateMenuDOMelement(menuObj,hierarchyPath,0,hierarchyParentElement,contentTxtFile);
      }
      for(var key in hierarchyObj){
        if(key=="files"){
          continue;
        }
        if(typeof key=="string"&&key.endsWith("/")){
          console.log("ParseMenuText:doing parse:next key:"+key);
          DoForEachRecursively(hierarchyObj[key],key,hierarchyParentElement);
          continue;
        }
      }
    }
    console.log("ParseMenuText:do parsing now!");
    DoForEachRecursively(hierarchy,null,null);
  }
}
//  DOM element, using Jquery
function CreateMenuDOMelement(options,path,index,parent=null,contentTxtFile=null){
  console.log("CreateMenuDOMelement:path:"+path+";parent:"+parent+";contentTxtFile:"+contentTxtFile);
  const element=document.createElement(options.child);
  if(parent!=null){
    $(parent).append(element);
  }else{
    $(options.parent).append(element);
  }
  $(element).addClass(`${options.className} ${options.className}_${index}`);
  var menuItemText=path.substring(path.lastIndexOf("\\")+1,path.lastIndexOf("/"));
  var button=null;
  if(contentTxtFile!=null){
    button=document.createElement("BUTTON");
    $(element).append(button);
    const buttonText=document.createTextNode(menuItemText);
    $(button).append(buttonText);
    $(button).addClass(`${options.className}_button ${options.className}_button_${index}`);
    button.onclick=async function(){
      var contentTxtFilePathToFetch=contentTxtFile.substring(1).replaceAll("\\","/");
      console.log("contentTxtFilePathToFetch:"+contentTxtFilePathToFetch);
      const contentText=await FetchTextFromFile(contentTxtFilePathToFetch);
      ParseContentText(contentText);
    };
  }else{
    $(element).text(menuItemText);
  }
  console.log(`wiki menu:DOM element created: ${options.child} ;element classes: ${element.className} ;button: ${button}`);
  return element;
}

  //document.getElementsByClassName("menu_text")[0].innerHTML=string;
  //console.log(document.getElementsByClassName("menu_text")[0].innerHTML);


//const root = "WikiContent";
//const homunculusSystem = "HomunculusSystem";
//const selectedHomunculus = "Vanilmirth";

async function FetchTextFromFile(path) {
  const response = await fetch(path);
  const rawText = await response.text();
  const stringText = rawText.toString();
  const splitedText = stringText.split("\n");
  return splitedText;
}

//const text = await FetchTextFromFile(
//  root + "/" + homunculusSystem + "/" + selectedHomunculus + "/" + "Content.txt"
//);

//Parse text functions

// DOM element using Jquery
let contentDOMelementsCreated={};
let contentDOMelementsCreatedCount=0;
function CreateDOMelement(options, content, index) {
  
  const element = document.createElement(options.child);
  contentDOMelementsCreated[contentDOMelementsCreatedCount++]=element;
  $(options.parent).append(element);
  $(element).text(content);
  $(element).addClass(`${options.className} ${options.className}_${index}`);
  //console.log(`Element created: ${options.child} - ${index}`);
}


//Parse text

const titleObj = new CreateDOMobject(".wiki_content", "h2", "title");
const textObj = new CreateDOMobject(".wiki_content", "section", "text");

function ParseContentText(text) {
  for(var index in contentDOMelementsCreated){
    console.log("contentDOMelementCreated:"+contentDOMelementsCreated[index]);
    contentDOMelementsCreated[index].remove();
    delete contentDOMelementsCreated[index];
  }
  contentDOMelementsCreated={};
  contentDOMelementsCreatedCount=0;
  let titleIndex = 0,
    textIndex = 0;
  for (const line of text) {
    if (line.charAt(0) == "#") {
      let title = RemoveCharsFromStringAt(line, 0);
      CreateDOMelement(titleObj, title, titleIndex);
      titleIndex++;
    } else {
      CreateDOMelement(textObj, line, textIndex);
      textIndex++;
    }
  }
}

//ParseText(text);

//testthis();
//export const testthisExport=testthis;
//const module = {};

//window.onload=function(){
//const hierarchy1 = require("./hierarchy.js");
//hierarchy1.RequestDirectoryHierarchyRecursivelyHTML(0);
  //RequestDirectoryHierarchyRecursivelyHTML(0);
//}
//import { testthisExport } from "./index.js"
//testthisExport();
//module.testthisHTML=testthisExport;
//module.testthisHTML();