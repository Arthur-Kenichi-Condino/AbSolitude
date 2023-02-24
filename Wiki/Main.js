console.log("I'm alive!");
function JQueryFadeOutByClassName(className,timeout){
    console.log("JQueryFadeOutByClassName:"+className+" in "+timeout+" ms");
    setTimeout(function(){
        console.log("JQueryFadeOutByClassName:running setTimeout function for "+className);
        $(document.getElementsByClassName(className)).fadeOut('fast');
    },timeout);
}
function JQueryFadeInElementByClassName(className,timeout){
    console.log("JQueryFadeInElementByClassName:"+className+" in "+timeout+" ms");
    setTimeout(function(){
        console.log("JQueryFadeInElementByClassName:running setTimeout function for "+className);
        $(document.getElementsByClassName(className)).fadeIn('fast');
    },timeout);
}