using System;
using YARG.Core;
using YARG.Core.Engine;
using YARG.Core.Game;
using YARG.Core.Input;
using YARG.Core.Replays;
using YARG.Input;
using YARG.Settings.Customization;
using YARG.Themes;

namespace YARG.Player
{
    public class YargPlayer : IDisposable
    {
        public event MenuInputEvent MenuInput;

        public YargProfile Profile { get; private set; }

        /// <summary>
        /// Whether or not the player is sitting out. This is not needed in <see cref="Profile"/> as
        /// players that are sitting out are not included in replays.
        /// </summary>
        public bool SittingOut;

        public bool InputsEnabled { get; private set; }
        public ProfileBindings Bindings { get; private set; }

        public EnginePreset    EnginePreset    { get; private set; }
        public ThemePreset     ThemePreset     { get; private set; }
        public ColorProfile    ColorProfile    { get; private set; }
        public CameraPreset    CameraPreset    { get; private set; }
        public HighwayPreset   HighwayPreset   { get; private set; }
        public RockMeterPreset RockMeterPreset { get; private set; }

        public PowerChallengeModifiers ActivePowers { get; set; } = PowerChallengeModifiers.None;

        /// <summary>
        /// Engine tuning values derived from this player's active Power Challenge modifiers.
        /// Falls back to standard (no-power) values outside of Power Challenge.
        /// </summary>
        public (int MaxMultiplierBonus, int StarPowerMultiplier, int StarPowerPhraseGainPercent, int StarPowerGeneratorStreakPercent, int NotesPerMultiplierIncrease, int BaseMultiplierOffset, int SpeedFreakBonusThreshold, double SpeedFreakBonusSongLength, int StreakGuardianMaxShields) GetPowerChallengeEngineOptions(double songLength)
        {
            if (!GlobalVariables.State.IsPowerChallenge)
            {
                return (0, 2, 25, 0, 10, 1, 0, 0, 0);
            }

            bool speedFreak = ActivePowers.HasFlag(PowerChallengeModifiers.SpeedFreak);

            return (
                ActivePowers.HasFlag(PowerChallengeModifiers.MultiplierExtender) ? 2 : 0,
                ActivePowers.HasFlag(PowerChallengeModifiers.StarPowerNova) ? 6 : 2,
                ActivePowers.HasFlag(PowerChallengeModifiers.StarPowerAmplifier) ? 100 : 25,
                ActivePowers.HasFlag(PowerChallengeModifiers.StarPowerGenerator) ? 10 : 0,
                speedFreak ? 5 : 10,
                speedFreak ? 2 : 1,
                speedFreak ? 3 : 0,
                speedFreak ? songLength : 0,
                ActivePowers.HasFlag(PowerChallengeModifiers.StreakGuardian) ? 2 : 0
            );
        }

        public const int POWER_CHALLENGE_MAX_STARS = 21;
        public const int ALL_POWERFUL_MAX_STARS = 31; // 21 + 5 stars of Speed Freak + 5 stars of Streak Guardian.
        /// <summary>
        /// Fraction of the original 4->5 step used for each of the extended range's flat steps (stars 6-20).
        /// </summary>
        private const float EXTENDED_RANGE_STEP_FRACTION = 0.9f;

        /// <summary>
        /// Extends a 6-star threshold curve up to <see cref="POWER_CHALLENGE_MAX_STARS"/> stars for Power Challenge. Stars 1-5 keep the exact original (quickplay) thresholds. Stars 6-20 each cost a flat <see cref="EXTENDED_RANGE_STEP_FRACTION"/> of the original 4->5 step, and the final star (21) is a deliberate hard push using the full original 5->6 ("gold star") step.
        /// </summary>
        public static float[] GetStarMultiplierThresholds(float[] baseThresholds)
        {
            if (!GlobalVariables.State.IsPowerChallenge)
            {
                return baseThresholds;
            }

            var extended = new float[POWER_CHALLENGE_MAX_STARS];
            Array.Copy(baseThresholds, extended, baseThresholds.Length - 1);

            float step45 = baseThresholds[^2] - baseThresholds[^3];
            float step56 = baseThresholds[^1] - baseThresholds[^2];
            float extendedStep = step45 * EXTENDED_RANGE_STEP_FRACTION;

            int rampStart = baseThresholds.Length - 1;   // first extended index (star 6)
            int lastIndex = POWER_CHALLENGE_MAX_STARS - 1;

            // Stars 6-20 each cost a fraction of the original 4->5 step.
            for (int i = rampStart; i < lastIndex; i++)
            {
                extended[i] = extended[i - 1] + extendedStep;
            }

            // Final star: a deliberate hard push, same size as the original 5->6 ("gold star") jump.
            extended[lastIndex] = extended[lastIndex - 1] + step56;

            return extended;
        }

