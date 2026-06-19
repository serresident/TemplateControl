using Xunit;

namespace TemplateControl.Tests;

/// <summary>
/// Tests for <see cref="NumericPad"/> — covers property defaults,
/// value coerce, and event contract.
/// </summary>
[Collection("Avalonia")]
public class NumericPadTests
{
    [Fact]
    public void Constructor_SetsExpectedDefaults()
    {
        var pad = new NumericPad();

        Assert.Null(pad.Value);
        Assert.Equal(0m,               pad.Minimum);
        Assert.Equal(decimal.MaxValue, pad.Maximum);
        Assert.Equal(10,               pad.MaxLength);
        Assert.True(pad.ShowDecimalSeparator);
    }

    [Fact]
    public void Value_AcceptsNull()
    {
        var pad = new NumericPad { Value = 42m };
        pad.Value = null;
        Assert.Null(pad.Value);
    }

    [Fact]
    public void Minimum_CanBeSetToPositiveValue()
    {
        var pad = new NumericPad { Minimum = 5m };
        Assert.Equal(5m, pad.Minimum);
    }

    [Fact]
    public void Maximum_CanBeSetToArbitraryValue()
    {
        var pad = new NumericPad { Maximum = 999m };
        Assert.Equal(999m, pad.Maximum);
    }

    [Fact]
    public void ValueChanged_Fires_WhenValueChanges()
    {
        var pad = new NumericPad();
        bool fired = false;
        pad.ValueChanged += (_, _) => fired = true;

        pad.Value = 7m;

        Assert.True(fired);
    }

    [Fact]
    public void ValueChanged_Fires_WithCorrectOldAndNewValues()
    {
        var pad = new NumericPad { Value = 5m };
        decimal? capturedOld = null, capturedNew = null;
        pad.ValueChanged += (_, e) => { capturedOld = e.OldValue; capturedNew = e.NewValue; };

        pad.Value = 9m;

        Assert.Equal(5m, capturedOld);
        Assert.Equal(9m, capturedNew);
    }

    [Fact]
    public void ShowDecimalSeparator_CanBeDisabled()
    {
        var pad = new NumericPad { ShowDecimalSeparator = false };
        Assert.False(pad.ShowDecimalSeparator);
    }

    [Fact]
    public void MaxLength_CanBeChanged()
    {
        var pad = new NumericPad { MaxLength = 5 };
        Assert.Equal(5, pad.MaxLength);
    }
}
