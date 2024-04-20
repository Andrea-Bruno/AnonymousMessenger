using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CustomViewElements;
using Anonymous.DesignHandler;
using Anonymous.ViewModels;
using Xamarin.Forms.Xaml;
using Utils;
using static EncryptedMessaging.Contacts;

namespace Anonymous.Views
{
    public class Grouping<K, T> : Observable<T>
    {
        public K Key { get; private set; }

        public Grouping(K key, IEnumerable<T> items)
        {
            Key = key;
            foreach (var item in items)
                this.Items.Add(item);
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FAQPage : BasePage
    {
        private ObservableCollection<QuestionAnswerViewModel> _questionAnswers = new ObservableCollection<QuestionAnswerViewModel>();
        private QuestionAnswerViewModel _clickedItem;

        public FAQPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _questionAnswers?.Clear();

            List<QuestionAnswerViewModel> topics = new List<QuestionAnswerViewModel>();

            topics.Add(new QuestionAnswerViewModel
            {
                Title = Localization.Resources.Dictionary.General,
                isExpanded = false,
                QuestionList = new ObservableCollection<QuestionAnswersInnerViewModel>{
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.WhatIsAnonymousAnswer, Title = Localization.Resources.Dictionary.WhatIsAnonymous, isExpanded = false },
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.WhoIsAnonymousForAnswer, Title = Localization.Resources.Dictionary.WhoIsAnonymousFor, isExpanded = false },
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.WhoCanIMessageAnswer, Title = Localization.Resources.Dictionary.WhoCanIMessage, isExpanded = false }
                }
            });

            topics.Add(new QuestionAnswerViewModel
            {
                Title = Localization.Resources.Dictionary.DownloadAndInstallation,
                isExpanded = false,
                QuestionList = new ObservableCollection<QuestionAnswersInnerViewModel>{
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowToUpdateAnonymousAnswer, Title = Localization.Resources.Dictionary.HowToUpdateAnonymous, isExpanded = false },
                }
            });

            topics.Add(new QuestionAnswerViewModel
            {
                Title = Localization.Resources.Dictionary.ContactAndGroup,
                isExpanded = false,
                QuestionList = new ObservableCollection<QuestionAnswersInnerViewModel>{
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowCanICreateNewGroupAnswer, Title = Localization.Resources.Dictionary.HowCanICreateNewGroup, isExpanded = false },
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowCanIAddNewMemberAnswer, Title = Localization.Resources.Dictionary.HowCanIAddNewMember, isExpanded = false }
                }

            });

            topics.Add(new QuestionAnswerViewModel
            {
                Title = Localization.Resources.Dictionary.Security,
                isExpanded = false,
                QuestionList = new ObservableCollection<QuestionAnswersInnerViewModel>{
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.WhatIsSecurityStatusForAnonymousAnswer, Title = Localization.Resources.Dictionary.WhatIsSecurityStatusForAnonymous, isExpanded = false },
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowCanISetLimitVisibilityOfNumberOfMessageAnswer, Title = Localization.Resources.Dictionary.HowCanISetLimitVisibilityOfNumberOfMessage, isExpanded = false },
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowCanIChangeDurationOfMessageAnswer, Title = Localization.Resources.Dictionary.HowCanIChangeDurationOfMessage, isExpanded = false }
                }
            });
            topics.Add(new QuestionAnswerViewModel
            {
                Title = Localization.Resources.Dictionary.Troubleshooting,
                isExpanded = false,
                QuestionList = new ObservableCollection<QuestionAnswersInnerViewModel>{
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.NotificationProblemsAnswer, Title = Localization.Resources.Dictionary.NotificationProblems, isExpanded = false },
                }
            });

            topics.Add(new QuestionAnswerViewModel
            {
                Title = Localization.Resources.Dictionary.AccountAndProfile,
                isExpanded = false,
                QuestionList = new ObservableCollection<QuestionAnswersInnerViewModel>{
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowDoUserCanEditProfileAnswer, Title = Localization.Resources.Dictionary.HowDoUserCanEditProfile, isExpanded = false },
                    new QuestionAnswersInnerViewModel { Description = Localization.Resources.Dictionary.HowDoUserCanEditNameOfAnotherUserAnswer, Title = Localization.Resources.Dictionary.HowDoUserCanEditNameOfAnotherUser, isExpanded = false },
                }
            });

            _questionAnswers.AddRange(topics);
            Questions.ItemsSource = _questionAnswers;
        }
      
        void Questions_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            _clickedItem = e.ItemData as QuestionAnswerViewModel;
        }

        private void Back_Clicked(object sender, EventArgs e)
        {
            sender.HandleButtonSingleClick();
            OnBackButtonPressed();
        }
    }
}