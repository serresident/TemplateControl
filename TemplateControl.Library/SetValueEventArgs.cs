using System;
using Avalonia.Interactivity;

namespace TemplateControl
{
    /// <summary>
    /// Event arguments for a critical change request.
    /// Fired when the requested value is outside safe limits or the jump is too large (>20%).
    /// </summary>
    public class CriticalChangeEventArgs : RoutedEventArgs
    {
        public decimal ProposedValue { get; }
        public decimal CurrentValue { get; }
        public double ChangePercent { get; }

        public CriticalChangeEventArgs(RoutedEvent routedEvent, decimal proposedValue, decimal currentValue, double changePercent)
            : base(routedEvent)
        {
            ProposedValue = proposedValue;
            CurrentValue = currentValue;
            ChangePercent = changePercent;
        }
    }
}
