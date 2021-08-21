using System;

public class helperFunctions
{
    /// LOOKUP TABLE FOR BYTE TO HEX STRING CONVERSION (A FAST WAY)
    private readonly uint[] _lookup32;

    public helperFunctions()
    {
        _lookup32 = CreateLookup32();
    }

    private uint[] CreateLookup32()
    {
        var result = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            string s = i.ToString("X2");
            result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
        }
        return result;
    }

    public string ByteArrayToHexViaLookup32(byte[] bytes)
    {
        var lookup32 = _lookup32;
        var result = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            var val = lookup32[bytes[i]];
            result[2*i] = (char)val;
            result[2*i + 1] = (char) (val >> 16);
        }
        return new string(result);
    }

    public string ToHexString(float f)
    {
        var bytes = BitConverter.GetBytes(f);
        var i = BitConverter.ToInt32(bytes, 0);
        return i.ToString("X8");
    }

    public string ToMsgHexValue(varType_e valueType, string displayValue, string endianness)
    {
        string hexValue, msg;

        switch (valueType)
        { 
            case varType_e.eFloat:
                float f;
                float.TryParse(displayValue.Replace('.', ','), out f);
                hexValue = ToHexString(f);
                if (endianness=="little")
                {
                    hexValue = hexValue.Substring(6, 2) + hexValue.Substring(4, 2) + hexValue.Substring(2, 2) + hexValue.Substring(0, 2);
                }                
                msg = "f" + hexValue;
                break;

            case varType_e.eU32:
                UInt32 U32 = Convert.ToUInt32(displayValue);
                hexValue = U32.ToString("X8");
                if (endianness=="little")
                {
                    hexValue = hexValue.Substring(6, 2) + hexValue.Substring(4, 2) + hexValue.Substring(2, 2) + hexValue.Substring(0, 2);
                }
                msg = "W" + hexValue;            
                break;

            case varType_e.eI32:
                Int32 I32 = Convert.ToInt32(displayValue);
                hexValue = I32.ToString("X8");
                if (endianness=="little")
                {
                    hexValue = hexValue.Substring(6, 2) + hexValue.Substring(4, 2) + hexValue.Substring(2, 2) + hexValue.Substring(0, 2);
                }
                msg = "w" + hexValue;                
                break;

            case varType_e.eU16:
                UInt16 U16 = Convert.ToUInt16(displayValue);
                hexValue = U16.ToString("X4");
                if (endianness=="little")
                {
                    hexValue = hexValue.Substring(2, 2) + hexValue.Substring(0, 2);
                }
                msg = "I" + hexValue;
                break;

            case varType_e.eI16:
                Int16 I16 = Convert.ToInt16(displayValue);
                hexValue = I16.ToString("X4");
                if (endianness=="little")
                {
                    hexValue = hexValue.Substring(2, 2) + hexValue.Substring(0, 2);
                }
                msg = "i" + hexValue;                         
                break;

            case varType_e.eU8:
                Byte U8;
                Byte.TryParse(displayValue, out U8);
                hexValue = U8.ToString("X2");
                msg = "B" + hexValue; 
                break;

            case varType_e.eI8:
                SByte I8 = Convert.ToSByte(displayValue);
                hexValue = I8.ToString("X2");
                msg = "b" + hexValue;            
                break;

            case varType_e.eUnknown:
                msg ="";
                break;

            default:
                msg ="";
                break;                   
        }
        return msg;
    }
}
