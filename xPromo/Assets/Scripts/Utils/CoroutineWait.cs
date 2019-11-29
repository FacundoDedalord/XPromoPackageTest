using System;
using System.Collections;
using UnityEngine;
/// Util class for creating simple corroutines
/// Requires a monobehaviour to use it.
public static class CoroutineWait
{
    public delegate bool BoolEvaluation();
    
    /// Wait for X seconds before calling the given action
    public static Coroutine ForSeconds(MonoBehaviour owner, float seconds, Action action = null) {
        if(seconds < 0) {
            seconds = 0;
        }
		return owner.StartCoroutine(CorroutineWaitForSeconds(seconds, action));
	}

    /// Wait for the given evaluation to become true before continuing
    public static Coroutine UntilTrue(MonoBehaviour owner, BoolEvaluation eval) {
		return owner.StartCoroutine(CorroutineWaitUntilTrue(eval));
	}

	private static IEnumerator CorroutineWaitForSeconds(float seconds, Action action) {
		yield return new WaitForSeconds(seconds);
		action?.Invoke();
	}
    private static IEnumerator CorroutineWaitUntilTrue(BoolEvaluation eval) {
        while(!eval())
        {
            yield return null;
        }
	}
}