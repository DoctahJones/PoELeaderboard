using PoELeaderboard.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using PoELeaderboard.APIRequests;
using System.Threading;

namespace PoELeaderboard.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ICommand updateLeaguesCommand;
        private ICommand characterListClickCommand;
        private ICommand refreshTimeCommand;
        private ICommand requestPeriodCommand;

        private int progressBarMaxValue = 100;
        private int progressBarCurrentValue = 0;
        private string progressBarText = "";

        private int timeToRefetchLeaderboard;


        //have a list of tabbed char models being tracked as well as curentyl active one.
        private LeaderboardViewModel currentViewModel;
        private ObservableCollection<LeaderboardViewModel> characterViewModels;

        private RateLimitedAPIRequestDispatcher requestDispatcher;

        private ILeagueListUpdater leagueListUpdater;


        public LeaderboardCollection LeaderboardCollection { get; private set; }

        public int ProgressBarMaxValue
        {
            get { return progressBarMaxValue; }
            set
            {
                progressBarMaxValue = value;
                NotifyPropertyChangedEvent("ProgressBarMaxValue");
            }
        }

        public DialogDisplayer DialogDisplayer { get; set; }

        public int ProgressBarCurrentValue
        {
            get { return progressBarCurrentValue; }
            set
            {
                progressBarCurrentValue = value;
                NotifyPropertyChangedEvent("ProgressBarCurrentValue");
            }
        }

        public string ProgressBarText
        {
            get { return progressBarText; }
            set
            {
                progressBarText = value;
                NotifyPropertyChangedEvent("ProgressBarText");
            }
        }

        public LeaderboardViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                if (currentViewModel != value)
                {
                    currentViewModel = value;
                    NotifyPropertyChangedEvent("CurrentViewModel");
                }
            }
        }

        public ObservableCollection<LeaderboardViewModel> CharacterViewModels
        {
            get
            {
                if (characterViewModels == null)
                {
                    characterViewModels = new ObservableCollection<LeaderboardViewModel>();
                }
                return characterViewModels;
            }
        }

        private void CharacterViewModelsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChangedEvent("CharacterViewModels");
        }

        public ICommand UpdateLeaguesCommand
        {
            get
            {
                if (updateLeaguesCommand == null)
                {
                    updateLeaguesCommand = new RelayCommand(command => this.UpdateLeaguesList());
                }
                return updateLeaguesCommand;
            }
        }

        public ICommand CharacterListClickCommand
        {
            get
            {
                if (characterListClickCommand == null)
                {
                    characterListClickCommand = new RelayCommand(command => this.CharacterListItemClicked(command));
                }
                return characterListClickCommand;
            }
        }

        public ICommand RefreshTimeCommand
        {
            get
            {
                if (refreshTimeCommand == null)
                {
                    refreshTimeCommand = new RelayCommand(command => this.RefreshTimeMenuClicked(command));
                }
                return refreshTimeCommand;
            }
        }

        public ICommand RequestPeriodCommand
        {
            get
            {
                if (requestPeriodCommand == null)
                {
                    requestPeriodCommand = new RelayCommand(command => this.RequestPeriodMenuClicked(command));
                }
                return requestPeriodCommand;
            }
        }



        public MainViewModel()
        {
            DialogDisplayer = new DialogDisplayer(this);
            timeToRefetchLeaderboard = 5;

            requestDispatcher = new RateLimitedAPIRequestDispatcher(TimeSpan.FromSeconds(2.5));

            var updater = new LeaderboardUpdater(this.requestDispatcher);
            updater.LeaderboardUpdateProgress += LeaderboardUpdateProgressed;
            LeaderboardCollection = new LeaderboardCollection(updater);

            this.leagueListUpdater = new LeagueListUpdater(this.requestDispatcher);

            characterViewModels = new ObservableCollection<LeaderboardViewModel>();
            characterViewModels.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(CharacterViewModelsCollectionChanged);
            var lvm = AddNewEmptyLeaderboardTab();
            CurrentViewModel = lvm;
        }


        public async Task<Leaderboard> UpdateLeaderboardAsync(League league, string type, int difficulty)
        {
            Leaderboard lb = LeaderboardCollection.GetLeaderboard(league, type, difficulty);
            
            if (lb != null && (DateTime.Now - lb.LastTimeUpdated).TotalMinutes < timeToRefetchLeaderboard)
            {
                return lb;
            }
            else
            {
                ChangeLeaderboardViewModelsSearchButtons(true);
                Exception ex = null;
                try
                {
                    await LeaderboardCollection.GetFullLeaderboardFromWeb(league, type, difficulty);
                }
                catch (Exception e)
                {
                    ex = e;
                }
                if (ex != null)
                {
                    await ShowErrorMessageOnCaughtException(ex, "UpdateLeaderboardAsync");
                    ChangeLeaderboardViewModelsSearchButtons(false);
                    return null;
                }
                else
                {
                    ChangeLeaderboardViewModelsSearchButtons(false);
                    return LeaderboardCollection.GetLeaderboard(league, type, difficulty);
                }
            }
        }

        public void LeaderboardUpdateProgressed(object sender, LeaderboardUpdateProgressEventArgs e)
        {
            if (e != null)
            {
                ProgressBarMaxValue = e.UpdatesTotal;
                ProgressBarCurrentValue = e.UpdatesCompleted;
                switch (e.LeaderboardUpdateState)
                {
                    case LeaderboardUpdateActionState.FETCHING_DETAILS:
                        ProgressBarText = "Fetching league details for " + e.LeagueUpdated.Name + " . .";
                        break;
                    case LeaderboardUpdateActionState.UPDATING:
                        ProgressBarText = String.Format("Updating {0} . . {1}/{2}", e.LeagueUpdated.Name, e.UpdatesCompleted, e.UpdatesTotal);
                        break;
                    case LeaderboardUpdateActionState.RETRYING:
                        ProgressBarText = String.Format("Retrying failed updates for {0} . . {1}/{2}", e.LeagueUpdated.Name, e.UpdatesCompleted, e.UpdatesTotal);
                        break;
                    case LeaderboardUpdateActionState.COMPLETED:
                        var textCompleted = String.Format("Completed update for {0} {1}/{2}", e.LeagueUpdated.Name, e.UpdatesCompleted, e.UpdatesTotal);
                        ProgressBarText = textCompleted;
                        ClearProgressBarAfterDelayIfUnchanged(e.UpdatesTotal, e.UpdatesCompleted, textCompleted, 7500);
                        break;
                    case LeaderboardUpdateActionState.CANCELLED:
                        var textCancelled = String.Format("Cancelled update for {0}.", e.LeagueUpdated.Name);
                        ProgressBarText = textCancelled;
                        ClearProgressBarAfterDelayIfUnchanged(e.UpdatesTotal, e.UpdatesCompleted, textCancelled, 5000);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ClearProgressBarAfterDelayIfUnchanged(int initialMaxValue, int initalCurrentValue, string initialText, int delayMs)
        {
            Task.Delay(delayMs).ContinueWith(_ =>
            {
                if (ProgressBarMaxValue == initialMaxValue && ProgressBarCurrentValue == initalCurrentValue && ProgressBarText == initialText) //progress bar hasnt changed after delay.
                {
                    ProgressBarMaxValue = 1;
                    ProgressBarCurrentValue = 0;
                    ProgressBarText = "";
                }
            });
        }

        public async void UpdateLeaguesList()
        {
            List<League> leagues = null;
            Exception exception = null;
            ProgressBarMaxValue = 2;
            ProgressBarCurrentValue = 0;
            var text = "Updating League list . . . ";
            ProgressBarText = text;
            try
            {
                leagues = await this.leagueListUpdater.GetLeagueList();
            }
            catch (Exception e)
            {
                exception = e;
            }
            if (exception != null)
            {
                await ShowErrorMessageOnCaughtException(exception, "UpdateLeaguesList");
                ClearProgressBarAfterDelayIfUnchanged(2, 0, text, 1000);
                return;
            }
            ProgressBarCurrentValue = 1;
            if (leagues.Count > 0 && !CompareLeagueListsForEquality(leagues, characterViewModels.ElementAt(0).Leagues.ToList()))
            {
                foreach (var vm in characterViewModels)
                {
                    var newLeagues = new ObservableCollection<League>(leagues);
                    newLeagues.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(vm.LeaguesCollectionChanged);
                    vm.Leagues = newLeagues;
                }
            }
            ProgressBarCurrentValue = 2;
            text = "League list updated";
            ProgressBarText = text;
            ClearProgressBarAfterDelayIfUnchanged(2, 2, text, 7500);
        }

        private async Task ShowErrorMessageOnCaughtException(Exception exception, string currentMethod)
        {
            var PoEEx = exception as PoEApiException;
            if (PoEEx != null)
            {
                await this.DialogDisplayer.ShowMessageAsync(PoEEx.Message, PoEEx.Error.error.message);
                return;
            }
            var webEx = exception as WebException;
            if (webEx != null)
            {
                await this.DialogDisplayer.ShowMessageAsync("A web error occurred in " + currentMethod, webEx.Message);
                return;
            }
            var httpEx = exception as System.Net.Http.HttpRequestException;
            if (httpEx != null)
            {
                await this.DialogDisplayer.ShowMessageAsync("A web error occurred in " + currentMethod, httpEx.Message);
                return;
            }
            var timeoutEx = exception as TaskCanceledException;
            if (timeoutEx != null)
            {
                await this.DialogDisplayer.ShowMessageAsync("A web request in " + currentMethod + " timed out.", timeoutEx.Message);
                return;
            }
            var cancelEx = exception as OperationCanceledException;
            if (cancelEx != null)
            {
                return;
            }
            throw exception;
        }

        private bool CompareLeagueListsForEquality(List<League> firstList, List<League> secondList)
        {
            if (firstList.Count != secondList.Count)
            {
                return false;
            }
            if (Enumerable.SequenceEqual(firstList.OrderBy(t => t.Name), secondList.OrderBy(t => t.Name)))
            {
                return true;
            }
            return false;
        }

        private void CharacterListItemClicked(object parameter)
        {
            var vm = parameter as LeaderboardViewModel;
            if (vm != null)
            {
                if (vm.CharacterSearchedForInLeaderboard.CharacterName == "New" && vm.CharacterSearchedForInLeaderboard.CharacterClass == "New")
                {
                    var newLvm = AddNewEmptyLeaderboardTab();
                    ChangeCurrentLeaderboardViewModel(vm);

                    var newChar = new Character();
                    vm.CharacterSearchedForInLeaderboard = newChar;
                }
                else
                {
                    ChangeCurrentLeaderboardViewModel(vm);
                }
            }
        }

        private void RefreshTimeMenuClicked(object command)
        {
            var text = command as string;
            if (text != null)
            {
                switch (text)
                {
                    case "Always":
                        this.timeToRefetchLeaderboard = 0;
                        break;
                    case "2m":
                        this.timeToRefetchLeaderboard = 2;
                        break;
                    case "5m":
                        this.timeToRefetchLeaderboard = 5;
                        break;
                    case "10m":
                        this.timeToRefetchLeaderboard = 10;
                        break;
                    case "30m":
                        this.timeToRefetchLeaderboard = 30;
                        break;

                    default:
                        break;
                }
            }
        }

        private void RequestPeriodMenuClicked(object command)
        {
            var text = command as string;
            if (text != null)
            {
                switch (text)
                {
                    case "2.5s":
                        this.requestDispatcher.DelayPeriod = TimeSpan.FromSeconds(2.5);
                        break;
                    case "3s":
                        this.requestDispatcher.DelayPeriod = TimeSpan.FromSeconds(3);
                        break;
                    case "3.5s":
                        this.requestDispatcher.DelayPeriod = TimeSpan.FromSeconds(3.5);
                        break;
                    case "4s":
                        this.requestDispatcher.DelayPeriod = TimeSpan.FromSeconds(4);
                        break;
                    case "4.5s":
                        this.requestDispatcher.DelayPeriod = TimeSpan.FromSeconds(4.5);
                        break;
                    case "5s":
                        this.requestDispatcher.DelayPeriod = TimeSpan.FromSeconds(5);
                        break;
                    default:
                        break;
                }
            }
        }

        public LeaderboardViewModel AddNewEmptyLeaderboardTab()
        {
            LeaderboardViewModel newLvm;
            if (this.characterViewModels.Count > 0)
            {
                IEnumerable<League> leagues = this.characterViewModels.ElementAt(0).Leagues;
                newLvm = new LeaderboardViewModel(this, leagues);
                newLvm.SearchCancelButtonText = this.characterViewModels.ElementAt(0).SearchCancelButtonText;
            }
            else
            {
                newLvm = new LeaderboardViewModel(this);
            }
            newLvm.CharacterSearchedForInLeaderboard = Character.NewDummyCharacter();
            CharacterViewModels.Add(newLvm);
            return newLvm;
        }

        private void ChangeCurrentLeaderboardViewModel(LeaderboardViewModel lvm)
        {
            this.CurrentViewModel = lvm;
        }

        public void RemoveLeaderboardViewModel(LeaderboardViewModel lvm)
        {
            if (this.CharacterViewModels.Count > 1
                && lvm.CharacterSearchedForInLeaderboard.CharacterName != "New"
                && lvm.CharacterSearchedForInLeaderboard.CharacterClass != "New")
            {
                if (CurrentViewModel == lvm)
                {
                    CurrentViewModel = CharacterViewModels.Where(vm => vm != lvm).First();
                }
                this.CharacterViewModels.Remove(lvm);
            }
        }

        private void ChangeLeaderboardViewModelsSearchButtons(bool searching)
        {
            string text;
            if (searching)
            {
                text = "Cancel";
            }
            else
            {
                text = "Search";
            }
            foreach (var lvm in CharacterViewModels)
            {
                lvm.SearchCancelButtonText = text;
            }
        }

        public void CancelUpdates()
        {
            LeaderboardCollection.CancelUpdates();
            ChangeLeaderboardViewModelsSearchButtons(false);
        }
    }

}
