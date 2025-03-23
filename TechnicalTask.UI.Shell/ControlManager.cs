using System.Windows;
using System.Windows.Controls;
using TechnicalTask.UI.Abstractions;

namespace TechnicalTask.UI.Shell
{
    class ControlManager : IControlManager //Запам'ятовує та додає елементи на необхідну область
    {
        private readonly Dictionary<string, UIElement> controlDictionary = new Dictionary<string, UIElement>();
        public void Register<T>(string key, T element) where T : UIElement
        {
            UIElement userControl = (UIElement)element;
            this.controlDictionary.Add(key, userControl);
        }
        public void Place(string containerName, string regionName, string elementName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ContentControl containerControl = this.GetControl(containerName) as ContentControl;
                ContentControl elementControl = this.GetControl(elementName) as ContentControl;
                object region = containerControl.FindName(regionName);
                ((Grid)region).Children.Clear();
                if (elementControl != null)
                {
                    ((Grid)region).Children.Add(elementControl);
                }
            }));

        }
        public UIElement GetControl(string key)
        {
            UIElement userControl = null;
            if (this.controlDictionary.ContainsKey(key))
            {
                userControl = this.controlDictionary[key];
            }
            else
            {
                throw new Exception($"The control '{key}' is missing");

            }
            return userControl;
        }
    }
}
