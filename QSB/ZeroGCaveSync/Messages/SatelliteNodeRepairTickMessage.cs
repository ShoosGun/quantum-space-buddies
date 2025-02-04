﻿using QSB.Messaging;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync.Messages;

internal class SatelliteNodeRepairTickMessage : QSBWorldObjectMessage<QSBSatelliteNode, float>
{
	public SatelliteNodeRepairTickMessage(float repairFraction) : base(repairFraction) { }

	public override void OnReceiveRemote() => WorldObject.RepairTick(Data);
}