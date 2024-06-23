using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Core;
using Saes.Protos;
using Saes.Protos.ModelServices;

#nullable disable

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class OrganizationService : Saes.Protos.ModelServices.OrganizationService.OrganizationServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<OrganizationService> _logger;
        private readonly IMapper _mapper;

        public OrganizationService(SaesContext saesContext, ILogger<OrganizationService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.OrganizationLookupResponse> Search(Protos.ModelServices.OrganizationLookup request, ServerCallContext context)
        {
            var query = _ctx.Organizations.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            query = request.BusinessEntityID != null ? query.Where(x => x.BusinessEntityId == request.BusinessEntityID) : query;
            query = request.ChiefAccountantFullName != null ? query.Where(x => x.ChiefAccountantFullName.Contains(request.ChiefAccountantFullName)) : query;
            query = request.FullName != null ? query.Where(x => x.FullName.Contains(request.FullName)) : query;
            query = request.INN != null ? query.Where(x => x.Inn.Contains(request.INN)) : query;
            query = request.ShortName != null ? query.Where(x => x.ShortName.Contains(request.ShortName)) : query;
            query = request.OrganizationID != null ? query.Where(x => x.OrganizationId == request.OrganizationID) : query;

            query = query
                .Include(x => x.BusinessAddress)
                .Include(x => x.BusinessEntity);

            var response = new OrganizationLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<OrganizationDto>(_mapper.Config)));

            return response;
        }

        public override async Task<OrganizationLookupResponse> Add(OrganizationDataRequest request, ServerCallContext context)
        {
            if (request.IsOwnerJournalAccountingCPI == null)
                throw new ArgumentNullException(nameof(request.IsOwnerJournalAccountingCPI));

            BusinessEntityType businessEntityType = await _ctx.BusinessEntityTypes.SingleAsync(x => x.Name == "Организация");

            BusinessEntity businessEntity = _ctx.BusinessEntities.Add(new BusinessEntity { BusinessEntityType = businessEntityType }).Entity;

            Organization organization = new Organization
            {
                Okpo = request.OKPO,
                Okved = request.OKVED,
                FullName = request.FullName,
                ShortName = request.ShortName,
                ChiefAccountantFullName = request.ChiefAccountantFullName,
                DirectorFullName = request.DirectorFullName,
                Inn = request.INN,
                Ogrn = request.OGRN,
                DateOfAssignmentOgrn = request.DateOfAssignmentOGRN?.ToDateTime().ToLocalTime(),
                Kpp = request.KPP,
                BusinessEntity = businessEntity,
                IsOwnerJournalAccountingCpi = request.IsOwnerJournalAccountingCPI.Value
            };

            organization = _ctx.Organizations.Add(organization).Entity;

            await _ctx.SaveChangesAsync();

            var response = new OrganizationLookupResponse();

            response.Data.Add(organization.Adapt<OrganizationDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(OrganizationDataRequest request, ServerCallContext context)
        {
            if (request.IsOwnerJournalAccountingCPI == null)
                throw new ArgumentNullException(nameof(request.IsOwnerJournalAccountingCPI));

            if (request.BusinessEntityID == null)
            {
                throw new ArgumentNullException(nameof(request.BusinessEntityID));
            }

            Organization organization = await _ctx.Organizations.FirstOrDefaultAsync(x => x.BusinessEntityId == request.BusinessEntityID);

            if (organization == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found"));
            }

            organization.Okpo = request.OKPO;
            organization.Okved = request.OKVED;
            organization.FullName = request.FullName;
            organization.ShortName = request.ShortName;
            organization.DirectorFullName = request.DirectorFullName;
            organization.Inn = request.INN;
            organization.Ogrn = request.OGRN;
            organization.DateOfAssignmentOgrn = request.DateOfAssignmentOGRN?.ToDateTime().ToLocalTime();
            organization.Kpp = request.KPP;
            organization.IsOwnerJournalAccountingCpi = request.IsOwnerJournalAccountingCPI.Value;

            _ctx.Organizations.Update(organization);

            await _ctx.SaveChangesAsync();

            var response = new StatusResponse
            {
                Result = true
            };

            return response;
        }

        public override async Task<StatusResponse> Remove(OrganizationLookup request, ServerCallContext context)
        {
            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{nameof(request.OrganizationID)} was null or empty"));
            }

            var entity = await _ctx.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == request.OrganizationID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"{request.OrganizationID}"));
            }

            _ctx.Organizations.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
