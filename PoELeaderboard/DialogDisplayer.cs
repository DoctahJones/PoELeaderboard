using MahApps.Metro.Controls.Dialogs;
using PoELeaderboard.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoELeaderboard
{
    public class DialogDisplayer
    {
        private MainViewModel mainViewModel;

        private IDialogCoordinator dialogCoordinator;

        public DialogDisplayer(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            dialogCoordinator = DialogCoordinator.Instance;
        }

        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative)
        {
             return await this.dialogCoordinator.ShowMessageAsync(this.mainViewModel, title, message, style);
        }
        

    }
}
