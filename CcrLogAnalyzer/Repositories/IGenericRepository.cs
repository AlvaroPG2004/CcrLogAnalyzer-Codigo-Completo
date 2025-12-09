using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CcrLogAnalyzer.Repositories
{
    public interface IGenericRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetTopAsync(int count, string identifierName = "");
        Task<T> GetAsync(int id, string identifierName = "");
        Task<int> BulkUpdateAsync(IList<T> objectsList);
        Task<int> BulkSaveAsync(IList<T> objectsList);
        Task<int> BulkDeleteAsync(IList<T> objectsList);
        Task<int> GetMaxId(string identifierName);
        void SetMapping();
    }
}
