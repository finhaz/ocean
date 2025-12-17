using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ocean.Communication
{
    public class Dataasera: Page
    {
        public TextBox runAddr { get; set; }
        public Dataasera() 
        {
            runAddr = new TextBox();
            runAddr.Text = "128";
        }
    }
}
