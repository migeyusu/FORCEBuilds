using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using FORCEBuild.Persistence.Serialization;


namespace FORCEBuild.Net.Base
{
    public static class ProtocolExtension
    {

        public static T GetStruct<T>(this Socket socket) where T : struct
        {
            var len = Marshal.SizeOf(typeof(T));
            var receiveBytes = new byte[len];
            socket.Receive(receiveBytes, len, SocketFlags.None);
            return receiveBytes.ToStruct<T>();
        }


        //public static void SendResponse(this Socket socket, bool issuccess,
        //    byte[] datas)
        //{
        //    var response = new ResponseHead(issuccess, datas.Length);
        //    socket.Send(response.ToBytes());
        //    if (datas.Length!=0) {
        //        socket.Send(datas);
        //    }
        //}

        //public static void SendRequest(this Socket socket, CallType callType,
        //    byte[] datas)
        //{
        //    var requestHead = new RequestHead(callType, datas.Length);
        //    socket.Send(requestHead.ToBytes());
        //    socket.Send(datas);
        //}

        public static MemoryStream GetSpecificLenStream(this Socket socket, int count)
        {
            var bufferBytes = new byte[2048];
            var receive = 0;
            var receiveStream = new MemoryStream();
            while (receive < count)
            {
                var read = socket.Receive(bufferBytes);
                receive += read;
                receiveStream.Write(bufferBytes, 0, read);
            }
            receiveStream.Seek(0, SeekOrigin.Begin);
            return receiveStream;
        }
    }
}