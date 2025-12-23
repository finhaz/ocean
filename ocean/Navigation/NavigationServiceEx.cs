using SomeNameSpace;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ocean.Navigation
{
    public class NavigationServiceEx
    {
        public event NavigatedEventHandler Navigated;

        public event NavigationFailedEventHandler NavigationFailed;

        private Frame _frame;

        public Frame Frame
        {
            get
            {
                if (this._frame == null)
                {
                    this._frame = new Frame() { NavigationUIVisibility = NavigationUIVisibility.Hidden };
                    this.RegisterFrameEvents();
                }

                return this._frame;
            }
            set
            {
                this.UnregisterFrameEvents();
                this._frame = value;
                this.RegisterFrameEvents();
            }
        }

        public bool CanGoBack => this.Frame.CanGoBack;

        public bool CanGoForward => this.Frame.CanGoForward;

        public void GoBack() => this.Frame.GoBack();

        public void GoForward() => this.Frame.GoForward();

        public bool Navigate(Uri sourcePageUri, object extraData = null)
        {
            if (this.Frame.CurrentSource != sourcePageUri)
            {
                return this.Frame.Navigate(sourcePageUri, extraData);
            }

            return false;
        }

        public bool Navigate(Type sourceType)
        {
            if (this.Frame.NavigationService?.Content?.GetType() != sourceType)
            {
                return this.Frame.Navigate(Activator.CreateInstance(sourceType));
            }

            return false;
        }

        public void load_table()
        {
            //CommonRes.dt1 = DB_Access.GetDBTable("PARAMETER_RUN");
            //CommonRes.dt2 = DB_Access.GetDBTable("PARAMETER_SET");
            //CommonRes.dt3 = DB_Access.GetDBTable("PARAMETER_FACTOR");

            //CommonRes.dt1 = DB_sqlite.GetDBTable("PARAMETER_RUN");
            //CommonRes.dt2 = DB_sqlite.GetDBTable("PARAMETER_SET");
            //CommonRes.dt3 = DB_sqlite.GetDBTable("PARAMETER_FACTOR");
        }
        public void WindowClosing()
        {
            //MessageBox.Show("数据将保存！");

            //DB_Access.UpdateDBTable(CommonRes.dt1, "PARAMETER_RUN");
            //DB_Access.UpdateDBTable(CommonRes.dt2, "PARAMETER_SET");
            //DB_Access.UpdateDBTable(CommonRes.dt3, "PARAMETER_FACTOR");

            //DB_sqlite.UpdateDBTable(CommonRes.dt1, "PARAMETER_RUN");
            //DB_sqlite.UpdateDBTable(CommonRes.dt2, "PARAMETER_SET");
            //DB_sqlite.UpdateDBTable(CommonRes.dt3, "PARAMETER_FACTOR");
        }


        private void RegisterFrameEvents()
        {
            if (this._frame != null)
            {
                this._frame.Navigated += this.Frame_Navigated;
                this._frame.NavigationFailed += this.Frame_NavigationFailed;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (this._frame != null)
            {
                this._frame.Navigated -= this.Frame_Navigated;
                this._frame.NavigationFailed -= this.Frame_NavigationFailed;
            }
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => this.NavigationFailed?.Invoke(sender, e);

        private void Frame_Navigated(object sender, NavigationEventArgs e) => this.Navigated?.Invoke(sender, e);
    }
}