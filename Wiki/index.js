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

function RemoveCharAt(string, index) {
  return string.slice(0, index) + string.slice(index + 1);
}

function SetCharAt(string, index, char) {
  return string.substr(0, index) + char + string.substr(index + 1);
}

// DOM element using Jquery

function CreateDOMelement(options, content, index) {
  
  const element = document.createElement(options.child);
  $(options.parent).append(element);
  $(element).text(content);
  $(element).addClass(`${options.className} ${options.className}_${index}`);
  console.log(`Element created: ${options.child} - ${index}`);
}

// DOM create element object 

function CreateDOMobject(parent, child, className) {
  this.parent = parent;
  this.child = child;
  this.className = className;
}

//Parse text

const titleObj = new CreateDOMobject(".wiki", "h2", "title");
const textObj = new CreateDOMobject(".wiki", "section", "text");

function ParseText(text) {
  let titleIndex = 0,
    textIndex = 0;
  for (const line of text) {
    if (line.charAt(0) == "#") {
      let title = RemoveCharAt(line, 0);
      CreateDOMelement(titleObj, title, titleIndex);
      titleIndex++;
    } else {
      CreateDOMelement(textObj, line, textIndex);
      textIndex++;
    }
  }
}

ParseText(text);
