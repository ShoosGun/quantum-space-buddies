﻿using Cysharp.Threading.Tasks;
using QSB.ElevatorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.ElevatorSync
{
	public class ElevatorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken cancellationToken)
			=> QSBWorldSync.Init<QSBElevator, Elevator>();
	}
}