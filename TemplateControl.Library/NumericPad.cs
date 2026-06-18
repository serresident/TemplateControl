using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace TemplateControl
{
    /// <summary>
    /// Routed event args carrying old and new values for property change notifications.
    /// </summary>
    public class NumericValueChangedEventArgs : RoutedEventArgs
    {
        public decimal? OldValue { get; }
        public decimal? NewValue { get; }

        public NumericValueChangedEventArgs(RoutedEvent routedEvent, decimal? oldValue, decimal? newValue)
            : base(routedEvent)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// A lookless numeric keypad control for touch/terminal input scenarios (POS, SCADA terminals).
    /// Provides digit buttons, decimal separator, backspace, clear and submit functionality.
    /// All visual representation is defined entirely through <see cref="ControlTemplate"/>.
    /// </summary>
    [TemplatePart("PART_CancelButton", typeof(Button))]
    [TemplatePart("PART_Display", typeof(TextBlock))]
    [TemplatePart("PART_ClearButton", typeof(Button))]
    [TemplatePart("PART_DecimalButton", typeof(Button))]
    [PseudoClasses(":error", ":empty")]
    public class NumericPad : TemplatedControl
    {
        #region Styled Properties

        public static readonly StyledProperty<decimal?> ValueProperty =
            AvaloniaProperty.Register<NumericPad, decimal?>(
                nameof(Value),
                defaultValue: null);

        public static readonly StyledProperty<decimal> MinimumProperty =
            AvaloniaProperty.Register<NumericPad, decimal>(
                nameof(Minimum),
                defaultValue: 0m,
                coerce: CoerceMinimum);

        public static readonly StyledProperty<decimal> MaximumProperty =
            AvaloniaProperty.Register<NumericPad, decimal>(
                nameof(Maximum),
                defaultValue: decimal.MaxValue,
                coerce: CoerceMaximum);

        public static readonly StyledProperty<int> MaxLengthProperty =
            AvaloniaProperty.Register<NumericPad, int>(nameof(MaxLength), defaultValue: 10);

        public static readonly StyledProperty<bool> ShowDecimalSeparatorProperty =
            AvaloniaProperty.Register<NumericPad, bool>(nameof(ShowDecimalSeparator), defaultValue: true);

        #endregion

        #region Routed Events

        public static readonly RoutedEvent<NumericValueChangedEventArgs> ValueChangedEvent =
            RoutedEvent.Register<NumericPad, NumericValueChangedEventArgs>(
                nameof(ValueChanged), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> SubmitEvent =
            RoutedEvent.Register<NumericPad, RoutedEventArgs>(
                nameof(Submit), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> CancelEvent =
            RoutedEvent.Register<NumericPad, RoutedEventArgs>(
                nameof(Cancel), RoutingStrategies.Bubble);

        public event EventHandler<NumericValueChangedEventArgs>? ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public event EventHandler<RoutedEventArgs>? Submit
        {
            add => AddHandler(SubmitEvent, value);
            remove => RemoveHandler(SubmitEvent, value);
        }

        public event EventHandler<RoutedEventArgs>? Cancel
        {
            add => AddHandler(CancelEvent, value);
            remove => RemoveHandler(CancelEvent, value);
        }

        #endregion

        #region Property Accessors

        public decimal? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
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

        public int MaxLength
        {
            get => GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public bool ShowDecimalSeparator
        {
            get => GetValue(ShowDecimalSeparatorProperty);
            set => SetValue(ShowDecimalSeparatorProperty, value);
        }

        #endregion

        #region Coerce Logic

        private static decimal CoerceMinimum(AvaloniaObject obj, decimal value)
        {
            if (obj is NumericPad pad)
            {
                if (value > pad.Maximum) return pad.Maximum;
            }
            return value;
        }

        private static decimal CoerceMaximum(AvaloniaObject obj, decimal value)
        {
            if (obj is NumericPad pad)
            {
                if (value < pad.Minimum) return pad.Minimum;
            }
            return value;
        }

        #endregion

        #region Template Parts

        private TextBlock? _displayTextBlock;
        private Button? _cancelButton;
        private Button? _clearButton;
        private Button? _backspaceButton;
        private Button? _submitButton;
        private Button? _decimalButton;

        #endregion

        #region Internal State

        private string _inputBuffer = string.Empty;
        private string _originalBuffer = string.Empty;
        private bool _isFreshInput = true;
        private bool _isInternalChange = false;
        private DispatcherTimer? _errorTimer;

        #endregion

        public NumericPad()
        {
            UpdatePseudoClasses();
        }

        private void SetValueInternally(decimal? value)
        {
            _isInternalChange = true;
            try
            {
                Value = value;
            }
            finally
            {
                _isInternalChange = false;
            }
        }

        #region OnApplyTemplate — Memory Management

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            // Unsubscribe from old template parts to prevent memory leaks
            UnsubscribeTemplateParts();

            base.OnApplyTemplate(e);

            // Find new template parts (null-safe)
            _displayTextBlock = e.NameScope.Find<TextBlock>("PART_Display");
            _cancelButton = e.NameScope.Find<Button>("PART_CancelButton");
            _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
            _backspaceButton = e.NameScope.Find<Button>("PART_BackspaceButton");
            _submitButton = e.NameScope.Find<Button>("PART_SubmitButton");
            _decimalButton = e.NameScope.Find<Button>("PART_DecimalButton");

            // Subscribe to specific PART_ button events
            if (_cancelButton != null)
                _cancelButton.Click += OnCancelClick;

            if (_clearButton != null)
                _clearButton.Click += OnClearClick;

            if (_backspaceButton != null)
                _backspaceButton.Click += OnBackspaceClick;

            if (_submitButton != null)
                _submitButton.Click += OnSubmitClick;

            if (_decimalButton != null)
            {
                _decimalButton.Click += OnDecimalClick;
                _decimalButton.IsEnabled = ShowDecimalSeparator;
            }

            // Global digit button routing: intercept all Button.Click inside this control tree
            AddHandler(Button.ClickEvent, OnGlobalButtonClick, RoutingStrategies.Bubble);

            // Sync display
            UpdateDisplay();
            UpdatePseudoClasses();
        }

        private void UnsubscribeTemplateParts()
        {
            if (_cancelButton != null)
                _cancelButton.Click -= OnCancelClick;

            if (_clearButton != null)
                _clearButton.Click -= OnClearClick;

            if (_backspaceButton != null)
                _backspaceButton.Click -= OnBackspaceClick;

            if (_submitButton != null)
                _submitButton.Click -= OnSubmitClick;

            if (_decimalButton != null)
                _decimalButton.Click -= OnDecimalClick;

            // Remove global handler
            RemoveHandler(Button.ClickEvent, OnGlobalButtonClick);

            // Stop error timer if running
            _errorTimer?.Stop();
            _errorTimer = null;
        }

        #endregion

        #region Property Changed

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty)
            {
                var oldValue = change.GetOldValue<decimal?>();
                var newValue = change.GetNewValue<decimal?>();

                // Sync internal buffer if value was set externally
                if (!_isInternalChange)
                {
                    if (newValue.HasValue)
                    {
                        string newValStr = newValue.Value.ToString(CultureInfo.InvariantCulture);
                        _inputBuffer = newValStr;
                        _originalBuffer = newValStr;
                        _isFreshInput = true;
                        UpdateDisplay();
                    }
                    else
                    {
                        _inputBuffer = string.Empty;
                        _originalBuffer = string.Empty;
                        _isFreshInput = true;
                        UpdateDisplay();
                    }
                }

                UpdatePseudoClasses();

                RaiseEvent(new NumericValueChangedEventArgs(ValueChangedEvent, oldValue, newValue));
            }
            else if (change.Property == ShowDecimalSeparatorProperty)
            {
                if (_decimalButton != null)
                    _decimalButton.IsEnabled = ShowDecimalSeparator;
            }
            else if (change.Property == MinimumProperty || change.Property == MaximumProperty)
            {
                // Re-coerce Value when bounds change
                CoerceValue(ValueProperty);
            }
        }

        #endregion

        #region Button Click Handlers

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CancelEvent));
            e.Handled = true;
        }

        private void OnGlobalButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is Button button)
            {
                // Skip known PART_ buttons — they have their own handlers
                if (button == _clearButton || button == _backspaceButton ||
                    button == _submitButton || button == _decimalButton)
                    return;

                // Check CommandParameter first, then Content
                string? digit = button.CommandParameter as string
                                ?? button.Content as string;

                if (digit != null && digit.Length == 1 && char.IsDigit(digit[0]))
                {
                    AppendDigit(digit);
                }
            }
        }

        private void OnClearClick(object? sender, RoutedEventArgs e)
        {
            _inputBuffer = string.Empty;
            _isFreshInput = false;
            SetValueInternally(null);
            UpdateDisplay();
            UpdatePseudoClasses();
            e.Handled = true;
        }

        private void OnBackspaceClick(object? sender, RoutedEventArgs e)
        {
            _isFreshInput = false;
            if (_inputBuffer.Length > 0)
            {
                _inputBuffer = _inputBuffer.Substring(0, _inputBuffer.Length - 1);
                TryApplyBuffer();
                UpdateDisplay();
            }
            e.Handled = true;
        }

        private void OnSubmitClick(object? sender, RoutedEventArgs e)
        {
            if (Value.HasValue && Value.Value < Minimum)
            {
                RevertToOriginalWithError();
                return;
            }
            RaiseEvent(new RoutedEventArgs(SubmitEvent));
            e.Handled = true;
        }

        private void OnDecimalClick(object? sender, RoutedEventArgs e)
        {
            if (!ShowDecimalSeparator) return;

            if (_isFreshInput)
            {
                _inputBuffer = "0.";
                _isFreshInput = false;
                UpdateDisplay();
                e.Handled = true;
                return;
            }

            // Prevent multiple decimal separators
            if (!_inputBuffer.Contains('.'))
            {
                if (_inputBuffer.Length == 0)
                    _inputBuffer = "0.";
                else
                    _inputBuffer += ".";

                UpdateDisplay();
            }
            e.Handled = true;
        }

        #endregion

        #region Input Logic & Validation

        private void AppendDigit(string digit)
        {
            // Check MaxLength
            string candidateBuffer = _isFreshInput ? digit : _inputBuffer + digit;
            
            if (candidateBuffer.Length > MaxLength)
            {
                RevertToOriginalWithError();
                return;
            }

            // Pre-validate: parse the candidate and check bounds
            if (decimal.TryParse(candidateBuffer, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal candidateValue))
            {
                if (candidateValue > Maximum)
                {
                    RevertToOriginalWithError();
                    return;
                }

                _isFreshInput = false;
                _inputBuffer = candidateBuffer;
                SetValueInternally(candidateValue);
                UpdateDisplay();
            }
        }

        private void RevertToOriginalWithError()
        {
            ShowErrorState();
            _inputBuffer = _originalBuffer;
            _isFreshInput = true;
            
            if (decimal.TryParse(_inputBuffer, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed))
            {
                SetValueInternally(parsed);
            }
            else
            {
                SetValueInternally(null);
            }
            
            UpdateDisplay();
        }

        private void TryApplyBuffer()
        {
            if (string.IsNullOrEmpty(_inputBuffer))
            {
                SetValueInternally(null);
                return;
            }

            if (decimal.TryParse(_inputBuffer, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed))
            {
                if (parsed <= Maximum)
                {
                    SetValueInternally(parsed);
                }
                else
                {
                    SetValueInternally(null);
                }
            }
            // Buffer contains partial input like "0." — keep display but don't update Value
        }

        private void ShowErrorState()
        {
            PseudoClasses.Set(":error", true);

            _errorTimer?.Stop();
            _errorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _errorTimer.Tick += (s, e) =>
            {
                PseudoClasses.Set(":error", false);
                _errorTimer.Stop();
            };
            _errorTimer.Start();
        }

        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key >= Avalonia.Input.Key.D0 && e.Key <= Avalonia.Input.Key.D9)
            {
                AppendDigit((e.Key - Avalonia.Input.Key.D0).ToString());
                e.Handled = true;
            }
            else if (e.Key >= Avalonia.Input.Key.NumPad0 && e.Key <= Avalonia.Input.Key.NumPad9)
            {
                AppendDigit((e.Key - Avalonia.Input.Key.NumPad0).ToString());
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Back)
            {
                OnBackspaceClick(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Enter || e.Key == Avalonia.Input.Key.Return)
            {
                OnSubmitClick(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Escape)
            {
                OnCancelClick(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Avalonia.Input.Key.Decimal || e.Key == Avalonia.Input.Key.OemPeriod || e.Key == Avalonia.Input.Key.OemComma)
            {
                OnDecimalClick(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        #endregion

        #region Display & PseudoClasses

        private void UpdateDisplay()
        {
            if (_displayTextBlock != null)
            {
                _displayTextBlock.Text = string.IsNullOrEmpty(_inputBuffer)
                    ? string.Empty
                    : _inputBuffer;
            }
        }

        private void UpdatePseudoClasses()
        {
            PseudoClasses.Set(":empty", Value == null);
        }

        #endregion
    }
}
