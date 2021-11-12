using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for SaveOrResetUC.xaml
  /// </summary>
  public partial class SaveOrResetUC : UserControl
  {
    public delegate bool ExistingChanges();
    public delegate void SaveChanges();
    public delegate void ResetChanges();

    private ExistingChanges _existingChangesCallback;
    private SaveChanges _saveChangesCallback;
    private ResetChanges _resetChangesCallback;

    private TabControl _tabControl;
    private TabItem _thisTabItem;
    private bool _active = false;

    public SaveOrResetUC()
    {
      InitializeComponent();

      lbSaved.Visibility = Visibility.Hidden;

      ShortcutSingleton.Instance.Save += OnSaveTriggered;
    }


    private void OnSaveTriggered()
    {
      if (IsVisible)
        saveChanges();
    }

    public void Init( TabControl parent, TabItem thisTabItem,
                      ExistingChanges existingChangesCallback, SaveChanges saveChangesCallback, ResetChanges resetChangesCallback)
    {
      _existingChangesCallback = existingChangesCallback;
      _saveChangesCallback = saveChangesCallback;
      _resetChangesCallback = resetChangesCallback;

      _tabControl = parent;
      _thisTabItem = thisTabItem;

      var p = thisTabItem.Parent;

      _tabControl.SelectionChanged += parent_SelectionChanged;
    }


    private void parent_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_active && _tabControl.SelectedItem != _thisTabItem) // Seems like the user wants to move away from this tab
      {
        if (existingChanges())
        {
          Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
          {
            e.Handled = true;

            // Switch back to the originating tab to make the tabe visible to the user (context information)
            // Remember the tab the user wanted to go
            var newTarget = _tabControl.SelectedItem;
            _tabControl.SelectedItem = _thisTabItem;

            var result = MessageBox.Show("Sollen die Änderungen gespeichert werden?", "Speichern?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
              saveChanges();  // Save changes
              _tabControl.SelectedItem = newTarget; // Go to the requested tab
            }
            else if (result == MessageBoxResult.No)
            {
              resetChanges(); // Reset changes
              _tabControl.SelectedItem = newTarget; // Go to the requested tab
            }
          }));
        }
      }

      // Set flag accordingly whether we are still on this tab
      _active = _tabControl.SelectedItem == _thisTabItem;
    }

    private bool existingChanges()
    {
      return _existingChangesCallback();
    }

    private void saveChanges()
    {
      showStatus(EStatus.Save);
      _saveChangesCallback();
    }

    private void resetChanges()
    {
      showStatus(EStatus.Reset);
      _resetChangesCallback();
    }


    enum EStatus { Reset, Save };
    private void showStatus(EStatus status)
    {
      if (status == EStatus.Reset)
      {
        lbSaved.Content = "Zurückgesetzt ...";
        lbSaved.Foreground = Brushes.DarkRed;
      }
      else if (status == EStatus.Save)
      {
        lbSaved.Content = "Gespeichert ...";
        lbSaved.Foreground = Brushes.DarkGreen;
      }

      lbSaved.Visibility = Visibility.Visible;
      Task.Delay(2000).ContinueWith(t => 
      {
        lbSaved.Visibility = Visibility.Hidden;
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
      resetChanges();
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
      saveChanges();
    }
  }
}
