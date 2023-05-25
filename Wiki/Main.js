//Paths

const root = "WikiContent/";
const homunculusSystem = "HomunculusSystem/";
const selectedHomunculus = "Vanilmirth/";

async function FetchText(path) {
    const response = await fetch(path);
    const rawText = await response.text();
    const stringText = rawText.toString();
    const splitedText = stringText.split("\n");
    return splitedText;
}

const text = await FetchText(root + homunculusSystem + selectedHomunculus + "Content.txt");

function ParseText(text) {
    for (line of text) {
        console.log(line);
    }
}

ParseText(text);