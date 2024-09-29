using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace ocean.ViewModels
{
    public class VM_PageTextBox 
    {
        public TextBox<string> ChangeTextBox { get; set; }
        public TextBox<string> ChangeTextBox2 { get; set; }

        public Button btd {  get; set; }

        public VM_PageTextBox()
        {
            ChangeTextBox = new TextBox<string>();
            ChangeTextBox.TextChangeCallBack = (text) => { MessageBox.Show(text); };//声明TextChange

            ChangeTextBox2 = new TextBox<string>();
            ChangeTextBox2.TextChangeCallBack = (text) => { MessageBox.Show(text); };//声明TextChange

            btd = new Button();
            btd.MouseDoubleClick += (sender, e) => { MessageBox.Show("按了"); };//双击处理
        }
    }
}
