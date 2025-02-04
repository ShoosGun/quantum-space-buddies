﻿using QSB.Messaging;
using QSB.ShipSync.WorldObjects;

namespace QSB.ShipSync.Messages.Hull;

internal class HullChangeIntegrityMessage : QSBWorldObjectMessage<QSBShipHull, float>
{
	public HullChangeIntegrityMessage(float integrity) : base(integrity) { }

	public override void OnReceiveRemote() => WorldObject.ChangeIntegrity(Data);
}