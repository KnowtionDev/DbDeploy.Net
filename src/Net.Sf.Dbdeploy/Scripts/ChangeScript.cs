using System;
using System.IO;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class ChangeScript : IComparable
    {
        private readonly Int64 id;
        private readonly FileInfo file;
        private readonly String description;

        public ChangeScript(Int64 id)
            : this(id, "test")
        {
        }

        public ChangeScript(Int64 id, FileInfo file)
        {
            this.id = id;
            this.file = file;
            description = file.Name;
        }

        public ChangeScript(Int64 id, string description)
        {
            this.id = id;
            file = null;
            this.description = description;
        }

        public FileInfo GetFile()
        {
            return file;
        }

        public Int64 GetId()
        {
            return id;
        }

        public String GetDescription()
        {
            return description;
        }

        public int CompareTo(object o)
        {
            ChangeScript other = (ChangeScript) o;
            return id.CompareTo(other.id);
        }

        public override string ToString()
        {
            if (file != null)
                return "#" + id + ": " + file.Name;

            return "#" + id;
        }
    }
}