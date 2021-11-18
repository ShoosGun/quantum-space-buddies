﻿using QSB.Player;

namespace QSB.QuantumSync.WorldObjects
{
	internal class QSBQuantumMoon : QSBQuantumObject<QuantumMoon>
	{
		public override void Init(QuantumMoon moonObject, int id)
		{
			ObjectId = id;
			AttachedObject = moonObject;
			ControllingPlayer = QSBPlayerManager.LocalPlayerId;
			base.Init(moonObject, id);
		}
	}
}
