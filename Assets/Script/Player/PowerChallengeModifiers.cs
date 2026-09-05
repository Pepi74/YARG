using System;

namespace YARG.Player
{
    [Flags]
    public enum PowerChallengeModifiers
    {
        None               = 0,
        SpeedFreak         = 1 << 0, // Multiplier starts at 2x and advances every 5 notes instead of 10; sustaining 3x or higher (base, before Star Power) earns up to 5 bonus stars on top of the score-based total.
        StarPowerGenerator = 1 << 1, // Passively grants 10% of a full Star Power bar every 10-note streak.
        CrowdHyper         = 1 << 2, // Not implemented yet.
        StarPowerAmplifier = 1 << 3, // Fills the entire Star Power bar from a single phrase (100%, up from the default 25%).
        StarPowerNova      = 1 << 4, // Increases the Star Power score multiplier to x6.
        MultiplierExtender = 1 << 5, // Raises the max score multiplier (BaseMaxMultiplier + 2).
        StreakGuardian     = 1 << 6, // Not implemented yet.
        Resurrector        = 1 << 7, // Not implemented yet.
    }
}