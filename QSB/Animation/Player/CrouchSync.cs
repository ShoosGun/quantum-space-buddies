﻿using Mirror;
using QSB.Utility.VariableSync;
using UnityEngine;

namespace QSB.Animation.Player;

public class CrouchSync : NetworkBehaviour
{
	public AnimFloatParam CrouchParam { get; } = new AnimFloatParam();

	private const float CrouchSmoothTime = 0.05f;
	public const int CrouchLayerIndex = 1;

	private PlayerCharacterController _playerController;
	private Animator _bodyAnim;

	public FloatVariableSyncer CrouchVariableSyncer;

	public void Init(PlayerCharacterController playerController, Animator bodyAnim)
	{
		_playerController = playerController;
		_bodyAnim = bodyAnim;
	}

	public void Update()
	{
		if (isLocalPlayer)
		{
			SyncLocalCrouch();
			return;
		}

		SyncRemoteCrouch();
	}

	private void SyncLocalCrouch()
	{
		if (_playerController == null)
		{
			return;
		}

		var jumpChargeFraction = _playerController.GetJumpCrouchFraction();
		CrouchVariableSyncer.Value = jumpChargeFraction;
	}

	private void SyncRemoteCrouch()
	{
		if (_bodyAnim == null)
		{
			return;
		}

		CrouchParam.Target = CrouchVariableSyncer.Value;
		CrouchParam.Smooth(CrouchSmoothTime);
		var jumpChargeFraction = CrouchParam.Current;
		_bodyAnim.SetLayerWeight(CrouchLayerIndex, jumpChargeFraction);
	}
}