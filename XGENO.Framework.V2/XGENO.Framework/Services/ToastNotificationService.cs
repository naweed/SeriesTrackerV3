using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace XGENO.Framework.Services
{
    public class ToastNotificationService
    {
        public ToastNotificationService()
        {
        }

        public void ShowSimpleToast(string content, string title)
        {
            StringBuilder template = new StringBuilder();
            template.Append("<toast><visual><binding template='ToastGeneric'>");
            template.AppendFormat("<text>{0}</text>", title);
            template.AppendFormat("<text>{0}</text>", content);
            template.Append("</binding></visual></toast>");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(template.ToString());
            ToastNotification toast = new ToastNotification(xml);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void ShowToastWithImage(string content, string title, string logo)
        {
            StringBuilder template = new StringBuilder();
            template.Append("<toast><visual><binding template='ToastGeneric'>");
            template.AppendFormat("<text>{0}</text>", title);
            template.AppendFormat("<text>{0}</text>", content);
            template.AppendFormat("<image placement=\"AppLogoOverride\" src=\"{0}\" />", logo);
            template.Append("</binding></visual></toast>");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(template.ToString());
            ToastNotification toast = new ToastNotification(xml);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void ShowToastWithBigImage(string content, string title, string contentLine2, string bigImage)
        {
            StringBuilder template = new StringBuilder();
            template.Append("<toast><visual><binding template='ToastGeneric'>");
            template.AppendFormat("<text>{0}</text>", title);
            template.AppendFormat("<text>{0}</text>", content);
            if(!string.IsNullOrEmpty(contentLine2))
                template.AppendFormat("<text>{0}</text>", contentLine2);
            template.AppendFormat("<image placement=\"inline\" src=\"{0}\" />", bigImage);
            template.Append("</binding></visual></toast>");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(template.ToString());
            ToastNotification toast = new ToastNotification(xml);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void ShowToastWithBigImageAbove(string content, string title, string contentLine2, string bigImage)
        {
            StringBuilder template = new StringBuilder();
            template.Append("<toast><visual><binding template='ToastGeneric'>");
            template.AppendFormat("<image placement=\"inline\" src=\"{0}\" />", bigImage);
            template.AppendFormat("<text>{0}</text>", title);
            template.AppendFormat("<text>{0}</text>", content);
            if (!string.IsNullOrEmpty(contentLine2))
                template.AppendFormat("<text>{0}</text>", contentLine2);
            template.Append("</binding></visual></toast>");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(template.ToString());
            ToastNotification toast = new ToastNotification(xml);

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

    }

}
