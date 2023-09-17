using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace SKz_Bot
{
    public class Program
    {
        private DiscordSocketClient _client;

        public static Task Main(string[] args) => new Program().MainAsync();


        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            await _client.LoginAsync(TokenType.Bot, Token.tokenSecret);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);            
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task Client_Ready()
        {
            // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
            var guild = _client.GetGuild(1147989899400708146);

            SlashCommandBuilder[] globalCommand = new SlashCommandBuilder[]
            {
                new SlashCommandBuilder()
                    .WithName("first-global-command")
                    .WithDescription("This is my first global slash command")
            };
            SlashCommandBuilder[] guildCommand = new SlashCommandBuilder[]
            {
                new SlashCommandBuilder()
                    // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
                    .WithName("first-command")
                    // Descriptions can have a max length of 100.
                    .WithDescription("This is my first guild slash command!"),
                new SlashCommandBuilder()
                    .WithName("list-roles")
                    .WithDescription("Lists all roles of a user.")
                    .AddOption("user", ApplicationCommandOptionType.User, "The users whos roles you want to be listed", isRequired: true)
            };
            
            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                for (var i = 0; i < guildCommand.Length; i++)
                {
                    await guild.CreateApplicationCommandAsync(guildCommand[i].Build());
                    await Log(new LogMessage(LogSeverity.Info, "Client", $"GuildCommand {guildCommand[i].Name} updated"));
                }

                // With global commands we don't need the guild.
                for (var i = 0; i < globalCommand.Length; i++)
                {
                    await _client.CreateGlobalApplicationCommandAsync(globalCommand[i].Build());
                    await Log(new LogMessage(LogSeverity.Info, "Client", $"GlobalCommand {globalCommand[i].Name} updated"));
                }
                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch (ApplicationCommandException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                await Log(new LogMessage(LogSeverity.Error, "Client", $"Failed to build commands \n See the following json: \n {json}", exception));
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync($"You executed {command.Data.Name}");
        }

    }
}