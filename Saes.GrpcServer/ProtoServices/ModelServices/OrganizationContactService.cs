using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Core;
using Saes.Protos;
using Saes.Protos.ModelServices;

#nullable disable

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class OrganizationContactService : Saes.Protos.ModelServices.OrganizationContactService.OrganizationContactServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly ILogger<OrganizationContactService> _logger;
        private readonly IMapper _mapper;

        public OrganizationContactService(SaesContext saesContext, ILogger<OrganizationContactService> logger, IMapper mapper)
        {
            _ctx = saesContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<Protos.ModelServices.OrganizationContactLookupResponse> Search(Protos.ModelServices.OrganizationContactLookup request, ServerCallContext context)
        {
            var query = _ctx.OrganizationContacts.AsQueryable();

            if (request.OrganizationContactID != null)
            {
                query = query.Where(x => x.OrganizationContactId == request.OrganizationContactID);
                goto EndFilters;
            }

            query = request.ContactTypeID != null ? query.Where(x => x.OrganizationContactId == request.ContactTypeID) : query;
            query = request.OrganizationID != null ? query.Where(x => x.OrganizationId == request.OrganizationID) : query;

            EndFilters:

            query = query.Include(x => x.ContactType);
            //    .Include(x => x.BusinessEntity);

            var response = new OrganizationContactLookupResponse();

            var entities = await query.ToListAsync();

            response.Data.AddRange(entities.Select(x => x.Adapt<OrganizationContactDto>(_mapper.Config)));

            return response;
        }

        public override async Task<OrganizationContactLookupResponse> Add(OrganizationContactDataRequest request, ServerCallContext context)
        {
            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на добавление контакта организации отсутствовал идентификатор организации"));
            }

            if (!request.ContactTypeID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на добавление контакта организации отсутствовал идентификатор типа контакта"));
            }

            if (!await _ctx.Organizations.AnyAsync(x => x.OrganizationId == request.OrganizationID))
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"В запросе на добавление контакта организации был указан несуществующий идентификатор организации"));
            }

            var entity = new OrganizationContact
            {
                OrganizationId = request.OrganizationID.Value,
                Value = request.Value,
                ContactTypeId = request.ContactTypeID.Value,
                Note = request.Note
            };

            var addedEntity = await _ctx.OrganizationContacts.AddAsync(entity);

            await _ctx.SaveChangesAsync();

            var response = new OrganizationContactLookupResponse();

            response.Data.Add(addedEntity.Entity.Adapt<OrganizationContactDto>(_mapper.Config));

            return response;
        }

        public override async Task<StatusResponse> Edit(OrganizationContactDataRequest request, ServerCallContext context)
        {
            if (!request.OrganizationContactID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на изменение контакта организации отсутствовал идентификатор контакта"));
            }

            if (!request.OrganizationID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на изменение контакта организации отсутствовал идентификатор организации"));
            }

            if (!request.ContactTypeID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на изменение контакта организации отсутствовал идентификатор типа контакта"));
            }

            var entity = await _ctx.OrganizationContacts.FirstOrDefaultAsync(x => x.OrganizationContactId == request.OrganizationContactID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"В запросе на изменение контакта организации был указан несуществующий идентификатор контакта"));
            }

            entity.OrganizationId = request.OrganizationID.Value;
            entity.Value = request.Value;
            entity.ContactTypeId = request.ContactTypeID.Value;
            entity.Note = request.Note;

            _ctx.OrganizationContacts.Update(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }

        public override async Task<StatusResponse> Remove(OrganizationContactLookup request, ServerCallContext context)
        {
            if (!request.OrganizationContactID.HasValue)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"В запросе на изменение контакта организации отсутствовал идентификатор контакта"));
            }

            var entity = await _ctx.OrganizationContacts.FirstOrDefaultAsync(x => x.OrganizationContactId == request.OrganizationContactID);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"В запросе на изменение контакта организации был указан несуществующий идентификатор контакта"));
            }

            _ctx.OrganizationContacts.Remove(entity);

            await _ctx.SaveChangesAsync();

            return new StatusResponse { Result = true };
        }
    }
}
