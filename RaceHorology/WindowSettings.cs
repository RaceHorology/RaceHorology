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

using System.Linq;
using System.Windows;

namespace RaceHorology
{
  public class WindowSettings
  {
    public static void LoadWindowSettings(Window window)
    {
      if (Properties.Settings.Default._windowState != -1)
        window.WindowState = (System.Windows.WindowState)Properties.Settings.Default._windowState;
      else
        window.WindowState = System.Windows.WindowState.Maximized;

      if (Properties.Settings.Default._windowLocationLeft != 0 && Properties.Settings.Default._windowLocationTop != 0)
      {
        window.Left = Properties.Settings.Default._windowLocationLeft;
        window.Top = Properties.Settings.Default._windowLocationTop;
      }

      if (Properties.Settings.Default._windowLocationWidth != 0 && Properties.Settings.Default._windowLocationHeight != 0)
      {
        window.Width = Properties.Settings.Default._windowLocationWidth;
        window.Height = Properties.Settings.Default._windowLocationHeight;
      }

      if (!IsVisibleOnAnyScreen(window))
      {
        window.WindowState = System.Windows.WindowState.Maximized;
        window.Left = SystemParameters.VirtualScreenLeft;
        window.Top = SystemParameters.VirtualScreenTop;
        window.Width = SystemParameters.VirtualScreenWidth;
        window.Height = SystemParameters.VirtualScreenHeight;
      }
    }

    public static void SaveWindowSettings(Window window)
    {
      Properties.Settings.Default._windowState = (int)window.WindowState;
      Properties.Settings.Default._windowLocationLeft = window.Left;
      Properties.Settings.Default._windowLocationTop = window.Top;
      Properties.Settings.Default._windowLocationWidth = window.Width;
      Properties.Settings.Default._windowLocationHeight = window.Height;
      Properties.Settings.Default.Save();
    }

    private static bool IsVisibleOnAnyScreen(Window window)
    {
      return System.Windows.Forms.Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(new System.Drawing.Rectangle((int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height)));
    }
  }
}
