using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace WPFControls.Extensions
{
    public class IntegerUpDownExtension : IntegerUpDown
    {        

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                GetBindingExpression(TextProperty).UpdateSource();
                this.Dispatcher.Invoke((ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
            }                
        }
    }
}
