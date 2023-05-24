async function ParserText(path) {
    const response = await fetch(path);
    const data = await response.text();
    console.log(data);
}

ParserText("WikiContent/HomunculusSystem/Vanilmirth/Content.txt");