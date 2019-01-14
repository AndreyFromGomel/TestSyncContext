using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TestSyncContext
{
    public partial class MainPage : ContentPage
    {
        private TaskCompletionSource<bool> _tcs;
        private CancellationTokenSource _cts;

        public MainPage()
        {
            InitializeComponent();
            Console.WriteLine(App.GetMonoVersion());
            Label.Text = App.GetMonoVersion();
        }

        private async void StartButton_OnClicked(object sender, EventArgs e)
        {
            _tcs = new TaskCompletionSource<bool>();
            _cts = new CancellationTokenSource();
            Label.Text = "Started";

            _cts.Token.Register(() => _tcs.TrySetCanceled());
            try
            {
                await _tcs.Task;
            }
            catch (OperationCanceledException)
            {
                Label.Text += " -> OCE ";
                Console.WriteLine("Catching OperationCanceledException");
            }

        }

        private void StopButton_OnClicked(object sender, EventArgs e)
        {
            _cts.Cancel();
            Label.Text += " -> SBC ";
            Console.WriteLine("Stop button clicked");
        }
    }
}
