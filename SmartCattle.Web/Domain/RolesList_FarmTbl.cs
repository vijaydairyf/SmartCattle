﻿using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartCattle.Web.Domain
{
    public class RolesList_FarmTbl
    {
        public virtual int ID { get; set; }
        public virtual string jName { get; set; }
        public virtual string uId { get; set; }
        public virtual string Permissions { get; set; }
        public virtual string Comment { get; set; }
    }

    class RolesList_FarmTblMapping : ClassMap<RolesList_FarmTbl>
    {
        public RolesList_FarmTblMapping()
        {
            Id(x => x.ID);
            Map(x => x.jName).Nullable();
            Map(x => x.uId).Nullable();
            Map(x => x.Permissions).Nullable();
            Map(x => x.Comment).Nullable();

            Table("SmartCattle.RolesList_FarmTbl");
        }
    }
}