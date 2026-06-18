using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TemplateControl.Library.Converters
{
    public class SafeZoneColorConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 4)
                return new SolidColorBrush(Color.Parse("#00FF66")); // Default green

            try
            {
                double value = values[0] is IConvertible c1 ? c1.ToDouble(culture) : 0;
                double safeMin = values[1] is IConvertible c2 ? c2.ToDouble(culture) : 0;
                double safeMax = values[2] is IConvertible c3 ? c3.ToDouble(culture) : 100;
                bool isEnabled = values[3] is bool isEnabledBool ? isEnabledBool : true;

                if (!isEnabled)
                    return new SolidColorBrush(Color.Parse("#00FF66")); // Default green if disabled

                if (safeMax <= safeMin)
                    return new SolidColorBrush(Color.Parse("#00FF66")); // Invalid range, default green

                double mid = (safeMin + safeMax) / 2.0;
                double range = (safeMax - safeMin) / 2.0;

                double distance = Math.Abs(value - mid);
                double ratio = distance / range;

                if (ratio >= 1.0)
                {
                    return new SolidColorBrush(Color.Parse("#FF4444")); // Red
                }
                else if (ratio <= 0.0)
                {
                    return new SolidColorBrush(Color.Parse("#00FF66")); // Green
                }
                else
                {
                    // Interpolate
                    if (ratio < 0.5)
                    {
                        // Green to Yellow
                        // Green: 0, 255, 102
                        // Yellow: 255, 215, 0
                        double localRatio = ratio / 0.5;
                        byte r = (byte)(0 + (255 - 0) * localRatio);
                        byte g = (byte)(255 + (215 - 255) * localRatio);
                        byte b = (byte)(102 + (0 - 102) * localRatio);
                        return new SolidColorBrush(Color.FromArgb(255, r, g, b));
                    }
                    else
                    {
                        // Yellow to Red
                        // Yellow: 255, 215, 0
                        // Red: 255, 68, 68
                        double localRatio = (ratio - 0.5) / 0.5;
                        byte r = (byte)(255 + (255 - 255) * localRatio);
                        byte g = (byte)(215 + (68 - 215) * localRatio);
                        byte b = (byte)(0 + (68 - 0) * localRatio);
                        return new SolidColorBrush(Color.FromArgb(255, r, g, b));
                    }
                }
            }
            catch
            {
                return new SolidColorBrush(Color.Parse("#00FF66"));
            }
        }
    }
}
