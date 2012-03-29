using System;
using System.Collections.Generic;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class Controller
    {
        private readonly DatabaseSchemaVersionManager schemaManager;
        private readonly ChangeScriptExecuter changeScriptExecuter;
        private readonly ChangeScriptRepository changeScriptRepository;
        private readonly PrettyPrinter prettyPrinter = new PrettyPrinter();

        public Controller(DatabaseSchemaVersionManager schemaManager,
                          ChangeScriptRepository changeScriptRepository,
                          ChangeScriptExecuter changeScriptExecuter)
        {
            this.schemaManager = schemaManager;
            this.changeScriptRepository = changeScriptRepository;
            this.changeScriptExecuter = changeScriptExecuter;
        }

        public void ProcessDoChangeScripts(Int64 lastChangeToApply, List<Int64> appliedChanges)
        {
            if (lastChangeToApply != Int64.MaxValue)
            {
                Info("Only applying changes up and including change script #" + lastChangeToApply);
            }
            Info("Changes currently applied to database:\n  " + prettyPrinter.Format(appliedChanges));

            List<ChangeScript> doChangeScripts = changeScriptRepository.GetOrderedListOfDoChangeScripts();
            Info("Scripts available:\n  " + prettyPrinter.FormatChangeScriptList(doChangeScripts));

            changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateVersionCheck());
            List<Int64> changesToApply = LoopThruDoScripts(lastChangeToApply, doChangeScripts, appliedChanges);
            Info("To be applied:\n  " + prettyPrinter.Format(changesToApply));
        }

        public void ProcessUndoChangeScripts(Int64 lastChangeToApply, List<Int64> appliedChanges)
        {
            List<ChangeScript> undoChangeScripts = changeScriptRepository.GetOrderedListOfUndoChangeScripts();
            LoopThruUndoScripts(lastChangeToApply, undoChangeScripts, appliedChanges);
        }

        private List<Int64> LoopThruDoScripts(Int64 lastChangeToApply, IEnumerable<ChangeScript> doChangeScripts, ICollection<Int64> appliedChanges)
        {
            List<Int64> changesToApply = new List<Int64>();
            foreach (ChangeScript changeScript in doChangeScripts)
            {
                Int64 changeScriptId = changeScript.GetId();

                if (changeScriptId <= lastChangeToApply && !appliedChanges.Contains(changeScriptId))
                {
                    changesToApply.Add(changeScriptId);

                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateDoDeltaFragmentHeader(changeScript));
                    changeScriptExecuter.ApplyChangeDoScript(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateDoDeltaFragmentFooter(changeScript));
                }
            }
            return changesToApply;
        }

        private void LoopThruUndoScripts(Int64 lastChangeToApply, IEnumerable<ChangeScript> undoChangeScripts, ICollection<Int64> appliedChanges)
        {
            foreach (ChangeScript changeScript in undoChangeScripts)
            {
                Int64 changeScriptId = changeScript.GetId();

                if (changeScriptId <= lastChangeToApply && !appliedChanges.Contains(changeScriptId))
                {
					changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateUndoDeltaFragmentHeader(changeScript));
					changeScriptExecuter.ApplyChangeUndoScript(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateUndoDeltaFragmentFooter(changeScript));
                }
            }
        }

        private static void Info(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}