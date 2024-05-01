using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_SYSTEM.APPLICATION.Repositories.Interfaces.IUnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IFacultyService Faculty { get; }
        public IStudentRepository Students { get; }
        public IGenericRepository<Staff> Staff {  get; }
        public int Save();
        public Task<int> SaveAsync();
    }
}
