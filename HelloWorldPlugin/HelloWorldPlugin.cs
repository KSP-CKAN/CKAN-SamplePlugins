using CKAN.Versioning;
using CKAN.GUI;

namespace HelloWorldPlugin
{

    public class HelloWorldPlugin : IGUIPlugin
    {
        public override void Initialize()
        {
            Main.Instance.currentUser.RaiseError("Hello World!");
        }

        public override void Deinitialize()
        {
        }

        public override string GetName() => "Hello World Plugin";

        public override ModuleVersion GetVersion() => new ModuleVersion(VERSION);

        private static readonly string VERSION = "v1.0.1";
    }
}
