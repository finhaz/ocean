using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Communication
{
    /// <summary>
    /// 串口数据处理工具类，封装串口数据的格式化、解析等功能
    /// </summary>
    public class SerialDataProcessor
    {
        // 私有构造函数，防止外部实例化
        private SerialDataProcessor() { }

        // 静态只读实例（懒加载，线程安全）
        private static readonly Lazy<SerialDataProcessor> _instance = new Lazy<SerialDataProcessor>(() => new SerialDataProcessor());

        // 全局访问点
        public static SerialDataProcessor Instance => _instance.Value;


        // 可配置的默认前缀和换行符，便于全局修改
        private const string DefaultTxPrefix = "TX:";
        private const string DefaultNewLine = "\r\n";
        private const string HexSeparator = " ";

        /// <summary>
        /// 将串口发送的字节数组格式化为十六进制字符串（带TX前缀和换行）
        /// </summary>
        /// <param name="buffer">待格式化的字节数组</param>
        /// <param name="length">有效数据长度</param>
        /// <param name="useUpperCase">是否使用大写十六进制（默认true）</param>
        /// <returns>格式化后的字符串</returns>
        /// <exception cref="ArgumentNullException">字节数组为空时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">有效长度越界时抛出</exception>
        public string FormatTxDataToHexString(byte[] buffer, int length, bool useUpperCase = true)
        {
            // 1. 参数校验，避免空引用和越界异常
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "字节数组不能为空");
            if (length < 0 || length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "有效长度必须在0到数组长度之间");

            // 2. 无有效数据时返回空前缀+换行（或根据需求返回空字符串）
            if (length == 0)
                return $"{DefaultTxPrefix}{DefaultNewLine}";

            // 3. 使用StringBuilder高效拼接字符串
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultTxPrefix);

            // 4. 格式化每个字节为两位十六进制，补零并控制大小写
            string format = useUpperCase ? "X2" : "x2";
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(buffer[i].ToString(format));
                stringBuilder.Append(HexSeparator);
            }

            // 5. 移除最后一个分隔符，添加换行符
            if (stringBuilder.Length > DefaultTxPrefix.Length)
                stringBuilder.Length--;
            stringBuilder.Append(DefaultNewLine);

            return stringBuilder.ToString();
        }

        // 扩展：接收数据格式化方法（示例）
        public string FormatRxDataToHexString(byte[] buffer, int length, bool useUpperCase = true)
        {
            // 逻辑与发送类似，仅前缀改为"RX:"
            const string DefaultRxPrefix = "RX:";

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (length < 0 || length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == 0)
                return $"{DefaultRxPrefix}{DefaultNewLine}";

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultRxPrefix);

            string format = useUpperCase ? "X2" : "x2";
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(buffer[i].ToString(format));
                stringBuilder.Append(HexSeparator);
            }

            if (stringBuilder.Length > DefaultRxPrefix.Length)
                stringBuilder.Length--;
            stringBuilder.Append(DefaultNewLine);

            return stringBuilder.ToString();
        }

        // 扩展：字节数组转ASCII字符串（示例）
        public string ConvertToAsciiString(byte[] buffer, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (length < 0 || length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return Encoding.ASCII.GetString(buffer, 0, length);
        }
    }
}
