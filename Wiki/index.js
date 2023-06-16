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

const text = await FetchText(root + "/" + homunculusSystem + "/" + selectedHomunculus + "/" + "Content.txt");

//Parse text functions

function RemoveCharAt(string,index) {
    return string.slice(0, index) + string.slice(index + 1);
}

function SetCharAt(string,index,char){
    return string.substr(0, index) + char + string.substr(index + 1);
}

// DOM element using Jquery 

function CreateDOMelement(parent,child,content,index,className) {
    const element = document.createElement(child);
    $(parent).append(element);
    $(element).text(content);
    $(element).addClass(`${className} ${className}_${index}`);
    console.log(`Element created: ${child} - ${index}`);
}

//Parse text 

function ParseText(text) {
    let titleIndex = 0 ,textIndex = 0;
    for (const line of text) {
        if (line.charAt(0) == "#") {
            let title = RemoveCharAt(line, 0);
            CreateDOMelement(".wiki","h2", title, titleIndex, "title");
            titleIndex++;
        } else {
            CreateDOMelement(".wiki","section",line,textIndex,"text")
            textIndex++;
        }
    }
}

ParseText(text);

