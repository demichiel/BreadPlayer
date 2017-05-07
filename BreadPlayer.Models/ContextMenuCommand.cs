using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace BreadPlayer.Models
{
    public class ContextMenuCommand : ObservableObject
    {
        public ContextMenuCommand(ICommand command, string text, object cmdPara = null)
        {
            Command = command;
            Text = text;
            CommandParameter = cmdPara;
        }
        string text;
        public string Text
        {
            get { return text; }
            set { Set(ref text, value); }
        }
        public ICommand Command
        {
            get; private set;
        }
        public object CommandParameter
        {
            get; private set;
        }
    }
}
