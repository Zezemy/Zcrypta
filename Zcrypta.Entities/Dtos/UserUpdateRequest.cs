using Zcrypta.Entities.Dtos;
using Zcrypta.Entities.Models;

namespace Zcrypta.Entities
{
    public class UserUpdateRequest
    {
        public UserModel UserModel { get; set; }
    }
    public class UserUpdateResponseMessage: BaseResponse
    {
    }
}