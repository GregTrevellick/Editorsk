using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Editorsk
{
    abstract class BaseCommand<T> where T : BaseCommand<T>, new()
    {
        public static T Instance { get; private set; }
        public DTE2 DTE { get; private set; }

        private OleMenuCommandService CommandService { get; set; }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            Instance = new T
            {
                DTE = (DTE2)serviceProvider.GetService(typeof(DTE)),
                CommandService = (OleMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService))
            };

            Instance.SetupCommands();
        }

        protected abstract void SetupCommands();

        protected void RegisterCommand(CommandID commandId, Action action)
        {
            var menuCommand = new OleMenuCommand((s, e) => action(), commandId);
            CommandService.AddCommand(menuCommand);
        }

        protected void RegisterCommand(Guid commandGuid, int commandId, Action action)
        {
            var cmd = new CommandID(commandGuid, commandId);
            RegisterCommand(cmd, action);
        }

        public TextDocument GetTextDocument()
        {
            return DTE.ActiveDocument?.Object("TextDocument") as TextDocument;
        }

        public IDisposable UndoContext(string name)
        {
            DTE.UndoContext.Open(name);
            return new Disposable(DTE.UndoContext.Close);
        }

        public IEnumerable<string> GetSelectedLines(TextDocument document)
        {
            // TODO: Use document.Selection.TextRanges instead
            return document.Selection.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
