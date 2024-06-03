using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHDestructorService: Saes.Protos.ModelServices.JournalInstanceForCIHDestructorService.JournalInstanceForCIHDestructorServiceBase
    {
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public JournalInstanceForCIHDestructorService(SaesContext ctx, IMapper mapper, ILogger<JournalInstanceForCIHDestructorService> logger)
        {
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }
        public override async Task<JournalInstanceForCIHDestructorLookupResponse> Search(JournalInstanceForCIHDestructorLookup request, ServerCallContext context)
        {
            var query = _ctx.JournalInstanceForCihdestructors.AsQueryable();

            query = query.Where(x => x.SysIsDeleted == false);

            // Тут фильтрация

            //query = query
            //    .Include(x => x.SignFile)
            //    .Include(x => x.ReceivedFrom);

            var response = new JournalInstanceForCIHDestructorLookupResponse();

            var dtos = await query.ProjectToType<JournalInstanceForCIHDestructorDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<JournalInstanceForCIHDestructorLookupResponse> Add(JournalInstanceForCIHDestructorDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(JournalInstanceForCIHDestructorDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(JournalInstanceForCIHDestructorLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
