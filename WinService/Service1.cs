using System.ServiceProcess;

namespace WinService
{
    public partial class Service1 : ServiceBase
    {
        private TcpServer.TcpServer _server = null;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _server = new TcpServer.TcpServer(5555).Start();
        }

        protected override void OnStop()
        {
            if (_server != null)
                _server.Stop();

            _server = null;
        }
    }
}
