using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Helper
{
    public class FileEncodingHelper
    {
        public static Encoding GetEncoding(string filePath)
        {
            // 读取文件前三个字节
            var buffer = new byte[3];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, 3);
            }

            return buffer[0] switch
            {
                // 判断编码格式
                0xEF when buffer[1] == 0xBB && buffer[2] == 0xBF => Encoding.UTF8,
                0xFE when buffer[1] == 0xFF => Encoding.BigEndianUnicode,
                0xFF when buffer[1] == 0xFE => Encoding.Unicode,
                _ => Encoding.Default
            };
        }
    }
}
