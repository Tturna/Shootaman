using System;
using System.Collections;
using UnityEngine;

namespace Tturna.Utility
{
    public static class Tt_Helpers
    {
        /// <summary>
        /// Executes a given Action after given seconds. Remember to call using StartCoroutine()
        /// </summary>
        /// <param name="method"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IEnumerator DelayExecute(Action method, float delay)
        {
            yield return new WaitForSeconds(delay);
            method();
        }

        public static Camera MainCamera => Camera.main;

        /// <summary>
        /// Destroys a target UnityEngine.Object after given seconds. Remember to call using StartCoroutine()
        /// </summary>
        /// <param name="target"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IEnumerator DelayDestroy(UnityEngine.Object target, float delay)
        {
            yield return new WaitForSeconds(delay);
            UnityEngine.Object.Destroy(target);
        }

        public static IEnumerator ExecuteOverTime(Action<float> method, float delay, float duration)
        {
            yield return new WaitForSeconds(delay);

            float timer = duration;
            while (timer > 0)
            {
                method(timer);
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
