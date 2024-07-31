
// using System;
// using System.Globalization;
// using Avalonia.Data;
// using Avalonia.Data.Converters;

// namespace OpenKNX.Toolbox.Converter;

// public class TextCaseConverter : IValueConverter
// {
//     public static readonly TextCaseConverter Instance = new();

//     public object? Convert(object? value, Type targetType, object? parameter, 
//                                                             CultureInfo culture)
//     {
//         if (value is bool sourceText && parameter is string negate
//             && targetType.IsAssignableTo(typeof(string)))
//         {
//             bool visible = sourceText;
//             if(negate == "true")
//                 visible = !visible;

//             if(visible)
//                 return Visibility
//         }
//         // converter used for the wrong type
//         return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
//     }

//     public object ConvertBack(object? value, Type targetType, 
//                                 object? parameter, CultureInfo culture)
//     {
//       throw new NotSupportedException();
//     }
// }