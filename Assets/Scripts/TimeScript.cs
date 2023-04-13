using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public static class TimeScript 
{
    public const string ntpServer2 = "ntp0.NL.net.";
    public const string ntpServer1 = "ntp.ix.ru";

    public static DateTime GetNetworkTime(string ntpServer)
    {
        var ntpData = new byte[48];

        //���������� Leap Indicator, Version Number and Mode values
        ntpData[0] = 0x1B; //LI = 0 (��� ��������������), VN = 3 (������ IPv4), Mode = 3 (������)

        //�������� ����� �� ��� �������
        var addresses = Dns.GetHostEntry(ntpServer).AddressList;

        //���������� ���� 123 ��� NTP 
        var ipEndPoint = new IPEndPoint(addresses[0], 123);


        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            //��������� �����������    
            socket.Connect(ipEndPoint);

            socket.ReceiveTimeout = 3000;
            //��������� ������
            socket.Send(ntpData);

            //�������� �����
            socket.Receive(ntpData);
            socket.Close();
        }

        //�������� ��� �������� � ����� � ��������
        const byte serverReplyTime = 40;

        //�������� ������ ����� ������� � ��������� ���� � �����
        ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

        //�������� ��������� ����� � ��������� ���� � �����
        ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

        //������ ������� ������ big-endian � little-endian
        intPart = SwapEndianness(intPart);
        fractPart = SwapEndianness(fractPart);

        //�������� ���-�� �����������, ��������� � 1900 ����
        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

        //��������� ����� � **UTC**
        var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);


        return networkDateTime.ToLocalTime();
    }

    static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                       ((x & 0x0000ff00) << 8) + ((x & 0x00ff0000) >> 8) +
                       ((x & 0xff000000) >> 24));
    }
}
