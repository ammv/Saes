using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class FileService: Protos.ModelServices.FileService.FileServiceBase
    {
        private readonly ILogger<FileService> _logger;
        private readonly SaesContext _ctx;
        private readonly IMapper _mapper;

        public FileService(ILogger<FileService> logger, SaesContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public override async Task<FileLookupResponse> Search(FileLookup request, ServerCallContext context)
        {

            var query = _ctx.Files.AsQueryable();

            //query = query.Where(x => x.SysIsDeleted == false);

            //query = request.FileId != null ? query.Where(x => x.FileId == request.FileId) : query;
            //query = request.Name != null ? query.Where(x => x.Name.Contains(request.Name)) : query;

           // query = query.Include(x => x.File);

            var response = new FileLookupResponse();

            var dtos = await query.ProjectToType<FileDto>(_mapper.Config).ToListAsync();

            response.Data.AddRange(dtos);

            return response;
        }

        public override Task<FileLookupResponse> Add(FileDataRequest request, ServerCallContext context)
        {
            return base.Add(request, context);
        }

        public override Task<StatusResponse> Edit(FileDataRequest request, ServerCallContext context)
        {
            return base.Edit(request, context);
        }

        public override Task<StatusResponse> Remove(FileLookup request, ServerCallContext context)
        {
            return base.Remove(request, context);
        }
    }
}
