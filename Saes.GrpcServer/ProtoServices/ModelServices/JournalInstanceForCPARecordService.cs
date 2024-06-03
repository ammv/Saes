using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCPARecordService: Saes.Protos.ModelServices.JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCPARecordService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCPARecordService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalInstanceForCPARecordLookupResponse> Search(JournalInstanceForCPARecordLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCparecords.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            // Тут фильтрация

            query = query
                .Include(x => x.Organization)
                .Include(x => x.SignFile)
                .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceForCPARecordLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCPARecordDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<JournalInstanceForCPARecordLookupResponse> Add(JournalInstanceForCPARecordDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(JournalInstanceForCPARecordDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(JournalInstanceForCPARecordLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
