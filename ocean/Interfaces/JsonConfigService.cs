using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ocean.Interfaces
{
    public class JsonConfigService
    {
        /// <summary>
        /// 导出对象到 JSON 文件
        /// </summary>
        public void ExportToFile<T>(T data, string filePath)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 从 JSON 文件导入并返回对象
        /// </summary>
        public T ImportFromFile<T>(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
