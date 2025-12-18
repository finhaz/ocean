using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ocean.Communication

{
    public class Modbusset : INotifyPropertyChanged
    {
        // 1. 声明属性变更事件
        public event PropertyChangedEventHandler PropertyChanged;

        // 2. 触发事件的方法（供属性的set调用）
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 3. 将sadd从**字段**改为**私有字段+公共属性**（带通知）
        private string _sadd = "0"; // 初始值可根据你的业务调整
        public string Sadd // 推荐帕斯卡命名法（首字母大写），XAML绑定可兼容小写（但建议统一）
        {
            get => _sadd;
            set
            {
                if (_sadd != value)
                {
                    _sadd = value;
                    OnPropertyChanged(); // 关键：值变化时触发通知，UI会同步更新
                }
            }
        }
        public string kind_num ;
        public string snum { get; set; }
        public string dSelectedOption { get; set; }
        public static DataTable dt1 = new DataTable();
        public DataTable dtm { get; set; }
        public Modbusset() 
        {
            dt1.Columns.Add("ID", typeof(int));   
            dt1.Columns.Add("Name", typeof(string)); 
            dt1.Columns.Add("Value", typeof(double)); 
            dt1.Columns.Add("Command", typeof(double)); 
            dt1.Columns.Add("IsButtonClicked", typeof(bool));
            dt1.Columns.Add("Unit", typeof(string));
            dt1.Columns.Add("Rangle", typeof(string));
            dt1.Columns.Add("SelectedOption", typeof(string));
            dt1.Columns.Add("Addr", typeof(int));
            dt1.Columns.Add("Number", typeof(int));
            dt1.Columns.Add("NOffSet", typeof(int));
            dt1.Columns.Add("NBit", typeof(int));
            dtm = dt1;
            kind_num = "保持寄存器(RW)";
            Sadd = "1";
            snum = "1";
            dSelectedOption = "保持寄存器(RW)";
        }
    }
}
