using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Service1Test
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            serviceInstaller.ServiceName = "Service1";
            const string ServiceNameParameterName = "ServiceName";
            if (!String.IsNullOrWhiteSpace(Context.Parameters[ServiceNameParameterName]))
            {
                serviceInstaller.ServiceName = Context.Parameters[ServiceNameParameterName];
            }

            serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;

            base.Install(stateSaver);
        }
    }
}
