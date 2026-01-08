using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ocean.Converters
{
    /// <summary>
    /// 基于NBit值控制传输类型下拉框中浮点数选项是否启用的转换器
    /// </summary>
    public class NBitToTransferTypeEnableConverter : IMultiValueConverter
    {
        /// <summary>
        /// 转换逻辑：NBit<32 且 选项是浮点数 → 禁用（返回false）
        /// </summary>
        /// <param name="values">values[0] = NBit值; values[1] = 当前下拉选项值</param>
        /// <param name="targetType">目标类型（bool）</param>
        /// <param name="parameter">附加参数（未使用）</param>
        /// <param name="culture">文化信息</param>
        /// <returns>是否启用当前选项</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // 校验参数有效性，默认启用
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return true;

            // 解析NBit值（处理类型转换异常）
            if (!int.TryParse(values[0].ToString(), out int nBit))
                return true;

            // 获取当前下拉选项（请根据你的实际选项文本修改）
            string currentOption = values[1].ToString();
            string floatOptionText = "浮点数"; // 替换为你实际的浮点数选项文本

            // 核心判断逻辑（NBit<32时禁用浮点数选项）
            if (nBit < 32 && currentOption.Equals(floatOptionText, StringComparison.Ordinal))
            {
                return false; // 禁用浮点数选项
            }

            return true; // 其他情况启用
        }

        // 双向转换无需实现
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("该转换器仅支持单向转换");
        }
    }
}
