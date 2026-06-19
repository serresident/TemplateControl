using Xunit;

namespace TemplateControl.Tests;

/// <summary>
/// Unit and integration tests for <see cref="SetValueControl"/>.
///
/// Coverage areas (aligned with IEC 61511 HMI quality expectations):
///   1.  Property defaults
///   2.  Value / Minimum / Maximum coerce (safety-critical: value must NEVER leave [Min,Max])
///   3.  Safe-zone coerce
///   4.  ValueChanged event contract
///   5.  TryProposeValue — safe path (no overlay)
///   6.  TryProposeValue — bounds rejection
///   7.  TryProposeValue — safe-zone warning
///   8.  TryProposeValue — critical jump warning (&gt;20 %)
///   9.  DisplayMode.Minimal bypass
///  10.  CriticalChangeEventArgs data integrity
///  11.  RecommendedValue coerce
/// </summary>
[Collection("Avalonia")]
public class SetValueControlTests
{
    // ═══════════════════════════════════════════════════════════════════════
    // 1. Property Defaults
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_SetsExpectedDefaults()
    {
        var ctrl = new SetValueControl();

        Assert.Equal(0m,   ctrl.Value);
        Assert.Equal(0m,   ctrl.CurrentValue);
        Assert.Equal(0m,   ctrl.Minimum);
        Assert.Equal(100m, ctrl.Maximum);
        Assert.Equal(2,    ctrl.DecimalPlaces);
        Assert.True(ctrl.SafeZonesEnabled);
        Assert.True(ctrl.CriticalJumpWarningEnabled);
        Assert.Equal(SetValueDisplayMode.Full, ctrl.DisplayMode);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. Value Coerce — safety-critical
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Value_ClampedToMinimum_WhenSetBelowRange()
    {
        var ctrl = new SetValueControl { Minimum = 10m, Maximum = 100m };
        ctrl.Value = -5m;
        Assert.Equal(10m, ctrl.Value);
    }

    [Fact]
    public void Value_ClampedToMaximum_WhenSetAboveRange()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m };
        ctrl.Value = 999m;
        Assert.Equal(100m, ctrl.Value);
    }

    [Fact]
    public void Value_AcceptsExactBoundaryValues()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m };

        ctrl.Value = 0m;
        Assert.Equal(0m, ctrl.Value);

        ctrl.Value = 100m;
        Assert.Equal(100m, ctrl.Value);
    }

    [Fact]
    public void Value_ClampedCorrectly_WithNegativeRange()
    {
        var ctrl = new SetValueControl { Minimum = -50m, Maximum = 50m };

        ctrl.Value = -100m;
        Assert.Equal(-50m, ctrl.Value);

        ctrl.Value = 100m;
        Assert.Equal(50m, ctrl.Value);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. Minimum / Maximum Coerce
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void Minimum_CoercedToMaximum_WhenSetAboveMaximum()
    {
        var ctrl = new SetValueControl { Maximum = 50m };
        ctrl.Minimum = 200m;
        Assert.Equal(50m, ctrl.Minimum);
    }

    [Fact]
    public void Maximum_CoercedToMinimum_WhenSetBelowMinimum()
    {
        var ctrl = new SetValueControl { Minimum = 30m };
        ctrl.Maximum = 10m;
        Assert.Equal(30m, ctrl.Maximum);
    }

    [Fact]
    public void Value_ReclampsAfterMaximumIsLowered()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m };
        ctrl.Value = 90m;
        ctrl.Maximum = 50m;
        Assert.True(ctrl.Value <= 50m, $"Value {ctrl.Value} must not exceed new Maximum 50");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 4. Safe Zone Coerce
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void SafeMinimum_ClampedToMinimum_WhenSetBelowMinimum()
    {
        var ctrl = new SetValueControl { Minimum = 20m, Maximum = 100m };
        ctrl.SafeMinimum = 5m;
        Assert.Equal(20m, ctrl.SafeMinimum);
    }

    [Fact]
    public void SafeMaximum_ClampedToMaximum_WhenSetAboveMaximum()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m };
        ctrl.SafeMaximum = 200m;
        Assert.Equal(100m, ctrl.SafeMaximum);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 5. ValueChanged event contract
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void ValueChanged_Fires_WithCorrectOldAndNewValues()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m, Value = 20m };
        decimal capturedOld = -1m, capturedNew = -1m;
        ctrl.ValueChanged += (_, e) =>
        {
            capturedOld = e.OldValue ?? -1m;
            capturedNew = e.NewValue ?? -1m;
        };

        ctrl.Value = 55m;

        Assert.Equal(20m, capturedOld);
        Assert.Equal(55m, capturedNew);
    }

    [Fact]
    public void ValueChanged_DoesNotFire_WhenValueIsUnchanged()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m, Value = 50m };
        int fired = 0;
        ctrl.ValueChanged += (_, _) => fired++;

        ctrl.Value = 50m;
        Assert.Equal(0, fired);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 6. TryProposeValue — safe path
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void TryProposeValue_SafeValue_AppliesAndFiresValueChanged()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 100m,
            SafeMinimum = 10m, SafeMaximum = 90m,
            CriticalJumpWarningEnabled = false,
            CurrentValue = 50m, Value = 50m
        };
        decimal? applied = null;
        ctrl.ValueChanged += (_, e) => applied = e.NewValue;

        ctrl.TryProposeValue(60m);

        Assert.Equal(60m, applied);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 7. TryProposeValue — bounds rejection
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void TryProposeValue_BelowMinimum_DoesNotChangeValue()
    {
        var ctrl = new SetValueControl { Minimum = 10m, Maximum = 100m, Value = 50m };
        int fired = 0;
        ctrl.ValueChanged += (_, _) => fired++;

        ctrl.TryProposeValue(5m);

        Assert.Equal(0, fired);
        Assert.Equal(50m, ctrl.Value);
    }

    [Fact]
    public void TryProposeValue_AboveMaximum_DoesNotChangeValue()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m, Value = 50m };
        int fired = 0;
        ctrl.ValueChanged += (_, _) => fired++;

        ctrl.TryProposeValue(150m);

        Assert.Equal(0, fired);
        Assert.Equal(50m, ctrl.Value);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 8. TryProposeValue — safe-zone warning
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void TryProposeValue_OutsideSafeZone_RaisesCriticalChangeRequested()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 100m,
            SafeMinimum = 20m, SafeMaximum = 80m,
            SafeZonesEnabled = true,
            CriticalJumpWarningEnabled = false,
            CurrentValue = 50m, Value = 50m
        };
        bool warned = false;
        ctrl.CriticalChangeRequested += (_, _) => warned = true;

        ctrl.TryProposeValue(10m);

        Assert.True(warned, "CriticalChangeRequested must fire for out-of-safe-zone values");
    }

    [Fact]
    public void TryProposeValue_SafeZonesDisabled_NoWarning()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 100m,
            SafeMinimum = 20m, SafeMaximum = 80m,
            SafeZonesEnabled = false,
            CriticalJumpWarningEnabled = false,
            CurrentValue = 50m, Value = 50m
        };
        bool warned = false;
        ctrl.CriticalChangeRequested += (_, _) => warned = true;

        ctrl.TryProposeValue(5m);

        Assert.False(warned);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 9. TryProposeValue — critical jump warning (>20 %)
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void TryProposeValue_JumpOver20Percent_RaisesCriticalChangeRequested()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 1000m,
            SafeMinimum = 0m, SafeMaximum = 1000m,
            CriticalJumpWarningEnabled = true,
            CurrentValue = 100m, Value = 100m
        };
        bool warned = false;
        ctrl.CriticalChangeRequested += (_, _) => warned = true;

        ctrl.TryProposeValue(130m); // +30 %

        Assert.True(warned, "CriticalChangeRequested must fire for jumps > 20 %");
    }

    [Fact]
    public void TryProposeValue_JumpUnder20Percent_NoWarning()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 1000m,
            SafeMinimum = 0m, SafeMaximum = 1000m,
            CriticalJumpWarningEnabled = true,
            CurrentValue = 100m, Value = 100m
        };
        bool warned = false;
        ctrl.CriticalChangeRequested += (_, _) => warned = true;

        ctrl.TryProposeValue(115m); // +15 %

        Assert.False(warned);
    }

    [Fact]
    public void TryProposeValue_FromZeroCurrentValue_IsAlwaysBigJump()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 1000m,
            SafeMinimum = 0m, SafeMaximum = 1000m,
            CriticalJumpWarningEnabled = true,
            CurrentValue = 0m, Value = 0m
        };
        bool warned = false;
        ctrl.CriticalChangeRequested += (_, _) => warned = true;

        ctrl.TryProposeValue(1m);

        Assert.True(warned, "Any change from CurrentValue=0 must trigger critical warning");
    }

    [Fact]
    public void TryProposeValue_JumpWarningDisabled_AlwaysApplies()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 1000m,
            SafeMinimum = 0m, SafeMaximum = 1000m,
            CriticalJumpWarningEnabled = false,
            CurrentValue = 10m, Value = 10m
        };
        decimal? applied = null;
        ctrl.ValueChanged += (_, e) => applied = e.NewValue;

        ctrl.TryProposeValue(900m);

        Assert.Equal(900m, applied);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 10. DisplayMode.Minimal — bypasses ALL safety guards
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void TryProposeValue_InMinimalMode_BypassesSafeZoneAndJumpGuards()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 100m,
            SafeMinimum = 40m, SafeMaximum = 60m,
            SafeZonesEnabled = true,
            CriticalJumpWarningEnabled = true,
            CurrentValue = 50m, Value = 50m,
            DisplayMode = SetValueDisplayMode.Minimal
        };
        bool warned = false;
        decimal? applied = null;
        ctrl.CriticalChangeRequested += (_, _) => warned = true;
        ctrl.ValueChanged += (_, e) => applied = e.NewValue;

        ctrl.TryProposeValue(5m);

        Assert.False(warned, "Minimal mode must bypass all safety warnings");
        Assert.Equal(5m, applied);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 11. CriticalChangeEventArgs data integrity
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void CriticalChangeRequested_EventArgs_ContainCorrectProposedAndCurrentValues()
    {
        var ctrl = new SetValueControl
        {
            Minimum = 0m, Maximum = 1000m,
            SafeMinimum = 0m, SafeMaximum = 1000m,
            CriticalJumpWarningEnabled = true,
            CurrentValue = 100m, Value = 100m
        };
        CriticalChangeEventArgs? captured = null;
        ctrl.CriticalChangeRequested += (_, e) => captured = e;

        ctrl.TryProposeValue(200m); // +100 %

        Assert.NotNull(captured);
        Assert.Equal(200m, captured!.ProposedValue);
        Assert.Equal(100m, captured.CurrentValue);
        Assert.True(captured.ChangePercent > 0.99,
            $"ChangePercent should be ~1.0 (100 %), got {captured.ChangePercent:P1}");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 12. RecommendedValue coerce
    // ═══════════════════════════════════════════════════════════════════════

    [Fact]
    public void RecommendedValue_ClampedToRange()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m };

        ctrl.RecommendedValue = 150m;
        Assert.Equal(100m, ctrl.RecommendedValue); // clamped to Maximum

        ctrl.RecommendedValue = -10m;
        Assert.Equal(0m, ctrl.RecommendedValue); // clamped to Minimum
    }

    [Fact]
    public void RecommendedValue_ReClampedWhenMaximumIsLowered()
    {
        var ctrl = new SetValueControl { Minimum = 0m, Maximum = 100m };
        ctrl.RecommendedValue = 80m;
        ctrl.Maximum = 50m;
        Assert.True(ctrl.RecommendedValue <= 50m,
            $"RecommendedValue {ctrl.RecommendedValue} must not exceed new Maximum 50");
    }

    [Fact]
    public void RecommendedValue_IsNullByDefault()
    {
        var ctrl = new SetValueControl();
        Assert.Null(ctrl.RecommendedValue);
    }
}
