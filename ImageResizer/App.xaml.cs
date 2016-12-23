using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageResizer
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string openImagePath { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if(e.Args.Length > 0)
                openImagePath = e.Args[0].ToString();
            base.OnStartup(e);
        }
    }


}
