﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;


namespace app.core.Views.CustomControls
{
    /// <summary>
    // AutoCompleteView for Xamarin Forms.
    // Thanks Dottor Pagliaccius!
    // https://github.com/DottorPagliaccius/Xamarin-Custom-Controls
    /// </summary>

    public class MemberNotFoundException : Exception
    {

        public MemberNotFoundException()
        {
        }

        public MemberNotFoundException(string message) : base(message)
        {
        }

        public MemberNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class SearchMemberPropertyTypeException : Exception
    {
        public SearchMemberPropertyTypeException()
        {
        }

        public SearchMemberPropertyTypeException(string message) : base(message)
        {
        }

        public SearchMemberPropertyTypeException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public delegate void DataUpdateEventHandler(object sender, EventArgs e);

    public partial class AutoCompleteView : ContentView
    {
        private PropertyInfo _searchMemberCachePropertyInfo;

        private ObservableCollection<object> _availableSuggestions;

        public event EventHandler<EventArgs> OnSelectedItem;

        public static readonly BindableProperty OnSelectedItemProperty = BindableProperty.Create(nameof(OnSelectedItem),
            typeof(EventHandler), typeof(AutoCompleteView), default(EventHandler));

        public static readonly BindableProperty SearchTextChangedCommandProperty =
            BindableProperty.Create(nameof(SearchTextChangedCommand), typeof(ICommand), typeof(AutoCompleteView), default(ICommand));

        public static readonly BindableProperty SearchMemberProperty =
            BindableProperty.Create(nameof(SearchMember), typeof(string), typeof(AutoCompleteView), string.Empty);

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AutoCompleteView), string.Empty);

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate),
            typeof(DataTemplate), typeof(AutoCompleteView), default(DataTemplate));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AutoCompleteView), new List<object>());

        public static readonly BindableProperty EmptyTextProperty =
            BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(AutoCompleteView), string.Empty);

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AutoCompleteView), null, BindingMode.TwoWay);

        public static readonly BindableProperty SelectedItemCommandProperty =
            BindableProperty.Create(nameof(SelectedItemCommand), typeof(ICommand), typeof(AutoCompleteView), default(ICommand));

        public static readonly BindableProperty SuggestionBackgroundColorProperty =
            BindableProperty.Create(nameof(SuggestionBackgroundColor), typeof(Color), typeof(AutoCompleteView), Color.White);

        public static readonly BindableProperty SuggestionBorderColorProperty =
            BindableProperty.Create(nameof(SuggestionBorderColor), typeof(Color), typeof(AutoCompleteView), Color.Silver);

        public static readonly BindableProperty SuggestionBorderSizeProperty =
            BindableProperty.Create(nameof(SuggestionBorderSize), typeof(Thickness), typeof(AutoCompleteView), new Thickness(1));

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(AutoCompleteView), Color.Black);

        public static readonly BindableProperty PlaceholderTextColorProperty =
            BindableProperty.Create(nameof(PlaceholderTextColor), typeof(Color), typeof(AutoCompleteView), Color.Silver);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(AutoCompleteView), Font.Default.FontSize);

        public static readonly BindableProperty SeparatorColorProperty =
            BindableProperty.Create(nameof(SeparatorColor), typeof(Color), typeof(AutoCompleteView), Color.Silver);

        public static readonly BindableProperty SeparatorHeightProperty =
            BindableProperty.Create(nameof(SeparatorHeight), typeof(double), typeof(AutoCompleteView), 1.5d);

        public static readonly BindableProperty SearchEntryFocusProperty =
            BindableProperty.Create(nameof(SearchEntryFocus), typeof(bool), typeof(AutoCompleteView), true);

        public bool SearchEntryFocus
        {
            get
            {
                return (bool)GetValue(SearchEntryFocusProperty);
            }
            set
            {
                SetValue(SearchEntryFocusProperty, value);

                if (value)
                    MainEntry.Focus();
                else
                    MainEntry.Unfocus();
            }
        }

        public ICommand SearchTextChangedCommand
        {
            get { return (ICommand) GetValue(SearchTextChangedCommandProperty); }
            set { SetValue(SearchTextChangedCommandProperty, value); }
        }

        public ICommand SelectedItemCommand
        {
            get { return (ICommand) GetValue(SelectedItemCommandProperty); }
            set { SetValue(SelectedItemCommandProperty, value); }
        }

        public string Placeholder
        {
            get { return (string) GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
                OnPropertyChanged();
            }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public string EmptyText
        {
            get { return (string) GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        public Color TextColor
        {
            get { return (Color) GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public Color SuggestionBackgroundColor
        {
            get { return (Color) GetValue(SuggestionBackgroundColorProperty); }
            set { SetValue(SuggestionBackgroundColorProperty, value); }
        }

        public Color SuggestionBorderColor
        {
            get { return (Color) GetValue(SuggestionBorderColorProperty); }
            set { SetValue(SuggestionBorderColorProperty, value); }
        }

        public Thickness SuggestionBorderSize
        {
            get { return (Thickness) GetValue(SuggestionBorderSizeProperty); }
            set { SetValue(SuggestionBorderSizeProperty, value); }
        }

        public double FontSize
        {
            get { return (double) GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public Color PlaceholderTextColor
        {
            get { return (Color) GetValue(PlaceholderTextColorProperty); }
            set { SetValue(PlaceholderTextColorProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        public string SearchMember
        {
            get { return (string) GetValue(SearchMemberProperty); }
            set { SetValue(SearchMemberProperty, value); }
        }

        public Color SeparatorColor
        {
            get { return (Color) GetValue(SeparatorColorProperty); }
            set { SetValue(SeparatorColorProperty, value); }
        }

        public double SeparatorHeight
        {
            get { return (double) GetValue(SeparatorHeightProperty); }
            set { SetValue(SeparatorHeightProperty, value); }
        }

        public bool ShowSeparator
        {
            get { return SuggestedItemsRepeaterView.ShowSeparator; }
            set { SuggestedItemsRepeaterView.ShowSeparator = value; }
        }

        public bool OpenOnFocus { get; set; }
        public int MaxResults { get; set; }

        public AutoCompleteView()
        {
            InitializeComponent();

            _availableSuggestions = new ObservableCollection<object>();

            SuggestedItemsRepeaterView.SelectedItemCommand = new Command(SuggestedRepeaterItemSelected);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == PlaceholderProperty.PropertyName && SelectedItem == null)
            {
                MainEntry.Text = Placeholder;
            }

            if (propertyName == SelectedItemProperty.PropertyName)
            {
                if (SelectedItem != null)
                {
                    var propertyInfo = GetSearchMember(SelectedItem.GetType());

                    var selectedItem = ItemsSource.Cast<object>()
                        .FirstOrDefault(x => propertyInfo.GetValue(x).ToString() ==
                                             propertyInfo.GetValue(SelectedItem).ToString());

                    if (selectedItem != null)
                    {
                        try
                        {
                            MainEntry.TextChanged -= SearchText_TextChanged;

                            MainEntry.Text = propertyInfo.GetValue(SelectedItem).ToString();
                        }
                        finally
                        {
                            MainEntry.TextChanged -= SearchText_TextChanged;
                        }

                        FilterSuggestions(MainEntry.Text, false);

                        MainEntry.TextColor = TextColor;
                    }
                    else
                    {
                        MainEntry.Text = Placeholder;
                        MainEntry.TextColor = PlaceholderTextColor;
                    }
                }
                else
                {
                    MainEntry.Text = Placeholder;
                    MainEntry.TextColor = PlaceholderTextColor;
                }
            }

            if (propertyName == SearchMemberProperty.PropertyName)
            {
                _searchMemberCachePropertyInfo = null;
            }

            if (propertyName == PlaceholderTextColorProperty.PropertyName)
            {
                MainEntry.TextColor = PlaceholderTextColor;
            }

            if (propertyName == ItemTemplateProperty.PropertyName)
            {
                SuggestedItemsRepeaterView.ItemTemplate = ItemTemplate;
            }

            if (propertyName == SuggestionBackgroundColorProperty.PropertyName)
            {
                SuggestedItemsInnerContainer.BackgroundColor = SuggestionBackgroundColor;
            }

            if (propertyName == SuggestionBorderColorProperty.PropertyName)
            {
                SuggestedItemsContainer.BackgroundColor = SuggestionBorderColor;
            }

            if (propertyName == SuggestionBorderSizeProperty.PropertyName)
            {
                SuggestedItemsContainer.Padding = SuggestionBorderSize;
            }

            if (propertyName == EmptyTextProperty.PropertyName)
            {
                SuggestedItemsRepeaterView.EmptyText = EmptyText;
            }

            if (propertyName == SeparatorColorProperty.PropertyName)
            {
                SuggestedItemsRepeaterView.SeparatorColor = SeparatorColor;
            }

            if (propertyName == SeparatorHeightProperty.PropertyName)
            {
                SuggestedItemsRepeaterView.SeparatorHeight = SeparatorHeight;
            }
        }

        private void SearchText_Focused(object sender, FocusEventArgs e)
        {
            MainEntry.Text = Placeholder;
            HandleFocus(e.IsFocused);
        }

        private void SearchText_Unfocused(object sender, FocusEventArgs e)
        {
            HandleFocus(e.IsFocused);
        }

        private void HandleFocus(bool isFocused)
        {
            MainEntry.TextChanged -= SearchText_TextChanged;

            try
            {
                if (isFocused)
                {
                    if (string.Equals(MainEntry.Text, Placeholder, StringComparison.OrdinalIgnoreCase))
                    {
                        MainEntry.Text = string.Empty;
                        MainEntry.TextColor = TextColor;
                    }

                    if (OpenOnFocus)
                    {
                        FilterSuggestions(MainEntry.Text);
                    }
                }
                else
                {
                    var items = ItemsSource.Cast<object>();

                    if (MainEntry.Text.Length == 0 || (items.Any() &&
                                                       !items.Any(x => string.Equals(
                                                           GetSearchMember(items.First().GetType()).GetValue(x)
                                                               .ToString(), MainEntry.Text, StringComparison.Ordinal))))
                    {
                        MainEntry.Text = Placeholder;
                        MainEntry.TextColor = PlaceholderTextColor;
                    }
                    else
                        MainEntry.TextColor = TextColor;

                    ShowHideListbox(false);
                }
            }
            finally
            {
                MainEntry.TextChanged += SearchText_TextChanged;
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchTextChangedCommand?.Execute(MainEntry.Text);

            if (string.Equals(MainEntry.Text, Placeholder, StringComparison.OrdinalIgnoreCase))
            {
                if (_availableSuggestions.Any())
                {
                    _availableSuggestions.Clear();

                    ShowHideListbox(false);
                }

                return;
            }

            FilterSuggestions(MainEntry.Text);
        }

        private void FilterSuggestions(string text, bool openSuggestionPanel = true)
        {
            var filteredSuggestions = ItemsSource.Cast<object>();

            if (text.Length > 0 && filteredSuggestions.Any())
            {
                var property = GetSearchMember(filteredSuggestions.First().GetType());

                if (property == null)
                    throw new MemberNotFoundException(
                        $"There's no corrisponding property the matches SearchMember value '{SearchMember}'");

                if (property.PropertyType != typeof(string))
                    throw new SearchMemberPropertyTypeException($"Property '{SearchMember}' must be of type string");

                filteredSuggestions = filteredSuggestions
                    .Where(x => property.GetValue(x).ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(x => property.GetValue(x).ToString());
            }

            _availableSuggestions = new ObservableCollection<object>(filteredSuggestions.Take(MaxResults));

            ShowHideListbox(openSuggestionPanel);
        }

        private PropertyInfo GetSearchMember(Type type)
        {
            if (_searchMemberCachePropertyInfo != null)
                return _searchMemberCachePropertyInfo;

            if (string.IsNullOrEmpty(SearchMember))
                throw new MemberNotFoundException("You must specify SearchMember property");

            _searchMemberCachePropertyInfo = type.GetRuntimeProperty(SearchMember);

            if (_searchMemberCachePropertyInfo == null)
                throw new MemberNotFoundException(
                    $"There's no corrisponding property the matches SearchMember value '{SearchMember}'");

            if (_searchMemberCachePropertyInfo.PropertyType != typeof(string))
                throw new SearchMemberPropertyTypeException($"Property '{SearchMember}' must be of type string");

            return _searchMemberCachePropertyInfo;
        }

        private void SuggestedRepeaterItemSelected(object selectedItem)
        {
            MainEntry.Text = GetSelectedText(selectedItem);
            MainEntry.TextColor = TextColor;

            ShowHideListbox(false);

            _availableSuggestions.Clear();

            SelectedItem = selectedItem;

            SelectedItemCommand?.Execute(selectedItem);
            MainEntry.Text = string.Empty;
            OnSelectedItem?.Invoke(this, new EventArgs());
        }

        private string GetSelectedText(object selectedItem)
        {
            var property = selectedItem.GetType().GetRuntimeProperty(SearchMember);

            if (property == null)
                throw new MemberNotFoundException(
                    $"There's no corrisponding property the matches DisplayMember value '{SearchMember}'");

            return property.GetValue(selectedItem).ToString();
        }

        private void ShowHideListbox(bool show)
        {
            if (show)
                SuggestedItemsRepeaterView.ItemsSource = _availableSuggestions;

            SuggestedItemsContainer.IsVisible = show;
        }
    }

    public class RepeaterView : StackLayout
    {
        public class InvalidViewException : Exception
        {
            public InvalidViewException()
            {
            }

            public InvalidViewException(string message) : base(message)
            {
            }

            public InvalidViewException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        public event DataUpdateEventHandler OnDataUpdate;

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate),
            typeof(DataTemplate), typeof(RepeaterView), default(DataTemplate));

        public static readonly BindableProperty SeparatorTemplateProperty =
            BindableProperty.Create(nameof(SeparatorTemplate), typeof(DataTemplate), typeof(RepeaterView),
                default(DataTemplate));

        public static readonly BindableProperty EmptyTextTemplateProperty =
            BindableProperty.Create(nameof(EmptyTextTemplate), typeof(DataTemplate), typeof(RepeaterView),
                default(DataTemplate));

        public static readonly BindableProperty EmptyTextProperty =
            BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(RepeaterView), string.Empty);

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource),
            typeof(ICollection), typeof(RepeaterView), new List<object>(), BindingMode.TwoWay, null,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                ItemsChanged(bindable, (ICollection) oldValue, (ICollection) newValue);
            });

        public static readonly BindableProperty SelectedItemCommandProperty =
            BindableProperty.Create(nameof(SelectedItemCommand), typeof(ICommand), typeof(RepeaterView),
                default(ICommand));

        public static readonly BindableProperty SeparatorColorProperty =
            BindableProperty.Create(nameof(SeparatorColor), typeof(Color), typeof(RepeaterView), Color.Default);

        public static readonly BindableProperty SeparatorHeightProperty =
            BindableProperty.Create(nameof(SeparatorHeight), typeof(double), typeof(RepeaterView), 1.5d);

        public ICollection ItemsSource
        {
            get { return (ICollection) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate) GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public DataTemplate SeparatorTemplate
        {
            get { return (DataTemplate) GetValue(SeparatorTemplateProperty); }
            set { SetValue(SeparatorTemplateProperty, value); }
        }

        public DataTemplate EmptyTextTemplate
        {
            get { return (DataTemplate) GetValue(EmptyTextTemplateProperty); }
            set { SetValue(EmptyTextTemplateProperty, value); }
        }

        public string EmptyText
        {
            get { return (string) GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        public ICommand SelectedItemCommand
        {
            get { return (ICommand) GetValue(SelectedItemCommandProperty); }
            set { SetValue(SelectedItemCommandProperty, value); }
        }

        public Color SeparatorColor
        {
            get { return (Color) GetValue(SeparatorColorProperty); }
            set { SetValue(SeparatorColorProperty, value); }
        }

        public double SeparatorHeight
        {
            get { return (double) GetValue(SeparatorHeightProperty); }
            set { SetValue(SeparatorHeightProperty, value); }
        }

        public bool ShowSeparator { get; set; }

        public RepeaterView()
        {
            Spacing = 0;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == SelectedItemCommandProperty.PropertyName)
            {
                if (SelectedItemCommand == null)
                    return;

                foreach (var view in Children)
                {
                    BindSelectedItemCommand(view);
                }
            }

            if (propertyName == SeparatorTemplateProperty.PropertyName)
            {
                UpdateItems();
            }
        }

        private static void ItemsChanged(BindableObject bindable, ICollection oldValue, ICollection newValue)
        {
            var repeater = (RepeaterView) bindable;

            if (oldValue != null)
            {
                var observable = oldValue as INotifyCollectionChanged;

                if (observable != null)
                {
                    observable.CollectionChanged -= repeater.CollectionChanged;
                }
            }

            if (newValue != null)
            {
                repeater.UpdateItems();

                var observable = repeater.ItemsSource as INotifyCollectionChanged;

                if (observable != null)
                {
                    observable.CollectionChanged += repeater.CollectionChanged;
                }
            }
        }

        private void UpdateItems()
        {
            if (ItemsSource.Count == 0 && (EmptyTextTemplate != null || !string.IsNullOrEmpty(EmptyText)))
            {
                BuildEmptyText();
            }
            else
                BuildItems(ItemsSource);

            OnDataUpdate?.Invoke(this, new EventArgs());
        }

        private View BuildSeparator()
        {
            if (SeparatorTemplate != null)
            {
                var content = SeparatorTemplate.CreateContent();
                if (!(content is View) && !(content is ViewCell))
                {
                    throw new InvalidViewException("Templated control must be a View or a ViewCell");
                }

                return (content is View) ? content as View : ((ViewCell) content).View;
            }
            else
            {
                return new BoxView
                {
                    HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, true),
                    BackgroundColor = SeparatorColor,
                    HeightRequest = SeparatorHeight
                };
            }
        }

        private void BuildEmptyText()
        {
            Children.Clear();

            if (EmptyTextTemplate == null)
                Children.Add(new Label {Text = EmptyText});
            else
            {
                var content = EmptyTextTemplate.CreateContent();
                if (!(content is View) && !(content is ViewCell))
                {
                    throw new InvalidViewException("Templated control must be a View or a ViewCell");
                }

                var view = (content is View) ? content as View : ((ViewCell) content).View;

                Children.Add(view);
            }
        }

        public void BuildItems(ICollection sourceItems)
        {
            Children.Clear();

            foreach (object item in sourceItems)
            {
                Children.Add(GetItemView(item));
            }
        }

        private View GetItemView(object item)
        {
            var content = ItemTemplate.CreateContent();
            if (!(content is View) && !(content is ViewCell))
            {
                throw new InvalidViewException("Templated control must be a View or a ViewCell");
            }

            var view = content is View ? content as View : ((ViewCell) content).View;

            view.BindingContext = item;

            if (SelectedItemCommand != null && SelectedItemCommand.CanExecute(item))
                BindSelectedItemCommand(view);

            if (ShowSeparator && ItemsSource.Cast<object>().Last() != item)
            {
                var container = new StackLayout {Spacing = 0};

                container.Children.Add(view);
                container.Children.Add(BuildSeparator());

                return container;
            }

            return view;
        }

        private void BindSelectedItemCommand(View view)
        {
            if (!SelectedItemCommand.CanExecute(view.BindingContext))
                return;

            var tapGestureRecognizer =
                new TapGestureRecognizer {Command = SelectedItemCommand, CommandParameter = view.BindingContext};

            if (view.GestureRecognizers.Any())
                view.GestureRecognizers.Clear();

            view.GestureRecognizers.Add(tapGestureRecognizer);
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var items = ItemsSource.Cast<object>().ToList();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    var index = e.NewStartingIndex;

                    foreach (var newItem in e.NewItems)
                    {
                        Children.Insert(index++, GetItemView(newItem));
                    }
                    break;

                case NotifyCollectionChangedAction.Move:

                    var moveItem = items[e.OldStartingIndex];

                    Children.RemoveAt(e.OldStartingIndex);
                    Children.Insert(e.NewStartingIndex, GetItemView(moveItem));
                    break;

                case NotifyCollectionChangedAction.Remove:

                    Children.RemoveAt(e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:

                    Children.RemoveAt(e.OldStartingIndex);
                    Children.Insert(e.NewStartingIndex, GetItemView(items[e.NewStartingIndex]));
                    break;

                case NotifyCollectionChangedAction.Reset:

                    BuildItems(ItemsSource);
                    break;
            }

            OnDataUpdate?.Invoke(this, new EventArgs());
        }
    }
}
