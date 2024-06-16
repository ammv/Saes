using Grpc.Core;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Protos;
using Saes.Protos.ModelServices;

namespace Saes.GrpcServer.ProtoServices.ModelServices
{
    public class JournalInstanceForCIHDestructorService : Saes.Protos.ModelServices.JournalInstanceForCIHDestructorService.JournalInstanceForCIHDestructorServiceBase
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

            query = request.DestructorID != null ? query.Where(x => x.DestructorId == request.DestructorID) : query;
            query = request.JournalInstanceForCIHDestructorID != null ? query.Where(x => x.JournalInstanceForCihdestructorId == request.JournalInstanceForCIHDestructorID) : query;
            query = request.RecordID != null ? query.Where(x => x.RecordId == request.RecordID) : query;

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

        public override async Task<StatusResponse> BulkUpdate(JournalInstanceForCIHDestructorBulkUpdateRequest request, ServerCallContext context)
        {
            var record = await _ctx.JournalInstanceForCihrecords.Include(x => x.JournalInstanceForCihdestructors).FirstOrDefaultAsync(x => x.JournalInstanceForCihrecordId == request.RecordID);

            if (record == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Specified Record ID {request.RecordID} not found"));
            }

            using var transaction = await _ctx.Database.BeginTransactionAsync();

            try
            {
                // Adding new destructors
                foreach (var destructorId in request.DestructorsIds)
                {
                    var destructor = record.JournalInstanceForCihdestructors.FirstOrDefault(x => x.DestructorId == destructorId);
                    if (destructor != null)
                    {
                        if(destructor.SysIsDeleted)
                        {
                            destructor.SysIsDeleted = false;
                        }

                    }
                    else
                    {
                        record.JournalInstanceForCihdestructors.Add(new JournalInstanceForCihdestructor { DestructorId = destructorId });

                    }
                }

                foreach (var destructor in record.JournalInstanceForCihdestructors)
                {
                    if (request.DestructorsIds.FirstOrDefault(x => x == destructor.DestructorId) == 0)
                    {
                        record.JournalInstanceForCihdestructors.Remove(destructor);
                    }
                }

                await _ctx.SaveChangesAsync();

                await transaction.CommitAsync();

                return new StatusResponse { Result = true };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new StatusResponse { Result = false };
            }
        }
    }
}
