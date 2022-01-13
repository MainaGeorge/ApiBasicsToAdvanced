﻿using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using Entities;
using Entities.Models;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext context) : base(context)
        {
        }

        public IEnumerable<Company> GetAllCompanies(bool trackChanges)
        {
            return FindAll(trackChanges)
                .OrderBy(c => c.Name)
                .ToList();
        }

        public Company GetCompany(Guid companyId, bool trackChanges)
        {
            return FindByCondition(c => c.Id == companyId, trackChanges)
                .SingleOrDefault();
        }

        public void CreateCompany(Company company)
        {
            Create(company);
        }

        public IEnumerable<Company> GetByIds(IEnumerable<Guid> companyIds, bool trackChanges)
        {
            return FindByCondition(x => companyIds.Contains(x.Id), trackChanges).ToList();
        }
    }
}