        /// <summary>
        /// Whether or not the score is valid.
        /// </summary>
        /// <remarks>Could be invalidated due abusing pauses or no fail mode.</remarks>
        public bool IsScoreValid { get; set; } = true;

        public bool IsReplay { get; private set; }
        public int ReplayIndex = -1;

        /// <summary>
        /// Overrides the engine parameters in the gameplay player.
        /// This is only used when loading replays.
        /// </summary>
        public BaseEngineParameters EngineParameterOverride { get; set; }

        public bool IsMissingMicrophone => !IsReplay && Profile.GameMode == GameMode.Vocals && Bindings.Microphone == null && !Profile.IsBot;
        public bool IsMissingInputDevice => !IsReplay && Profile.GameMode != GameMode.Vocals && !Bindings.HasDeviceAssigned && !Profile.IsBot;

        public YargPlayer(YargProfile profile, ProfileBindings bindings)
        {
            Profile = profile;
            Bindings = bindings;
            IsReplay = false;
        }

        public YargPlayer(ReplayFrame frame, ReplayData replay)
        {
            Profile = frame.Profile;
            Bindings = null;
            EngineParameterOverride = frame.EngineParameters;
            IsReplay = true;

            EnginePreset = CustomContentManager.EnginePresets.GetPresetById(Profile.EnginePreset)
                ?? EnginePreset.Default;
            ThemePreset = CustomContentManager.ThemePresets.GetPresetById(Profile.ThemePreset)
                ?? ThemePreset.Default;
            ColorProfile = replay.GetColorProfile(Profile.ColorProfile)
                ?? CustomContentManager.ColorProfiles.GetPresetById(Profile.ColorProfile)
                ?? ColorProfile.Default;
            CameraPreset = replay.GetCameraPreset(Profile.CameraPreset)
                ?? CustomContentManager.CameraSettings.GetPresetById(Profile.CameraPreset)
                ?? CameraPreset.Default;

            HighwayPreset = CustomContentManager.HighwayPresets.GetPresetById(Profile.HighwayPreset)
                ?? HighwayPreset.Default;

            RockMeterPreset = replay.GetRockMeterPreset(Profile.RockMeterPreset)
                ?? CustomContentManager.RockMeterPresets.GetPresetById(Profile.RockMeterPreset)
                ?? RockMeterPreset.Normal;
        }

        public void SwapToProfile(YargProfile profile, ProfileBindings bindings, bool resolveDevices)
        {
            // Force-disable inputs
            bool enabled = InputsEnabled;
            DisableInputs();

            // Swap to the new profile
            Bindings?.Dispose();
            Profile = profile;
            Bindings = bindings;

            // Resolve bindings
            if (resolveDevices)
            {
                Bindings?.ResolveDevices();
            }

            // Re-enable inputs
            if (enabled)
            {
                EnableInputs();
            }
        }

        public void RefreshPresets()
        {
            EnginePreset = CustomContentManager.EnginePresets.GetPresetById(Profile.EnginePreset)
                ?? EnginePreset.Default;
            Profile.EnginePreset = EnginePreset.Id;
            ThemePreset = CustomContentManager.ThemePresets.GetPresetById(Profile.ThemePreset)
                ?? ThemePreset.Default;
            Profile.ThemePreset = ThemePreset.Id;
            ColorProfile = CustomContentManager.ColorProfiles.GetPresetById(Profile.ColorProfile)
                ?? ColorProfile.Default;
            Profile.ColorProfile = ColorProfile.Id;
            CameraPreset = CustomContentManager.CameraSettings.GetPresetById(Profile.CameraPreset)
                ?? CameraPreset.Default;
            Profile.CameraPreset = CameraPreset.Id;
            HighwayPreset = CustomContentManager.HighwayPresets.GetPresetById(Profile.HighwayPreset)
                ?? HighwayPreset.Default;
            Profile.HighwayPreset = HighwayPreset.Id;
            RockMeterPreset = CustomContentManager.RockMeterPresets.GetPresetById(Profile.RockMeterPreset) ??
                RockMeterPreset.Normal;
            Profile.RockMeterPreset = RockMeterPreset.Id;

        }

        public void EnableInputs()
        {
            if (InputsEnabled || Bindings == null)
            {
                return;
            }

            Bindings.EnableInputs();
            Bindings.MenuInputProcessed += OnMenuInput;
            InputManager.RegisterPlayer(this);

            InputsEnabled = true;
        }

        public void DisableInputs()
        {
            if (!InputsEnabled || Bindings == null)
            {
                return;
            }

            Bindings.DisableInputs();
            Bindings.MenuInputProcessed -= OnMenuInput;
            InputManager.UnregisterPlayer(this);

            InputsEnabled = false;
        }

        private void OnMenuInput(ref GameInput input)
        {
            MenuInput?.Invoke(this, ref input);
        }

        public void Dispose()
        {
            DisableInputs();
            Bindings?.Dispose();
        }
    }
}