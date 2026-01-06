using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.ViewModels
{
    // 你的命名空间
    public class ModbusDataItem : ObservableObject
    {
        private int _id;
        private string _name;
        private object _value; // 核心：用object存储int/double
        private double _command;
        private bool _isButtonClicked;
        private string _unit;
        private string _rangle;
        private string _selectedOption;
        private int _addr;
        private int _number;
        private int _nOffSet;
        private int _nBit;
        private float _coefficient;
        private int _offset;
        private int _decimalPlaces;
        private string _transferType;
        private string _displayType; // 浮点数/整形数
        private int _byteOrder;
        private int _wordOrder;
        private bool _isDrawCurve;
        private int _intervalTime;

        // 所有属性按原DataTable列名定义，保持Binding兼容
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public object Value // 核心属性：存储int或double
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public double Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public bool IsButtonClicked
        {
            get => _isButtonClicked;
            set => SetProperty(ref _isButtonClicked, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public string Rangle
        {
            get => _rangle;
            set => SetProperty(ref _rangle, value);
        }

        public string SelectedOption
        {
            get => _selectedOption;
            set => SetProperty(ref _selectedOption, value);
        }

        public int Addr
        {
            get => _addr;
            set => SetProperty(ref _addr, value);
        }

        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public int NOffSet
        {
            get => _nOffSet;
            set => SetProperty(ref _nOffSet, value);
        }

        public int NBit
        {
            get => _nBit;
            set => SetProperty(ref _nBit, value);
        }

        public float Coefficient
        {
            get => _coefficient;
            set => SetProperty(ref _coefficient, value);
        }

        public int Offset
        {
            get => _offset;
            set => SetProperty(ref _offset, value);
        }

        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set => SetProperty(ref _decimalPlaces, value);
        }

        public string TransferType
        {
            get => _transferType;
            set => SetProperty(ref _transferType, value);
        }

        public string DisplayType
        {
            get => _displayType;
            set
            {
                // 核心逻辑：切换DisplayType时，自动转换Value的类型
                if (SetProperty(ref _displayType, value))
                {
                    ConvertValueByDisplayType();
                }
            }
        }

        public int ByteOrder
        {
            get => _byteOrder;
            set => SetProperty(ref _byteOrder, value);
        }

        public int WordOrder
        {
            get => _wordOrder;
            set => SetProperty(ref _wordOrder, value);
        }

        public bool IsDrawCurve
        {
            get => _isDrawCurve;
            set => SetProperty(ref _isDrawCurve, value);
        }

        public int IntervalTime
        {
            get => _intervalTime;
            set => SetProperty(ref _intervalTime, value);
        }

        /// <summary>
        /// 根据DisplayType自动转换Value的类型（仅当前行生效）
        /// </summary>
        private void ConvertValueByDisplayType()
        {
            if (Value == null)
            {
                Value = DisplayType == "十进制整数" ? 0 : 0.0;
                return;
            }

            try
            {
                switch (DisplayType)
                {
                    case "十进制整数":
                        // 浮点数转十进制整数（四舍五入）
                        double decimalDouble = Convert.ToDouble(Value);
                        Value = Convert.ToInt32(Math.Round(decimalDouble));
                        break;

                    case "十六进制整数":
                        // 浮点数转整数（作为十六进制的存储值，显示时转格式）
                        double hexDouble = Convert.ToDouble(Value);
                        Value = Convert.ToInt32(Math.Round(hexDouble)); // 存储为int
                        break;

                    case "浮点数":
                    default:
                        // 整数（十进制/十六进制）转浮点数
                        int floatInt = Convert.ToInt32(Value);
                        Value = Convert.ToDouble(floatInt);
                        break;
                }
            }
            catch (Exception)
            {
                // 转换失败时设默认值
                Value = DisplayType == "整形数" ? 0 : 0.0;
            }
        }
    }
}
