using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Communication
{
    public class Modbusset
    {
        public string kind_num ;
        public string sadd { get; set; }
        public static DataTable dt1 = new DataTable();
        public DataTable dtm { get; set; }
        public Modbusset() 
        {
            dt1.Columns.Add("序号", typeof(int));   
            dt1.Columns.Add("名称", typeof(string)); 
            dt1.Columns.Add("数值", typeof(double)); 
            dt1.Columns.Add("指令", typeof(double)); 
            dt1.Columns.Add("写", typeof(double));
            dt1.Columns.Add("单位", typeof(string));
            dt1.Columns.Add("范围", typeof(string));
            dt1.Columns.Add("区块", typeof(string));
            dt1.Columns.Add("地址", typeof(int));
            dt1.Columns.Add("数量", typeof(int));
            dt1.Columns.Add("位偏移", typeof(int));
            dt1.Columns.Add("位数", typeof(int));
            dtm = dt1;
            kind_num = "保持寄存器(RW)";
            sadd = "1";
        }
    }
}
