//Paths

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

const menuObj = new CreateDOMobject(".wiki_menu", "menu_section", "menu_text");

function testthis(string){
  console.log("testthis");
  CreateDOMelement(menuObj, string, 0);
  document.getElementsByClassName("menu_text")[0].innerHTML=string;
  console.log(document.getElementsByClassName("menu_text")[0].innerHTML);
}
//testthis();
//export const testthisExport=testthis;
//const module = {};
import { RequestDirectoryHierarchyRecursivelyExport as RequestDirectoryHierarchyRecursively } from "./hierarchy.js"
RequestDirectoryHierarchyRecursively(0,testthis)
//window.onload=function(){
//const hierarchy1 = require("./hierarchy.js"); 
//hierarchy1.RequestDirectoryHierarchyRecursivelyHTML(0);
  //RequestDirectoryHierarchyRecursivelyHTML(0);
//}
//import { testthisExport } from "./index.js"
//testthisExport();
//module.testthisHTML=testthisExport;
//module.testthisHTML();