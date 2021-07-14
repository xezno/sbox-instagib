using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandbox;

namespace Instagib.Utils
{
	public class Stats
	{
		public enum PacketIds
		{
			Heartbeat = 0,
			Handshake = 1,
			
			EventGameStart = 2,
			EventPlayerJoin = 3,
			EventPlayerLeave = 4,
			EventPlayerKilled = 5,
			EventPlayerRespawn = 6,
			EventPlayerChat = 7,
			EventPlayerDeath = 8,
		};

		public static Stats Instance { get; private set; }

		private WebSocket socket;
		private string socketHost = "localhost";
		
		private bool IsServer { get; set; }
		
		private PlayerStats LastPlayerStats { get; set; }
		
		public Stats( bool isServer )
		{
			Instance = this;
			socket = new();

			socket.Connect( $"ws://{socketHost}:8142" );
			socket.OnMessageReceived += message =>
			{
				// Log.Info( $"WS: received {message}" );
				var asObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>( message );

				if ( (PacketIds)asObject["packetId"].GetInt32() == PacketIds.Heartbeat )
				{
					// Heartbeat contains player stats
					LastPlayerStats = JsonSerializer.Deserialize<PlayerStats>( asObject["data"].GetString() );
					
					// Send a heartbeat back
					SendPacket( PacketIds.Heartbeat );
				}
			};
			
			SendPacket( PacketIds.Handshake, Local.SteamId.ToString() );

			if ( isServer )
			{
				SendPacket( PacketIds.EventGameStart );
			}
			
			IsServer = isServer;
		}

		private void SendPacket( PacketIds id, string data = null )
		{
			var asJson = JsonSerializer.Serialize( 
				new Dictionary<string, object>()
				{
					{ "packetId", id },	
					{ "data", data },
					{ "date", DateTimeOffset.Now.ToUnixTimeSeconds() },
					{ "steamid", Local.SteamId },
					{ "name", Local.DisplayName },
					{ "isHost", IsServer }
				}
			);
			socket.Send( asJson );
			// Log.Info( $"WS: sent {asJson}" );
		}

		[Event.PlayerJoined]
		public void OnPlayerJoined()
		{
			Log.Info( "WS: Player joined event sent" );
			SendPacket( PacketIds.EventPlayerJoin );	
		}
				
		[Event.PlayerLeft]
		public void OnPlayerLeft()
		{
			Log.Info( "WS: Player left event sent" );
			SendPacket( PacketIds.EventPlayerLeave );	
		}
		
		[Event.PlayerRespawn]
		public void OnPlayerRespawn()
		{
			Log.Info( "WS: Player respawn event sent" );
			SendPacket( PacketIds.EventPlayerRespawn );	
		}
		
		[Event.PlayerKilled]
		public void OnPlayerKilled( string attacker, string victim )
		{
			Log.Info( "WS: Player killed event sent" );
			SendPacket( PacketIds.EventPlayerKilled, victim );	
		}
		
		[Event.PlayerChat]
		public void OnPlayerChat( string message )
		{
			Log.Info( "WS: Player chat event sent" );
			SendPacket( PacketIds.EventPlayerChat, message );	
		}

		[Event.PlayerDeath]
		public void OnPlayerDeath( string attacker, string victim )
		{
			Log.Info( "WS: Confirm player death event sent" );
			SendPacket( PacketIds.EventPlayerDeath, attacker );
		}

		public PlayerStats RequestStats()
		{
			return LastPlayerStats;
		}
	}

	public class PlayerStats
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }
		
		[JsonPropertyName("kills")]
		public int Kills { get; set; }
		
		[JsonPropertyName("deaths")]
		public int Deaths { get; set; }
		
		[JsonPropertyName("kdr")]
		public float Kdr { get; set; }
		
		[JsonPropertyName("accuracy")]
		public float Accuracy { get; set; }
		
		[JsonPropertyName("timePlayed")]
		public int TimePlayed { get; set; } // Seconds
		
		[JsonPropertyName("timeRegistered")]
		public long TimeRegistered { get; set; }
	}
}
