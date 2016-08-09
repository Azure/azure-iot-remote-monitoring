using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface IDocumentDBClient<T> where T : new()
    {
        /// <summary>
        /// Gets a document by its id.
        /// </summary>
        /// <param name="id">The id of the document to get</param>
        Task<T> GetAsync(string id);

        /// <summary>
        /// Returns a <see cref="IQueryable{T}"/> that can be used to query db.
        /// </summary>
        Task<IQueryable<T>> QueryAsync();

        /// <summary>
        /// Saves a document to the the db.
        /// </summary>
        /// <param name="data">The data of the document to save.</param>
        Task<T> SaveAsync(T data);

        /// <summary>
        /// Deletes a document from the db.
        /// </summary>
        /// <param name="id">The id of the document to delete</param>
        Task DeleteAsync(string id);
    }
}