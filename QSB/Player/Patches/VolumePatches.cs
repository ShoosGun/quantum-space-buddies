﻿using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.Player.Patches;

internal class VolumePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FluidVolume), nameof(FluidVolume.OnEffectVolumeEnter))]
	public static void OnEffectVolumeEnter(FluidVolume __instance, GameObject hitObj)
	{
		var comp = hitObj.GetComponent<RemotePlayerFluidDetector>();
		if (comp != null)
		{
			comp.AddVolume(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FluidVolume), nameof(FluidVolume.OnEffectVolumeExit))]
	public static void OnEffectVolumeExit(FluidVolume __instance, GameObject hitObj)
	{
		var comp = hitObj.GetComponent<RemotePlayerFluidDetector>();
		if (comp != null)
		{
			comp.RemoveVolume(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RingRiverFluidVolume), nameof(RingRiverFluidVolume.OnEffectVolumeEnter))]
	public static void OnEffectVolumeEnter(RingRiverFluidVolume __instance, GameObject hitObj)
	{
		var comp = hitObj.GetComponent<RemotePlayerFluidDetector>();
		if (comp != null)
		{
			comp.AddVolume(__instance);
		}
	}
}
