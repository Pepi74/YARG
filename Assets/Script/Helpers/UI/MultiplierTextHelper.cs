using Cysharp.Text;
using TMPro;

namespace YARG.Helpers.UI
{
    public static class MultiplierTextHelper
    {
        /// <summary>
        /// Create a TMP cache of all available multiplier texts.
        /// </summary>
        /// <param name="maxMultiplier">The max multiplier to generate WITHOUT Star Power.</param>
        /// <param name="multiplierTextPrefab">The prefab of the text to instantiate.</param>
        /// <param name="isMultiplayer"> Whether we are playing multiplayer (and therefore do not need to generate SP multipliers).</param>
        /// <typeparam name="T">TextMeshPro type being used</typeparam>
        /// <returns>Array where the corresponding TMP object is at arr[multiplier - 2]</returns>
        public static T[] CreateMultiplierTextCache<T>(int maxMultiplier, T multiplierTextPrefab,
            bool isMultiplayer, int starPowerMultiplier = 2) where T : TMP_Text
        {
            var textCache = isMultiplayer ? new T[maxMultiplier - 1] : new T[maxMultiplier * starPowerMultiplier - 1];
            for (int i = 2; i <= maxMultiplier; i++)
            {
                if (textCache[i - 2] == null)
                {
                    textCache[i - 2] = GenerateMultiplierText(i, multiplierTextPrefab);
                }
            }

            // Starts at i=1 (not 2) so the lowest combo tier's SP-multiplied value is always generated. Needed because Star Power Nova's multiplier can exceed maxMultiplier, breaking the old assumption that this value was always covered by the non-SP loop.
            if (!isMultiplayer)
            {
                for (int i = 1; i <= maxMultiplier; i++)
                {
                    // Also SP, but only in single-player as multiplayer uses band multipliers
                    if (textCache[i * starPowerMultiplier - 2] == null)
                    {
                        textCache[i * starPowerMultiplier - 2] = GenerateMultiplierText(i * starPowerMultiplier, multiplierTextPrefab);
                    }
                }
            }

            return textCache;
        }

        private static T GenerateMultiplierText<T>(int multiplier, T multiplierTextPrefab) where T : TMP_Text
        {
            var text = UnityEngine.Object.Instantiate(multiplierTextPrefab, multiplierTextPrefab.transform.parent);
            text.SetTextFormat("{0}<sub>x</sub>", multiplier);
            text.enabled = false;
            return text;
        }
    }
}