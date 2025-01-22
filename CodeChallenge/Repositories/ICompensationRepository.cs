﻿using CodeChallenge.Models;
using System.Threading.Tasks;
using System;

namespace CodeChallenge.Repositories
{
    public interface ICompensationRepository
    {
        Compensation GetByEmployeeId(string employeeId);
        Compensation Add(Compensation compensation);
        Task SaveAsync();
    } 
}
