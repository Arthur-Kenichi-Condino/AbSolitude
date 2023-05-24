console.log("trying to become alive...");
//
//var rootPath=".";
//var rootURL=null;
//const allScripts=document.getElementsByTagName('script');
//for(var i=allScripts.length-1;i>=0;--i){
//    var src=allScripts[i].src;
//    console.log("searching allScripts...src:"+src);
//    if(src.substring(src.length-'/Main.js'.length)=='/Main.js'){
//        //console.log("found Main.js src:"+src);
//        rootURL=src.substring(0,src.length-'Main.js'.length);
//        console.log("found Main.js rootURL:"+rootURL);
//    }
//}
function RequestDirectoryIndexDataRecursivelyThenParseToWikiElementInnerHtmlByClassName(dir,className){
    const dirRequest=new XMLHttpRequest();
    dirRequest.open("GET",dir,true);
    //  https://stackoverflow.com/questions/54857700/list-files-on-a-server-via-front-end-javascript
    dirRequest.onload=()=>{
        var dirRequestResp=null;
        var allText=null;
        //  Succesful response
        console.log("dirRequest.status:"+dirRequest.status);
        if(dirRequest.status>=200&&dirRequest.status<400){
            dirRequestResp=dirRequest.responseText;
            console.log("dirRequestResp:"+dirRequestResp);
        }
        if(allText!==null){
            document.getElementsByClassName(className)[0].innerHTML=allText;
        }
    };
    dirRequest.send(null);
}
function ParseDirectoryIndexDataTreeToWikiElementInnerHtmlByClassName(dir,className){
    console.log("ParseDirectoryIndexDataTreeToWikiElementInnerHtmlByClassName:"+className);
    RequestDirectoryIndexDataRecursivelyThenParseToWikiElementInnerHtmlByClassName(dir,className);
}
//  https://stackoverflow.com/questions/14446447/how-to-read-a-local-text-file-in-the-browser
function ParseTextFileToWikiElementInnerHtmlByClassName(file,className){
    const rawFileRequest=new XMLHttpRequest();
    rawFileRequest.open("GET",file,false);
    rawFileRequest.onreadystatechange=function(){
        if(rawFileRequest.readyState===4){
            if(rawFileRequest.status===200||rawFileRequest.status==0){
                //var allText=rawFileRequest.responseText
                //    .replaceAll("\\br\r\n","<br/>")
                //    .replaceAll("\\br","<br/>")
                //    .replaceAll("\\t","<p class=\"Title\">")
                //    .replaceAll("\\p","<p class=\"Paragraph\">")
                //    .replaceAll("\r\n","</p>");
                //console.log(allText);
                //document.getElementsByClassName(className)[0].innerHTML=allText;
            }
        }
    }
    rawFileRequest.send(null);
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
console.log("I'm alive!");