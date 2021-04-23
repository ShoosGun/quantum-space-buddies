﻿using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class PlayerStateMessage : PlayerMessage
	{
		public string PlayerName { get; set; }
		public PlayerState PlayerState { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerName = reader.ReadString();
			PlayerState = new PlayerState
			{
				IsReady = reader.ReadBoolean(),
				FlashlightActive = reader.ReadBoolean(),
				SuitedUp = reader.ReadBoolean(),
				ProbeLauncherEquipped = reader.ReadBoolean(),
				SignalscopeEquipped = reader.ReadBoolean(),
				TranslatorEquipped = reader.ReadBoolean(),
				ProbeActive = reader.ReadBoolean()
			};
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerName);
			writer.Write(PlayerState.IsReady);
			writer.Write(PlayerState.FlashlightActive);
			writer.Write(PlayerState.SuitedUp);
			writer.Write(PlayerState.ProbeLauncherEquipped);
			writer.Write(PlayerState.SignalscopeEquipped);
			writer.Write(PlayerState.TranslatorEquipped);
			writer.Write(PlayerState.ProbeActive);
		}
	}
}