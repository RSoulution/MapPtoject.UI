﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TechnicalTask.Entities;

namespace TechnicalTask.UI.Control
{
    public class ObjectActive : EntObject //We change the entity class of the DB object to suit our needs
    {
        public bool IsActive { get; set; } //Is this object active

        public DateTime DateTime { get; set; } //When was it last updated?

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

        public override string ToString() //Rewriting ToString for easy addition to ListBox
        {
            return $"Object {Id}";
        }
    }
}
