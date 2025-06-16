import os
import discord
from discord.utils import get
from discord.ext import commands,tasks
from dotenv import load_dotenv
load_dotenv()
TOKEN=os.getenv('DISCORD_TOKEN')
GUILD=os.getenv('DISCORD_GUILD')
if TOKEN is None or GUILD is None:#{
 print('os.getenv failed to get essential data:shutdown')
 quit()
#}
intents=discord.Intents.default()
intents.guilds         =True
intents.guild_messages =True
intents.message_content=True
client=commands.Bot(command_prefix='cmd.',intents=intents)
#event on_ready
@client.event
async def on_ready():#{
 print(f'{client.user}, the robot, is now connected:check if it\'s in the right guild:')
 for guild in client.guilds:#{
  print("guild.name:"+guild.name+";guild.id:"+str(guild.id))
  if guild.name!=GUILD:#{
   print('connected to a guild it should not have connected:shutdown')
   quit()
  #}
  global guildId
  guildId=guild.id
 #}
#}
#end of on_ready
#event on_message
@client.event
async def on_message(message):#{
 print("on_message:"+message.content)
 for role in message.author.roles:
  print("author roles:"+str(role))
 await client.process_commands(message)
#}
#end of on_message
@client.event
async def on_command_error(context,error):
 await context.send(f'{error}')
 raise error
@client.command(name="ping")
async def command_ping(context):#{
 await context.send("pong")
#}
role_DbD=["joga Dead by Daylight"]
async def isChannel_DdB_startVoice(context):
 return context.channel.id==1370993554641387520
@client.command(name="DbDVoice",case_sensitive=True)
@commands.has_any_role(*role_DbD)
@commands.check(isChannel_DdB_startVoice)
async def command_DbD_startVoice(context):#{
 await context.send("to create a voice channel for DbD player "+str(context.author))
 guild=client.get_guild(guildId)
 category=get(guild.categories,id=1370989264728358984)
 voiceChannel=await guild.create_voice_channel(f'DbD voice channel for {context.author}',category=category)
 print("created a new voice channel!")
 print(f"moving {context.author} to channel...")
#}
#begin app
client.run(TOKEN)