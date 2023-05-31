using System.Runtime.CompilerServices;

namespace MauiTestApp;

public partial class MainPage : ContentPage
{
 #region Construction
 /////////////////////////////////////////////////////////////////////////////////////////////

 public MainPage()
 {
  InitializeComponent();
  IncrementClickCount = new Command(OnIncrementClickCount, ValidateIncrementClickCount);
  EnableIncrementClickCount = new Command(OnEnableIncrementClickCount);
  DisableIncrementClickCount = new Command(OnDisableIncrementClickCount);

  EnableControls = new Command(OnEnableControls);
  DisableControls = new Command(OnDisableControls);

  BindingContext = this;

  ControlsEnabled = true;
  TrueAfterEnabledCB.IsChecked = true;
 }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region Button Enabling Visualization Bug
 /////////////////////////////////////////////////////////////////////////////////////////////

 private void OnCounterClicked(object sender, EventArgs e) { ClickCount++; }

 public Command IncrementClickCount { get; }

 private void OnIncrementClickCount() { ClickCount++; }

 private bool ValidateIncrementClickCount() { return CanClickCount; }

 public int ClickCount { get => clickCount; set => SetProperty(ref clickCount, value); }
 private int clickCount;

 public bool CanClickCount { get => canClickCount; set => SetProperty(ref canClickCount, value, null, () => IncrementClickCount.ChangeCanExecute()); }
 private bool canClickCount;

 public Command EnableIncrementClickCount { get; }

 private void OnEnableIncrementClickCount() { CanClickCount = true; }

 public Command DisableIncrementClickCount { get; }

 private void OnDisableIncrementClickCount() { CanClickCount = false; }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region Display Missing Buttons Bug
 /////////////////////////////////////////////////////////////////////////////////////////////

 public Command EnableControls { get; }

 private void OnEnableControls() { ControlsEnabled = true; }

 public Command DisableControls { get; }

 private void OnDisableControls() { ControlsEnabled = false; }

 public bool ControlsEnabled { get => controlsEnabled; set => SetProperty(ref controlsEnabled, value); }
 private bool controlsEnabled;

 public List<string> PickerItems { get; } = new() { "Item 1", "Item 2", "Item 3" };

 public string PickerItem { get; set; } = "Item 1";

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion

 #region INotifyPropertyChanged
 /////////////////////////////////////////////////////////////////////////////////////////////

 protected bool SetProperty<T>(
  ref T backingStore,
  T value,
  string[] otherPropertyNames = null,
  Action onChanged = null,
  [CallerMemberName] string propertyName = "")
 {
  if (EqualityComparer<T>.Default.Equals(backingStore, value))
   return false;

  backingStore = value;
  onChanged?.Invoke();
  OnPropertyChanged(propertyName);
  if (otherPropertyNames != null)
   foreach (string otherPropertyName in otherPropertyNames)
    OnPropertyChanged(otherPropertyName);
  return true;
 }

 /////////////////////////////////////////////////////////////////////////////////////////////
 #endregion
}

