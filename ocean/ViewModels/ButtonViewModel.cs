using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ocean.ViewModels
{
    public class VM_PageTextBox
    {
        public TextBox<string> ChangeTextBox { get; set; }
        public VM_PageTextBox()
        {
            ChangeTextBox = new TextBox<string>();
            ChangeTextBox.TextChangeCallBack = (text) => { MessageBox.Show(text); };//声明TextChange
            MessageBox.Show("1");
        }
    }
}
