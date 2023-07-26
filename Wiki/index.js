const rootPath=".\\WikiContent/";
const hierarchy={};
import{RequestDirectoryHierarchyRecursivelyExport as RequestDirectoryHierarchyRecursively}from"./hierarchy.js"
RequestDirectoryHierarchyRecursively(rootPath,hierarchy,0,ParseMenuText);
const menuObj=new CreateDOMobject(".wiki_menu","menu_section","menu_text");
var sentRequestsCount=1;
function ParseMenuText(newRequestsCount){
  sentRequestsCount+=newRequestsCount;
  sentRequestsCount--;
  console.log("ParseMenuText:sentRequestsCount:"+sentRequestsCount);
  if(sentRequestsCount<=0){
    var menuText=[];
    function DoForEachRecursively(hierarchyObj){
      for(var key in hierarchyObj){
        if(key=="files"){
          console.log("ParseMenuText for files");
          for(var key2 in hierarchyObj[key]){
            var fileInHierarchy=hierarchyObj[key][key2];
            console.log("ParseMenuText:fileInHierarchy:"+fileInHierarchy);
          }
          continue;
        }
        if(typeof key=="string"&&key.endsWith("/")){
          console.log("ParseMenuText:doing parse:next key"+key);
          menuText.push(key+"\n");
          DoForEachRecursively(hierarchyObj[key]);
        }
      }
    }
    console.log("ParseMenuText:do parsing now");
    DoForEachRecursively(hierarchy);
    CreateDOMelement(menuObj,menuText.join(),0);
  }
  //document.getElementsByClassName("menu_text")[0].innerHTML=string;
  //console.log(document.getElementsByClassName("menu_text")[0].innerHTML);
}

const root = "WikiContent";
const homunculusSystem = "HomunculusSystem";
const selectedHomunculus = "Vanilmirth";

async function FetchText(path) {
  const response = await fetch(path);
  const rawText = await response.text();
  const stringText = rawText.toString();
  const splitedText = stringText.split("\n");
  return splitedText;
}

const text = await FetchText(
  root + "/" + homunculusSystem + "/" + selectedHomunculus + "/" + "Content.txt"
);

//Parse text functions

// DOM element using Jquery

function CreateDOMelement(options, content, index) {
  
  const element = document.createElement(options.child);
  $(options.parent).append(element);
  $(element).text(content);
  $(element).addClass(`${options.className} ${options.className}_${index}`);
  //console.log(`Element created: ${options.child} - ${index}`);
}

// DOM create element object 

function CreateDOMobject(parent, child, className) {
  this.parent = parent;
  this.child = child;
  this.className = className;
}

//Parse text

const titleObj = new CreateDOMobject(".wiki_content", "h2", "title");
const textObj = new CreateDOMobject(".wiki_content", "section", "text");

function ParseText(text) {
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

ParseText(text);

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