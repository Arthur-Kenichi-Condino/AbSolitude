function RemoveCharsFromStringAt(string,startIndex,count=1){
  return string.slice(0,startIndex)+string.slice(startIndex+count);
}
function InsertCharsInStringAt(string,index,chars){
  return string.substr(0,index)+chars+string.substr(index+chars.length);
}
//console.log("util.js is alive now");