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

function RemoveCharAt(string,index) {
    return string.slice(0, index) + string.slice(index + 1);
}

function ParseText(text) {
    for (let line of text) {
        if (line.charAt(0) == "#") {
            console.log(RemoveCharAt(line, 0));
        }
    }
}

ParseText(text);

