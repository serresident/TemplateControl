using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TemplateControl.Demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Setup Demo data
            var setValueCtrl = this.FindControl<SetValueControl>("DemoSetValueControl");
            if (setValueCtrl != null)
            {
                setValueCtrl.RecentValues = new decimal[] { 70.0m, 72.5m, 75.0m };
            }
        }

        private void OnSetValueChanged(object? sender, NumericValueChangedEventArgs e)
        {
            var log = this.FindControl<TextBlock>("DemoLog");
            if (log != null)
            {
                string msg = $"[{DateTime.Now:HH:mm:ss}] Value applied: {e.OldValue:F1} -> {e.NewValue:F1}\n";
                log.Text = msg + log.Text;
            }
        }

        private void OnCriticalChangeRequested(object? sender, CriticalChangeEventArgs e)
        {
            var log = this.FindControl<TextBlock>("DemoLog");
            if (log != null)
            {
                string msg = $"[{DateTime.Now:HH:mm:ss}] ⚠️ OVERLAY TRIGGERED: Attempt to set {e.ProposedValue:F1}. Change: {e.ChangePercent:P1}\n";
                log.Text = msg + log.Text;
            }
        }
    }
}