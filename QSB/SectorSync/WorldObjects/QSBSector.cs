﻿using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.SectorSync.WorldObjects;

public class QSBSector : WorldObject<Sector>
{
	public Sector.Name Type => AttachedObject.GetName();
	public Transform Transform
	{
		get
		{
			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Error - Tried to get Transform from QSBSector {ObjectId} with null AttachedObject!\r\n{Environment.StackTrace}", MessageType.Error);
				return null;
			}

			return AttachedObject.transform;
		}
	}
	public Vector3 Position => Transform.position;

	public bool IsFakeSector => AttachedObject is FakeSector;
	public FakeSector FakeSector => (FakeSector)AttachedObject;

	public override async UniTask Init(CancellationToken ct)
	{
		if (IsFakeSector)
		{
			QSBSectorManager.Instance.FakeSectors.Add(this);
		}
	}

	public override void OnRemoval()
	{
		if (IsFakeSector)
		{
			QSBSectorManager.Instance.FakeSectors.Remove(this);
		}
	}

	public override void SendInitialState(uint to) { }

	public bool ShouldSyncTo(DynamicOccupant occupantType)
	{
		if (AttachedObject == null)
		{
			DebugLog.ToConsole($"Warning - AttachedObject for sector id:{ObjectId} is null!", MessageType.Warning);
			return false;
		}

		if (!AttachedObject.gameObject.activeInHierarchy)
		{
			return false;
		}

		if (occupantType == DynamicOccupant.Ship && Type == Sector.Name.Ship)
		{
			return false;
		}

		if (AttachedObject.name is "Sector_Shuttle" or "Sector_NomaiShuttleInterior")
		{
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				var shuttleController = AttachedObject.gameObject.GetComponentInParent<NomaiShuttleController>();
				if (shuttleController == null)
				{
					DebugLog.ToConsole($"Warning - Expected to find a NomaiShuttleController for {AttachedObject.name}!", MessageType.Warning);
					return false;
				}

				if (!shuttleController.IsPlayerInside())
				{
					return false;
				}
			}
			else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
			{
				var shuttleController = QSBWorldSync.GetUnityObjects<EyeShuttleController>().First();
				if (shuttleController == null)
				{
					DebugLog.ToConsole($"Warning - Expected to find a EyeShuttleController for {AttachedObject.name}!", MessageType.Warning);
					return false;
				}

				if (!shuttleController._isPlayerInside)
				{
					return false;
				}
			}
		}

		return true;
	}

	public float CalculateScore(OWRigidbody rigidbody)
	{
		var sqrDistance = (Position - rigidbody.GetPosition()).sqrMagnitude;
		var radius = GetRadius();
		var velocity = GetRelativeVelocity(rigidbody);

		return sqrDistance + radius * radius + velocity;
	}

	private float GetRadius()
	{
		// TODO : make this work for other stuff, not just shaped triggervolumes
		var trigger = AttachedObject.GetTriggerVolume();
		if (trigger && trigger.GetShape())
		{
			return trigger.GetShape().CalcWorldBounds().radius;
		}

		return 0f;
	}

	private float GetRelativeVelocity(OWRigidbody rigidbody)
	{
		var sectorRigidbody = AttachedObject.GetOWRigidbody();
		if (sectorRigidbody && rigidbody)
		{
			var relativeVelocity = rigidbody.GetVelocity() - sectorRigidbody.GetPointVelocity(rigidbody.GetPosition());
			return relativeVelocity.sqrMagnitude;
		}

		return 0;
	}
}
