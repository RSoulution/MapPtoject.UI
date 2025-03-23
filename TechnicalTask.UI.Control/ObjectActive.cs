using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TechnicalTask.Entities;

namespace TechnicalTask.UI.Control
{
    public class ObjectActive : EntObject //Змінюємо клас сутності об'єкта БД під наші потреби
    {
        public bool IsActive { get; set; } //Чи активний цей об'єкт

        public DateTime DateTime { get; set; } //Коли востаннє оновлювався

        public ObjectActive(int id, double azimuth, double lat, double lon, bool active, DateTime dateTime): base(id, azimuth, lat, lon) { 
            IsActive = active;
            DateTime = dateTime;
        }
        public ObjectActive(EntObject entityObject, bool active, DateTime dateTime)
        {
            Id = entityObject.Id;
            Azimuth = entityObject.Azimuth;
            Latitude = entityObject.Latitude;
            Longitude = entityObject.Longitude;
            IsActive = active;
            DateTime = dateTime;
        }

        public override string ToString() //Переписуємо ToString для зручного додавання в ListBox
        {
            return $"Object {Id}";
        }
    }
}
