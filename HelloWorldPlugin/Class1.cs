using System;

namespace HelloWorldPlugin
{

    public class HelloWorldPlugin : CKAN.IGUIPlugin
    {

        private readonly string VERSION = "v1.0.1";

        public override void Initialize()
        {
            CKAN.Main.Instance.m_User.RaiseError("Hello World!");
        }

        public override void Deinitialize()
        {
            
        }

        public override string GetName()
        {
            return "Hello World- plugin";
        }

        public override CKAN.Version GetVersion()
        {
            return new CKAN.Version(VERSION);
        }

    }

}
