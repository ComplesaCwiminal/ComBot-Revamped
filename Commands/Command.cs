using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComBot_Revamped.Commands
{
    public abstract class Command
    {

        public abstract string[] Names
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        public abstract Dictionary<string, string> Arguments
        {
            get;
        }

        public abstract Task Run(CancellationToken ct, params string[] args);


        public virtual void Init()
        {

        }

        public virtual void ConsoleReady()
        {

        }

        public virtual void OnRegister()
        {

        }

        public virtual void PostRegister()
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnInterruption()
        {

        }
    }
}