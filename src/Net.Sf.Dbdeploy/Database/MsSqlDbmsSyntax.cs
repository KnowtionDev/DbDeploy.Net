using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class MsSqlDbmsSyntax : DbmsSyntax
    {
        public override string GenerateTrace(string format, params object[] args)
        {
            var trace = String.Format(format, args).Replace("'", "''");
            return string.Format("PRINT N'{0}'", trace);
        }

        public override string GenerateScriptHeader()
        {
            return string.Empty;
        }

        public override string GenerateTimestamp()
        {
            return "getdate()";
        }

        public override string GenerateUser()
        {
            return "user_name()";
        }

        public override string GenerateStatementDelimiter()
        {
            return Environment.NewLine + "GO";
        }

        public override string GenerateCommit()
        {
            return string.Empty;
        }

    	public override string GenerateBeginTransaction()
    	{
    		return "BEGIN TRANSACTION" + Environment.NewLine;
    	}

    	public override string GenerateCommitTransaction()
    	{
    		return Environment.NewLine + "COMMIT TRANSACTION";
    	}
    }
}