using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ocean.Interfaces
{
    public interface IFileDialogService
    {
        string SaveFileDialog(string title, string filter, string defaultFileName);
        string OpenFileDialog(string title, string filter);
    }

    // 真正实现（只在View层）
    public class FileDialogService : IFileDialogService
    {
        public string SaveFileDialog(string title, string filter, string defaultFileName)
        {
            var dlg = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = defaultFileName
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        public string OpenFileDialog(string title, string filter)
        {
            var dlg = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }

}
