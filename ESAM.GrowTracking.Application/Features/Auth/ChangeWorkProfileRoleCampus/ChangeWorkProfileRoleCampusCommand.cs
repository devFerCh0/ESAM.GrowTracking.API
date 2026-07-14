using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus
{
    public record ChangeWorkProfileRoleCampusCommand : IRequest<Result<ChangeWorkProfileRoleCampusResponse>>
    {
        public int WorkProfileId { get; init; }

        public int RoleId { get; init; }

        public int CampusId { get; init; }

        public ChangeWorkProfileRoleCampusCommand(int workProfileId, int roleId, int campusId)
        {
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}