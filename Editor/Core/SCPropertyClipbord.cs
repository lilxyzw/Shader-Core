using System;
using System.Collections.Generic;
using System.Linq;

namespace jp.lilxyzw.shadercore
{
    public struct SCPropertyClipboard
    {
        public string name;
        public string type;
        public float x;
        public float y;
        public float z;
        public float w;
        public int ix;
        public int iy;
        public int iz;
        public int iw;
        public uint ux;
        public uint uy;
        public uint uz;
        public uint uw;
        public string reference;

        public SCPropertyClipboard(string value)
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
            ix = 0;
            iy = 0;
            iz = 0;
            iw = 0;
            ux = 0;
            uy = 0;
            uz = 0;
            uw = 0;
            reference = null;

            try
            {
                var values = value.Split('>');
                name = values[0];
                type = values[1];
                switch (type)
                {
                    case "float": x = float.Parse(values[2]); break;
                    case "float4": x = float.Parse(values[2]); y = float.Parse(values[3]); z = float.Parse(values[4]); w = float.Parse(values[5]); break;
                    case "int": ix = int.Parse(values[2]); break;
                    case "int4": ix = int.Parse(values[2]); iy = int.Parse(values[3]); iz = int.Parse(values[4]); iw = int.Parse(values[5]); break;
                    case "uint": ux = uint.Parse(values[2]); break;
                    case "uint4": ux = uint.Parse(values[2]); uy = uint.Parse(values[3]); uz = uint.Parse(values[4]); uw = uint.Parse(values[5]); break;
                    case "reference": reference = values[2]; break;
                    default: break;
                }
            }
            catch
            {
                throw new Exception($"Invalid clipbord text: {value}");
            }
        }

        public override readonly string ToString()
        {
            return $"{name}>{type}>" + type switch
            {
                "float" => x,
                "float4" => $"{x}>{y}>{z}>{w}",
                "int" => ix,
                "int4" =>  $"{ix}>{iy}>{iz}>{iw}",
                "uint" => ux,
                "uint4" =>  $"{ux}>{uy}>{uz}>{uw}",
                "reference" => reference,
                _ => ""
            };
        }

        public static string ToText(IEnumerable<SCPropertyClipboard> values)
        {
            return string.Join('?', values);
        }

        public static SCPropertyClipboard[] FromText(string value)
        {
            return value.Split('?').Select(t => new SCPropertyClipboard(t)).ToArray();
        }
    }
}
