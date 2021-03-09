using System.Threading.Tasks;
using ac.api.Models.DTO;

namespace ac.api.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string emailTo, string body, string subject, EmailOptionsDTO optionsDTO);
    }
}