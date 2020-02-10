using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Utilities.Net
{
    public static class DnsHelper
    {
        #region Fields
        private static readonly object SyncRoot = new object();
        private static string _localIpAddress;
        #endregion

        /// <summary>
        /// 현재 서버의 IP주소를 반환한다.
        /// cf. https://stackoverflow.com/a/50386894/193178
        /// </summary>
        public static string LocalIpAddress
        {
            get
            {
                if (_localIpAddress == null)
                {
                    lock (SyncRoot)
                    {
                        if (_localIpAddress == null)
                        {
                            // 수정 전
                            //var host = Dns.GetHostEntry(Dns.GetHostName());

                            //foreach (var ip in host.AddressList)
                            //{
                            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
                            //    {
                            //        _localIpAddress = ip.ToString();
                            //        break;
                            //    }
                            //}

                            // 수정 후
                            // NOTE(jhlee): 네트워크 인터페이스 어댑터가 많을 때 위와 같은 방식으로 하면
                            // AD 조인 시 사용한 IP가 안 나올 수 있음.
                            var firstUpInterface = NetworkInterface.GetAllNetworkInterfaces()
                                .OrderByDescending(c => c.Speed)
                                .FirstOrDefault(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);

                            if (firstUpInterface != null)
                            {
                                var props = firstUpInterface.GetIPProperties();

                                // get first IPV4 address assigned to this interface
                                var firstIpV4Address = props.UnicastAddresses
                                    .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                                    .Select(c => c.Address)
                                    .FirstOrDefault();

                                _localIpAddress = firstIpV4Address.ToString();
                            }
                        }
                    }
                }

                return _localIpAddress;
            }
        }
    }
}
