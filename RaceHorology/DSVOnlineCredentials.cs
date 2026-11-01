/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
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
using System.Security.Cryptography;
using System.Text;

namespace RaceHorology
{
  /// <summary>
  /// Access to the (optional) credentials for the DSV online services (point list download via HTTP Basic authentication).
  /// The password is stored encrypted for the current Windows user (DPAPI) in the user settings.
  /// </summary>
  public static class DSVOnlineCredentials
  {
    public static string Username
    {
      get { return Properties.Settings.Default.DSVOnline_Username; }
      set { Properties.Settings.Default.DSVOnline_Username = value; }
    }

    public static string Password
    {
      get { return unprotect(Properties.Settings.Default.DSVOnline_Password); }
      set { Properties.Settings.Default.DSVOnline_Password = protect(value); }
    }


    static string protect(string plain)
    {
      if (string.IsNullOrEmpty(plain))
        return string.Empty;

      byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), null, DataProtectionScope.CurrentUser);
      return Convert.ToBase64String(encrypted);
    }

    static string unprotect(string encrypted)
    {
      if (string.IsNullOrEmpty(encrypted))
        return string.Empty;

      try
      {
        byte[] plain = ProtectedData.Unprotect(Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plain);
      }
      catch (Exception)
      {
        // e.g. settings copied from another Windows user; password needs to be entered again
        return string.Empty;
      }
    }
  }
}
