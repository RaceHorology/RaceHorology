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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;

namespace RaceHorology
{
  public class RegistryTools
  {
    // Save a value.
    public static void SaveSetting(string app_name, string name, object value)
    {
      RegistryKey reg_key = Registry.CurrentUser.OpenSubKey("Software", true);
      RegistryKey sub_key = reg_key.CreateSubKey(app_name);
      sub_key.SetValue(name, value);
    }

    // Get a value.
    public static object GetSetting(string app_name, string name, object default_value)
    {
      RegistryKey reg_key = Registry.CurrentUser.OpenSubKey("Software", true);
      RegistryKey sub_key = reg_key.CreateSubKey(app_name);
      return sub_key.GetValue(name, default_value);
    }

    // Delete a value.
    public static void DeleteSetting(string app_name, string name)
    {
      RegistryKey reg_key = Registry.CurrentUser.OpenSubKey("Software", true);
      RegistryKey sub_key = reg_key.CreateSubKey(app_name);
      try
      {
        sub_key.DeleteValue(name);
      }
      catch
      {
      }
    }
  }
}
