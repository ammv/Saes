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

            query = query.Include(x => x.BusinessAddress)
                .Include(x => x.BusinessEntity);

            var response = new OrganizationLookupResponse();

            var dtos = await query.ProjectToType<Protos.OrganizationDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override async Task<OrganizationLookupResponse> Add(OrganizationDataRequest request, ServerCallContext context)
        {
            if (request.IsOwnerJournalAccountingCPI == null)
                throw new ArgumentNullException(nameof(request.IsOwnerJournalAccountingCPI));

            BusinessEntityType businessEntityType = _ctx.BusinessEntityTypes.FirstOrDefault(x => x.Name == "организация");

            BusinessEntity businessEntity = new BusinessEntity { BusinessEntityType = businessEntityType };

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
                DateOfAssignmentOgrn = request.DateOfAssignmentOGRN.ToDateTime().ToLocalTime(),
                Kpp = request.KPP,
                BusinessEntity = businessEntity
            };

            organization = _ctx.Organizations.Add(organization).Entity;

            await _ctx.SaveChangesAsync();

            var response = new OrganizationLookupResponse();

            response.Data.Add(organization.Adapt<OrganizationDto>());

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

            Organization organization = _ctx.Organizations.Single(x => x.BusinessEntityId == request.BusinessEntityID);

            organization.Okpo = organization.Okpo == request.OKPO ? organization.Okpo: request.OKPO;
            organization.Okved = organization.Okved == request.OKVED ? organization.Okved : request.OKVED;
            organization.FullName = organization.FullName == request.FullName ? organization.FullName : request.FullName;
            organization.ShortName = organization.ShortName == request.ShortName ? organization.ShortName : request.ShortName;
            organization.DirectorFullName = organization.DirectorFullName == request.DirectorFullName ? organization.DirectorFullName : request.DirectorFullName;
            organization.Inn = organization.Inn == request.INN ? organization.Inn : request.INN;
            organization.Ogrn = organization.Ogrn == request.OGRN ? organization.Ogrn : request.OGRN;
            organization.DateOfAssignmentOgrn = organization.DateOfAssignmentOgrn == request.DateOfAssignmentOGRN?.ToDateTime().ToLocalTime() ? organization.DateOfAssignmentOgrn : request.DateOfAssignmentOGRN?.ToDateTime().ToLocalTime();
            organization.FullName = organization.FullName == request.FullName ? organization.FullName : request.FullName;
            organization.Kpp = organization.Kpp == request.KPP ? organization.Kpp : request.KPP;

            _ctx.Organizations.AddOrUpdate(organization);

            await _ctx.SaveChangesAsync();

            var response = new StatusResponse
            {
                Result = true
            };

            return response;
        }
    }
}
