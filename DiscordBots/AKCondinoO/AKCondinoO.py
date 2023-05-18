import os
import discord
from dotenv import load_dotenv
load_dotenv()
print('run os.getenv')
TOKEN=os.getenv('DISCORD_TOKEN')
GUILD=os.getenv('DISCORD_GUILD')
if TOKEN is None or GUILD is None:
    print('os.getenv failed to get essential data')
    quit()
print('DISCORD_TOKEN:'+TOKEN)
print('DISCORD_GUILD:'+GUILD)
intents=discord.Intents.all()
#  TO DO: set individual intents flags to true or false
client=discord.Client(intents=intents)
@client.event
async def on_ready():
    print(f'{client.user} robot is now connected to the following discord guilds:')
    for guild in client.guilds:
        print("guild.name:"+guild.name)
client.run(TOKEN)