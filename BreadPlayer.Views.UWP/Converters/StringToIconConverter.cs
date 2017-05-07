using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace BreadPlayer.Converters
{
	class StringToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SymbolIcon symbol = null;
            if (value != null)
            {
                string val = value.ToString();

                if (val == "No Repeat")
                {
                    symbol = new SymbolIcon(Symbol.Sync);
                }
                else if (val == "Repeat Song")
                    symbol = new SymbolIcon(Symbol.RepeatOne);
                else
                    symbol = new SymbolIcon(Symbol.RepeatAll);
            }
            else
                symbol = new SymbolIcon(Symbol.Sync);
            if (parameter?.ToString() == "char")
                return (char)(symbol.Symbol);
            else
                return symbol;
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            return value;
        }
    }
}
