﻿using Cysharp.Threading.Tasks;
using System;

namespace QSB.Utility
{
	public static class Delay
	{
		public static void RunNextFrame(Action action) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate);
			action();
		});

		public static void RunFramesLater(int n, Action action) => UniTask.Create(async () =>
		{
			await UniTask.DelayFrame(n, PlayerLoopTiming.LastPostLateUpdate);
			action();
		});

		public static void RunWhen(Func<bool> predicate, Action action) => UniTask.Create(async () =>
		{
			await UniTask.WaitUntil(predicate, PlayerLoopTiming.LastPostLateUpdate);
			action();
		});
	}
}
