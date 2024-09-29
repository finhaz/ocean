using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//using InfusionBagSmartLabeler.Common.Command;
//using InfusionBagSmartLabeler.Model;
//using InfusionBagSmartLabeler.utils;
using System.Text.RegularExpressions;



namespace ocean.ViewModels
{
    public class VM_PageTextBox 
    {
        public TextBox<string> ChangeTextBox { get; set; }
        public TextBox<string> ChangeTextBox2 { get; set; }
        public TextBox ChangeTextBox3 { get; set; }


        //private int _BeltSpeed = int.Parse(Config.getInstance().getConfig("BeltSpeed"));
        //public ICommand MyCommand => new MyCommand(MyAction, MyCanExec);
        bool isCanExec = true;

        public ICommand ButtonIncrease => new MyCommand(ButtonIncreaseAction, MyCanExec);

        public event PropertyChangedEventHandler PropertyChanged;



        public VM_PageTextBox()
        {
            ChangeTextBox = new TextBox<string>();
            ChangeTextBox.TextChangeCallBack = (text) => { MessageBox.Show(text); };//声明TextChange

            ChangeTextBox2 = new TextBox<string>();
            ChangeTextBox2.TextChangeCallBack = (text) => { MessageBox.Show(text); };//声明TextChange

            ChangeTextBox3 = new TextBox();
            ChangeTextBox3.TextChanged += (sender, e) => { MessageBox.Show("变了"); };//双击处理


        }

        private bool MyCanExec(object parameter)
        {
            return isCanExec;
        }

        private void ButtonIncreaseAction(object parameter)
        {
            MessageBox.Show("h!");
        }



    }
}
