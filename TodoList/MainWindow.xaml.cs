using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace TodoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [Serializable]
        class ProjectHolder : INotifyPropertyChanged
        {
            [field: NonSerialized]
            public event PropertyChangedEventHandler PropertyChanged;

            private void recalcComplete()
            {
                double max = proj.Count;
                double comp = getCompletedProjects();

                double frac = comp / max;

                ProjectsComplete = (Int32)(frac * 100.0);
                OnPropertyChanged("TaskStatus");
            }

            public void ItemChanged(object sender, PropertyChangedEventArgs e)
            {
                recalcComplete();
            }

            public ProjectHolder()
            {
                Projects = new ObservableCollection<Project>();
            }
            
            public void UpdatePropertyHandlers()
            {
                Projects.ToList().ForEach(x => { x.PropertyChanged += ItemChanged; x.UpdatePropertyHandler(); });
                Projects.CollectionChanged += Projects_CollectionChanged;
            }

            private void Projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                recalcComplete();
            }

            ObservableCollection<Project> proj;
            int projectComplete;

            public ObservableCollection<Project> Projects
            {
                get { return proj; }
                set {
                    proj = value;
                    OnPropertyChanged("Projects");
                }
            }

            public int ProjectsComplete {
                get { return projectComplete; }
                set {
                    projectComplete = value;
                    OnPropertyChanged("ProjectsComplete");
                }
            }

            public string TaskStatus
            {
                get {
                    double completed = getCompletedTasks();
                    double total = getTotalTasks();
                    double per = (completed / total) * 100.0;
                    if (double.IsNaN(per))
                        per = 100.0;
                    return completed + "/" + total + " " + per.ToString("0.##") + "%";
                }
                set { }
            }

            private Int32 getCompletedTasks()
            {
                int completed = 0;
                foreach(var p in proj)
                {
                    completed += p.getCompletedTasks();
                }
                return completed;
            }

            private Int32 getTotalTasks()
            {
                int tasks = 0;
                foreach (var p in proj)
                {
                    tasks += p.Tasks.Count();
                }
                return tasks;
            }

            private Int32 getCompletedProjects()
            {
                Int32 completeCount = 0;
                proj.ToList().ForEach(x => completeCount += x.IsComplete);
                return completeCount;
            }

            // Create the OnPropertyChanged method to raise the event
            protected void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }

            public void savePath(string path)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream outStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                formatter.Serialize(outStream, this);
                outStream.Close();
            }

        }
        
        ProjectHolder ph;
        String lastFile;
        public MainWindow()
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/TodoList/";
            Directory.CreateDirectory(filePath);
            InitializeComponent();

            IFormatter formatter = new BinaryFormatter();
            try
            {
                Stream inStream = new FileStream(filePath + "todo.info", FileMode.Open, FileAccess.Read);
                lastFile = (String)formatter.Deserialize(inStream);
            }
            catch (Exception) { }

            if (!String.IsNullOrEmpty(lastFile))
            {
                openFile(lastFile);
            }
            else
            {
                ph = new ProjectHolder();
                projList.DataContext = ph;
                ph.UpdatePropertyHandlers();
            }
        }
        
        private void AddProject(object sender, RoutedEventArgs e)
        {
            string name = projName.Text;
            if (!string.IsNullOrEmpty(name))
            {
                Project p = new Project();
                p.PropertyChanged += ph.ItemChanged;
                p.ProjectName = name;
                projName.Text = "";
                ph.Projects.Add(p);
                projList.SelectedValue = p;
                ph.ItemChanged(null, null);
            }
        }
        
        private void AddTask(object sender, RoutedEventArgs e)
        {
            Project p = projList.SelectedValue as Project;
            if(p != null)
            {
                Task t = new Task();
                p.Tasks.Insert(0, t);
                taskList.SelectedValue = t;
            }
        }

        private void RemoveProject(object sender, RoutedEventArgs e)
        {
            Project p = projList.SelectedValue as Project;
            if (p != null)
            {
                ph.Projects.Remove(p);
            }
        }

        private void RemoveTask(object sender, RoutedEventArgs e)
        {
            Project p = projList.SelectedValue as Project;
            if (p != null)
            {
                Task t = taskList.SelectedValue as Task;
                if(t != null)
                {
                    p.Tasks.Remove(t);
                }
            }
        }

        private void openFile(string file)
        {
            IFormatter formatter = new BinaryFormatter();
            ph = new ProjectHolder();

            try
            {
                Stream inStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                ph = (ProjectHolder)formatter.Deserialize(inStream);
                inStream.Close();
            }
            catch (Exception) { }

            DataContext = ph;
            projList.DataContext = ph;
            ph.UpdatePropertyHandlers();
        }

        private void UpdateProjectName(object sender, RoutedEventArgs e)
        {
            Project p = projList.SelectedValue as Project;
            if (p != null)
            {
                string name = projName.Text;
                if (!string.IsNullOrEmpty(name))
                {
                    int index = ph.Projects.IndexOf(p);
                    ph.Projects.RemoveAt(index);
                    p.ProjectName = name;
                    ph.Projects.Insert(index, p);
                    projName.Text = "";
                    projList.SelectedValue = p;
                }
            }
        }

        private void Taskup_Click(object sender, RoutedEventArgs e)
        {
            var proj = projList.SelectedItem as Project;
            if (proj != null)
            {
                if (taskList.SelectedIndex >= 1)
                {
                    proj.Tasks.Move(taskList.SelectedIndex, taskList.SelectedIndex - 1);
                }
            }
        }

        private void Taskdown_Click(object sender, RoutedEventArgs e)
        {
            var proj = projList.SelectedItem as Project;
            if (proj != null)
            {
                if (taskList.SelectedIndex < (taskList.Items.Count - 1))
                {
                    proj.Tasks.Move(taskList.SelectedIndex, taskList.SelectedIndex + 1);
                }
            }
        }

        private void Projup_Click(object sender, RoutedEventArgs e)
        {
            if (projList.SelectedIndex >= 1)
            {
                ph.Projects.Move(projList.SelectedIndex, projList.SelectedIndex - 1);
            }
        }

        private void Projdown_Click(object sender, RoutedEventArgs e)
        {
            if (projList.SelectedIndex < (projList.Items.Count - 1))
            {
                ph.Projects.Move(projList.SelectedIndex, projList.SelectedIndex + 1);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/TodoList/";
            IFormatter formatter = new BinaryFormatter();
            Stream outStream = new FileStream(filePath + "todo.info", FileMode.Create, FileAccess.Write);

            formatter.Serialize(outStream, lastFile);
            outStream.Close();

            ph.savePath(lastFile);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.DefaultExt = ".todo";
            diag.AddExtension = true;
            diag.Filter = "todolist files | *.todo";
            if (diag.ShowDialog().GetValueOrDefault())
            {
                ph.savePath(diag.FileName);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.DefaultExt = ".todo";
            diag.AddExtension = true;
            diag.Filter = "todolist files | *.todo";
            if (diag.ShowDialog().GetValueOrDefault())
            {
                lastFile = diag.FileName;
                openFile(diag.FileName);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ph = new ProjectHolder();
            projList.DataContext = ph;
            ph.UpdatePropertyHandlers();
        }
    }
}
