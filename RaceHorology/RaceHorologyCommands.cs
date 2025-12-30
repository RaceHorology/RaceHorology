using System.Windows.Input;


namespace RaceHorology.Commands
{
  public static class RaceHorologyCommands
  {
    public static readonly RoutedUICommand Exit = new RoutedUICommand
    (
      "Exit",
      "Exit",
      typeof(RaceHorologyCommands),
      new InputGestureCollection()
      {
        new KeyGesture(Key.F4, ModifierKeys.Alt)
      }
    );

    public static readonly RoutedUICommand HandTime = new RoutedUICommand
    (
      "HandTime",
      "HandTime",
      typeof(RaceHorologyCommands)
    );

    public static readonly RoutedUICommand CertDesigner = new RoutedUICommand
    (
      "CertDesigner",
      "CertDesigner",
      typeof(RaceHorologyCommands)
    );


    public static readonly RoutedUICommand ImportTime = new RoutedUICommand
    (
      "ImportTime",
      "ImportTime",
      typeof(RaceHorologyCommands)
    );

    public static readonly RoutedUICommand DeleteRunResults = new RoutedUICommand
    (
      "DeleteRunResults",
      "DeleteRunResults",
      typeof(RaceHorologyCommands)
    );

    public static readonly RoutedUICommand Documentation = new RoutedUICommand
    (
      "Documentation",
      "Documentation",
      typeof(RaceHorologyCommands)
    );

    public static readonly RoutedUICommand AutoUpdate = new RoutedUICommand
    (
      "AutoUpdate",
      "AutoUpdate",
      typeof(RaceHorologyCommands)
    );

    public static readonly RoutedUICommand WA = new RoutedUICommand
    (
      "WA",
      "WA",
      typeof(RaceHorologyCommands)
    );

    //Define more commands here, just like the one above
  }
}


