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
            dt1.Columns.Add("ID", typeof(int));   
            dt1.Columns.Add("Name", typeof(string)); 
            dt1.Columns.Add("Value", typeof(double)); 
            dt1.Columns.Add("Command", typeof(double)); 
            dt1.Columns.Add("IsButtonClicked", typeof(bool));
            dt1.Columns.Add("Unit", typeof(string));
            dt1.Columns.Add("Rangle", typeof(string));
            dt1.Columns.Add("IsSelected", typeof(bool));
            dt1.Columns.Add("Addr", typeof(int));
            dt1.Columns.Add("Number", typeof(int));
            dt1.Columns.Add("NOffSet", typeof(int));
            dt1.Columns.Add("NBit", typeof(int));
            dtm = dt1;
            kind_num = "保持寄存器(RW)";
            sadd = "1";
        }
    }
}
