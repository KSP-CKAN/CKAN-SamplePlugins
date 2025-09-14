using System.Windows.Forms;

using log4net.Appender;
using log4net.Core;
using log4net.Layout;

using CKAN.GUI;

namespace LogViewPlugin
{
    class LogAppender : AppenderSkeleton
    {

        private TextBox m_Textbox;

        public LogAppender(TextBox textbox)
        {
            m_Textbox = textbox;
            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%newline%date %-5level %logger – %message%newline";
            layout.ActivateOptions();
            Layout = layout;
            ActivateOptions();
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var message = RenderLoggingEvent(loggingEvent);
            Util.Invoke(m_Textbox, () => m_Textbox.AppendText(message));
        }
    }
}
