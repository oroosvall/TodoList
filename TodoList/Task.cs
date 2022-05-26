using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoList
{
    [Serializable]
    class Task
    {

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public Task()
        {
            taskName = "Test";
        }

        string taskName;
        string taskDetails;
        int estTime;
        int timeTaken;
        bool done;

        public string TaskName { get => taskName; set { taskName = value; OnPropertyChanged("TaskName"); } }
        public int EstTime { get => estTime; set { estTime = value; OnPropertyChanged("EstTime"); } }
        public int TimeTaken { get => timeTaken; set { timeTaken = value; OnPropertyChanged("TimeTaken"); } }
        public string TaskDetails { get => taskDetails; set { taskDetails = value; OnPropertyChanged("TaskDetails"); } }
        public bool Done { get => done;
            set { done = value; OnPropertyChanged("Done"); }
        }

        public new string ToString => taskName;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                if (name == "Done")
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

    }
}
