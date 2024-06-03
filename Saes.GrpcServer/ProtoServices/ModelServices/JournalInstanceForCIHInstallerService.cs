using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHInstallerService: Saes.Protos.ModelServices.JournalInstanceForCIHInstallerService.JournalInstanceForCIHInstallerServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCIHInstallerService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCIHInstallerService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalInstanceForCIHInstallerLookupResponse> Search(JournalInstanceForCIHInstallerLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCihinstallers.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            // Тут фильтрация

            //query = query
            //    .Include(x => x.SignFile)
            //    .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceForCIHInstallerLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCIHInstallerDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<JournalInstanceForCIHInstallerLookupResponse> Add(JournalInstanceForCIHInstallerDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(JournalInstanceForCIHInstallerDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(JournalInstanceForCIHInstallerLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
