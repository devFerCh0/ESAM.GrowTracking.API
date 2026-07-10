using ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus
{
    public record ChangeRoleCampusCommand : IRequest<Result<ChangeRoleCampusResponse>>
    {
        public int RoleId { get; init; }

        public int CampusId { get; init; }

        public ChangeRoleCampusCommand(int roleId, int campusId)
        {
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}