﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernateStart.Domain;


namespace DataBaseManager
{
    class AutoMapper
    {
        private readonly ModelMapper _modelMapper;
        public AutoMapper()
        {
            _modelMapper = new ModelMapper();
        }
    }
}