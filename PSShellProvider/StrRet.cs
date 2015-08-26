using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSShellProvider
{
    internal unsafe struct StrRetNative
    {
        public enum StrRetType {
            WStr	= 0,
            Offset	= 0x1,
            CStr	= 0x2
        }
        public StrRetType uType;
        public fixed byte cStr[260];
    }

    internal class StrRet
    {
        private string value;
        private int offset;

        public unsafe StrRet(StrRetNative value)
        {
            IntPtr dataStart = (IntPtr)(&value.cStr[0]);
            switch (value.uType)
            {
                case StrRetNative.StrRetType.CStr:
                    this.value = Marshal.PtrToStringAnsi(dataStart, 260); //MAX_PATH
                    return;
                case StrRetNative.StrRetType.WStr:
                    IntPtr strAddr = *(IntPtr*)(dataStart + sizeof(IntPtr) - sizeof(int));
                    try
                    {
                        this.value = Marshal.PtrToStringUni(strAddr);
                        return;
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(strAddr);
                    }
                case StrRetNative.StrRetType.Offset:
                    offset = *(int*)dataStart;
                    return;
                default:
                    throw new ArgumentException(String.Format("Invalid type of StrRet: {0}", (int)value.uType), "pNativeData");
            }
        }

        public StrRet(string value)
        {
            this.value = value;
        }

        public StrRet(int offset)
        {
            this.offset = offset;
        }

        public string GetValue(IdList list)
        {
            if (value != null)
                return value;
            int curOffset = 0;
            int idx = 0;
            while (curOffset < offset)
            {
                byte[] curId = list.Parts[idx].Value;
                if (curOffset + 2 + curId.Length > offset)
                {
                    return GetValueFrom(list, idx, offset-curOffset);
                }
                curOffset += curId.Length + 2;
                idx++;
            }
            throw new ArgumentException(String.Format("The offset {0} is not within the IdList", offset), "list");
        }

        private string GetValueFrom(IdList list, int startIdx, int startOffset)
        {
            StringBuilder sb = new StringBuilder();
            int innerOffset = startOffset - 2;
            int idIdx = startIdx;
            List<byte> ansiStr = new List<byte>();
            while (true)
            {
                byte[] curId = list.Parts[idIdx].Value;
                byte[] cb = BitConverter.GetBytes((ushort)(curId.Length + sizeof(ushort)));
                for (; innerOffset < curId.Length; innerOffset++)
                {
                    byte b;
                    if (innerOffset < 0)
                        b = cb[innerOffset + 2];
                    else
                        b = curId[innerOffset];
                    if (b == 0)
                    {
                        return Encoding.Default.GetString(ansiStr.ToArray());
                    }
                    else
                    {
                        ansiStr.Add(b);
                    }
                }
                innerOffset = -2;
                idIdx++;
            }
        }
    }
}
