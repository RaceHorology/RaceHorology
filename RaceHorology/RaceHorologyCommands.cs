using System;
using System.Collections.Generic;
using System.Windows;
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

    //Define more commands here, just like the one above
  }
}


