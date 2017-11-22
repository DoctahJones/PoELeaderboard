using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoELeaderboard.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PoELeaderboard.ViewModels
{
    /// <summary>
    /// Class that contains the details about this tabs current leaderboard and character.
    /// </summary>
    public class LeaderboardViewModel : ViewModelBase
    {
        /// <summary>
        /// The parent MainViewModel.
        /// </summary>
        private MainViewModel mainWindowViewModel;
        /// <summary>
        /// The name that the user has type into the character name box.
        /// </summary>
        private string characterName;
        /// <summary>
        /// The list of leagues available to search through.
        /// </summary>
        private ObservableCollection<League> leagues;
        /// <summary>
        /// The currently selected league from the list.
        /// </summary>
        private League selectedLeague;
        /// <summary>
        /// The types of leaderboard that can be searched for.
        /// </summary>
        private IEnumerable<string> leaderboardTypes;
        /// <summary>
        /// The currently selected leaderboard type.
        /// </summary>
        private string selectedLeaderboardType;
        /// <summary>
        /// The list of difficulties that can be searched for.
        /// </summary>
        private IEnumerable<Difficulty> difficulties;
        /// <summary>
        /// The currently selected difficulty from the list.
        /// </summary>
        private Difficulty selectedDifficulty;
        /// <summary>
        /// Whether or not the user can currently choose a difficulty.
        /// </summary>
        private bool difficultiesEnabled = true;
        /// <summary>
        /// The leaderboard that this view model is currently displaying.
        /// </summary>
        private Leaderboard leaderboard;
        /// <summary>
        /// The character that the user is looking for in the list.
        /// </summary>
        private Character characterSearchedForInLeaderboard;
        /// <summary>
        /// The league the user had selected when they clicked the search button.
        /// </summary>
        private string leagueSearchedFor;
        /// <summary>
        /// The league the user had selected when they clicked the search button.
        /// </summary>
        private League leagueSearchedForObject;
        /// <summary>
        /// The type of league the user had selected when they clicked the search button.
        /// </summary>
        private string typeSearchedFor;
        /// <summary>
        /// The difficulty the user had selected when they clicked the search button.
        /// </summary>
        private Difficulty difficultySearchedFor;
        /// <summary>
        /// The last time the leaderboard this view model is displaying was updated.
        /// </summary>
        private DateTime lastTimeUpdated;
        /// <summary>
        /// The text displayed on the search/cancel button.
        /// </summary>
        private string searchCancelButtonText = "Search";
        /// <summary>
        /// The currently selected character in the leaderboard.
        /// </summary>
        private Character selectedCharacterInGrid;
        /// <summary>
        /// The command that is run when the user clicks the search/cancel button.
        /// </summary>
        private ICommand searchCommand;
        /// <summary>
        /// The command that is run when the user clicks the close tab button.
        /// </summary>
        private ICommand closeTabCommand;

        public string CharacterName
        {
            get { return characterName; }
            set
            {
                characterName = value;
                NotifyPropertyChangedEvent("CharacterName");
            }
        }

        public ObservableCollection<League> Leagues
        {
            get { return leagues; }
            set
            {
                leagues = value;
                NotifyPropertyChangedEvent("Leagues");
            }
        }

        public League SelectedLeague
        {
            get { return selectedLeague; }
            set
            {
                selectedLeague = value;
                NotifyPropertyChangedEvent("SelectedLeague");
            }
        }

        public IList<string> LeaderboardTypes
        {
            get { return leaderboardTypes.ToList(); }
        }

        public string SelectedLeaderboardType
        {
            get { return selectedLeaderboardType; }
            set
            {
                selectedLeaderboardType = value;
                NotifyPropertyChangedEvent("SelectedLeaderboardType");
                EnableOrDisableDifficultyBasedOnType(value);
            }
        }

        public IList<Difficulty> Difficulties
        {
            get { return difficulties.ToList(); }

        }

        public Difficulty SelectedDifficulty
        {
            get { return selectedDifficulty; }
            set
            {
                selectedDifficulty = value;
                NotifyPropertyChangedEvent("SelectedDifficulty");
            }
        }

        public bool DifficultiesEnabled
        {
            get { return this.difficultiesEnabled; }
            set
            {
                this.difficultiesEnabled = value;
                NotifyPropertyChangedEvent("DifficultiesEnabled");
            }
        }

        public Leaderboard Leaderboard
        {
            get { return leaderboard; }
            set
            {
                leaderboard = value;
                NotifyPropertyChangedEvent("Leaderboard");
            }
        }

        public Character CharacterSearchedForInLeaderboard
        {
            get
            {
                if (characterSearchedForInLeaderboard == null)
                {
                    return Character.NewDummyCharacter();
                }
                else
                {
                    return characterSearchedForInLeaderboard;
                }
            }
            set
            {
                    characterSearchedForInLeaderboard = value;
                    NotifyPropertyChangedEvent("CharacterSearchedForInLeaderboard");
            }
        }

        public string LeagueSearchedFor
        {
            get
            {
                string returnString = "";
                if (this.leagueSearchedFor != default(string) && CharacterSearchedForInLeaderboard.CharacterName != "")
                {
                    returnString += "\n";
                }
                return returnString += leagueSearchedFor;
            }
            set
            {
                leagueSearchedFor = value;
                NotifyPropertyChangedEvent("LeagueSearchedFor");
            }
        }

        public string SearchCancelButtonText
        {
            get { return searchCancelButtonText; }
            set
            {
                searchCancelButtonText = value;
                NotifyPropertyChangedEvent("SearchCancelButtonText");
            }
        }

        public Character SelectedCharacterInGrid
        {
            get { return this.selectedCharacterInGrid; }
            set
            {
                selectedCharacterInGrid = value;
                NotifyPropertyChangedEvent("SelectedCharacterInGrid");
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                {
                    searchCommand = new RelayCommand(p => this.SearchCancelButtonClicked(p));
                }
                return searchCommand;
            }
        }

        public ICommand CloseTabCommand
        {
            get
            {
                if (closeTabCommand == null)
                {
                    closeTabCommand = new RelayCommand(p => CloseThisTab());
                }
                return closeTabCommand;
            }
        }

        public LeaderboardViewModel(MainViewModel mainWindowViewModel, IEnumerable<League> leagues = null)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            Leagues = leagues == null ? new ObservableCollection<League>() : new ObservableCollection<League>(leagues);
            Leagues.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LeaguesCollectionChanged);

            difficulties = Constants.GetDifficulties();
            SelectedDifficulty = Difficulties.FirstOrDefault();
            leaderboardTypes = Constants.GetLeaderboardTypes();
            SelectedLeaderboardType = LeaderboardTypes.FirstOrDefault();

            this.mainWindowViewModel.LeaderboardCollection.LeaderboardUpdate += LeaderboardCollectionUpdated;
        }

        private void LeaderboardCollectionUpdated(object sender, LeaderboardUpdateEventArgs e)
        {
            if (String.Equals(e.LeagueUpdated.Name, this.leagueSearchedFor, StringComparison.InvariantCultureIgnoreCase) &&
                String.Equals(e.TypeUpdated, this.typeSearchedFor, StringComparison.InvariantCultureIgnoreCase)
                && e.DifficultyUpdated == this.difficultySearchedFor.IntValue)
            {
                var updatedLeaderboard = this.mainWindowViewModel.LeaderboardCollection.GetLeaderboard(e.LeagueUpdated, this.typeSearchedFor, this.difficultySearchedFor.IntValue);

                if (Leaderboard != null && updatedLeaderboard.LastTimeUpdated == Leaderboard.LastTimeUpdated)
                {
                    return;
                }
                Leaderboard = updatedLeaderboard;
                this.lastTimeUpdated = e.TimeUpdated;
            }
        }

        public void LeaguesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChangedEvent("Leagues");
        }

        private void EnableOrDisableDifficultyBasedOnType(string value)
        {
            if (String.Equals(value, "League", StringComparison.InvariantCultureIgnoreCase))
            {
                DifficultiesEnabled = false;
            }
            else
            {
                DifficultiesEnabled = true;
            }
        }

        private void SearchCancelButtonClicked(object param)
        {
            var p = param as string;
            if (p != null)
            {
                if (p == "Search")
                {
                    SearchForCharacterAsync();
                }
                if (p == "Cancel")
                {
                    this.mainWindowViewModel.CancelUpdates();
                }
            }
        }

        private async void SearchForCharacterAsync()
        {
            if (!await IsLeagueFilledAndDisplayWarningIfNotAsync())
            {
                return;
            }
            if (this.CharacterSearchedForInLeaderboard.CharacterClass == "New" && this.CharacterSearchedForInLeaderboard.CharacterClass == "New")
            {
                this.mainWindowViewModel.AddNewEmptyLeaderboardTab();
            }
            SetLocalSearchedForValues();

            var l = await this.mainWindowViewModel.UpdateLeaderboardAsync(this.leagueSearchedForObject, this.typeSearchedFor.ToLower(), this.SelectedDifficulty.IntValue);
            if (l != null)
            {
                Leaderboard = l;
                if (CharacterSearchedForInLeaderboard.CharacterName.Trim() != "")
                {
                    await FindAndSelectSearchedForCharacterAsync();
                }
            }
            
        }

        private async Task<bool> IsLeagueFilledAndDisplayWarningIfNotAsync()
        {
            if (SelectedLeague == null)
            {
                await this.mainWindowViewModel.DialogDisplayer.ShowMessageAsync("Invalid League", "Select a League to search. The list of leagues can be updated from the menu.");
                return false;
            }
            return true;
        }

        private void SetLocalSearchedForValues()
        {
            this.CharacterSearchedForInLeaderboard = new Character { CharacterName = (CharacterName ?? "") };
            this.leagueSearchedForObject = this.SelectedLeague;
            this.LeagueSearchedFor = this.SelectedLeague.Name;
            this.typeSearchedFor = this.SelectedLeaderboardType;
            this.difficultySearchedFor = this.SelectedDifficulty;
        }

        private async Task FindAndSelectSearchedForCharacterAsync()
        {
            var charWithSearchedName = Leaderboard.LeaderboardCharacters.Where(c => c.CharacterName == CharacterSearchedForInLeaderboard.CharacterName);
            if (charWithSearchedName.Count() == 1)
            {
                CharacterSearchedForInLeaderboard = charWithSearchedName.First();
                SelectedCharacterInGrid = charWithSearchedName.First();
            }
            else if (charWithSearchedName.Count() == 0)
            {
                await this.mainWindowViewModel.DialogDisplayer.ShowMessageAsync("Character not found.", CharacterSearchedForInLeaderboard.CharacterName + " could not be found.");
            }
        }

        private void CloseThisTab()
        {
            this.mainWindowViewModel.RemoveLeaderboardViewModel(this);
        }

    }
}
