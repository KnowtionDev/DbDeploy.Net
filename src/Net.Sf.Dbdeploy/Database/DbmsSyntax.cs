using System.Text;

namespace Net.Sf.Dbdeploy.Database
{
    public interface IDbmsSyntax
    {
        string GenerateTrace(string format, params object[] args);

        string GenerateScriptHeader();

        string GenerateTimestamp();

        string GenerateUser();

        string GenerateStatementDelimiter();

        string GenerateCommit();

    	string GenerateBeginTransaction();
    	
		string GenerateCommitTransaction();

		string GenerateVersionCheck(string tableName, string currentVersion, string deltaSet);
    }

	public abstract class DbmsSyntax : IDbmsSyntax
	{
	    public virtual string GenerateTrace(string format, params object[] args)
	    {
	        return string.Empty;
	    }

	    public abstract string GenerateScriptHeader();
		public abstract string GenerateTimestamp();
		public abstract string GenerateUser();
		public abstract string GenerateStatementDelimiter();
		public abstract string GenerateCommit();
		public abstract string GenerateBeginTransaction();
		public abstract string GenerateCommitTransaction();

		public virtual string GenerateVersionCheck(string tableName, string currentVersion, string deltaSet)
		{
			var builder = new StringBuilder();
			builder.AppendLine("DECLARE @currentDatabaseVersion INTEGER, @errMsg VARCHAR(1000)");
			builder.Append("SELECT @currentDatabaseVersion = MAX(change_number) FROM ").Append(tableName).AppendLine(" WHERE delta_set = '" + deltaSet + "'");
			builder.Append("IF (@currentDatabaseVersion <> ").Append(currentVersion).AppendLine(")");
			builder.AppendLine("BEGIN");
			builder.Append("    SET @errMsg = 'Error: current database version on delta_set <").Append(deltaSet).Append("> is not ").Append(currentVersion).AppendLine(", but ' + CONVERT(VARCHAR, @currentDatabaseVersion)");
			builder.AppendLine("    RAISERROR (@errMsg, 16, 1)");
			builder.AppendLine("END");
			builder.AppendLine(GenerateStatementDelimiter()).AppendLine();
			return builder.ToString();
		}
	}
}