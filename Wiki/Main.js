console.log("trying to become alive...");
//
const allScripts=document.getElementsByTagName('script');
var rootPath=null;
for(var i=allScripts.length-1;i>=0;--i){
    var src=allScripts[i].src;
    //console.log("searching allScripts...src:"+src);
    if(src.substring(src.length-'AbSolitude/Wiki/Main.js'.length)=='AbSolitude/Wiki/Main.js'){
        //console.log("found Main.js src:"+src);
        rootPath=src.substring(0,src.length-'Main.js'.length);
        console.log("found Main.js rootPath:"+rootPath);
    }
}
const dirRequest=new XMLHttpRequest();
dirRequest.open("GET",rootPath,true);
var dirRequestResp=null;
//  https://stackoverflow.com/questions/54857700/list-files-on-a-server-via-front-end-javascript
dirRequest.onload=()=>{
    //  Succesful response
    console.log("dirRequest.status:"+dirRequest.status);
    if(dirRequest.status>=200&&dirRequest.status<400){
        dirRequestResp=dirRequest.responseText;
        //console.log("dirRequestResp:"+dirRequestResp);
    }
};
dirRequest.send(null);
console.log("I'm alive!");
function ParseFileSystemTreeDataToWikiElementInnerHtmlByClassName(className){
    console.log("ParseFileSystemTreeDataToWikiElementInnerHtmlByClassName:"+className);
    if(dirRequestResp!==null){
        document.getElementsByClassName(className)[0].innerHTML=dirRequestResp;
    }
}
//  https://stackoverflow.com/questions/14446447/how-to-read-a-local-text-file-in-the-browser
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