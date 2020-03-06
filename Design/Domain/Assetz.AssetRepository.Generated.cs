﻿//------------------------------------------------------------------------------
// This is auto-generated code.
//------------------------------------------------------------------------------
// This code was generated by Entity Developer tool using the template for generating Repositories and a Unit of Work for EF Core model.
// Code is generated on: 2020/03/06 15:21:45
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;

namespace Assetz
{
    public partial class AssetRepository : EntityFrameworkRepository<Asset>, IAssetRepository
    {
        public AssetRepository(AssetzModel context) : base(context)
        {
        }

        public virtual ICollection<Asset> GetAll()
        {
            return objectSet.ToList();
        }

        public virtual Asset GetByKey(int _Id)
        {
            return objectSet.SingleOrDefault(e => e.Id == _Id);
        }

        public new AssetzModel Context 
        {
            get 
            {
                return (AssetzModel)base.Context;
            }
        }
    }
}
