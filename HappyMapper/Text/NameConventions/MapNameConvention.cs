using System;
using AutoMapper.ConfigurationAPI;
using AutoMapper.Extended.Net4;

namespace HappyMapper.Text
{
    public class MapNameConvention
    {
        public string SrcParam { get; set; }
        public string DestParam { get; set; }
        public string Method { get; set; }
    }
}