﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Toasts
{
    public sealed class ToastsDef
    {
        public static void SendNotification_OneStringAndURL(string Title, string Content, string Url)
        {
            if(Url==null)
            {
                SendNotification_TwoString(Title, Content);
                return;
            }
            Windows.UI.Notifications.ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(Title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(Content));

            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", $"ms-appx:///assets/StoreLogo.png");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "logo");

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"toast\",\"param\":\"" + Url + "\"}");

            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", $"ms-winsoundevent:Notification.Default");
            toastNode.AppendChild(audio);

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        public static void SendNotification_TwoString(string Title, string Content)
        {
            SendNotification_TwoString(Title, Content, null, null);
        }
        public static void SendNotification_TwoString(string Title, string Content, string type, string param)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(Title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(Content));

            XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("src", $"ms-appx:///assets/StoreLogo.png");
            ((XmlElement)toastImageAttributes[0]).SetAttribute("alt", "logo");

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            if (type != null)
            {
                if (param == null)
                    ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"" + type + "\"}");
                else
                    ((XmlElement)toastNode).SetAttribute("launch", "{\"type\":\"" + type + "\",\"param\":\"" + param + "\"}");
            }
            XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", $"ms-winsoundevent:Notification.Default");
            toastNode.AppendChild(audio);

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
