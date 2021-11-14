using Entities.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<EmployeeEntity> ShapeData(IEnumerable<T> entities, string fieldsString);
        EmployeeEntity ShapeData(T entity, string fieldsString);
    }
}
