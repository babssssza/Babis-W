using System;
using System.Windows;
using System.Windows.Threading;

namespace BabisW
{
    public partial class InjectionToast : Window
    {
        private static InjectionToast currentToast;

        public InjectionToast()
        {
            InitializeComponent();
        }

        public static void ShowAttached()
        {
            currentToast?.Close();
            currentToast = new InjectionToast();
            currentToast.PositionInWorkArea();
            currentToast.Show();

            var closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            closeTimer.Tick += (sender, args) =>
            {
                closeTimer.Stop();
                currentToast?.Close();
                currentToast = null;
            };
            closeTimer.Start();
        }

        private void PositionInWorkArea()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 14;
            Top = workArea.Bottom - Height - 14;
        }
    }
}
