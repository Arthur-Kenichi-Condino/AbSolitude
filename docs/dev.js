const port=3030
const express=require('express');
const app=express();
app.set('view engine','ejs');
//  npm run dev
app.get('/',function(req,res){
  //res.send('I\'m alive!');
});
app.listen(port,()=>{
  console.log(`Example app listening on port ${port}`)
})