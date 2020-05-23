using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoList
{
    [Serializable]
    class Project : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public Project()
        {
            tasks = new ObservableCollection<Task>();
            tasks.CollectionChanged += Tasks_CollectionChanged;
            projectName = "test";
        }

        public void taskChanged(object sender, PropertyChangedEventArgs e)
        {
            Task t = (Task)sender;
            int idx = tasks.IndexOf(t);
            if(t.Done)
            {
                tasks.Move(idx, tasks.Count - 1);
            }
            else
            {
                tasks.Move(idx, 0);
            }

            OnPropertyChanged("TasksCompleted");
            OnPropertyChanged("ProjectCompletion");
        }

        public void UpdatePropertyHandler()
        {
            tasks.CollectionChanged += Tasks_CollectionChanged;
            tasks.ToList().ForEach(x => x.PropertyChanged += taskChanged);
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.NewItems != null)
            {
                foreach (var i in e.NewItems)
                {
                    (i as Task).PropertyChanged += taskChanged;
                }
            }

            OnPropertyChanged("IsComplete");
            OnPropertyChanged("TasksCompleted");
            OnPropertyChanged("ProjectCompletion");
            //throw new NotImplementedException();
        }

        public int getCompletedTasks()
        {
            int count = 0;
            tasks.ToList().ForEach(x => { if (x.Done) count++; });
            return count;
        }

        string projectName;
        ObservableCollection<Task> tasks;
        int complete = 0;

        public ObservableCollection<Task> Tasks {
            get => tasks;
            set { tasks = value; OnPropertyChanged("Tasks"); }
        }
        public string ProjectName {
            get => projectName;
            set { OnPropertyChanged("ProjectName"); projectName = value; }
        }

        public int TasksCompleted {
            get {
                if (tasks.Count != 0)
                {
                    double numTasks = tasks.Count;
                    double completedTasks = getCompletedTasks();
                    double completed = (completedTasks / numTasks) * 100;
                    complete = (numTasks == completedTasks) ? 1 : 0;
                    return (int)completed;
                }
                else
                {
                    complete = 1;
                    return 100;
                }
            }
            set { }
        }

        public string ProjectCompletion
        {
            get { return TasksCompleted.ToString() + "%"; }
        }

        public Int32 IsComplete { get { return complete; } }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
