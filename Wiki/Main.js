console.log("I'm alive!");
function ParseFileSystemTreeDataToWikiElementInnerHtmlByClassName(className,dir){
    console.log("ParseFileSystemTreeDataToWikiElementInnerHtmlByClassName:"+className+";dir:"+dir);
}
//https://stackoverflow.com/questions/14446447/how-to-read-a-local-text-file-in-the-browser
function ParseTextFileToWikiElementInnerHtmlByClassName(file,className){
    var rawFile=new XMLHttpRequest();
    rawFile.open("GET",file,false);
    rawFile.onreadystatechange=function(){
        if(rawFile.readyState===4){
            if(rawFile.status===200||rawFile.status==0){
                var allText=rawFile.responseText
                    .replaceAll("\\br\r\n","<br/>")
                    .replaceAll("\\br","<br/>")
                    .replaceAll("\\t","<p class=\"Title\">")
                    .replaceAll("\\p","<p class=\"Paragraph\">")
                    .replaceAll("\r\n","</p>");
                console.log(allText);
                document.getElementsByClassName(className)[0].innerHTML=allText;
            }
        }
    }
    rawFile.send(null);
}
function JQueryFadeOutElementByClassName(className,timeout){
    //console.log("JQueryFadeOutElementByClassName:"+className+" in "+timeout+" ms");
    setTimeout(function(){
        //console.log("JQueryFadeOutElementByClassName:running setTimeout function for "+className);
        $(document.getElementsByClassName(className)).fadeOut('fast');
    },timeout);
}
function JQueryFadeInElementByClassName(className,timeout){
    //console.log("JQueryFadeInElementByClassName:"+className+" in "+timeout+" ms");
    setTimeout(function(){
        //console.log("JQueryFadeInElementByClassName:running setTimeout function for "+className);
        $(document.getElementsByClassName(className)).fadeIn('fast');
    },timeout);
}