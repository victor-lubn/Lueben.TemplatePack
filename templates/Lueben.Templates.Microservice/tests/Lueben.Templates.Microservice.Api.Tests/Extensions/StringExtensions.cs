﻿using System.IO;

namespace Lueben.Templates.Microservice.Api.Tests.Extensions
{
    public static class StringExtensions
    {
        public static Stream GenerateStreamFromString(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
