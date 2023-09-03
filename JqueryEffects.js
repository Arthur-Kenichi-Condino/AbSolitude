function JQueryFadeOutElement(className,timeout){
  //console.log("JQueryFadeOutElement:"+className+" in "+timeout+" ms");
  setTimeout(function(){
    //console.log("JQueryFadeOutElement:running setTimeout function for "+className);
    $(document.getElementsByClassName(className)).fadeOut("fast");
  },timeout);
}
function JQueryFadeInElement(className,timeout){
  //console.log("JQueryFadeInElement:"+className+" in "+timeout+" ms");
  setTimeout(function(){
    //console.log("JQueryFadeInElement:running setTimeout function for "+className);
    $(document.getElementsByClassName(className)).fadeIn("fast");
  },timeout);
}
//console.log("JqueryEffects.js is alive now");