using SimpleTCP;
using System.Linq;
using System.Reflection;

namespace TcpServer
{
    public class TcpServer
    {
        private const char _DELIMITER_ = '|';

        private SimpleTcpServer _server = null;
        private int _port;

        private ServiceAPI _api = null;

        public TcpServer(int port) => this._port = port;

        public TcpServer Start()
        {
            _api = new ServiceAPI();
            
            _server = new SimpleTcpServer().Start(_port);
            _server.Delimiter = 0x13;

            _server.DelimiterDataReceived += (sender, msg) => {
                msg.ReplyLine(_response(msg.MessageString));
            };
            
            return this;
        }

        public void Stop()
        {
            _server.Stop();
            _api = null;
        }

        private string _response(string s)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf(_DELIMITER_) < 0)
                return $"No api specified!";

            string[] _para = s.Split(_DELIMITER_);

            MethodInfo _mi = 
                typeof(ServiceAPI)
                .GetMethod(_para[0], Enumerable.Repeat(typeof(string), _para.Length-1).ToArray());

            if (_mi == null)
                return $"Wrong api[{_para[0]}] is specified!";

            string r = (string)_mi.Invoke(_api, _para.Skip(1).ToArray());

            return r;
        }

    }
}
