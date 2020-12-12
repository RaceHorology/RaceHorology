/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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
using System.Windows.Controls;

using System.IO;

namespace RaceHorology
{

  /// <summary>
  /// Stores and displays tha last recently used files
  /// </summary>
  public class MruList
  {
    // The application's name.
    private string ApplicationName;

    // A list of the files.
    private int NumFiles;
    private List<FileInfo> FileInfos;

    // The File menu.
    private MenuItem MyMenu;

    // Raised when the user selects a file from the MRU list.
    public delegate void FileSelectedEventHandler(string file_name);
    public event FileSelectedEventHandler FileSelected;

    // Constructor.
    public MruList(string application_name, MenuItem menu, int num_files)
    {
      ApplicationName = application_name;
      MyMenu = menu;
      NumFiles = num_files;
      FileInfos = new List<FileInfo>();

      // Reload items from the registry.
      LoadFiles();

      // Display the items.
      ShowFiles();
    }

    // Load saved items from the Registry.
    private void LoadFiles()
    {
      // Reload items from the registry.
      for (int i = 0; i < NumFiles; i++)
      {
        string file_name = (string)RegistryTools.GetSetting(ApplicationName, "FilePath" + i.ToString(), "");
        if (file_name != "")
        {
          FileInfos.Add(new FileInfo(file_name));
        }
      }
    }

    // Save the current items in the Registry.
    private void SaveFiles()
    {
      // Delete the saved entries.
      for (int i = 0; i < NumFiles; i++)
      {
        RegistryTools.DeleteSetting(ApplicationName, "FilePath" + i.ToString());
      }

      // Save the current entries.
      int index = 0;
      foreach (FileInfo file_info in FileInfos)
      {
        RegistryTools.SaveSetting(ApplicationName, "FilePath" + index.ToString(), file_info.FullName);
        index++;
      }
    }

    // Remove a file's info from the list.
    private void RemoveFileInfo(string file_name)
    {
      // Remove occurrences of the file's information from the list.
      for (int i = FileInfos.Count - 1; i >= 0; i--)
      {
          if (FileInfos[i].FullName == file_name) FileInfos.RemoveAt(i);
      }
    }

    // Add a file to the list, rearranging if necessary.
    public void AddFile(string file_name)
    {
      // Remove the file from the list.
      RemoveFileInfo(file_name);

      // Add the file to the beginning of the list.
      FileInfos.Insert(0, new FileInfo(file_name));

      // If we have too many items, remove the last one.
      if (FileInfos.Count > NumFiles) FileInfos.RemoveAt(NumFiles);

      // Display the files.
      ShowFiles();

      // Update the Registry.
      SaveFiles();
    }

    // Remove a file from the list, rearranging if necessary.
    public void RemoveFile(string file_name)
    {
      // Remove the file from the list.
      RemoveFileInfo(file_name);

      // Display the files.
      ShowFiles();

      // Update the Registry.
      SaveFiles();
    }

    // Display the files in the menu items.
    private void ShowFiles()
    {
      // Delete existing items
      while (!MyMenu.Items.IsEmpty)
        MyMenu.Items.RemoveAt(0);

      // Insert Items
      for (int i = 0; i < FileInfos.Count; i++)
      {
        MenuItem subMenu = new MenuItem();
        subMenu.Header = string.Format("{0} {1}", i + 1, FileInfos[i].FullName);
        subMenu.Tag = FileInfos[i];
        subMenu.Click += File_Click;
        MyMenu.Items.Add(subMenu);
      }
    }

    // The user selected a file from the menu.
    private void File_Click(object sender, EventArgs e)
    {
      // Don't bother if no one wants to catch the event.
      if (FileSelected != null)
      {
        // Get the corresponding FileInfo object.
        MenuItem menu_item = sender as MenuItem;
        FileInfo file_info = menu_item.Tag as FileInfo;

        // Raise the event.
        FileSelected(file_info.FullName);
      }
    }
  }
}
