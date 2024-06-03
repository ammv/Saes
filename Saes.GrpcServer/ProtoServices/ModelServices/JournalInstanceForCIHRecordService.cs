using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHRecordService: Saes.Protos.ModelServices.JournalInstanceForCIHRecordService.JournalInstanceForCIHRecordServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCIHRecordService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCIHRecordService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalInstanceForCIHRecordLookupResponse> Search(JournalInstanceForCIHRecordLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCihrecords.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            // Тут фильтрация

            query = query
                .Include(x => x.Organization)
                .Include(x => x.SignFile)
                .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceForCIHRecordLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCIHRecordDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<JournalInstanceForCIHRecordLookupResponse> Add(JournalInstanceForCIHRecordDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(JournalInstanceForCIHRecordDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(JournalInstanceForCIHRecordLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
