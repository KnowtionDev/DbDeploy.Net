using System;
using System.Globalization;
using System.IO;
using System.Text;

using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class ChangeScriptExecuter
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        private readonly TextWriter output;
    	private readonly IDbmsSyntax _dbmsSyntax;
    	private readonly bool _useTransaction;
        private readonly Encoding _encoding;

        public ChangeScriptExecuter(TextWriter printStream, IDbmsSyntax dbmsSyntax, bool useTransaction,
                                    Encoding encoding)
        {
            output = printStream;
        	_dbmsSyntax = dbmsSyntax;
    		_useTransaction = useTransaction;
            _encoding = encoding;
            /* Header data: information and control settings for the entire script. */
            DateTime now = DateTime.Now;
            output.WriteLine("-- Script generated at " + now.ToString(new DateTimeFormatInfo().SortableDateTimePattern));
            output.WriteLine();
            output.WriteLine(dbmsSyntax.GenerateScriptHeader());
        }

        public ChangeScriptExecuter(TextWriter printStream, IDbmsSyntax dbmsSyntax, bool useTransaction)
            : this(printStream, dbmsSyntax, useTransaction, DefaultEncoding) {}

        public void ApplyChangeDoScript(ChangeScript script)
        {
            output.WriteLine();
            output.WriteLine("-- Change script: " + script);

			if (_useTransaction)
				output.WriteLine(_dbmsSyntax.GenerateBeginTransaction());

        	CopyFileDoContentsToStdOut(script.GetFile());
			
			if (_useTransaction) {
                output.WriteLine(_dbmsSyntax.GenerateStatementDelimiter());
                output.WriteLine(_dbmsSyntax.GenerateCommitTransaction());
            }
        }

        public void ApplyChangeUndoScript(ChangeScript script)
        {
            output.WriteLine();
            output.WriteLine("-- Change script: " + script);
			
			if (_useTransaction)
				output.WriteLine(_dbmsSyntax.GenerateBeginTransaction());

			CopyFileUndoContentsToStdOut(script.GetFile());

			if (_useTransaction)
				output.WriteLine(_dbmsSyntax.GenerateCommitTransaction());
		}

        private void CopyFileDoContentsToStdOut(FileSystemInfo file)
        {
            using (TextReader input = DetectEncodingAndOpenText(file.FullName))
            {
                string str;
                while ((str = input.ReadLine()) != null && !IsUndoToken(str))
                {
                    output.WriteLine(str);
                }
            }
        }

        private TextReader DetectEncodingAndOpenText(string fullName)
        {
            return new EncodingManager(_encoding).GetInputStream(fullName);
        }

        private static bool IsUndoToken(string text)
        {
            return "--//@UNDO" == text.Trim();
        }

        private void CopyFileUndoContentsToStdOut(FileSystemInfo file)
        {
            using (TextReader input = DetectEncodingAndOpenText(file.FullName))
            {
                string str;
                while ((str = input.ReadLine()) != null)
                {
                    if (IsUndoToken(str))
                    {
                        // Just keep looping until we find the magic "--//@UNDO"
                        break;
                    }
                }
                while ((str = input.ReadLine()) != null)
                {
                    output.WriteLine(str);
                }
            }
        }

        /* Should be called *after* insert of script contents. */

        public void ApplyDeltaFragmentHeaderOrFooterSql(string sql)
        {
            output.WriteLine();
            output.WriteLine(sql);
        }
    }
}
