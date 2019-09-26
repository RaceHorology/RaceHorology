using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DSVAlpin2
{
  public class CBItem
  {
    public string Text { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
      return Text;
    }
  }

  public static class CBItemExtensions
  {

    public static bool SelectCBItem(this ComboBox cmb, object value)
    {
      foreach (CBItem item in cmb.Items)
        if (object.Equals(item.Value, value))
        {
          cmb.SelectedValue = item;
          return true;
        }

      return false;
    }

  }


}
