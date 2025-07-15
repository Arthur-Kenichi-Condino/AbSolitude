const port=3232;
const hierarchy=require('./hierarchy.js');
const path=require('path');
const fs=require('fs');
const express=require('express');
const app=express();
app.set('view engine','ejs');
//  homepage-rota
const homepagePath=path.join(__dirname,'..','WikiContent','homepage.html');
app.get('/',function(request,response){
  console.log('get foi efetuado para a homepage');
  fs.readFile(homepagePath,'utf8',(error,homepageHtml)=>{
    if(error){
      console.error('erro ao ler homepage.html:',error);
      response.status(500).send('homepage.html está quebrado:algum administrador do site precisa corrigir o código.');
      return;
    }
    hierarchy.BuildDirectoryHierarchyArrayRecursively();
    //console.log("wikiContentDirectoryHierarchy:"+hierarchy.public.wikiContentDirectoryHierarchy);
    hierarchy.DoForEachRecursivelyAndCreateIndex(hierarchy.public.wikiContentDirectoryHierarchy,0);
    var homepageParsed=homepageHtml;
    response.send(homepageParsed);
  });
});
//  rota-padrão
app.use(express.static(path.join(__dirname,'..','docs')));
//  execute no terminal o seguinte para iniciar o servidor: 
// npm run dev
const server=app.listen(port,()=>{
  console.log(`updated app listening on port ${port}`);
})