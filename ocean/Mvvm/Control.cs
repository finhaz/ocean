using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//数据控件其实很好理解，它就是把UI控件中存储的数据提取出来,
//好让ViewModel可以通过修改数据来控制UI变化；
//当然，为了更好的控制UI变化，数据控件里还得包含一点管理UI的属性。

namespace ocean.Mvvm
{
    public class Control<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T _DataContent;
        public T DataContent { get { return _DataContent; } set { _DataContent = value; OnPropertyChanged(); } }

        public Visibility _Visibility;
        public Visibility Visibility { get { return _Visibility; } set { _Visibility = value; OnPropertyChanged(); } }

        public bool _IsReadOnly;
        public bool IsReadOnly { get { return _IsReadOnly; } set { _IsReadOnly = value; OnPropertyChanged(); } }

        public bool _IsEnabled;
        public bool IsEnabled { get { return _IsEnabled; } set { _IsEnabled = value; OnPropertyChanged(); } }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class TextBlock<T> : Control<T>
    {
        public T _Text;
        public T Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                OnPropertyChanged();
            }
        }
    }

    public class TextBox<T> : Control<T>
    {
        public Action<T> TextChangeCallBack = null;

        public T _Text;
        public T Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                if (TextChangeCallBack != null)
                {
                    TextChangeCallBack(_Text);
                }
                OnPropertyChanged();
            }
        }
    }


    public class NotifyObject: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }






}
