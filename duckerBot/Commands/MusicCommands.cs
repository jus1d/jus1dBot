﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.VisualBasic;

namespace duckerBot
{
    public partial class Commands : BaseCommandModule
    {
        // -join
        [Command("join"),
         Description(("bot joined to your voice channel")),
         RequirePermissions(Permissions.Administrator)]
        public async Task Join(CommandContext msg)
        {
            DiscordChannel channel = msg.Member.VoiceState.Channel;
            
            var lava = msg.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await msg.Channel.SendMessageAsync("Connection is not established");
                return;
            }
            
            var node = lava.ConnectedNodes.Values.First();
            
            if (msg.Member.VoiceState == null || msg.Member.VoiceState.Channel == null)
            {
                await msg.Channel.SendMessageAsync("You are not in a voice channel.");
                return;
            }
            
            await node.ConnectAsync(channel);
        }
        
        // -join channel
        [Command("join"),
         Description("bot joined to tagged voice channel"),
         RequirePermissions(Permissions.Administrator)]
        public async Task Join(CommandContext msg, [Description("voice channel")] DiscordChannel channel)
        {
            var lava = msg.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await msg.Channel.SendMessageAsync("Connection is not established");
                return;
            }
            
            var node = lava.ConnectedNodes.Values.First();

            await node.ConnectAsync(channel);
        }

        // -quit
        [Command("quit"),
         Description("bot quit from your channel"),
         RequirePermissions(Permissions.Administrator)]
        public async Task Quit(CommandContext msg)
        {
            DiscordChannel channel = msg.Member.VoiceState.Channel;
            
            var lava = msg.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await msg.Channel.SendMessageAsync("Connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await msg.Channel.SendMessageAsync("Not a valid voice channel.");
                return;
            }

            var connection = node.GetGuildConnection(channel.Guild);

            if (connection == null)
            {
                await msg.Channel.SendMessageAsync("I'm is not connected.");
                return;
            }

            await connection.DisconnectAsync();
        }
        
        // -quit channel
        [Command("quit"),
         Description("bot quit from tagged channel"),
         RequirePermissions(Permissions.Administrator)]
        public async Task Quit(CommandContext msg, [Description("voice channel to quit")] DiscordChannel channel)
        {
            var lava = msg.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await msg.Channel.SendMessageAsync("Connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await msg.Channel.SendMessageAsync("Not a valid voice channel.");
                return;
            }

            var connection = node.GetGuildConnection(channel.Guild);

            if (connection == null)
            {
                await msg.Channel.SendMessageAsync("I'm is not connected.");
                return;
            }

            await connection.DisconnectAsync();
        }
        
        // -play url
        [Command("play")]
        [Description("bot joined to your voice, and playing video or track by your search query")]
        public async Task Play(CommandContext msg, [Description("URL")] Uri url)
        {
            await Join(msg);
            
            if (msg.Member.VoiceState == null || msg.Member.VoiceState.Channel == null)
            {
                await msg.Channel.SendMessageAsync("You are not in a voice channel.");
                return;
            }
            var lava = msg.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(msg.Member.VoiceState.Guild);
            if (connection == null)
            {
                await msg.Channel.SendMessageAsync("ya is not connected.");
                return;
            }
            var loadResult = await node.Rest.GetTracksAsync(url);
            var track = loadResult.Tracks.First();
            await connection.PlayAsync(track);
            
            var playEmbed = new DiscordEmbedBuilder
            {
                Title = "Now playing",
                Description = $"[{track.Title}]({url})",
                Footer = { IconUrl = msg.User.AvatarUrl, Text = "Ordered by " + msg.User.Username },
                Color = mainEmbedColor
            };
            playEmbed.WithFooter("Ordered by " + msg.User.Username, msg.User.AvatarUrl);
            
            await msg.Channel.SendMessageAsync(playEmbed);
        }
        
        // -play search
        [Command("play")]
        [Description("bot joined to your voice and playing video by your search query")]
        public async Task Play(CommandContext msg, [Description("search query")] params string[] searchInput)
        {
            await Join(msg);

            if (msg.Member.VoiceState == null || msg.Member.VoiceState.Channel == null)
            {
                await msg.Channel.SendMessageAsync("You are not in a voice channel.");
                return;
            }

            var lava = msg.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(msg.Member.VoiceState.Guild);

            if (connection == null)
            {
                await msg.Channel.SendMessageAsync("ya is not connected.");
                return;
            }

            string search = "";
            for (int i = 0; i < searchInput.Length; i++)
            {
                search += searchInput[i] + " ";
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed 
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await msg.Channel.SendMessageAsync($"Track search failed for: {search}");
                return;
            }

            var track = loadResult.Tracks.First();

            await connection.PlayAsync(track);
            
            var playEmbed = new DiscordEmbedBuilder
            {
                Title = "Now playing",
                Description = $"[{track.Title}]({track.Uri})",
                Footer = { IconUrl = msg.User.AvatarUrl, Text = "Ordered by " + msg.User.Username },
                Color = mainEmbedColor
            };

            await msg.Channel.SendMessageAsync(playEmbed);
        }
        
        // -play 
        [Command("play"), 
         Description("resume playing music")]
        public async Task Play(CommandContext msg)
        {
            if (msg.Member.VoiceState == null || msg.Member.VoiceState.Channel == null)
            {
                var incorrectMusicCommand = new DiscordEmbedBuilder
                {
                    Title = "You are not in a voice channel",
                    Footer = { IconUrl = msg.User.AvatarUrl, Text = "For " + msg.User.Username }
                };
                await msg.Channel.SendMessageAsync(incorrectMusicCommand);
                return;
            }

            var lava = msg.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(msg.Member.VoiceState.Guild);

            if (connection.CurrentState.CurrentTrack == null)
            {
                var incorrectMusicCommand = new DiscordEmbedBuilder
                {
                    Title = "There are no tracks loaded",
                    Footer = { IconUrl = msg.User.AvatarUrl, Text = "For " + msg.User.Username }
                };
                return;
            }

            await connection.ResumeAsync();
        }
        
        // -pause
        [Command("pause"), 
         Description("pause playing music")]
        public async Task Pause(CommandContext msg)
        {
            if (msg.Member.VoiceState == null || msg.Member.VoiceState.Channel == null)
            {
                await msg.Channel.SendMessageAsync("You are not in a voice channel.");
                return;
            }

            var lava = msg.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(msg.Member.VoiceState.Guild);

            if (connection == null)
            {
                await msg.Channel.SendMessageAsync("Not connected.");
                return;
            }

            if (connection.CurrentState.CurrentTrack == null)
            {
                await msg.Channel.SendMessageAsync("There are no tracks loaded.");
                return;
            }
            await connection.PauseAsync();
        }

        [Command("pause"), Description("pause playing music")]
        public async Task Pause(CommandContext msg, params string[] text)
        {
            var incorrectPauseCommandEmbed = new DiscordEmbedBuilder
            {
                Title = "Missing argument",
                Description = "**Usage:** -pause",
                Footer = { IconUrl = msg.User.AvatarUrl, Text = "For " + msg.User.Username },
                Color = incorrectEmbedColor
            };
            await msg.Channel.SendMessageAsync(incorrectPauseCommandEmbed);
        }
        
        
        // -stop
        [Command("stop"), 
         Description("permanently stop bot playing and bot quit")]
        public async Task Stop(CommandContext msg)
        {
            var lava = msg.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await msg.Channel.SendMessageAsync("Connection is not established");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(msg.Member.VoiceState.Channel.Guild);

            if (connection == null)
            {
                await msg.Channel.SendMessageAsync("I'm is not connected.");
                return;
            }
            await connection.DisconnectAsync();
        }

        [Command("stop"), Description("stop music, and kicks bof from voice channel")]
        public async Task Stop(CommandContext msg, params string[] text)
        {
            await Quit(msg, msg.Member.VoiceState.Channel);
        }
    }
}