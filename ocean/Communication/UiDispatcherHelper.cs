using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ocean.Communication
{
    /// <summary>
    /// WPF UI调度工具类：提供线程安全的控件更新方法
    /// </summary>
    public static class UiDispatcherHelper
    {
        /// <summary>
        /// 线程安全地执行UI操作（同步）
        /// </summary>
        /// <param name="action">要执行的UI操作（如更新控件属性）</param>
        public static void ExecuteOnUiThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;

            if (dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }

        /// <summary>
        /// 线程安全地执行UI操作（异步，不阻塞调用线程）
        /// </summary>
        /// <param name="action">要执行的UI操作</param>
        public static void ExecuteOnUiThreadAsync(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dispatcher = Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;

            if (dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                dispatcher.BeginInvoke(action);
            }
        }
    }
}
