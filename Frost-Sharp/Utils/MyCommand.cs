using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Frost_Sharp.Utils {
	public class MyCommand : ICommand {
		private readonly Action _Action;

		public MyCommand(Action action) {
			_Action = action;
		}

		public void Execute(object parameter) {
			_Action();
		}

		public bool CanExecute(object parameter) {
			return true;
		}

		public event EventHandler CanExecuteChanged;
	}
}
