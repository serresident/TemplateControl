using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace TemplateControl
{
    /// <summary>
    /// An intelligent setpoint controller supporting adaptive step, safe zones,
    /// critical change confirmation, and numeric pad integration.
    /// Fully isolated from business logic per Codex guidelines.
    /// </summary>
    [TemplatePart("PART_NumPadPopup", typeof(Popup))]
    [TemplatePart("PART_QuickSetPanel", typeof(ItemsControl))]
    [TemplatePart("PART_Track", typeof(RangeBase))]
    [TemplatePart("PART_DecreaseBtn", typeof(Button))]
    [TemplatePart("PART_IncreaseBtn", typeof(Button))]
    [TemplatePart("PART_ConfirmOverlay", typeof(Border))]
    [TemplatePart("PART_ConfirmBtn", typeof(Button))]
    [TemplatePart("PART_CancelBtn", typeof(Button))]
    [TemplatePart("PART_NumPadToggle", typeof(Button))]
    [PseudoClasses(":numpad-open", ":warning", ":error", ":auto-precision", ":has-recommendation")]
    public class SetValueControl : TemplatedControl
    {
        #region Styled Properties

        public static readonly StyledProperty<decimal> ValueProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(Value),
                defaultValue: 0m,
                coerce: CoerceValue);

        public static readonly StyledProperty<decimal> CurrentValueProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(CurrentValue),
                defaultValue: 0m);

        public static readonly StyledProperty<decimal> MinimumProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(Minimum),
                defaultValue: 0m,
                coerce: CoerceMinimum);

        public static readonly StyledProperty<decimal> MaximumProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(Maximum),
                defaultValue: 100m,
                coerce: CoerceMaximum);

        public static readonly StyledProperty<decimal> SafeMinimumProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(SafeMinimum),
                defaultValue: 0m,
                coerce: CoerceSafeMinimum);

        public static readonly StyledProperty<decimal> SafeMaximumProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(SafeMaximum),
                defaultValue: 100m,
                coerce: CoerceSafeMaximum);

        public static readonly StyledProperty<decimal> StepProperty =
            AvaloniaProperty.Register<SetValueControl, decimal>(
                nameof(Step),
                defaultValue: 1m);

        public static readonly StyledProperty<bool> AdaptiveStepEnabledProperty =
            AvaloniaProperty.Register<SetValueControl, bool>(
                nameof(AdaptiveStepEnabled),
                defaultValue: false);

        public static readonly StyledProperty<decimal?> RecommendedValueProperty =
            AvaloniaProperty.Register<SetValueControl, decimal?>(
                nameof(RecommendedValue),
                defaultValue: null);

        public static readonly StyledProperty<IEnumerable<decimal>?> RecentValuesProperty =
            AvaloniaProperty.Register<SetValueControl, IEnumerable<decimal>?>(
                nameof(RecentValues),
                defaultValue: null);

        #endregion

        #region Routed Events

        public static readonly RoutedEvent<NumericValueChangedEventArgs> ValueChangedEvent =
            RoutedEvent.Register<SetValueControl, NumericValueChangedEventArgs>(
                nameof(ValueChanged), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<CriticalChangeEventArgs> CriticalChangeRequestedEvent =
            RoutedEvent.Register<SetValueControl, CriticalChangeEventArgs>(
                nameof(CriticalChangeRequested), RoutingStrategies.Bubble);

        public event EventHandler<NumericValueChangedEventArgs>? ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public event EventHandler<CriticalChangeEventArgs>? CriticalChangeRequested
        {
            add => AddHandler(CriticalChangeRequestedEvent, value);
            remove => RemoveHandler(CriticalChangeRequestedEvent, value);
        }

        #endregion

        #region Property Accessors

        public decimal Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public decimal CurrentValue
        {
            get => GetValue(CurrentValueProperty);
            set => SetValue(CurrentValueProperty, value);
        }

        public decimal Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public decimal Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public decimal SafeMinimum
        {
            get => GetValue(SafeMinimumProperty);
            set => SetValue(SafeMinimumProperty, value);
        }

        public decimal SafeMaximum
        {
            get => GetValue(SafeMaximumProperty);
            set => SetValue(SafeMaximumProperty, value);
        }

        public decimal Step
        {
            get => GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public bool AdaptiveStepEnabled
        {
            get => GetValue(AdaptiveStepEnabledProperty);
            set => SetValue(AdaptiveStepEnabledProperty, value);
        }

        public decimal? RecommendedValue
        {
            get => GetValue(RecommendedValueProperty);
            set => SetValue(RecommendedValueProperty, value);
        }

        public IEnumerable<decimal>? RecentValues
        {
            get => GetValue(RecentValuesProperty);
            set => SetValue(RecentValuesProperty, value);
        }

        #endregion

        #region Coerce Logic

        private static decimal CoerceValue(AvaloniaObject obj, decimal value)
        {
            if (obj is SetValueControl c)
            {
                if (value < c.Minimum) return c.Minimum;
                if (value > c.Maximum) return c.Maximum;
            }
            return value;
        }

        private static decimal CoerceMinimum(AvaloniaObject obj, decimal value)
        {
            if (obj is SetValueControl c && value > c.Maximum) return c.Maximum;
            return value;
        }

        private static decimal CoerceMaximum(AvaloniaObject obj, decimal value)
        {
            if (obj is SetValueControl c && value < c.Minimum) return c.Minimum;
            return value;
        }

        private static decimal CoerceSafeMinimum(AvaloniaObject obj, decimal value)
        {
            if (obj is SetValueControl c)
            {
                if (value < c.Minimum) return c.Minimum;
                if (value > c.Maximum) return c.Maximum;
            }
            return value;
        }

        private static decimal CoerceSafeMaximum(AvaloniaObject obj, decimal value)
        {
            if (obj is SetValueControl c)
            {
                if (value < c.Minimum) return c.Minimum;
                if (value > c.Maximum) return c.Maximum;
                if (value < c.SafeMinimum) return c.SafeMinimum;
            }
            return value;
        }

        #endregion

        #region Template Parts

        private Popup? _numPadPopup;
        private ItemsControl? _quickSetPanel;
        private RangeBase? _track;
        private Button? _decreaseBtn;
        private Button? _increaseBtn;
        private Border? _confirmOverlay;
        private Button? _confirmBtn;
        private Button? _cancelBtn;
        private Button? _numPadToggle;

        #endregion

        #region Internal State

        private decimal _pendingValue;
        private DispatcherTimer? _errorTimer;
        private bool _isSliderUpdating;

        #endregion

        public SetValueControl()
        {
            UpdatePseudoClasses();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            UnsubscribeTemplateParts();

            base.OnApplyTemplate(e);

            _numPadPopup = e.NameScope.Find<Popup>("PART_NumPadPopup");
            _quickSetPanel = e.NameScope.Find<ItemsControl>("PART_QuickSetPanel");
            _track = e.NameScope.Find<RangeBase>("PART_Track");
            _decreaseBtn = e.NameScope.Find<Button>("PART_DecreaseBtn");
            _increaseBtn = e.NameScope.Find<Button>("PART_IncreaseBtn");
            _confirmOverlay = e.NameScope.Find<Border>("PART_ConfirmOverlay");
            _confirmBtn = e.NameScope.Find<Button>("PART_ConfirmBtn");
            _cancelBtn = e.NameScope.Find<Button>("PART_CancelBtn");
            _numPadToggle = e.NameScope.Find<Button>("PART_NumPadToggle");

            if (_numPadPopup != null)
            {
                _numPadPopup.Opened += (s, ev) => PseudoClasses.Set(":numpad-open", true);
                _numPadPopup.Closed += (s, ev) => PseudoClasses.Set(":numpad-open", false);
            }

            if (_decreaseBtn != null) _decreaseBtn.Click += OnDecreaseClick;
            if (_increaseBtn != null) _increaseBtn.Click += OnIncreaseClick;
            if (_confirmBtn != null) _confirmBtn.Click += OnConfirmClick;
            if (_cancelBtn != null) _cancelBtn.Click += OnCancelClick;
            if (_numPadToggle != null) _numPadToggle.Click += OnNumPadToggleClick;

            if (_track != null)
            {
                _track.Minimum = (double)Minimum;
                _track.Maximum = (double)Maximum;
                _track.Value = (double)Value;
                _track.ValueChanged += OnTrackValueChanged;
            }

            // Global interception for NumericPad Submits and QuickSet Buttons
            AddHandler(NumericPad.SubmitEvent, OnNumericPadSubmit);
            AddHandler(Button.ClickEvent, OnGlobalButtonClick, RoutingStrategies.Bubble);

            UpdatePseudoClasses();
        }

        private void UnsubscribeTemplateParts()
        {
            if (_decreaseBtn != null) _decreaseBtn.Click -= OnDecreaseClick;
            if (_increaseBtn != null) _increaseBtn.Click -= OnIncreaseClick;
            if (_confirmBtn != null) _confirmBtn.Click -= OnConfirmClick;
            if (_cancelBtn != null) _cancelBtn.Click -= OnCancelClick;
            if (_numPadToggle != null) _numPadToggle.Click -= OnNumPadToggleClick;

            if (_track != null) _track.ValueChanged -= OnTrackValueChanged;

            RemoveHandler(NumericPad.SubmitEvent, OnNumericPadSubmit);
            RemoveHandler(Button.ClickEvent, OnGlobalButtonClick);

            _errorTimer?.Stop();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty)
            {
                var oldVal = change.GetOldValue<decimal>();
                var newVal = change.GetNewValue<decimal>();

                if (_track != null && !_isSliderUpdating)
                {
                    _isSliderUpdating = true;
                    _track.Value = (double)newVal;
                    _isSliderUpdating = false;
                }

                RaiseEvent(new NumericValueChangedEventArgs(ValueChangedEvent, oldVal, newVal));
            }
            else if (change.Property == MinimumProperty || change.Property == MaximumProperty)
            {
                CoerceValue(ValueProperty);
                if (_track != null)
                {
                    _track.Minimum = (double)Minimum;
                    _track.Maximum = (double)Maximum;
                }
            }
            else if (change.Property == AdaptiveStepEnabledProperty)
            {
                PseudoClasses.Set(":auto-precision", AdaptiveStepEnabled);
            }
            else if (change.Property == RecommendedValueProperty)
            {
                PseudoClasses.Set(":has-recommendation", RecommendedValue.HasValue);
            }
        }

        #region User Interaction

        private void OnDecreaseClick(object? sender, RoutedEventArgs e)
        {
            decimal step = CalculateCurrentStep();
            TryProposeValue(Value - step);
            e.Handled = true;
        }

        private void OnIncreaseClick(object? sender, RoutedEventArgs e)
        {
            decimal step = CalculateCurrentStep();
            TryProposeValue(Value + step);
            e.Handled = true;
        }

        private void OnNumPadToggleClick(object? sender, RoutedEventArgs e)
        {
            if (_numPadPopup != null)
            {
                _numPadPopup.IsOpen = !_numPadPopup.IsOpen;
            }
            e.Handled = true;
        }

        private void OnTrackValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isSliderUpdating) return;

            // Slider sets double, convert to decimal
            decimal val = (decimal)e.NewValue;
            TryProposeValue(val);
        }

        private void OnNumericPadSubmit(object? sender, RoutedEventArgs e)
        {
            if (e.Source is NumericPad pad && pad.Value.HasValue)
            {
                if (_numPadPopup != null) _numPadPopup.IsOpen = false;
                TryProposeValue(pad.Value.Value);
                pad.Value = null; // reset numpad after submit
            }
            e.Handled = true;
        }

        private void OnGlobalButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is Button btn && _quickSetPanel != null)
            {
                // Check if button is inside QuickSetPanel
                StyledElement? parent = btn;
                bool insidePanel = false;
                while (parent != null)
                {
                    if (parent == _quickSetPanel)
                    {
                        insidePanel = true;
                        break;
                    }
                    parent = parent.Parent;
                }

                if (insidePanel && btn.CommandParameter is decimal val)
                {
                    TryProposeValue(val);
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Logic

        private decimal CalculateCurrentStep()
        {
            if (!AdaptiveStepEnabled) return Step;

            decimal absVal = Math.Abs(Value);
            if (absVal < 1) return 0.01m;
            if (absVal < 10) return 0.1m;
            if (absVal < 100) return 1m;
            if (absVal < 1000) return 10m;
            return 100m;
        }

        private void TryProposeValue(decimal val)
        {
            if (val < Minimum || val > Maximum)
            {
                ShowErrorState();
                return;
            }

            _pendingValue = val;

            bool isUnsafe = val < SafeMinimum || val > SafeMaximum;
            
            // Check if jump is > 20%
            bool isBigJump = false;
            double changePercent = 0;
            if (CurrentValue != 0)
            {
                changePercent = Math.Abs((double)(val - CurrentValue) / (double)CurrentValue);
                if (changePercent > 0.2) isBigJump = true;
            }
            else if (val != 0)
            {
                 // if CurrentValue is 0, any change is theoretically infinite %, treat as big jump
                 isBigJump = true;
                 changePercent = 1.0; 
            }

            if (isUnsafe || isBigJump)
            {
                ShowWarningState(val, changePercent);
            }
            else
            {
                // Safe, apply directly
                Value = val;
                
                // Sync back slider if proposed value was not from slider
                if (_track != null)
                {
                    _isSliderUpdating = true;
                    _track.Value = (double)Value;
                    _isSliderUpdating = false;
                }
            }
        }

        private void OnConfirmClick(object? sender, RoutedEventArgs e)
        {
            Value = _pendingValue;
            PseudoClasses.Set(":warning", false);
            
            if (_track != null)
            {
                _isSliderUpdating = true;
                _track.Value = (double)Value;
                _isSliderUpdating = false;
            }
            e.Handled = true;
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            PseudoClasses.Set(":warning", false);
            
            if (_track != null)
            {
                _isSliderUpdating = true;
                _track.Value = (double)Value;
                _isSliderUpdating = false;
            }
            e.Handled = true;
        }

        private void ShowWarningState(decimal proposedValue, double percent)
        {
            PseudoClasses.Set(":warning", true);
            RaiseEvent(new CriticalChangeEventArgs(CriticalChangeRequestedEvent, proposedValue, CurrentValue, percent));
        }

        private void ShowErrorState()
        {
            PseudoClasses.Set(":error", true);

            _errorTimer?.Stop();
            _errorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            _errorTimer.Tick += (s, e) =>
            {
                PseudoClasses.Set(":error", false);
                _errorTimer?.Stop();
            };
            _errorTimer.Start();
        }

        private void UpdatePseudoClasses()
        {
            PseudoClasses.Set(":auto-precision", AdaptiveStepEnabled);
            PseudoClasses.Set(":has-recommendation", RecommendedValue.HasValue);
        }

        #endregion
    }
}
