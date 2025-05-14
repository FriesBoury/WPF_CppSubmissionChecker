namespace CppSubmissionChecker_ViewModel.DataClasses
{
	public class BatchOperationStats : ViewmodelBase
	{
		public event EventHandler Completed;
		private int _actionsCompleted = 0;
		private int _actionsStarted = 0;
		private int _numActionsToDo = 1;
		private readonly string _message;

		public string Message => $"{_message} - {_actionsStarted} of {_numActionsToDo}";
		public int NumActionsToDo
		{
			get => _numActionsToDo; set
			{
                if (_numActionsToDo == value) return;
				_numActionsToDo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(Message));
			}
		}
		public int ActionsStarted
		{
			get => _actionsStarted; set
			{
                if(_actionsStarted == value) return;
				_actionsStarted = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Progress));
				OnPropertyChanged(nameof(Message));

			}
		}
		public int ActionsCompleted
		{
			get => _actionsCompleted; set
			{
                if(_actionsCompleted == value) return;
				_actionsCompleted = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Progress));
				OnPropertyChanged(nameof(Message));

			}
		}

        public BatchOperationStats()
        {
			_message = "Doing bulk operations";
        }

        public BatchOperationStats(string message)
        {
			_message = message;
		}
        public float Progress => (float)ActionsCompleted / (float)NumActionsToDo;

		public void OnCompleted()
		{
			Completed?.Invoke(this, EventArgs.Empty);
		}
    }
}
