using ocean.Communication;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ocean.UI
{
    /// <summary>
    /// Modserial.xaml 的交互逻辑
    /// </summary>
    public partial class Modserial
    {

        public Modbusset mcom { get; set; }
        public int a = 1;
        public Modserial()
        {
            mcom = new Modbusset();
            InitializeComponent();
            
        }



        private void setsure_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
            // 创建新行并赋值
            DataRow newRow = mcom.dtm.NewRow();
            int radd = Int32.Parse(mcom.sadd);
            newRow["序号"] = a;
            newRow["区块"] = mcom.kind_num;
            newRow["地址"] = radd;
            a = a + 1;
            mcom.dtm.Rows.Add(newRow);
        }

        private void setcancel_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
        }

        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbProcho.SelectedIndex)
            {
                
                case 0: mcom.kind_num = "线圈状态(RW)"; break;
                case 1: mcom.kind_num = "离散输入(RO)"; break;
                case 2: mcom.kind_num = "保持寄存器(RW)"; break;
                case 3: mcom.kind_num = "输入寄存器(RO)"; break;
                default: mcom.kind_num = "保持寄存器(RW)"; break;
                
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Visible;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrodx.SelectedItem is DataRowView selectedRowView)
            {
                mcom.dtm.Rows.Remove(selectedRowView.Row);
            }
            else
            {
                MessageBox.Show("请指定行！");
            }
        }

    }
}
