using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NegativeEddy.Bots.Twitch.AspNetCore;
using NegativeEddy.Bots.Twitch.SampleBot.Commands;
using System;

namespace NegativeEddy.Bots.Twitch.BlazorHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state.
            services.AddSingleton<UserState>();
            BotCommandManager cmdMgr = SetUpCommands();
            services.AddSingleton<BotCommandManager>(cmdMgr);

            services.AddTransient<IBot, SampleTwitchBot>();

            var twitchSettings = new TwitchAdapterSettings();
            Configuration.GetSection("twitchBot").Bind(twitchSettings);

            services.AddTwitchBotAdapter(twitchSettings);
        }

        private static BotCommandManager SetUpCommands()
        {
            var cmdMgr = new BotCommandManager();
            cmdMgr.Add(new IBotCommand[]
            {
                new BeforeAndAfterCommandDecorator( new EchoCommand()),
                new CoolDownOption( new EchoCommand() { Command="slowecho" })
                {
                    Cooldown =  TimeSpan.FromSeconds(10),
                    CooldownMessage ="Whoa there slick! too fast for me!"
                },
                new LGResponseCommand
                {
                    Command = "quote",
                    Template =
                        @"# response
                        - May the Force be with you
                        - There's no place like home
                        - I'll be back
                        - Houston, we have a problem"
                },
                new LGResponseCommand
                {
                    Command = "specs",
                    Template =
                        @"# response
                        - Not as good as most streamers
                        - Wouldn't you like to know?"
                },
                new TextResponseCommand("help", "", "sorry I can't help you"),
                new JoinCommand(),
                new LeaveCommand(),
            });
            return cmdMgr;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
