/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;

namespace RaceHorology.Settings
{
  /// <summary>
  ///   Persists a Window's Size, Location and WindowState to UserScopeSettings
  /// </summary>
  public class WindowSettings
  {
    #region Fields

    /// <summary>
    ///   Register the "Save" attached property and the "OnSaveInvalidated" callback
    /// </summary>
    public static readonly DependencyProperty SaveProperty = DependencyProperty.RegisterAttached("Save", typeof(bool), typeof(WindowSettings), new FrameworkPropertyMetadata(OnSaveInvalidated));

    private readonly Window mWindow;

    private WindowApplicationSettings mWindowApplicationSettings;

    #endregion Fields

    #region Constructors

    public WindowSettings(Window pWindow) { mWindow = pWindow; }

    #endregion Constructors

    #region Properties

    [Browsable(false)]
    public WindowApplicationSettings Settings
    {
      get
      {
        if (mWindowApplicationSettings == null) mWindowApplicationSettings = CreateWindowApplicationSettingsInstance();
        return mWindowApplicationSettings;
      }
    }

    #endregion Properties

    #region Methods

    public static void SetSave(DependencyObject pDependencyObject, bool pEnabled) { pDependencyObject.SetValue(SaveProperty, pEnabled); }

    protected virtual WindowApplicationSettings CreateWindowApplicationSettingsInstance() { return new WindowApplicationSettings(this); }

    /// <summary>
    ///   Load the Window Size Location and State from the settings object
    /// </summary>
    protected virtual void LoadWindowState()
    {
      Settings.Reload();
      if (Settings.Location != Rect.Empty)
      {
        mWindow.Left = Settings.Location.Left;
        mWindow.Top = Settings.Location.Top;
        mWindow.Width = Settings.Location.Width;
        mWindow.Height = Settings.Location.Height;
      }
      if (Settings.WindowState != WindowState.Maximized) mWindow.WindowState = Settings.WindowState;
    }

    /// <summary>
    ///   Save the Window Size, Location and State to the settings object
    /// </summary>
    protected virtual void SaveWindowState()
    {
      Settings.WindowState = mWindow.WindowState;
      Settings.Location = mWindow.RestoreBounds;
      Settings.Save();
    }

    /// <summary>
    ///   Called when Save is changed on an object.
    /// </summary>
    private static void OnSaveInvalidated(DependencyObject pDependencyObject, DependencyPropertyChangedEventArgs pDependencyPropertyChangedEventArgs)
    {
      var window = pDependencyObject as Window;
      if (window != null)
        if ((bool)pDependencyPropertyChangedEventArgs.NewValue)
        {
          var settings = new WindowSettings(window);
          settings.Attach();
        }
    }

    private void Attach()
    {
      if (mWindow != null)
      {
        mWindow.Closing += WindowClosing;
        mWindow.Initialized += WindowInitialized;
        mWindow.Loaded += WindowLoaded;
      }
    }

    private void WindowClosing(object pSender, CancelEventArgs pCancelEventArgs) { SaveWindowState(); }

    private void WindowInitialized(object pSender, EventArgs pEventArgs) { LoadWindowState(); }

    private void WindowLoaded(object pSender, RoutedEventArgs pRoutedEventArgs) { if (Settings.WindowState == WindowState.Maximized) mWindow.WindowState = Settings.WindowState; }

    #endregion Methods

    #region Nested Types

    public class WindowApplicationSettings : ApplicationSettingsBase
    {
      #region Constructors

      public WindowApplicationSettings(WindowSettings pWindowSettings) { }

      #endregion Constructors

      #region Properties

      [UserScopedSetting]
      public Rect Location
      {
        get
        {
          if (this["Location"] != null) return ((Rect)this["Location"]);
          return Rect.Empty;
        }
        set { this["Location"] = value; }
      }

      [UserScopedSetting]
      public WindowState WindowState
      {
        get
        {
          if (this["WindowState"] != null) return (WindowState)this["WindowState"];
          return WindowState.Normal;
        }
        set { this["WindowState"] = value; }
      }

      #endregion Properties
    }

    #endregion Nested Types
  }
}
