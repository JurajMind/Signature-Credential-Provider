using System;
using System.Windows;
using System.Windows.Threading;
using EXOE.CsharpHelper;

namespace Client2
{
    public class DispatcherHelper
    {
        private Dispatcher _uiDispatcher;

        private DispatcherHelper()
        {
        }

        public void Init(Application app)
        {
            _uiDispatcher = app.Dispatcher;
        }

        public void Init(Window app)
        {
            _uiDispatcher = app.Dispatcher;
        }

        public void Invoke(Action action)
        {
            _uiDispatcher.Invoke(action);
        }

        public static DispatcherHelper Instance()
        {
            return Singleton<DispatcherHelper>.Instance;
        }
    }
}