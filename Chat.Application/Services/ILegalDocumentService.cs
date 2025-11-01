using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Application.Services
{
    public interface ILegalDocumentService
    {
        Task IndexLegalDocumentsAsync(CancellationToken cancellationToken = default);
        Task<string> SearchLegalDocumentsAsync(string query, int topK = 3, CancellationToken cancellationToken = default);
    }
}
