using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Rocket_Remote
{
    [RunInstaller(true)]
    public partial class SetupInstaller : System.Configuration.Install.Installer
    {
        public SetupInstaller() : base()
        {
            // Attach the 'Committed' event.
            this.Committed += new InstallEventHandler(SetupInstaller_Committed) ;
        }

        // Event handler for 'Committed' event.
        private void SetupInstaller_Committed(object sender, InstallEventArgs e)
        {
            Console.WriteLine("");
            Console.WriteLine("Committed Event occurred.");
            Console.WriteLine("");
        }

        // Override the 'Install' method.
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
        }

        // Override the 'Commit' method.
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        // Override the 'Rollback' method.
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

    }
}
