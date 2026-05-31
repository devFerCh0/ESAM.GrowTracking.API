using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Results;
using MediatR;

namespace ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus
{
    public record AssumeRoleCampusCommand : IRequest<Result<AssumeRoleCampusResponse>>
    {
        public int? WorkProfileId { get; init; }

        public int? RoleId { get; init; }

        public int? CampusId { get; init; }

        public AssumeRoleCampusCommand(int? workProfileId, int? roleId, int? campusId)
        {
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }
    }
}