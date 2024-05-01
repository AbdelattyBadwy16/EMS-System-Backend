using EMS_SYSTEM.APPLICATION.Repositories.Interfaces;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.GenericRepository;
using EMS_SYSTEM.APPLICATION.Repositories.Interfaces.IUnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_SYSTEM.APPLICATION.Repositories.Services.UnitOfWork
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly UnvcenteralDataBaseContext _context;

        public IFacultyService Faculty { get; private set; }
        public IStudentRepository Students { get; private set; }
        public IGenericRepository<Staff> Staff {  get; private set; }
        public UnitOfWork(UnvcenteralDataBaseContext _context)
        {
            this._context = _context;
            Faculty = new FacultyService(_context);
            Students = new StudentRepository(_context);
            Staff = new GenericRepository<Staff>(_context);
        }
        public int Save()
        {
            return _context.SaveChanges();
        }
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
