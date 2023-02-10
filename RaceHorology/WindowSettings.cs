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

using System.Windows;

namespace RaceHorology
{
  public class WindowSettings
  {
    private static double _windowLeft = 0;
    private static double _windowTop = 0;
    private static double _windowWidth = 1024;
    private static double _windowHeight = 800;
    private static int _windowState = (int)System.Windows.WindowState.Maximized;
    private static int _windowScreen = 0 ;

    public static double WindowLeft
    {
      get { return _windowLeft; }
      set { _windowLeft = value; }
    }

    public static double WindowTop
    {
      get { return _windowTop; }
      set { _windowTop = value; }
    }

    public static double WindowWidth
    {
      get { return _windowWidth; }
      set { _windowWidth = value; }
    }

    public static double WindowHeight
    {
      get { return _windowHeight; }
      set { _windowHeight = value; }
    }

    public static int WindowState
    {
      get { return _windowState; }
      set { _windowState = value; }
    }

    public static int WindowScreen
    {
      get { return _windowScreen; }
      set { _windowScreen = value; }
    }

    public static void Load()
    {
      if (Properties.Settings.Default._windowLeft != -1)
        WindowLeft = Properties.Settings.Default._windowLeft;

      if (Properties.Settings.Default._windowTop != -1)
        WindowTop = Properties.Settings.Default._windowTop;

      if (Properties.Settings.Default._windowWidth != -1)
        WindowWidth = Properties.Settings.Default._windowWidth;

      if (Properties.Settings.Default._windowHeight != -1)
        WindowHeight = Properties.Settings.Default._windowHeight;

      if (Properties.Settings.Default._windowState != -1)
        WindowState = Properties.Settings.Default._windowState;

      if (Properties.Settings.Default._windowScreen != -1)
        WindowScreen = Properties.Settings.Default._windowScreen;
    }

    public static void Save()
    {
      Properties.Settings.Default._windowLeft = WindowLeft;
      Properties.Settings.Default._windowTop = WindowTop;
      Properties.Settings.Default._windowWidth = WindowWidth;
      Properties.Settings.Default._windowHeight = WindowHeight;
      Properties.Settings.Default._windowState = WindowState;
      Properties.Settings.Default._windowScreen = WindowScreen;
      Properties.Settings.Default.Save();
    }

    public static void ApplyToWindow(Window window)
    {

      if (_windowState == (int)System.Windows.WindowState.Maximized)
      {
        window.WindowState = System.Windows.WindowState.Maximized;
      }
      else
      {
        window.WindowState = System.Windows.WindowState.Normal;
        window.Left = _windowLeft;
        window.Top = _windowTop;
        window.Width = _windowWidth;
        window.Height = _windowHeight;

        var screen = System.Windows.Forms.Screen.AllScreens[_windowScreen];
        if (!screen.WorkingArea.Contains(new System.Drawing.Rectangle((int)_windowLeft, (int)_windowTop, (int)_windowWidth, (int)_windowHeight)))
        {
          if (System.Windows.Forms.Screen.AllScreens.Length > 1)
          {
            var firstScreen = System.Windows.Forms.Screen.AllScreens[0];
            window.Left = firstScreen.WorkingArea.Left;
            window.Top = firstScreen.WorkingArea.Top;
          }
          else
          {
            window.WindowState = System.Windows.WindowState.Maximized;
          }
        }
      }
    }
  }
}