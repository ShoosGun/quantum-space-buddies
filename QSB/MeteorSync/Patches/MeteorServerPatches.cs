﻿using System;
using HarmonyLib;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QSB.MeteorSync.Patches
{
	public class MeteorServerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
		public static bool FixedUpdate(MeteorLauncher __instance)
		{
			if (__instance._launchedMeteors != null)
			{
				for (var i = __instance._launchedMeteors.Count - 1; i >= 0; i--)
				{
					if (__instance._launchedMeteors[i] == null)
					{
						__instance._launchedMeteors.QuickRemoveAt(i);
					}
					else if (__instance._launchedMeteors[i].isSuspended)
					{
						__instance._meteorPool.Add(__instance._launchedMeteors[i]);
						__instance._launchedMeteors.QuickRemoveAt(i);
					}
				}
			}
			if (__instance._launchedDynamicMeteors != null)
			{
				for (var j = __instance._launchedDynamicMeteors.Count - 1; j >= 0; j--)
				{
					if (__instance._launchedDynamicMeteors[j] == null)
					{
						__instance._launchedDynamicMeteors.QuickRemoveAt(j);
					}
					else if (__instance._launchedDynamicMeteors[j].isSuspended)
					{
						__instance._dynamicMeteorPool.Add(__instance._launchedDynamicMeteors[j]);
						__instance._launchedDynamicMeteors.QuickRemoveAt(j);
					}
				}
			}
			if (__instance._initialized && Time.time > __instance._lastLaunchTime + __instance._launchDelay)
			{
				if (!__instance._areParticlesPlaying)
				{
					__instance._areParticlesPlaying = true;
					foreach (var particleSystem in __instance._launchParticles)
					{
						particleSystem.Play();
					}

					var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
					QSBEventManager.FireEvent(EventNames.QSBMeteorPreLaunch, qsbMeteorLauncher.ObjectId);
					DebugLog.DebugWrite($"{qsbMeteorLauncher.LogName} - pre launch");
				}
				if (Time.time > __instance._lastLaunchTime + __instance._launchDelay + 2.3f)
				{
					__instance.LaunchMeteor();
					__instance._lastLaunchTime = Time.time;
					__instance._launchDelay = Random.Range(__instance._minInterval, __instance._maxInterval);
					__instance._areParticlesPlaying = false;
					foreach (var particleSystem in __instance._launchParticles)
					{
						particleSystem.Stop();
					}
				}
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
		public static bool LaunchMeteor(MeteorLauncher __instance)
		{
			var flag = __instance._dynamicMeteorPool != null && (__instance._meteorPool == null || Random.value < __instance._dynamicProbability);
			MeteorController meteorController = null;
			var poolIndex = 0;
			if (!flag)
			{
				if (__instance._meteorPool.Count == 0)
				{
					Debug.LogWarning("MeteorLauncher is out of Meteors!", __instance);
				}
				else
				{
					poolIndex = __instance._meteorPool.Count - 1;
					meteorController = __instance._meteorPool[poolIndex];
					meteorController.Initialize(__instance.transform, __instance._detectableField, __instance._detectableFluid);
					__instance._meteorPool.QuickRemoveAt(poolIndex);
					__instance._launchedMeteors.Add(meteorController);
				}
			}
			else if (__instance._dynamicMeteorPool.Count == 0)
			{
				Debug.LogWarning("MeteorLauncher is out of Dynamic Meteors!", __instance);
			}
			else
			{
				poolIndex = __instance._dynamicMeteorPool.Count - 1;
				meteorController = __instance._dynamicMeteorPool[poolIndex];
				meteorController.Initialize(__instance.transform, null, null);
				__instance._dynamicMeteorPool.QuickRemoveAt(poolIndex);
				__instance._launchedDynamicMeteors.Add(meteorController);
			}
			if (meteorController != null)
			{
				var launchSpeed = Random.Range(__instance._minLaunchSpeed, __instance._maxLaunchSpeed);
				var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) + __instance.transform.TransformDirection(__instance._launchDirection) * launchSpeed;
				var angularVelocity = __instance.transform.forward * 2f;
				meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity, angularVelocity);
				if (__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
				{
					__instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
					__instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
				}

				var qsbMeteorLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
				QSBEventManager.FireEvent(EventNames.QSBMeteorLaunch, qsbMeteorLauncher.ObjectId, flag, poolIndex, launchSpeed);
				DebugLog.DebugWrite($"{qsbMeteorLauncher.LogName} - launch {flag} {poolIndex} {launchSpeed}");
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
		public static bool Impact(MeteorController __instance,
			GameObject hitObject, Vector3 impactPoint, Vector3 impactVel)
		{
			var componentInParent = hitObject.GetComponentInParent<FragmentIntegrity>();
			var damage = Random.Range(__instance._minDamage, __instance._maxDamage);
			if (componentInParent != null)
			{
				if (!componentInParent.GetIgnoreMeteorDamage())
				{
					componentInParent.AddDamage(damage);
				}
				else if (componentInParent.GetParentFragment() != null && !componentInParent.GetParentFragment().GetIgnoreMeteorDamage())
				{
					componentInParent.GetParentFragment().AddDamage(damage);
				}
			}
			MeteorImpactMapper.RecordImpact(impactPoint, componentInParent);
			__instance._intactRenderer.enabled = false;
			__instance._impactLight.enabled = true;
			__instance._impactLight.intensity = __instance._impactLightCurve.Evaluate(0f);
			var rotation = Quaternion.LookRotation(impactVel);
			foreach (var particleSystem in __instance._impactParticles)
			{
				particleSystem.transform.rotation = rotation;
				particleSystem.Play();
			}
			__instance._impactSource.PlayOneShot(AudioType.BH_MeteorImpact);
			foreach (var owCollider in __instance._owColliders)
			{
				owCollider.SetActivation(false);
			}
			__instance._owRigidbody.MakeKinematic();
			__instance.transform.SetParent(hitObject.GetAttachedOWRigidbody().transform);
			FragmentSurfaceProxy.UntrackMeteor(__instance);
			FragmentCollisionProxy.UntrackMeteor(__instance);
			__instance._ignoringCollisions = false;
			__instance._hasImpacted = true;
			__instance._impactTime = Time.time;

			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);
			impactPoint = Locator._brittleHollow.transform.InverseTransformPoint(impactPoint);
			QSBEventManager.FireEvent(EventNames.QSBMeteorImpact, qsbMeteor.ObjectId, impactPoint, damage);
			DebugLog.DebugWrite($"{qsbMeteor.LogName} - impact! {hitObject.name} {impactPoint} {impactVel} {damage}");

			return false;
		}


		[HarmonyPostfix]
		[HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Suspend), new Type[0])]
		public static void Suspend(MeteorController __instance)
		{
			if (!MeteorManager.MeteorsReady)
			{
				return;
			}

			var qsbMeteor = QSBWorldSync.GetWorldFromUnity<QSBMeteor>(__instance);
			DebugLog.DebugWrite($"{qsbMeteor.LogName} - suspended");
		}
	}
}
