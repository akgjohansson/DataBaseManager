using System;
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
        }            _modelMapper.Class<Tabell>(e =>            {
                e.Id(p => p.TabellId, p => p.Generator(Generators.GuidComb));
                e.Property(p => p.Name);
                e.Property(p => p.Age);
                e.Set(p => p.Utländsk, p =>
                {
                    p.Inverse(true);
                    p.Cascade(Cascade.All);
                    p.Key(k => k.Column(col => col.Name("TabellId")));
                }, p => p.OneToMany());
            });
            _modelMapper.Class<Utländsk>(e =>            {
                e.Id(p => p.UtländskId, p => p.Generator(Generators.GuidComb));
                e.Property(p => p.Name);
                e.Property(p => p.Age);
                e.ManyToOne(p => p.Utländsk, mapper =>
               {
                   mapper.Column("UtländskId");
                   mapper.NotNullable(true);
                   mapper.Cascade(Cascade.None);
               });            });
