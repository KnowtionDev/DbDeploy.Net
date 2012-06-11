using System.IO;
using System.Text;

namespace Net.Sf.Dbdeploy
{
    public class EncodingManager
    {
        protected readonly Encoding encoding;

        public EncodingManager(string encodingName) : this(encodingName, Encoding.UTF8) {}

        public EncodingManager(string encodingName, Encoding defaultEncoding)
            : this(GetEncoding(encodingName, defaultEncoding)) {}

        public EncodingManager(Encoding encoding)
        {
            this.encoding = encoding;
        }

        private static Encoding GetEncoding(string encodingName, Encoding defaultEncoding)
        {
            var result = defaultEncoding;
            if (!(string.IsNullOrEmpty(encodingName) || string.IsNullOrEmpty(encodingName.Trim())))
            {
                Encoding.GetEncoding(encodingName, result.EncoderFallback, result.DecoderFallback);
            }
            return result;
        }

        public StreamWriter GetOutputStream(string fullName, bool append = false)
        {
            return new StreamWriter(fullName, append, this.encoding);
        }

        public TextReader GetInputStream(string fullName)
        {
            return new StreamReader(fullName, this.encoding);
        }
    }
}