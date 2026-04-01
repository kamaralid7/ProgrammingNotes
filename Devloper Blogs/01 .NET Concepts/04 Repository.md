us this to further in your knowledge base
01 EF Core Include Explanation

explain me ths
  var dockPlans = await _docksPlanReadOnlySqlRepository.GetListAsync(predicate,
      x => x.Include(a => a.TrailerAssignment)
                  .ThenInclude(a => a.TrailerAppointments)
              .Include(a => a.TrailerAssignment)
                  .ThenInclude(a => a.TrailerSessionAssignments)
                      .ThenInclude(b => b.TrailerSession), ct);

explain me this 
            var (startDateTime, endDateTime) = appointmentLimitSetting == null
                ? (filter.PlanDateUtc, filter.PlanDateUtc.AddDays(1).AddSeconds(-1))
                : (filter.PlanDateUtc.Add(appointmentLimitSetting.StartTime), filter.PlanDateUtc.Add(appointmentLimitSetting.EndTime));


02 Trailer Assignment Sorting Logic

explain me what this class does
    public class TrailerAssigmentSortingConfiguration : DefaultSqlSortingConfiguration<TrailerAssignmentModel>
    {
        private readonly string TrailerAppointmentDatetimeUtcSortKey = $"{nameof(TrailerAssignmentModel.TrailerAppointments)}.{nameof(TrailerAppointmentModel.AppointmentDateTimeUtc)}";
        private readonly string TrailerAppointmentCheckInDatetimeUtcSortKey = $"{nameof(TrailerAssignmentModel.TrailerAppointments)}.{nameof(TrailerAppointmentModel.CheckInDatetimeUtc)}";
        private readonly string TrailerSessionStartDatetimeSortKey = $"{nameof(TrailerAssignmentModel.TrailerSessionAssignments)}.{nameof(TrailerSessionAssignmentModel.TrailerSession.StartDatetimeUtc)}";
        private readonly string AssignedPropertySortKey = nameof(TrailerAssignmentModel.AssignedPriority);
        private readonly string DockNameSortKey = nameof(TrailerAssignmentModel.DockName);

        public override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)
        {
            if (TrailerAppointmentDatetimeUtcSortKey.Equals(keySelector, StringComparison.OrdinalIgnoreCase))
            {
                Expression<Func<TrailerAssignmentModel, DateTime?>> lambda = x => x.TrailerAppointments
                    .Select(y => y.AppointmentDateTimeUtc)
                    .FirstOrDefault();

                return (lambda, typeof(DateTime?));
            }

            if (TrailerAppointmentCheckInDatetimeUtcSortKey.Equals(keySelector, StringComparison.OrdinalIgnoreCase))
            {
                Expression<Func<TrailerAssignmentModel, DateTime?>> lambda = x => x.TrailerAppointments
                    .Select(y => y.CheckInDatetimeUtc)
                    .FirstOrDefault();

                return (lambda, typeof(DateTime?));
            }

            if (TrailerSessionStartDatetimeSortKey.Equals(keySelector, StringComparison.OrdinalIgnoreCase))
            {
                Expression<Func<TrailerAssignmentModel, DateTime?>> lambda = x => x.TrailerSessionAssignments.Where(x => x.IsActive)
                    .Select(y => y.TrailerSession.StartDatetimeUtc)
                    .FirstOrDefault();

                return (lambda, typeof(DateTime?));
            }

            if (AssignedPropertySortKey.Equals(keySelector, StringComparison.OrdinalIgnoreCase))
            {
                Expression<Func<TrailerAssignmentModel, int>> lambda = x => x.AssignedPriority.GetValueOrDefault(int.MaxValue);

                return (lambda, typeof(int));
            }

            return base.MapSortingKeySelector(keySelector);
        }

        public override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector)
        {
            if (DockNameSortKey.Equals(keySelector, StringComparison.OrdinalIgnoreCase))
            {
                Expression<Func<TrailerAssignmentSortModel, string>> lambda = x => x.DockName;

                return (lambda, typeof(string));
            }

            return base.MapSortingKeySelector<TSortEntity>(keySelector);
        }
    }

03 Remove unnecessary variable assignment

Remove this useless assignment to local variable 'assignmentStatuses'. 
using Docks.Common.Configuration.Core.DomainServices.TrailerAppointments;
using Docks.Common.Exceptions.TrailerAppointments;
using Docks.Common.Exceptions.TrailerAssignments;
using Docks.Common.Exceptions.Trailers;
using Docks.Domain.AccountDevices;
using Docks.Domain.Constants;
using Docks.Domain.Enums;
using Docks.Domain.TrailerAppointments;
using Docks.Domain.TrailerAssignments;
using Docks.Domain.Trailers;
using Docks.DomainService.Core.Services.Docks;
using Docks.DomainService.Core.Services.TrailerAssignments.Validation;
using Docks.DomainService.Core.UseCaseServices.Appointments;
using Docks.Infrastructure.Core.Services.Statuses;
using Docks.Infrastructure.Core.Services.TrailerAppointments;
using Docks.Infrastructure.Core.Services.TrailerAssignments;
using Docks.Infrastructure.Core.Services.Trailers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docks.DomainService.UseCaseServices.Appointments
{
    public class LoadTrailerUseCaseValidationService : ILoadTrailerUseCaseValidationService
    {
        private static readonly AppointmentStatus[] ValidAppointmentStatuses =
        [
            AppointmentStatus.Scheduled,
            AppointmentStatus.Rescheduled,
            AppointmentStatus.PendingCheckIn,
            AppointmentStatus.Active
        ];

        private static readonly string[] ValidTrailerAssignmentStatuses =
        [
            TrailerStatuses.InYard
        ];

        private static readonly Dictionary<string, HashSet<string>> ValidTrailerStatusesForAssignment = new Dictionary<string, HashSet<string>>
        {
            { TrailerEntityStatuses.InYard, [TrailerStatuses.InYard, TrailerStatuses.AwaitingDock, TrailerStatuses.YardPending] },
            { TrailerEntityStatuses.AtDock, [TrailerStatuses.Scheduled, TrailerStatuses.AwaitingDock, TrailerStatuses.YardPending] }
        };


        private readonly IAppointmentQueryInfrastructureService _appointmentQueryInfrastructureService;
        private readonly ITrailerAssignmentQueryInfrastructureService _trailerQueryInfrastructureService;
        private readonly IStatusQueryInfrastructureService _statusQueryInfrastructureService;
        private readonly IDockAssignmentValidationService _dockAssignmentValidationService;
        private readonly ITrailerAssignmentIdentifierValidationService _trailerAssignmentIdentifierValidationService;
        private readonly ILoadTrailerConfiguration _loadTrailerConfiguration;
        private readonly ITrailerQueryInfrastructureService _queryInfrastructureService;

        public LoadTrailerUseCaseValidationService(
            IAppointmentQueryInfrastructureService appointmentQueryInfrastructureService,
            ITrailerAssignmentQueryInfrastructureService trailerQueryInfrastructureService,
            IStatusQueryInfrastructureService statusQueryInfrastructureService,
            IDockAssignmentValidationService dockAssignmentValidationService,
            ITrailerAssignmentIdentifierValidationService trailerAssignmentIdentifierValidationService,
            ILoadTrailerConfiguration loadTrailerConfiguration,
            ITrailerQueryInfrastructureService queryInfrastructureService)
        {
            _appointmentQueryInfrastructureService = appointmentQueryInfrastructureService;
            _trailerQueryInfrastructureService = trailerQueryInfrastructureService;
            _statusQueryInfrastructureService = statusQueryInfrastructureService;
            _dockAssignmentValidationService = dockAssignmentValidationService;
            _trailerAssignmentIdentifierValidationService = trailerAssignmentIdentifierValidationService;
            _loadTrailerConfiguration = loadTrailerConfiguration;
            _queryInfrastructureService = queryInfrastructureService;
        }

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct);
            else
            {
                 ValidateTrailerNumber(request.TrailerNumber);
            }

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAssignment  existingTrailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);

            return (appointment, trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

        private async Task<TrailerAppointment> ValidateAppointmentAsync(Guid accountId, Guid trailerAppointmentId, CancellationToken ct)
        {
            var appDataToInclude = new string[] {
                nameof(TrailerAppointment.Carrier),
                nameof(TrailerAppointment.TrailerAssignment),
                nameof(TrailerAppointment.OutboundTrailerAssignment)
            };

            var appointment = await _appointmentQueryInfrastructureService.GetAppointmentAsync(trailerAppointmentId, ct, appDataToInclude)
                ?? throw new AppointmentNotFoundException($"Appointment with id: '{trailerAppointmentId}' is not found on account: '{accountId}'.");

            if (!(appointment.DropNHook
                || (appointment.ShipmentType == ShipmentType.Outbound && appointment.LoadType == LoadType.PickUp)))
                throw new LoadTrailerInvalidAppointmentException($"Trailer cannot be loaded for Appointment with id: '{trailerAppointmentId}'");

            if (!ValidAppointmentStatuses.Contains(appointment.AppointmentStatus))
                throw new LoadTrailerInvalidAppointmentStatusException($"Trailer cannot be loaded for Appointment in status: '{appointment.AppointmentStatus}'");

            return appointment;
        }

        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(request.TrailerNumber) && request.TrailerAssignmentId.HasValue)
                throw new LoadTrailerInvalidTrailerAssignmentDataException("Only Trailer Number or Trailer AssignmentId can be provided at the same time.");
            
            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                    throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        }

        private async Task<(TrailerAssignment, TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            HashSet<string> assignmentStatuses = [];

            if (!(request.TrailerId > 0 && request.TrailerAssignmentId.HasValue))
                throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Both Trailer Id and Trailer AssignmentId should be provided at the same time.");   

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var trailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

            if(trailer == null)
                throw new InvalidTrailerIdException("Trailer ID is required.");

            ValidTrailerStatusesForAssignment.TryGetValue(trailer.TrailerEntityStatus.StatusName, out assignmentStatuses);
            var exsitingTrailerAssignment = await _trailerQueryInfrastructureService.GetAssignmentByTrailerIdAsync(request.AccountId, trailer.TrailerId, ct);
            
            if (exsitingTrailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment  is not found on account: '{request.AccountId}'.");

            if (exsitingTrailerAssignment.TrailerStatus == null || !assignmentStatuses.Contains(exsitingTrailerAssignment.TrailerStatus.StatusName))
                    throw new InvalidTrailerStatusForTrailerAssignmentUpdateException($"Trailer with Assignment in status {exsitingTrailerAssignment.TrailerStatus?.StatusName} cannot be assigned to dock.");           

            return (trailerAssignment, exsitingTrailerAssignment, appointment);
        }

        private async Task<AccountDevice> ValidateDockAsync(Guid accountId, string dockSerialNumber, CancellationToken ct)
        {
            var dock = await _dockAssignmentValidationService.ValidateDockAsync(accountId, dockSerialNumber, ct);
            var account = await _dockAssignmentValidationService.ValidateSessionAsync(accountId, dockSerialNumber, ct);

            dock.Merge(account);

            return dock;
        }

        private async Task ValidatePrefixAsync(TrailerAppointment trailerAppointment, string prefix, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new LoadTrailerPrefixRequiredException("Prefix is required.");

            if (prefix.Length > _loadTrailerConfiguration.PrefixMaxLength)
                throw new LoadTrailerPrefixLengthException($"Prefix cannot exceed {_loadTrailerConfiguration.PrefixMaxLength} characters.");

            var newUniqueIds = trailerAppointment.Identifiers.Select(i => prefix + i.UniqueId).ToList();
            await _trailerAssignmentIdentifierValidationService.ValidateUniqueIdDuplicatesAsync(trailerAppointment.AccountId, newUniqueIds, ct);

            foreach (var identifier in newUniqueIds)
            {
                _trailerAssignmentIdentifierValidationService.ValidateIdentifier(identifier);
            }
        }

        private static void ValidateTrailerNumber(string trailerNumber)
        {
            if (string.IsNullOrEmpty(trailerNumber))
                throw new InvalidTrailerNumberException("Trailer Number is required.");

            if (trailerNumber.Length > 15)
                throw new TrailerNumberMaxLengthExceededException("Trailer Number cannot exceed 15 characters.");
        }
    }
}

how to sole this SQ issue
Change this condition so that it does not always evaluate to 'True'. 
if (exsistingTrailerAssignmentToCheckOut is not null)
        public async Task<TrailerAppointment> LoadTrailerByTrailerIdAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct)
        {
            var (appointment, trailerAssignmentToCheckOut, exsistingTrailerAssignmentToCheckOut, appointmentToComplete, dock) = await _loadTrailerUseCaseValidationService.ValidateRequestByTrailerIdAsync(loadTrailerRequest, ct);

            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);
            loadTrailerRequest.TrailerNumber = exsistingTrailerAssignmentToCheckOut.TrailerNumber;

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct, true);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;


            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
            }

            if (exsistingTrailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(exsistingTrailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
            }

            return appointment;
        }


04 Test Cases for Trailer Load

write test cases for below method
 public async Task<TrailerAppointment> LoadTrailerByTrailerIdAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct)
 {
     var (appointment, trailerAssignmentToCheckOut, exsistingTrailerAssignmentToCheckOut, appointmentToComplete, dock) = await _loadTrailerUseCaseValidationService.ValidateRequestByTrailerIdAsync(loadTrailerRequest, ct);

     var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

     var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct, true);
     appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
     appointment.OutboundTrailerAssignment = newTrailerAssignment;


     await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

     _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
     _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

     if (trailerAssignmentToCheckOut is not null)
     {
         _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
         loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
     }

     if (exsistingTrailerAssignmentToCheckOut is not null)
     {
         _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(exsistingTrailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
     }

     return appointment;
 }

refer the test cases
 [TestMethod]
 public async Task LoadTrailerAsync_ValidatesRequest()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ILoadTrailerUseCaseValidationService>(_ =>
         _.ValidateRequestAsync(_request, _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_GetsTouchpoint()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITouchpointQueryInfrastructureService>(_ =>
         _.GetTouchpointAsync(Touchpoints.Website, _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_AssignmentIsNotNull_ChecksOutTrailer()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerCheckoutPublisherInfrastructureService>(_ =>
         _.PublishTrailerCheckoutMessage(It.Is<TrailerAssignment>(x => x.TrailerAssignmentId == _assignmentIdToCheckOut),
             It.Is<TrailerAppointment>(x => x.AppointmentId == _appointmentIdToComplete),
             It.Is<Touchpoint>(x => x.TouchPointId == TouchpointMock.Get(Touchpoints.Website).TouchPointId)), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_AssignmentIsNotNull_UsesTrailerNumberFromCheckedOutTrailerForNewTrailer()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentManagementInfrastructureService>(_ =>
         _.CreateAssignmentAsync(_accountId, _userId, It.Is<TrailerAssignment>(x => x.TrailerNumber == _trailerNumber), _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_AssignmentIsNotNullAndAppointmentHasOutboundTrailer_UsesTrailerNumberFromCheckedOutTrailerAndTrailerNumberFromAppointmentForNewTrailer()
 {
     var outboundTrailerNumber = "out4";

     _appointment.OutboundTrailerAssignment = new TrailerAssignment
     {
         TrailerNumber = outboundTrailerNumber
     };

     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentManagementInfrastructureService>(_ =>
         _.CreateAssignmentAsync(_accountId, _userId, It.Is<TrailerAssignment>(x => x.TrailerNumber == $"{outboundTrailerNumber} / {_trailerNumber}"), _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_AssignmentIsNull_UsesTrailerNumberFromRequestForNewTrailer()
 {
     SetupValidationResult(_appointment, null, null, _dock);
     var trailerNumber = "tn3";
     _request.TrailerNumber = trailerNumber;

     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentManagementInfrastructureService>(_ =>
         _.CreateAssignmentAsync(_accountId, _userId, It.Is<TrailerAssignment>(x => x.TrailerNumber == trailerNumber), _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_AssignmentIsNullAndAppointmentHasOutboundTrailer_UsesTrailerNumberFromRequestAndTrailerNumberFromAppointmentForNewTrailer()
 {
     var trailerNumber = "tn3";
     var outboundTrailerNumber = "out4";

     SetupValidationResult(_appointment, null, null, _dock);
     _request.TrailerNumber = trailerNumber;
     _appointment.OutboundTrailerAssignment = new TrailerAssignment
     {
         TrailerNumber = outboundTrailerNumber
     };

     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentManagementInfrastructureService>(_ =>
          _.CreateAssignmentAsync(_accountId, _userId, It.Is<TrailerAssignment>(x => x.TrailerNumber == $"{outboundTrailerNumber} / {trailerNumber}"), _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_CreatesNewTrailer()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentManagementInfrastructureService>(_ =>
         _.CreateAssignmentAsync(_accountId, _userId, It.Is<TrailerAssignment>(x =>
             x.TrailerNumber == _trailerNumber
             && x.AccountId == _accountId
             && x.CreatedByUserId == _userId
             && x.DockSerialNumber == _dockSerialNumber
             && x.DockName == _dockName
             && x.CarrierId == _carrierId
             && x.Identifiers.Count == 1
             && x.Identifiers[0].UniqueId == _prefix + _uniqueId
             && x.ShipmentType == _shipmentType
             && x.LoadType == _loadType
             && x.ProductType == _productType
             && x.Comments == _comments
             && x.CarrierDetentionFee == _truckDetentionFee
             && x.TouchPointId == _touchpoint.TouchPointId), _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_ChangesLocationToDock()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentLocationManagementInfrastructureService>(_ =>
         _.MoveToDockAsync(_createdTrailerAssignmentId, _dockSerialNumber, _dockName, _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_UpdatesOutboundTrailer()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<IAppointmentManagementInfrastructureService>(_ =>
         _.UpdateOutboundTrailer(_appointmentId, _createdTrailerAssignmentId, _ct), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_PublishesTrailerAssignmentMessage()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<ITrailerAssignmentPublisherInfrastructureService>(_ =>
         _.PublishTrailerAssignmentMessage(It.Is<TrailerAssignment>(x => x.TrailerAssignmentId == _createdTrailerAssignmentId),
             It.Is<AccountDevice>(x => x.SerialNumber == _dockSerialNumber), null, null, false, null), Times.Once);
 }

 [TestMethod]
 public async Task LoadTrailerAsync_PublishesTrailerAppointmentUpdatedMessage()
 {
     await _service.LoadTrailerAsync(_request, _ct);

     _mocker.Verify<IAppointmentPublisherInfrastructureService>(_ =>
         _.PublishAppointmentUpdated(_appointment, false, false, null), Times.Once);
 }




 these methods are throwing werror
        [TestMethod]
        public async Task LoadTrailerByTrailerIdAsync_PublishesTrailerAppointmentUpdatedMessage()
        {
            await _service.LoadTrailerByTrailerIdAsync(_request, _ct);

            _mocker.Verify<IAppointmentPublisherInfrastructureService>(_ =>
                _.PublishAppointmentUpdated(_appointment), Times.Once);
        }

        [TestMethod]
        public async Task LoadTrailerByTrailerIdAsync_PublishesTrailerAssignmentMessage()
        {
            await _service.LoadTrailerByTrailerIdAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentPublisherInfrastructureService>(_ =>
                _.PublishTrailerAssignmentMessage(It.Is<TrailerAssignment>(x => x.TrailerAssignmentId == _createdTrailerAssignmentId),
                    It.Is<AccountDevice>(x => x.SerialNumber == _dockSerialNumber)), Times.Once);
        }



05 Unit Test Case Creation

write unit test cases refering below method
        [TestMethod]
        public async Task ValidateRequestAsync_AppointmentIsNull_ThrowsAppointmentNotFoundException()
        {
            SetupAppoinment(null);

            await Assert.ThrowsExceptionAsync<AppointmentNotFoundException>(async () =>
                await _service.ValidateRequestAsync(_request, _ct));
        }
		
		        [TestMethod]
        [DynamicData(nameof(GetInvalidAppointmentCases), DynamicDataSourceType.Method)]
        public async Task ValidateRequestAsync_InvalidAppointment_ThrowsLoadTrailerInvalidAppointmentException(ShipmentType shipmentType, LoadType loadType, bool dropNHook)
        {
            _appointment.ShipmentType = shipmentType;
            _appointment.LoadType = loadType;
            _appointment.DropNHook = dropNHook;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentException>(async () =>
                await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(AppointmentStatus.Unscheduled)]
        [DataRow(AppointmentStatus.Cancelled)]
        [DataRow(AppointmentStatus.InProgress)]
        [DataRow(AppointmentStatus.Completed)]
        [DataRow(AppointmentStatus.LateCheckIn)]
        [DataRow(AppointmentStatus.EarlyCheckIn)]
        [DataRow(AppointmentStatus.Deleted)]
        public async Task ValidateRequestAsync_InvalidAppointmentStatus_ThrowsLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentStatusException>(async () =>
                 await _service.ValidateRequestAsync(_request, _ct));
        }
		
		        [TestMethod]
        public async Task ValidateRequestAsync_EmptyPrefix_ThrowsLoadTrailerPrefixRequiredException()
        {
            _request.Prefix = "";

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixRequiredException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }
		
		        [TestMethod]
        public async Task ValidateRequestAsync_InvalidPrefixLength_ThrowsLoadTrailerPrefixLengthException()
        {
            _request.Prefix = RandomHelper.GetCharacters(_prefixMaxLength + 1);

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixLengthException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }
		
		        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIdAndTrailerNumberArePassed_ThrowsLoadTrailerInvalidTrailerAssignmentDataException()
        {
            _request.TrailerAssignmentId = Guid.NewGuid();
            _request.TrailerNumber = "tn";

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentDataException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }
		
		TrailerAssignmentNotFoundException
		InvalidTrailerIdException
		TrailerAssignmentNotFoundException
		InvalidTrailerStatusForTrailerAssignmentUpdateException 
		
		       

for below method 

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAssignment  existingTrailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);

            return (appointment, trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

        private async Task<(TrailerAssignment, TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            HashSet<string> assignmentStatuses = [];

            if (!(request.TrailerId > 0 && request.TrailerAssignmentId.HasValue))
                throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Both Trailer Id and Trailer AssignmentId should be provided at the same time.");   

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var trailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

            if(trailer == null)
                throw new InvalidTrailerIdException("Trailer ID is required.");

            var trailerStatusIsValid = trailer.TrailerEntityStatus is not null
                        && ValidTrailerStatusesForAssignment.TryGetValue(trailer.TrailerEntityStatus.StatusName, out assignmentStatuses);

            var exsitingTrailerAssignment = await _trailerQueryInfrastructureService.GetAssignmentByTrailerIdAsync(request.AccountId, trailer.TrailerId, ct);
            
            if (exsitingTrailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{exsitingTrailerAssignment.TrailerAssignmentId}' is not found on account: '{request.AccountId}'.");

            if (!assignmentStatuses.Contains(exsitingTrailerAssignment.TrailerStatus.StatusName))
                    throw new InvalidTrailerStatusForTrailerAssignmentUpdateException($"Trailer with Assignment in status {exsitingTrailerAssignment.TrailerStatus?.StatusName} cannot be assigned to dock.");           

            return (trailerAssignment, exsitingTrailerAssignment, appointment);
        }


        private void SetupTrailerAssignmentByTrailerId(int trailerId, TrailerAssignment trailerAssignment)
        {
            assignment.TrailerId = trailerId;
            _mocker.Setup<ITrailerAssignmentQueryInfrastructureService, Task<TrailerAssignment>>(_ =>
              _.GetAssignmentByTrailerIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
              .ReturnsAsync(trailerAssignment);
        }


 public async Task<(TrailerAppointment, TrailerAssignment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
 {
     TrailerAssignment trailerAssignmentToCheckOut = null;
     TrailerAssignment  existingTrailerAssignmentToCheckOut = null;
     TrailerAppointment appointmentToComplete = null;
     var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
     var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

     await ValidatePrefixAsync(appointment, request.Prefix, ct);

     if (request.TrailerAssignmentId.HasValue)
         (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);

     return (appointment, trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete, dock);
 }
 private async Task<(TrailerAssignment, TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
 {
     HashSet<string> assignmentStatuses = [];

     if (!(request.TrailerId > 0 && request.TrailerAssignmentId.HasValue))
         throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Both Trailer Id and Trailer AssignmentId should be provided at the same time.");   

     var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

     if (trailerAssignment == null)
         throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

     var trailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

     if(trailer == null)
         throw new InvalidTrailerIdException("Trailer ID is required.");

     var trailerStatusIsValid = trailer.TrailerEntityStatus is not null
                 && ValidTrailerStatusesForAssignment.TryGetValue(trailer.TrailerEntityStatus.StatusName, out assignmentStatuses);

     var exsitingTrailerAssignment = await _trailerQueryInfrastructureService.GetAssignmentByTrailerIdAsync(request.AccountId, trailer.TrailerId, ct);
     
     if (exsitingTrailerAssignment == null)
         throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{exsitingTrailerAssignment.TrailerAssignmentId}' is not found on account: '{request.AccountId}'.");

     if (!assignmentStatuses.Contains(exsitingTrailerAssignment.TrailerStatus.StatusName))
             throw new InvalidTrailerStatusForTrailerAssignmentUpdateException($"Trailer with Assignment in status {exsitingTrailerAssignment.TrailerStatus?.StatusName} cannot be assigned to dock.");           

     return (trailerAssignment, exsitingTrailerAssignment, appointment);
 }
here when exsitingTrailerAssignment == null then InvalidTrailerStatusForTrailerAssignmentUpdateException is thrown but it propogates as null reference exception



because of null reference exception this test case is not working
       [TestMethod]
       public async Task ValidateRequestByTrailerIdAsync_ExistingTrailerAssignmentNotFound_ThrowsTrailerAssignmentNotFoundException()
       {
            _request.TrailerId = 10;
            SetupTrailer(_trailer);
            SetupTrailerAssignmentByTrailerId(null);

            await Assert.ThrowsExceptionAsync<TrailerAssignmentNotFoundException>(async () =>
               await _service.ValidateRequestByTrailerIdAsync(_request, _ct));
       }


the null reference exception is thrown here
            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);



can we optimize it further
try
        {
            (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = 
                await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);
        }
        catch (Exception ex) when (ex is TrailerAssignmentNotFoundException ||
                                   ex is InvalidTrailerIdException ||
                                   ex is InvalidTrailerStatusForTrailerAssignmentUpdateException)
        {
            // Log the exception if needed
            throw; // Rethrow without losing stack trace
        }


what does this mean
This exception was originally thrown at this call stack:
    Docks.DomainService.UseCaseServices.Appointments.LoadTrailerUseCaseValidationService.ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(Docks.Domain.TrailerAppointments.LoadTrailerRequest, System.Threading.CancellationToken) in LoadTrailerUseCaseValidationService.cs
    Docks.DomainService.UseCaseServices.Appointments.LoadTrailerUseCaseValidationService.ValidateRequestByTrailerIdAsync(Docks.Domain.TrailerAppointments.LoadTrailerRequest, System.Threading.CancellationToken) in LoadTrailerUseCaseValidationService.cs


how to remove unused variable trailerstatus
        private async Task<(TrailerAssignment, TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            HashSet<string> assignmentStatuses = [];

            if (!(request.TrailerId > 0 && request.TrailerAssignmentId.HasValue))
                throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Both Trailer Id and Trailer AssignmentId should be provided at the same time.");   

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var trailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

            if(trailer == null)
                throw new InvalidTrailerIdException("Trailer ID is required.");

            var trailerStatusIsValid = trailer.TrailerEntityStatus is not null
                        && ValidTrailerStatusesForAssignment.TryGetValue(trailer.TrailerEntityStatus.StatusName, out assignmentStatuses);

            var exsitingTrailerAssignment = await _trailerQueryInfrastructureService.GetAssignmentByTrailerIdAsync(request.AccountId, trailer.TrailerId, ct);
            
            if (exsitingTrailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment  is not found on account: '{request.AccountId}'.");

            if (exsitingTrailerAssignment.TrailerStatus == null || !assignmentStatuses.Contains(exsitingTrailerAssignment.TrailerStatus.StatusName))
                    throw new InvalidTrailerStatusForTrailerAssignmentUpdateException($"Trailer with Assignment in status {exsitingTrailerAssignment.TrailerStatus?.StatusName} cannot be assigned to dock.");           

            return (trailerAssignment, exsitingTrailerAssignment, appointment);
        }



rewrite thistest case

       [TestMethod]
       public async Task ValidateRequestByTrailerIdAsync_InvalidTrailerStatus_ThrowsInvalidTrailerStatusForTrailerAssignmentUpdateException()
       {
           var trailerAssignment = new TrailerAssignment { TrailerStatus = new TrailerStatus { StatusName = "InvalidStatus" } };
            SetupTrailerAssignmentByTrailerId(trailerAssignment);

            await Assert.ThrowsExceptionAsync<InvalidTrailerStatusForTrailerAssignmentUpdateException>(async () =>
               await _service.ValidateRequestByTrailerIdAsync(_request, _ct));
       }

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAssignment  existingTrailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);

            return (appointment, trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

using Docks.Common.Configuration.Core.DomainServices.TrailerAppointments;
using Docks.Common.Exceptions.TrailerAppointments;
using Docks.Common.Exceptions.TrailerAssignments;
using Docks.Common.Exceptions.Trailers;
using Docks.Domain.AccountDevices;
using Docks.Domain.Constants;
using Docks.Domain.Enums;
using Docks.Domain.TrailerAppointments;
using Docks.Domain.TrailerAssignments;
using Docks.Domain.Trailers;
using Docks.DomainService.Core.Services.Docks;
using Docks.DomainService.Core.Services.TrailerAssignments.Validation;
using Docks.DomainService.Core.UseCaseServices.Appointments;
using Docks.Infrastructure.Core.Services.Statuses;
using Docks.Infrastructure.Core.Services.TrailerAppointments;
using Docks.Infrastructure.Core.Services.TrailerAssignments;
using Docks.Infrastructure.Core.Services.Trailers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docks.DomainService.UseCaseServices.Appointments
{
    public class LoadTrailerUseCaseValidationService : ILoadTrailerUseCaseValidationService
    {
        private static readonly AppointmentStatus[] ValidAppointmentStatuses =
        [
            AppointmentStatus.Scheduled,
            AppointmentStatus.Rescheduled,
            AppointmentStatus.PendingCheckIn,
            AppointmentStatus.Active
        ];

        private static readonly string[] ValidTrailerAssignmentStatuses =
        [
            TrailerStatuses.InYard
        ];

        private static readonly Dictionary<string, HashSet<string>> ValidTrailerStatusesForAssignment = new Dictionary<string, HashSet<string>>
        {
            { TrailerEntityStatuses.InYard, [TrailerStatuses.InYard, TrailerStatuses.AwaitingDock, TrailerStatuses.YardPending] },
            { TrailerEntityStatuses.AtDock, [TrailerStatuses.Scheduled, TrailerStatuses.AwaitingDock, TrailerStatuses.YardPending] }
        };


        private readonly IAppointmentQueryInfrastructureService _appointmentQueryInfrastructureService;
        private readonly ITrailerAssignmentQueryInfrastructureService _trailerQueryInfrastructureService;
        private readonly IStatusQueryInfrastructureService _statusQueryInfrastructureService;
        private readonly IDockAssignmentValidationService _dockAssignmentValidationService;
        private readonly ITrailerAssignmentIdentifierValidationService _trailerAssignmentIdentifierValidationService;
        private readonly ILoadTrailerConfiguration _loadTrailerConfiguration;
        private readonly ITrailerQueryInfrastructureService _queryInfrastructureService;

        public LoadTrailerUseCaseValidationService(
            IAppointmentQueryInfrastructureService appointmentQueryInfrastructureService,
            ITrailerAssignmentQueryInfrastructureService trailerQueryInfrastructureService,
            IStatusQueryInfrastructureService statusQueryInfrastructureService,
            IDockAssignmentValidationService dockAssignmentValidationService,
            ITrailerAssignmentIdentifierValidationService trailerAssignmentIdentifierValidationService,
            ILoadTrailerConfiguration loadTrailerConfiguration,
            ITrailerQueryInfrastructureService queryInfrastructureService)
        {
            _appointmentQueryInfrastructureService = appointmentQueryInfrastructureService;
            _trailerQueryInfrastructureService = trailerQueryInfrastructureService;
            _statusQueryInfrastructureService = statusQueryInfrastructureService;
            _dockAssignmentValidationService = dockAssignmentValidationService;
            _trailerAssignmentIdentifierValidationService = trailerAssignmentIdentifierValidationService;
            _loadTrailerConfiguration = loadTrailerConfiguration;
            _queryInfrastructureService = queryInfrastructureService;
        }

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct);
            else
            {
                 ValidateTrailerNumber(request.TrailerNumber);
            }

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAssignment  existingTrailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);

            return (appointment, trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

        private async Task<TrailerAppointment> ValidateAppointmentAsync(Guid accountId, Guid trailerAppointmentId, CancellationToken ct)
        {
            var appDataToInclude = new string[] {
                nameof(TrailerAppointment.Carrier),
                nameof(TrailerAppointment.TrailerAssignment),
                nameof(TrailerAppointment.OutboundTrailerAssignment)
            };

            var appointment = await _appointmentQueryInfrastructureService.GetAppointmentAsync(trailerAppointmentId, ct, appDataToInclude)
                ?? throw new AppointmentNotFoundException($"Appointment with id: '{trailerAppointmentId}' is not found on account: '{accountId}'.");

            if (!(appointment.DropNHook
                || (appointment.ShipmentType == ShipmentType.Outbound && appointment.LoadType == LoadType.PickUp)))
                throw new LoadTrailerInvalidAppointmentException($"Trailer cannot be loaded for Appointment with id: '{trailerAppointmentId}'");

            if (!ValidAppointmentStatuses.Contains(appointment.AppointmentStatus))
                throw new LoadTrailerInvalidAppointmentStatusException($"Trailer cannot be loaded for Appointment in status: '{appointment.AppointmentStatus}'");

            return appointment;
        }

        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(request.TrailerNumber) && request.TrailerAssignmentId.HasValue)
                throw new LoadTrailerInvalidTrailerAssignmentDataException("Only Trailer Number or Trailer AssignmentId can be provided at the same time.");
            
            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                    throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        }

        private async Task<(TrailerAssignment, TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            HashSet<string> assignmentStatuses = [];

            if (!(request.TrailerId > 0 && request.TrailerAssignmentId.HasValue))
                throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Both Trailer Id and Trailer AssignmentId should be provided at the same time.");   

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var trailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

            if(trailer == null)
                throw new InvalidTrailerIdException("Trailer ID is required.");

            ValidTrailerStatusesForAssignment.TryGetValue(trailer.TrailerEntityStatus.StatusName, out assignmentStatuses);
            var exsitingTrailerAssignment = await _trailerQueryInfrastructureService.GetAssignmentByTrailerIdAsync(request.AccountId, trailer.TrailerId, ct);
            
            if (exsitingTrailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment  is not found on account: '{request.AccountId}'.");

            if (exsitingTrailerAssignment.TrailerStatus == null || !assignmentStatuses.Contains(exsitingTrailerAssignment.TrailerStatus.StatusName))
                    throw new InvalidTrailerStatusForTrailerAssignmentUpdateException($"Trailer with Assignment in status {exsitingTrailerAssignment.TrailerStatus?.StatusName} cannot be assigned to dock.");           

            return (trailerAssignment, exsitingTrailerAssignment, appointment);
        }

        private async Task<AccountDevice> ValidateDockAsync(Guid accountId, string dockSerialNumber, CancellationToken ct)
        {
            var dock = await _dockAssignmentValidationService.ValidateDockAsync(accountId, dockSerialNumber, ct);
            var account = await _dockAssignmentValidationService.ValidateSessionAsync(accountId, dockSerialNumber, ct);

            dock.Merge(account);

            return dock;
        }

        private async Task ValidatePrefixAsync(TrailerAppointment trailerAppointment, string prefix, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(prefix))
                throw new LoadTrailerPrefixRequiredException("Prefix is required.");

            if (prefix.Length > _loadTrailerConfiguration.PrefixMaxLength)
                throw new LoadTrailerPrefixLengthException($"Prefix cannot exceed {_loadTrailerConfiguration.PrefixMaxLength} characters.");

            var newUniqueIds = trailerAppointment.Identifiers.Select(i => prefix + i.UniqueId).ToList();
            await _trailerAssignmentIdentifierValidationService.ValidateUniqueIdDuplicatesAsync(trailerAppointment.AccountId, newUniqueIds, ct);

            foreach (var identifier in newUniqueIds)
            {
                _trailerAssignmentIdentifierValidationService.ValidateIdentifier(identifier);
            }
        }

        private async Task ValidateTrailerId(int trailerId, Guid accountId, CancellationToken ct)
        {
            var existingTrailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(trailerId, ct);

            if (existingTrailer == null || existingTrailer.AccountId != accountId)
                throw new InvalidTrailerIdException("Trailer ID is required.");
        }

        private static void ValidateTrailerNumber(string trailerNumber)
        {
            if (string.IsNullOrEmpty(trailerNumber))
                throw new InvalidTrailerNumberException("Trailer Number is required.");

            if (trailerNumber.Length > 15)
                throw new TrailerNumberMaxLengthExceededException("Trailer Number cannot exceed 15 characters.");
        }
    }
}



based on this rewrite test case
        private static readonly Dictionary<string, HashSet<string>> ValidTrailerStatusesForAssignment = new Dictionary<string, HashSet<string>>
        {
            { TrailerEntityStatuses.InYard, [TrailerStatuses.InYard, TrailerStatuses.AwaitingDock, TrailerStatuses.YardPending] },
            { TrailerEntityStatuses.AtDock, [TrailerStatuses.Scheduled, TrailerStatuses.AwaitingDock, TrailerStatuses.YardPending] }
        };


what does this mean
This exception was originally thrown at this call stack:
    Docks.DomainService.UseCaseServices.Appointments.LoadTrailerUseCaseValidationService.ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(Docks.Domain.TrailerAppointments.LoadTrailerRequest, System.Threading.CancellationToken) in LoadTrailerUseCaseValidationService.cs
    Docks.DomainService.UseCaseServices.Appointments.LoadTrailerUseCaseValidationService.ValidateRequestByTrailerIdAsync(Docks.Domain.TrailerAppointments.LoadTrailerRequest, System.Threading.CancellationToken) in LoadTrailerUseCaseValidationService.cs


06 Test case for function

write a test case for this function
  public async Task<(TrailerAppointment, TrailerAssignment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByTrailerIdAsync(LoadTrailerRequest request, CancellationToken ct)
  {
      TrailerAssignment trailerAssignmentToCheckOut = null;
      TrailerAssignment  existingTrailerAssignmentToCheckOut = null;
      TrailerAppointment appointmentToComplete = null;
      var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
      var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

      await ValidatePrefixAsync(appointment, request.Prefix, ct);

      if (request.TrailerAssignmentId.HasValue)
          (trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutByTrailerIdAsync(request, ct);

      return (appointment, trailerAssignmentToCheckOut, existingTrailerAssignmentToCheckOut, appointmentToComplete, dock);
  }
in this file

using Docks.Common.Configuration.Core.DomainServices.TrailerAppointments;
using Docks.Common.Configuration.Core.DomainServices.TrailerAssignments.Validation;
using Docks.Common.Exceptions.Docks;
using Docks.Common.Exceptions.TrailerAppointments;
using Docks.Common.Exceptions.TrailerAssignments;
using Docks.Common.Exceptions.TrailerAssignments.Identifiers;
using Docks.Domain.AccountDevices;
using Docks.Domain.Accounts;
using Docks.Domain.Carriers;
using Docks.Domain.Constants;
using Docks.Domain.Enums;
using Docks.Domain.TrailerAppointments;
using Docks.Domain.TrailerAssignments;
using Docks.Domain.Trailers;
using Docks.DomainService.Core.Services.Docks;
using Docks.DomainService.Core.Services.TrailerAssignments.Validation;
using Docks.DomainService.Core.UseCaseServices.Appointments;
using Docks.DomainService.Tests.Helpers;
using Docks.DomainService.Tests.Mocks;
using Docks.DomainService.UseCaseServices.Appointments;
using Docks.Infrastructure.Core.Services.TrailerAppointments;
using Docks.Infrastructure.Core.Services.TrailerAssignments;
using Docks.Infrastructure.Core.Services.Trailers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docks.DomainService.Tests.UseCaseServices.TrailerAppointments
{
    [TestClass]
    public class LoadTrailerUseCaseValidationServiceTests
    {
        private Guid _accountId;
        private Guid _appointmentId;
        private Guid _assignmentIdToCheckOut;
        private Guid _appointmentIdToComplete;
        private CancellationToken _ct;
        private string _dockSerialNumber;
        private string _prefix;
        private string _uniqueId;
        private int _prefixMaxLength;
        private int _uniqueIdMaxLength;
        private LoadTrailerRequest _request;
        private TrailerAppointment _appointment;
        private TrailerAssignment _assignmentToCheckOut;
        private TrailerAppointment _appointmentToComplete;
        private AccountDevice _dock;
        private Account _account;
        private Domain.Trailers.Trailer _trailer;
        private AutoMocker _mocker;
        private LoadTrailerUseCaseValidationService _service;

        [TestInitialize]
        public void Initialize()
        {
            _accountId = Guid.NewGuid();
            _appointmentId = Guid.NewGuid();
            _appointmentIdToComplete = Guid.NewGuid();
            _assignmentIdToCheckOut = Guid.NewGuid();
            _dockSerialNumber = "3245";
            _prefix = "prefix";
            _uniqueId = "ui";
            _prefixMaxLength = 7;
            _uniqueIdMaxLength = 10;

            _request = new LoadTrailerRequest
            {
                AccountId = _accountId,
                TrailerAppointmentId = _appointmentId,
                DockSerialNumber = _dockSerialNumber,
                Prefix = _prefix,
                TrailerAssignmentId = _assignmentIdToCheckOut,
            };

            _appointment = new TrailerAppointment
            {
                AccountId = _accountId,
                AppointmentId = _appointmentId,
                ShipmentType = ShipmentType.Outbound,
                LoadType = LoadType.PickUp,
                AppointmentStatus = AppointmentStatus.Scheduled,
                Identifiers = [
                    new TrailerAssignmentIdentifier {
                        UniqueId = _uniqueId
                    }
                ]
            };
            _trailer = new Domain.Trailers.Trailer
            {
                TrailerId = 123,
                AccountId = _accountId,
                CarrierId = 456,
                TrailerNumber = "TR12345",
                CreatedByUserId = Guid.NewGuid(),
                CreatedDatetimeUtc = DateTime.UtcNow,
                LastUpdatedByUserId = Guid.NewGuid(),
                LastUpdatedDatetimeUtc = DateTime.UtcNow,
                Carrier = new Carrier { CarrierId = 456, Name = "Carrier XYZ" },
                TrailerAssignmentId = Guid.NewGuid(),
                TrailerEntityStatusId = 1,
                TrailerEntityStatus = new TrailerEntityStatus { StatusId = 1, StatusName = "Active" },
                TrailerAssignment = new TrailerAssignment { TrailerAssignmentId = Guid.NewGuid() },
                TrailerAppointment = _appointment
            };


            _appointmentToComplete = new TrailerAppointment
            {
                AccountId = _accountId,
                AppointmentId = _appointmentIdToComplete,
            };

            _assignmentToCheckOut = new TrailerAssignment
            {
                TrailerAssignmentId = _assignmentIdToCheckOut,
                TrailerStatusId = TrailerStatusMock.Get(TrailerStatuses.InYard).StatusId
            };

            _dock = new AccountDevice
            {
                SerialNumber = _dockSerialNumber
            };
            _account = new Account { };

            _mocker = new AutoMocker();

            SetupAppoinment(_appointment);
            SetupAssignmentToCheckOut(_assignmentToCheckOut, _appointmentToComplete);

            _mocker.Setup<IDockAssignmentValidationService, Task<AccountDevice>>(_ =>
                _.ValidateDockAsync(_accountId, _dockSerialNumber, _ct))
                .ReturnsAsync(_dock);

            _mocker.Setup<IDockAssignmentValidationService, Task<Account>>(_ =>
                _.ValidateSessionAsync(_accountId, _dockSerialNumber, _ct))
                .ReturnsAsync(_account);

            _mocker.Setup<ILoadTrailerConfiguration, int>(_ => _.PrefixMaxLength)
                .Returns(_prefixMaxLength);

            _mocker.Setup<ITrailerAssignmentIdentifierValidationConfiguration, int>(_ => _.MaxUniqueIdLength)
               .Returns(_uniqueIdMaxLength);

            _mocker.SetupTrailerStatuses();

            _service = _mocker.CreateInstance<LoadTrailerUseCaseValidationService>();
        }

        [TestMethod]
        public async Task ValidateRequestAsync_GetsAppointment()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IAppointmentQueryInfrastructureService>(_ =>
                _.GetAppointmentAsync(_appointmentId, _ct, It.IsAny<string[]>()), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AppointmentIsNull_ThrowsAppointmentNotFoundException()
        {
            SetupAppoinment(null);

            await Assert.ThrowsExceptionAsync<AppointmentNotFoundException>(async () =>
                await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DynamicData(nameof(GetInvalidAppointmentCases), DynamicDataSourceType.Method)]
        public async Task ValidateRequestAsync_InvalidAppointment_ThrowsLoadTrailerInvalidAppointmentException(ShipmentType shipmentType, LoadType loadType, bool dropNHook)
        {
            _appointment.ShipmentType = shipmentType;
            _appointment.LoadType = loadType;
            _appointment.DropNHook = dropNHook;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentException>(async () =>
                await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DynamicData(nameof(GetValidAppointmentCases), DynamicDataSourceType.Method)]
        public async Task ValidateRequestAsync_ValidAppointment_DoesNotThrowLoadTrailerInvalidAppointmentException(ShipmentType shipmentType, LoadType loadType, bool dropNHook)
        {
            _appointment.ShipmentType = shipmentType;
            _appointment.LoadType = loadType;
            _appointment.DropNHook = dropNHook;

            await _service.ValidateRequestAsync(_request, _ct);
        }

        [TestMethod]
        [DataRow(AppointmentStatus.Unscheduled)]
        [DataRow(AppointmentStatus.Cancelled)]
        [DataRow(AppointmentStatus.InProgress)]
        [DataRow(AppointmentStatus.Completed)]
        [DataRow(AppointmentStatus.LateCheckIn)]
        [DataRow(AppointmentStatus.EarlyCheckIn)]
        [DataRow(AppointmentStatus.Deleted)]
        public async Task ValidateRequestAsync_InvalidAppointmentStatus_ThrowsLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentStatusException>(async () =>
                 await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(AppointmentStatus.Scheduled)]
        [DataRow(AppointmentStatus.Rescheduled)]
        [DataRow(AppointmentStatus.PendingCheckIn)]
        [DataRow(AppointmentStatus.Active)]
        public async Task ValidateRequestAsync_ValidAppointmentStatus_DoesNotThrowLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await _service.ValidateRequestAsync(_request, _ct);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesDock()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IDockAssignmentValidationService>(_ =>
               _.ValidateDockAsync(_accountId, _dockSerialNumber, _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesSession()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IDockAssignmentValidationService>(_ =>
               _.ValidateSessionAsync(_accountId, _dockSerialNumber, _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_EmptyPrefix_ThrowsLoadTrailerPrefixRequiredException()
        {
            _request.Prefix = "";

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixRequiredException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_InvalidPrefixLength_ThrowsLoadTrailerPrefixLengthException()
        {
            _request.Prefix = RandomHelper.GetCharacters(_prefixMaxLength + 1);

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixLengthException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesUniqueIdDuplicates()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentIdentifierValidationService>(_ =>
                _.ValidateUniqueIdDuplicatesAsync(_accountId,
                    It.Is<List<string>>(x => x.Count == 1 && x.Contains(_prefix + _uniqueId)), _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesIdentifier()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentIdentifierValidationService>(_ =>
                _.ValidateIdentifier(_prefix + _uniqueId, true), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_EmptyTrailerNumber_ThrowsInvalidTrailerNumberException()
        {
            _request.TrailerAssignmentId = null;
            _request.TrailerNumber = "";

            await Assert.ThrowsExceptionAsync<InvalidTrailerNumberException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_InvalidTrailerNumberLength_ThrowsTrailerNumberMaxLengthExceededException()
        {
            _request.TrailerAssignmentId = null;
            _request.TrailerNumber = RandomHelper.GetCharacters(16);

            await Assert.ThrowsExceptionAsync<TrailerNumberMaxLengthExceededException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIdAndTrailerNumberArePassed_ThrowsLoadTrailerInvalidTrailerAssignmentDataException()
        {
            _request.TrailerAssignmentId = Guid.NewGuid();
            _request.TrailerNumber = "tn";

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentDataException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIsNull_ThrowsTrailerAssignmentNotFoundException()
        {
            SetupAssignmentToCheckOut(null, null);

            await Assert.ThrowsExceptionAsync<TrailerAssignmentNotFoundException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(TrailerStatuses.Unscheduled)]
        [DataRow(TrailerStatuses.Scheduled)]
        [DataRow(TrailerStatuses.DockPending)]
        [DataRow(TrailerStatuses.DockAssigned)]
        [DataRow(TrailerStatuses.YardPending)]
        [DataRow(TrailerStatuses.AwaitingDock)]
        [DataRow(TrailerStatuses.SessionInProgress)]
        [DataRow(TrailerStatuses.CheckedOut)]
        [DataRow(TrailerStatuses.NotAssigned)]
        public async Task ValidateRequestAsync_InvalidAssignmentStatus_ThrowsLoadTrailerInvalidTrailerAssignmentStatusException(string statusName)
        {
            var status = TrailerStatusMock.Get(statusName);
            _assignmentToCheckOut.TrailerStatusId = status.StatusId;
            _assignmentToCheckOut.TrailerStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentStatusException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_Success()
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _service.ValidateRequestAsync(_request, _ct);

            Assert.AreEqual(_appointmentId, appointment.AppointmentId);
            Assert.AreEqual(_assignmentIdToCheckOut, trailerAssignmentToCheckOut.TrailerAssignmentId);
            Assert.AreEqual(_appointmentIdToComplete, appointmentToComplete.AppointmentId);
            Assert.AreEqual(_dockSerialNumber, dock.SerialNumber);
        }

        private void SetupAppoinment(TrailerAppointment appointment)
            => _mocker.Setup<IAppointmentQueryInfrastructureService, Task<TrailerAppointment>>(_ =>
                _.GetAppointmentAsync(_appointmentId, _ct, It.IsAny<string[]>()))
                .ReturnsAsync(appointment);

        private void SetupAssignmentToCheckOut(TrailerAssignment assignment, TrailerAppointment appointment)
            => _mocker.Setup<ITrailerAssignmentQueryInfrastructureService, Task<(TrailerAssignment, TrailerAppointment)>>(_ =>
              _.GetAssignmentWithAppointmentAsync(_accountId, _assignmentIdToCheckOut, _ct))
              .ReturnsAsync((assignment, appointment));

        private void SetupExistingTrailer(Domain.Trailers.Trailer trailer)
            => _mocker.Setup<ITrailerQueryInfrastructureService, Task<Domain.Trailers.Trailer>>(_ =>
                  _.GetTrailerByTrailerIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(trailer);


        private static IEnumerable<object[]> GetInvalidAppointmentCases()
            =>
            [
                [ShipmentType.Inbound, LoadType.None, false],
                [ShipmentType.Inbound, LoadType.LiveLoad, false],
                [ShipmentType.Inbound, LoadType.DropLoad, false],
                [ShipmentType.Inbound, LoadType.PreLoad, false],
                [ShipmentType.Inbound, LoadType.PickUp, false],
                [ShipmentType.Outbound, LoadType.None, false],
                [ShipmentType.Outbound, LoadType.LiveLoad, false],
                [ShipmentType.Outbound, LoadType.DropLoad, false],
                [ShipmentType.Outbound, LoadType.PreLoad, false]
            ];

        private static IEnumerable<object[]> GetValidAppointmentCases()
           =>
           [
                [ShipmentType.Outbound, LoadType.PickUp, false],
                [ShipmentType.Inbound, LoadType.None, true],
                [ShipmentType.Inbound, LoadType.LiveLoad, true],
                [ShipmentType.Inbound, LoadType.DropLoad, true],
                [ShipmentType.Inbound, LoadType.PreLoad, true],
                [ShipmentType.Inbound, LoadType.PickUp, true]
           ];
    }
}


07 JSON Object Creation

create a json object like below
{
  "dock_serial_number": "CG092F00A485",
  "trailer_assignment": {
    "carrier_id": 23,
    "trailer_number": "96001",
    "shipment_type": 1,
    "load_type": 1,
    "product_type": 1,
    "comments": "string",
    "set_temperature": 3,
    "actual_temperature": 4,
    "seal_number": "96001"
  },
  "driver_id": "C46923F9-18C7-4667-B90B-FFA6FB11E3A5"
}

using this data
if any missing data highlight it
{appointment_id: "d89ad5ac-7f79-4a6b-90bf-08dd6672c123",…}
account_id
: 
"fdad02a5-da6e-4e4d-8d26-97a0884aa1a7"
appointment_date_time_utc
: 
"2025-03-20T11:30:00"
appointment_id
: 
"d89ad5ac-7f79-4a6b-90bf-08dd6672c123"
appointment_status
: 
1
carrier
: 
{scac_code: "", truck_detention_fee: 68.34, demurrage_fee: 664, comments: "",…}
active
: 
true
carrier_id
: 
304496
comments
: 
""
created_date_time
: 
"2024-05-03T12:33:44.61"
created_date_time_utc
: 
"2024-05-03T12:33:44.61"
demurrage_fee
: 
664
demurrage_time_limit_enabled
: 
false
demurrage_time_limit_warning_enabled
: 
false
driver_check_in_comments
: 
""
driver_check_out_comments
: 
""
is_default
: 
false
name
: 
"Longins Transport Service"
scac_code
: 
""
time_limit_enabled
: 
false
time_limit_interval
: 
"01:00:00"
time_limit_warning_enabled
: 
false
time_limit_warning_interval
: 
"00:15:00"
truck_detention_fee
: 
68.34
carrier_id
: 
304496
comments
: 
""
created_datetime_utc
: 
"2025-03-20T10:31:31.95"
driver_status_id
: 
1
drop_n_hook
: 
false
duration
: 
"00:30:00"
identifiers
: 
[,…]
0
: 
{unique_id: "SH98984", unique_id_type: 1, is_primary: true, used_for_lookup: false, stop_sequence: 1,…}
is_primary
: 
true
line_items
: 
[]
stop_sequence
: 
1
unique_id
: 
"SH98984"
unique_id_type
: 
1
used_for_lookup
: 
false
load_type
: 
4
outbound_trailer_assignment
: 
{trailer_assignment_id: "3dcc65fc-d97d-4213-47f9-08dd6672c124",…}
account_id
: 
"fdad02a5-da6e-4e4d-8d26-97a0884aa1a7"
carrier
: 
{scac_code: "", truck_detention_fee: 68.34, demurrage_fee: 664, comments: "",…}
active
: 
true
carrier_id
: 
304496
comments
: 
""
created_date_time
: 
"2024-05-03T12:33:44.61"
created_date_time_utc
: 
"2024-05-03T12:33:44.61"
demurrage_fee
: 
664
demurrage_time_limit_enabled
: 
false
demurrage_time_limit_warning_enabled
: 
false
driver_check_in_comments
: 
""
driver_check_out_comments
: 
""
is_default
: 
false
name
: 
"Longins Transport Service"
scac_code
: 
""
time_limit_enabled
: 
false
time_limit_interval
: 
"01:00:00"
time_limit_warning_enabled
: 
false
time_limit_warning_interval
: 
"00:15:00"
truck_detention_fee
: 
68.34
carrier_detention_fee
: 
68.34
carrier_id
: 
304496
comments
: 
""
contacted
: 
false
created_datetime_utc
: 
"2025-03-20T10:35:00.1369562Z"
demurrage_fee
: 
0
dock_name
: 
"CG082F00AA51"
dock_serial_number
: 
"CG082F00AA51"
identifiers
: 
[,…]
load_type
: 
4
manually_created
: 
false
shipment_type
: 
2
time_limit_enabled
: 
false
time_limit_warning_enabled
: 
false
touch_point_id
: 
3
touchpoint
: 
{touch_point_id: 3, name: "Website", display_name: "Website", is_standard: true}
trailer_assignment_id
: 
"3dcc65fc-d97d-4213-47f9-08dd6672c124"
trailer_number
: 
"001 / tr659900"
outbound_trailer_assignment_id
: 
"3dcc65fc-d97d-4213-47f9-08dd6672c124"
shipment_type
: 
2
trailer_assignment
: 
{trailer_assignment_id: "fe7509c0-412d-42d4-47f8-08dd6672c124",…}
trailer_assignment_id
: 
"fe7509c0-412d-42d4-47f8-08dd6672c124"
trailer_status
: 
{status_id: 1, status_name: "Scheduled", display_name: "Scheduled",…}
trailer_status_id
: 
1
type
: 
1



create json data for below
{appointment_id: "cfcbfa4b-6f5b-4fff-e5b1-08dd66714a96",…}
account_id
: 
"fdad02a5-da6e-4e4d-8d26-97a0884aa1a7"
appointment_date_time_utc
: 
"2025-03-20T10:00:00"
appointment_id
: 
"cfcbfa4b-6f5b-4fff-e5b1-08dd66714a96"
appointment_status
: 
3
carrier
: 
{scac_code: "", truck_detention_fee: 68.34, demurrage_fee: 664, comments: "",…}
carrier_id
: 
304494
comments
: 
""
created_datetime_utc
: 
"2025-03-20T09:47:23.96"
custom_questions
: 
[]
driver_status
: 
{status_id: 1, status_name: "Scheduled", display_name: "Scheduled"}
driver_status_id
: 
1
drivers
: 
[{trailer_appointment_driver_id: "1d21c447-35e9-4651-5e6e-08dd66714aa5",…}]
drop_n_hook
: 
false
duration
: 
"00:30:00"
identifiers
: 
[,…]
load_type
: 
4
outbound_trailer_assignment
: 
{trailer_assignment_id: "f0270dd5-53ad-494d-8bb1-08dd66714aab",…}
outbound_trailer_assignment_id
: 
"f0270dd5-53ad-494d-8bb1-08dd66714aab"
shipment_type
: 
2
tags
: 
[]
touchpoint
: 
{touch_point_id: 3, name: "Website", display_name: "Website", is_standard: true}
trailer_assignment
: 
{trailer_assignment_id: "cc112307-9b89-4615-8bb0-08dd66714aab",…}
trailer_assignment_id
: 
"cc112307-9b89-4615-8bb0-08dd66714aab"
trailer_status
: 
{status_id: 1, status_name: "Scheduled", display_name: "Scheduled",…}
trailer_status_id
: 
1
type
: 
1


Need to validate the TrailerId as below:
Need to add validations around the TrailerEntity(TrailerId from the request) and associated TrailerAssignment(if any) states. Here the TrailerAssignment is TA2
Means TrailerId passed is allowed for load-trailer Action
(Example if TrailerEntity = AtDock, TrailerAssignment = InProgress we should not allow that trailer for loading)
Refer the TrailerAssignmentValidationService.ValidateTrailerAsync
        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAsync(Trailer newTrailer, TrailerAssignment trailerAssignment, CancellationToken ct)
        {
            HashSet<string> assignmentStatuses = [];
            TrailerAssignment assignment = null;
            TrailerAppointment appointment = null;
            var trailerStatusIsValid = newTrailer.TrailerEntityStatus is not null
                    && ValidTrailerStatusesForAssignment.TryGetValue(newTrailer.TrailerEntityStatus.StatusName, out assignmentStatuses);

            if (!trailerStatusIsValid)
                throw new InvalidTrailerStatusForTrailerAssignmentException($"Trailer has wrong status for dock assignment.");

            if (newTrailer.TrailerAssignmentId is not null)
            {
                (assignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(trailerAssignment.AccountId, newTrailer.TrailerAssignmentId.Value, ct);

                if (!assignmentStatuses.Contains(assignment.TrailerStatus.StatusName))
                    throw new InvalidTrailerStatusForTrailerAssignmentUpdateException($"Trailer with Assignment in status {assignment.TrailerStatus?.StatusName} cannot be assigned to dock.");
            }
            return (assignment, appointment);
        }


08 TrailerAssignment Validation Flow

Below is the logic we need to implement. Few of them is missing in the implementation.
Please work with @Vladimir.Kozlov if you need more inputs

Both the TrailerId and TrailerAssignmentId are mandatory in the request.
TrailerId (in the request) is the Trailer we are linking to the new TrailerAssignment TA2
TrailerAssignmentId say TA0 (in the request) - is the assignment we are checking out

Need to validate the TrailerId as below:
Need to add validations around the TrailerEntity(TrailerId from the request) and associated TrailerAssignment(if any) states. Here the TrailerAssignment is TA2
Means TrailerId passed is allowed for load-trailer Action
(Example if TrailerEntity = AtDock, TrailerAssignment = InProgress we should not allow that trailer for loading)
Refer the TrailerAssignmentValidationService.ValidateTrailerAsync
If the above condition is failed then throw error
If the above condition is good then we need to checkout the TrailerAssignment TA2 linked with the TrailerId as mentioned below.

Create a new TrailerAssignment say TA1 (with TrailerNumber = Preffix + TrailerEntity.TrailerNumber)

Update the Appointment Outbound TrailerAssignmentId with the TA1

New logic update the TrailerEntity->TrailerAssignementId with TA1

Post a message to Checkout TrailerAssignmentId TA0

Post a message to Checkout TrailerAssignmentId TA2
explain me this

what is this method doing
        public async Task<TrailerAppointment> LoadTrailerAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct)
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _loadTrailerUseCaseValidationService.ValidateRequestAsync(loadTrailerRequest, ct);
            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
                loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
            }

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;

            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            return appointment;
        }
        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct);
            else
                ValidateTrailerNumber(request.TrailerNumber);

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }



explain me this in simple words
Both the TrailerId and TrailerAssignmentId are mandatory in the request.
TrailerId (in the request) is the Trailer we are linking to the new TrailerAssignment TA2
TrailerAssignmentId say TA0 (in the request) - is the assignment we are checking out

Need to validate the TrailerId as below:
Need to add validations around the TrailerEntity(TrailerId from the request) and associated TrailerAssignment(if any) states. Here the TrailerAssignment is TA2
Means TrailerId passed is allowed for load-trailer Action


When performing Load Trailer User must select a trailer:

utilizing existing Trailer (User should be able to see Trailer number, Carrier, Location):  

a trailer in In-Yard new Trailer status & In Yard Trailer assignment status

the following Trailers should not be available for a User to select (greyed out):

a trailer in In-Yard new Trailer status & Awaiting Dock Trailer assignment status

a trailer in In-Yard new Trailer status & Awaiting Dock - Pending Check-In Trailer assignment status

a trailer in In-Yard new Trailer status & Dock Assigned - Pending Check-In Trailer assignment status

a trailer in In-Yard new Trailer status & Dock Assigned - Dock Assigned Trailer assignment status

a trailer in At Dock new Trailer status & In Progress Trailer assignment status

Add: enter trailer number (same UX as the carrier input) 

a trailer in Checked Out new Trailer status & Checked Out Trailer assignment status should not be displayed for User selection but allowed to be entered as “new” while the same Trailer record is going to be used behind the scenes

Selection of In-Yard Trailer should do the following:

Checkout the other trailer assignment (like User does on the daily schedule manually - need to do this automatically behind the scenes) 

Assign the trailer to the new trailer assignment 

Selection of ‘Add Trailer’ should:

check if this Trailer has been in the Facility before - if yes:

update Checked out Trailer status to In Yard

if it is a new Trailer:

create a new Trailer record using Carrier & Trailer number

On confirmation of manual check-in action, assign the trailer to the new trailer assignment

The action should still support a ‘tandem’ use case as today 



@Kamarali Dukandar Below is the logic we need to implement. Few of them is missing in the implementation.
Please work with @Vladimir.Kozlov if you need more inputs

Both the TrailerId and TrailerAssignmentId are mandatory in the request.
TrailerId (in the request) is the Trailer we are linking to the new TrailerAssignment TA2
TrailerAssignmentId say TA0 (in the request) - is the assignment we are checking out

Need to validate the TrailerId as below:
Need to add validations around the TrailerEntity(TrailerId from the request) and associated TrailerAssignment(if any) states. Here the TrailerAssignment is TA2
Means TrailerId passed is allowed for load-trailer Action
(Example if TrailerEntity = AtDock, TrailerAssignment = InProgress we should not allow that trailer for loading)
Refer the TrailerAssignmentValidationService.ValidateTrailerAsync
If the above condition is failed then throw error
If the above condition is good then we need to checkout the TrailerAssignment TA2 linked with the TrailerId as mentioned below.

Create a new TrailerAssignment say TA1 (with TrailerNumber = Preffix + TrailerEntity.TrailerNumber)

Update the Appointment Outbound TrailerAssignmentId with the TA1

New logic update the TrailerEntity->TrailerAssignementId with TA1

Post a message to Checkout TrailerAssignmentId TA0

Post a message to Checkout TrailerAssignmentId TA2


        public async Task<TrailerAppointment> LoadTrailerAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct)
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _loadTrailerUseCaseValidationService.ValidateRequestAsync(loadTrailerRequest, ct);
            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
                loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
            }

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;

            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            return appointment;
        }
		
		        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct);
            else
                ValidateTrailerNumber(request.TrailerNumber);

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }
		        private async Task<TrailerAppointment> ValidateAppointmentAsync(Guid accountId, Guid trailerAppointmentId, CancellationToken ct)
        {
            var appDataToInclude = new string[] {
                nameof(TrailerAppointment.Carrier),
                nameof(TrailerAppointment.TrailerAssignment),
                nameof(TrailerAppointment.OutboundTrailerAssignment)
            };

            var appointment = await _appointmentQueryInfrastructureService.GetAppointmentAsync(trailerAppointmentId, ct, appDataToInclude)
                ?? throw new AppointmentNotFoundException($"Appointment with id: '{trailerAppointmentId}' is not found on account: '{accountId}'.");

            if (!(appointment.DropNHook
                || (appointment.ShipmentType == ShipmentType.Outbound && appointment.LoadType == LoadType.PickUp)))
                throw new LoadTrailerInvalidAppointmentException($"Trailer cannot be loaded for Appointment with id: '{trailerAppointmentId}'");

            if (!ValidAppointmentStatuses.Contains(appointment.AppointmentStatus))
                throw new LoadTrailerInvalidAppointmentStatusException($"Trailer cannot be loaded for Appointment in status: '{appointment.AppointmentStatus}'");

            return appointment;
        }


09 Code Optimization Assistance

optimize it

            validateById ?? trailerAssignment.TrailerId = loadTrailerRequest.TrailerId;



        private async Task<TrailerAssignment> CreateNewTrailerAssignmentAsync(LoadTrailerRequest loadTrailerRequest, TrailerAppointment trailerAppointment,
            AccountDevice assignedDock, Touchpoint touchpoint, CancellationToken ct, bool validateById = false)
        {
            var prevTrailerNumber = trailerAppointment.OutboundTrailerAssignment is not null
                    ? $"{trailerAppointment.OutboundTrailerAssignment.TrailerNumber} / " : "";

            var trailerAssignment = new TrailerAssignment()
            {
                AccountId = trailerAppointment.AccountId,
                TrailerNumber = prevTrailerNumber + loadTrailerRequest.TrailerNumber,
                CreatedByUserId = loadTrailerRequest.UserId,
                DockSerialNumber = assignedDock.SerialNumber,
                DockName = assignedDock.Name,
                CarrierId = trailerAppointment.CarrierId.Value,
                Identifiers = GetIdentifiers(trailerAppointment, loadTrailerRequest.Prefix),
                ShipmentType = trailerAppointment.ShipmentType,
                LoadType = trailerAppointment.LoadType,
                ProductType = trailerAppointment.ProductType,
                Comments = trailerAppointment.Comments,
                CarrierDetentionFee = trailerAppointment.Carrier.TruckDetentionFee,
                Touchpoint = touchpoint,
                TouchPointId = touchpoint.TouchPointId,
                CreatedDatetimeUtc = DateTime.UtcNow
            };

            validateById ?? trailerAssignment.TrailerId = loadTrailerRequest.TrailerId;


polish it
added trailerId to while creating NewTrailerAssignment

polish this sentence
added trailerId to while creating NewTrailerAssignment


write a private static function which return this
            if (trailerSettings?.CheckedOutTrailersEnabled == true)
                predicate = predicate.AndAlso(x => x.TrailerEntityStatusId != сheckedOutTrailerStatus.StatusId || x.TrailerAssignment.CheckOutDatetimeUtc > DateTime.UtcNow.AddMinutes(-trailerSettings.CheckedOutTrailersInMinutes));
            else
                predicate = predicate.AndAlso(x => x.TrailerEntityStatusId != сheckedOutTrailerStatus.StatusId);


polish this msg
I fixed the PR comments for Quick Filter count bugfix could you please review it and approve it


10 Checked Out Count Adjustment

polish it
Hi currently this is not implemented the checked out count is implemented irrespective of Checked out settings we can implement it it is not a big change
in response too this chat 
Hi Kamarali, I have a question about Trailers - Quick Filters, there is a requirement:
Checked Out (if turned on in settings) - with count
So it means, that if this setting is turned off, we should not return Checked Out trailers in response




NEW

2:17
and the second thing, based on conversations with Irina and Michael, the Checked Out count should correlate with Trailer Settings.
for example, if account has checked_out_trailers_in_minutes = 60min, then after 60 min we should not count this trailer in Quick Filters


Hi,
Currently, this is not implemented. The Checked Out count is calculated regardless of the Checked Out settings. However, we can implement this—it’s a small change.
Additionally, based on discussions with Irina and Michael, the Checked Out count should align with Trailer Settings. For example, if an account has checked_out_trailers_in_minutes = 60, then after 60 minutes of checked out trailer, the trailer should no longer be counted in Quick Filters.
polish it


        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            if (!(string.IsNullOrEmpty(request.TrailerNumber) || int.IsNullOrEmpty(request.TrailerId)) && request.TrailerAssignmentId.HasValue)
                throw new LoadTrailerInvalidTrailerAssignmentDataException("Only Trailer Number or Trailer AssignmentId can be provided at the same time.");

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                 throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        }



how to check this
!int.IsNullOrEmpty(request.TrailerId)


can we club these 2 methods
        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(request.TrailerNumber) && request.TrailerAssignmentId.HasValue) 
                    throw new LoadTrailerInvalidTrailerAssignmentDataException("Only Trailer Number or Trailer AssignmentId can be provided at the same time.");

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                 throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        }

        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutByIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            if (request.TrailerId != 0 && request.TrailerAssignmentId.HasValue)
                throw new LoadTrailerInvalidTrailerAssignmentDataException("Only Trailer Number or Trailer AssignmentId can be provided at the same time.");

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        }



create errors like this 
        /// <response code="400">Error codes and descriptions in the error response: <br/>
        /// 400.143 - Unique Id Number has invalid length.<br/>
        /// 400.147 - Trailer Assignment not allowed.<br/>
        /// 400.148 - Device does not exist in Account.<br/>
        /// 400.239 - Trailer Number cannot exceed 15 characters.<br/>
        /// 400.280 - The Unique ID is already assigned to another shipment.<br/>
        /// 400.282 - There is block on dock device.<br/>
        /// 400.695 - Invalid Trailer status.<br/>
        /// 400.696 - Prefix has invalid length.<br/>
        /// 400.697 - Prefix is required.<br/>
        /// 400.698 - Trailer Number is required.<br/>
        /// 400.699 - Trailer cannot be loaded for Appointment.<br/>
        /// 400.700 - Invalid Appointment status for trailer load.<br/>
        /// 400.701 - Only Trailer Number or Trailer AssignmentId can be provided at the same time.<br/>
        /// </response>from below
AddingErrorResponse<AppointmentNotFoundException>(219, HttpStatusCode.NotFound, "Trailer appointment is not found on the account.");
AddingErrorResponse<LoadTrailerInvalidAppointmentException>(699, HttpStatusCode.BadRequest, $"Trailer cannot be loaded for Appointment.");
AddingErrorResponse<LoadTrailerInvalidAppointmentStatusException>(700, HttpStatusCode.BadRequest, "Invalid Appointment status for trailer load.");
AddingErrorResponse<InvalidSerialNumberException>(148, HttpStatusCode.BadRequest, "Invalid serial number specified.");
AddingErrorResponse<TrailerAssignmentNotAllowedException>(147, HttpStatusCode.BadRequest, "Trailer Assignment not allowed.");.
AddingErrorResponse<DockBlockedException>(282, HttpStatusCode.BadRequest, "There is block on dock device.");
AddingErrorResponse<InvalidSerialNumberException>(148, HttpStatusCode.BadRequest, "Invalid serial number specified.");
AddingErrorResponse<LoadTrailerPrefixRequiredException>(697, HttpStatusCode.BadRequest, "Prefix is required.");
AddingErrorResponse<LoadTrailerPrefixLengthException>(696, HttpStatusCode.BadRequest, $"Prefix cannot exceed {_loadTrailerConfiguration?.PrefixMaxLength} characters.");
AddingErrorResponse<TrailerAssignmentIdentifierUniqueIdNotUniqueException>(280, HttpStatusCode.BadRequest, "The Unique ID is already assigned to another shipment.");
AddingErrorResponse<TrailerAssignmentIdentifierUniqueIdNotUniqueException>(280, HttpStatusCode.BadRequest, "The Unique ID is already assigned to another shipment.");
 AddingErrorResponse<TrailerAssignmentIdentifierUniqueIdEmptyException>(420, HttpStatusCode.BadRequest, "Unique id cannot be null or empty.");
 AddingErrorResponse<TrailerAssignmentIdentifierUniqueIdMinLengthException>(355, HttpStatusCode.BadRequest, $"Unique Id Number should be at least {_trailerAssignmentIdentifierValidationConfiguration?.MinUniqueIdLength} characters");
 AddingErrorResponse<TrailerAssignmentIdentifierUniqueIdMaxLengthExceededException>(143, HttpStatusCode.BadRequest, $"Unique Id Number cannot exceed {_trailerAssignmentIdentifierValidationConfiguration?.MaxUniqueIdLength} characters");
 AddingErrorResponse<InvalidUniqueIdSpecialCharsException>(635, HttpStatusCode.BadRequest, "A valid unique Id can only contain letters and numbers.");
  AddingErrorResponse<LoadTrailerByIdInvalidTrailerAssignmentDataException>(749, HttpStatusCode.BadRequest, "Only Trailer Id or Trailer AssignmentId can be provided at the same time.");
 AddingErrorResponse<InvalidTrailerIdException>(750, HttpStatusCode.BadRequest, "Trailer Id is required.");
 AddingErrorResponse<TrailerAssignmentNotFoundException>(152, HttpStatusCode.NotFound, "Trailer assignment is not found on the account.");
  AddingErrorResponse<LoadTrailerInvalidTrailerAssignmentStatusException>(695, HttpStatusCode.BadRequest, "Invalid Trailer status.");



how to merge this 2 methods
        public async Task<TrailerAppointment> LoadTrailerAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct)
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _loadTrailerUseCaseValidationService.ValidateRequestAsync(loadTrailerRequest, ct);
            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
                loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
            }

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;

            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            return appointment;
        }

        public async Task<TrailerAppointment> LoadTrailerByIdAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct)
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _loadTrailerUseCaseValidationService.ValidateRequestByIdAsync(loadTrailerRequest, ct);
            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
                loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
            }

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;

            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            return appointment;
        }

generate a ref for this method
        public async Task<TrailerAppointment> LoadTrailerAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct, bool validateById = false)
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = validateById
                ? await _loadTrailerUseCaseValidationService.ValidateRequestByIdAsync(loadTrailerRequest, ct)
                : await _loadTrailerUseCaseValidationService.ValidateRequestAsync(loadTrailerRequest, ct);

            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
                loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
            }

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;

            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            return appointment;
        }

    public interface ILoadTrailerUseCaseService
    {
        Task<TrailerAppointment> LoadTrailerAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct, );       
    }
}


write test cases in below class
using Docks.Common.Configuration.Core.DomainServices.TrailerAppointments;
using Docks.Common.Configuration.Core.DomainServices.TrailerAssignments.Validation;
using Docks.Common.Exceptions.TrailerAppointments;
using Docks.Common.Exceptions.TrailerAssignments;
using Docks.Common.Exceptions.TrailerAssignments.Identifiers;
using Docks.Domain.AccountDevices;
using Docks.Domain.Accounts;
using Docks.Domain.Constants;
using Docks.Domain.Enums;
using Docks.Domain.TrailerAppointments;
using Docks.Domain.TrailerAssignments;
using Docks.DomainService.Core.Services.Docks;
using Docks.DomainService.Core.Services.TrailerAssignments.Validation;
using Docks.DomainService.Tests.Helpers;
using Docks.DomainService.Tests.Mocks;
using Docks.DomainService.UseCaseServices.Appointments;
using Docks.Infrastructure.Core.Services.TrailerAppointments;
using Docks.Infrastructure.Core.Services.TrailerAssignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docks.DomainService.Tests.UseCaseServices.TrailerAppointments
{
    [TestClass]
    public class LoadTrailerUseCaseValidationServiceTests
    {
        private Guid _accountId;
        private Guid _appointmentId;
        private Guid _assignmentIdToCheckOut;
        private Guid _appointmentIdToComplete;
        private CancellationToken _ct;
        private string _dockSerialNumber;
        private string _prefix;
        private string _uniqueId;
        private int _prefixMaxLength;
        private int _uniqueIdMaxLength;
        private LoadTrailerRequest _request;
        private TrailerAppointment _appointment;
        private TrailerAssignment _assignmentToCheckOut;
        private TrailerAppointment _appointmentToComplete;
        private AccountDevice _dock;
        private Account _account;

        private AutoMocker _mocker;
        private LoadTrailerUseCaseValidationService _service;

        [TestInitialize]
        public void Initialize()
        {
            _accountId = Guid.NewGuid();
            _appointmentId = Guid.NewGuid();
            _appointmentIdToComplete = Guid.NewGuid();
            _assignmentIdToCheckOut = Guid.NewGuid();
            _dockSerialNumber = "3245";
            _prefix = "prefix";
            _uniqueId = "ui";
            _prefixMaxLength = 7;
            _uniqueIdMaxLength = 10;

            _request = new LoadTrailerRequest
            {
                AccountId = _accountId,
                TrailerAppointmentId = _appointmentId,
                DockSerialNumber = _dockSerialNumber,
                Prefix = _prefix,
                TrailerAssignmentId = _assignmentIdToCheckOut,
            };

            _appointment = new TrailerAppointment
            {
                AccountId = _accountId,
                AppointmentId = _appointmentId,
                ShipmentType = ShipmentType.Outbound,
                LoadType = LoadType.PickUp,
                AppointmentStatus = AppointmentStatus.Scheduled,
                Identifiers = [
                    new TrailerAssignmentIdentifier {
                        UniqueId = _uniqueId
                    }
                ]
            };

            _appointmentToComplete = new TrailerAppointment
            {
                AccountId = _accountId,
                AppointmentId = _appointmentIdToComplete,
            };

            _assignmentToCheckOut = new TrailerAssignment
            {
                TrailerAssignmentId = _assignmentIdToCheckOut,
                TrailerStatusId = TrailerStatusMock.Get(TrailerStatuses.InYard).StatusId
            };

            _dock = new AccountDevice
            {
                SerialNumber = _dockSerialNumber
            };
            _account = new Account { };

            _mocker = new AutoMocker();

            SetupAppoinment(_appointment);
            SetupAssignmentToCheckOut(_assignmentToCheckOut, _appointmentToComplete);

            _mocker.Setup<IDockAssignmentValidationService, Task<AccountDevice>>(_ =>
                _.ValidateDockAsync(_accountId, _dockSerialNumber, _ct))
                .ReturnsAsync(_dock);

            _mocker.Setup<IDockAssignmentValidationService, Task<Account>>(_ =>
                _.ValidateSessionAsync(_accountId, _dockSerialNumber, _ct))
                .ReturnsAsync(_account);

            _mocker.Setup<ILoadTrailerConfiguration, int>(_ => _.PrefixMaxLength)
                .Returns(_prefixMaxLength);

            _mocker.Setup<ITrailerAssignmentIdentifierValidationConfiguration, int>(_ => _.MaxUniqueIdLength)
               .Returns(_uniqueIdMaxLength);

            _mocker.SetupTrailerStatuses();

            _service = _mocker.CreateInstance<LoadTrailerUseCaseValidationService>();
        }

        [TestMethod]
        public async Task ValidateRequestAsync_GetsAppointment()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IAppointmentQueryInfrastructureService>(_ =>
                _.GetAppointmentAsync(_appointmentId, _ct, It.IsAny<string[]>()), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AppointmentIsNull_ThrowsAppointmentNotFoundException()
        {
            SetupAppoinment(null);

            await Assert.ThrowsExceptionAsync<AppointmentNotFoundException>(async () =>
                await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DynamicData(nameof(GetInvalidAppointmentCases), DynamicDataSourceType.Method)]
        public async Task ValidateRequestAsync_InvalidAppointment_ThrowsLoadTrailerInvalidAppointmentException(ShipmentType shipmentType, LoadType loadType, bool dropNHook)
        {
            _appointment.ShipmentType = shipmentType;
            _appointment.LoadType = loadType;
            _appointment.DropNHook = dropNHook;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentException>(async () =>
                await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DynamicData(nameof(GetValidAppointmentCases), DynamicDataSourceType.Method)]
        public async Task ValidateRequestAsync_ValidAppointment_DoesNotThrowLoadTrailerInvalidAppointmentException(ShipmentType shipmentType, LoadType loadType, bool dropNHook)
        {
            _appointment.ShipmentType = shipmentType;
            _appointment.LoadType = loadType;
            _appointment.DropNHook = dropNHook;

            await _service.ValidateRequestAsync(_request, _ct);
        }

        [TestMethod]
        [DataRow(AppointmentStatus.Unscheduled)]
        [DataRow(AppointmentStatus.Cancelled)]
        [DataRow(AppointmentStatus.InProgress)]
        [DataRow(AppointmentStatus.Completed)]
        [DataRow(AppointmentStatus.LateCheckIn)]
        [DataRow(AppointmentStatus.EarlyCheckIn)]
        [DataRow(AppointmentStatus.Deleted)]
        public async Task ValidateRequestAsync_InvalidAppointmentStatus_ThrowsLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentStatusException>(async () =>
                 await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(AppointmentStatus.Scheduled)]
        [DataRow(AppointmentStatus.Rescheduled)]
        [DataRow(AppointmentStatus.PendingCheckIn)]
        [DataRow(AppointmentStatus.Active)]
        public async Task ValidateRequestAsync_ValidAppointmentStatus_DoesNotThrowLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await _service.ValidateRequestAsync(_request, _ct);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesDock()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IDockAssignmentValidationService>(_ =>
               _.ValidateDockAsync(_accountId, _dockSerialNumber, _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesSession()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IDockAssignmentValidationService>(_ =>
               _.ValidateSessionAsync(_accountId, _dockSerialNumber, _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_EmptyPrefix_ThrowsLoadTrailerPrefixRequiredException()
        {
            _request.Prefix = "";

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixRequiredException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_InvalidPrefixLength_ThrowsLoadTrailerPrefixLengthException()
        {
            _request.Prefix = RandomHelper.GetCharacters(_prefixMaxLength + 1);

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixLengthException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesUniqueIdDuplicates()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentIdentifierValidationService>(_ =>
                _.ValidateUniqueIdDuplicatesAsync(_accountId,
                    It.Is<List<string>>(x => x.Count == 1 && x.Contains(_prefix + _uniqueId)), _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesIdentifier()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentIdentifierValidationService>(_ =>
                _.ValidateIdentifier(_prefix + _uniqueId, true), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_EmptyTrailerNumber_ThrowsInvalidTrailerNumberException()
        {
            _request.TrailerAssignmentId = null;
            _request.TrailerNumber = "";

            await Assert.ThrowsExceptionAsync<InvalidTrailerNumberException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_InvalidTrailerNumberLength_ThrowsTrailerNumberMaxLengthExceededException()
        {
            _request.TrailerAssignmentId = null;
            _request.TrailerNumber = RandomHelper.GetCharacters(16);

            await Assert.ThrowsExceptionAsync<TrailerNumberMaxLengthExceededException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIdAndTrailerNumberArePassed_ThrowsLoadTrailerInvalidTrailerAssignmentDataException()
        {
            _request.TrailerAssignmentId = Guid.NewGuid();
            _request.TrailerNumber = "tn";

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentDataException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIsNull_ThrowsTrailerAssignmentNotFoundException()
        {
            SetupAssignmentToCheckOut(null, null);

            await Assert.ThrowsExceptionAsync<TrailerAssignmentNotFoundException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(TrailerStatuses.Unscheduled)]
        [DataRow(TrailerStatuses.Scheduled)]
        [DataRow(TrailerStatuses.DockPending)]
        [DataRow(TrailerStatuses.DockAssigned)]
        [DataRow(TrailerStatuses.YardPending)]
        [DataRow(TrailerStatuses.AwaitingDock)]
        [DataRow(TrailerStatuses.SessionInProgress)]
        [DataRow(TrailerStatuses.CheckedOut)]
        [DataRow(TrailerStatuses.NotAssigned)]
        public async Task ValidateRequestAsync_InvalidAssignmentStatus_ThrowsLoadTrailerInvalidTrailerAssignmentStatusException(string statusName)
        {
            var status = TrailerStatusMock.Get(statusName);
            _assignmentToCheckOut.TrailerStatusId = status.StatusId;
            _assignmentToCheckOut.TrailerStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentStatusException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_Success()
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _service.ValidateRequestAsync(_request, _ct);

            Assert.AreEqual(_appointmentId, appointment.AppointmentId);
            Assert.AreEqual(_assignmentIdToCheckOut, trailerAssignmentToCheckOut.TrailerAssignmentId);
            Assert.AreEqual(_appointmentIdToComplete, appointmentToComplete.AppointmentId);
            Assert.AreEqual(_dockSerialNumber, dock.SerialNumber);
        }

        private void SetupAppoinment(TrailerAppointment appointment)
            => _mocker.Setup<IAppointmentQueryInfrastructureService, Task<TrailerAppointment>>(_ =>
                _.GetAppointmentAsync(_appointmentId, _ct, It.IsAny<string[]>()))
                .ReturnsAsync(appointment);

        private void SetupAssignmentToCheckOut(TrailerAssignment assignment, TrailerAppointment appointment)
            => _mocker.Setup<ITrailerAssignmentQueryInfrastructureService, Task<(TrailerAssignment, TrailerAppointment)>>(_ =>
              _.GetAssignmentWithAppointmentAsync(_accountId, _assignmentIdToCheckOut, _ct))
              .ReturnsAsync((assignment, appointment));

        private static IEnumerable<object[]> GetInvalidAppointmentCases()
            =>
            [
                [ShipmentType.Inbound, LoadType.None, false],
                [ShipmentType.Inbound, LoadType.LiveLoad, false],
                [ShipmentType.Inbound, LoadType.DropLoad, false],
                [ShipmentType.Inbound, LoadType.PreLoad, false],
                [ShipmentType.Inbound, LoadType.PickUp, false],
                [ShipmentType.Outbound, LoadType.None, false],
                [ShipmentType.Outbound, LoadType.LiveLoad, false],
                [ShipmentType.Outbound, LoadType.DropLoad, false],
                [ShipmentType.Outbound, LoadType.PreLoad, false]
            ];

        private static IEnumerable<object[]> GetValidAppointmentCases()
           =>
           [
                [ShipmentType.Outbound, LoadType.PickUp, false],
                [ShipmentType.Inbound, LoadType.None, true],
                [ShipmentType.Inbound, LoadType.LiveLoad, true],
                [ShipmentType.Inbound, LoadType.DropLoad, true],
                [ShipmentType.Inbound, LoadType.PreLoad, true],
                [ShipmentType.Inbound, LoadType.PickUp, true]
           ];
    }
}


write test cases for this method in LoadTrailerUseCaseValidationServiceTests class

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerByIdAssignmentForCheckOutAsync(request, ct);
            else
                ValidateTrailerId(request.TrailerId);

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }


modify all these methods
        [TestMethod]
        [DataRow(AppointmentStatus.Unscheduled)]
        [DataRow(AppointmentStatus.Cancelled)]
        [DataRow(AppointmentStatus.InProgress)]
        [DataRow(AppointmentStatus.Completed)]
        [DataRow(AppointmentStatus.LateCheckIn)]
        [DataRow(AppointmentStatus.EarlyCheckIn)]
        [DataRow(AppointmentStatus.Deleted)]
        public async Task ValidateRequestAsync_InvalidAppointmentStatus_ThrowsLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidAppointmentStatusException>(async () =>
                 await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(AppointmentStatus.Scheduled)]
        [DataRow(AppointmentStatus.Rescheduled)]
        [DataRow(AppointmentStatus.PendingCheckIn)]
        [DataRow(AppointmentStatus.Active)]
        public async Task ValidateRequestAsync_ValidAppointmentStatus_DoesNotThrowLoadTrailerInvalidAppointmentStatusException(AppointmentStatus status)
        {
            _appointment.AppointmentStatus = status;

            await _service.ValidateRequestAsync(_request, _ct);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesDock()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IDockAssignmentValidationService>(_ =>
               _.ValidateDockAsync(_accountId, _dockSerialNumber, _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesSession()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<IDockAssignmentValidationService>(_ =>
               _.ValidateSessionAsync(_accountId, _dockSerialNumber, _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_EmptyPrefix_ThrowsLoadTrailerPrefixRequiredException()
        {
            _request.Prefix = "";

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixRequiredException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_InvalidPrefixLength_ThrowsLoadTrailerPrefixLengthException()
        {
            _request.Prefix = RandomHelper.GetCharacters(_prefixMaxLength + 1);

            await Assert.ThrowsExceptionAsync<LoadTrailerPrefixLengthException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesUniqueIdDuplicates()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentIdentifierValidationService>(_ =>
                _.ValidateUniqueIdDuplicatesAsync(_accountId,
                    It.Is<List<string>>(x => x.Count == 1 && x.Contains(_prefix + _uniqueId)), _ct), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_ValidatesIdentifier()
        {
            await _service.ValidateRequestAsync(_request, _ct);

            _mocker.Verify<ITrailerAssignmentIdentifierValidationService>(_ =>
                _.ValidateIdentifier(_prefix + _uniqueId, true), Times.Once);
        }

        [TestMethod]
        public async Task ValidateRequestAsync_EmptyTrailerNumber_ThrowsInvalidTrailerNumberException()
        {
            _request.TrailerAssignmentId = null;
            _request.TrailerNumber = "";

            await Assert.ThrowsExceptionAsync<InvalidTrailerNumberException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_InvalidTrailerNumberLength_ThrowsTrailerNumberMaxLengthExceededException()
        {
            _request.TrailerAssignmentId = null;
            _request.TrailerNumber = RandomHelper.GetCharacters(16);

            await Assert.ThrowsExceptionAsync<TrailerNumberMaxLengthExceededException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIdAndTrailerNumberArePassed_ThrowsLoadTrailerInvalidTrailerAssignmentDataException()
        {
            _request.TrailerAssignmentId = Guid.NewGuid();
            _request.TrailerNumber = "tn";

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentDataException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_AssignmentIsNull_ThrowsTrailerAssignmentNotFoundException()
        {
            SetupAssignmentToCheckOut(null, null);

            await Assert.ThrowsExceptionAsync<TrailerAssignmentNotFoundException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        [DataRow(TrailerStatuses.Unscheduled)]
        [DataRow(TrailerStatuses.Scheduled)]
        [DataRow(TrailerStatuses.DockPending)]
        [DataRow(TrailerStatuses.DockAssigned)]
        [DataRow(TrailerStatuses.YardPending)]
        [DataRow(TrailerStatuses.AwaitingDock)]
        [DataRow(TrailerStatuses.SessionInProgress)]
        [DataRow(TrailerStatuses.CheckedOut)]
        [DataRow(TrailerStatuses.NotAssigned)]
        public async Task ValidateRequestAsync_InvalidAssignmentStatus_ThrowsLoadTrailerInvalidTrailerAssignmentStatusException(string statusName)
        {
            var status = TrailerStatusMock.Get(statusName);
            _assignmentToCheckOut.TrailerStatusId = status.StatusId;
            _assignmentToCheckOut.TrailerStatus = status;

            await Assert.ThrowsExceptionAsync<LoadTrailerInvalidTrailerAssignmentStatusException>(async () =>
                  await _service.ValidateRequestAsync(_request, _ct));
        }

        [TestMethod]
        public async Task ValidateRequestAsync_Success()
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = await _service.ValidateRequestAsync(_request, _ct);

            Assert.AreEqual(_appointmentId, appointment.AppointmentId);
            Assert.AreEqual(_assignmentIdToCheckOut, trailerAssignmentToCheckOut.TrailerAssignmentId);
            Assert.AreEqual(_appointmentIdToComplete, appointmentToComplete.AppointmentId);
            Assert.AreEqual(_dockSerialNumber, dock.SerialNumber);
        }

call  await _service.ValidateRequestByIdAsync(_request, _ct); and change the names like ValidateRequestByIdAsync_ValidAppointmentStatus_DoesNotThrowLoadTrailerInvalidAppointmentStatusException



trailerId is int
  _request.TrailerId = RandomHelper.GetCharacters(16);


Currently, this is not implemented—the Checked Out count is calculated regardless of the Checked Out settings. However, we can implement this as it's a minor change.
Additionally, based on discussions with Irina and Michael, the Checked Out count should align with Trailer Settings. For example, if an account has checked_out_trailers_in_minutes = 60, then a trailer that has been checked out for more than 60 minutes should no longer be counted in Quick Filters.
make a check in comment for the above functionality in 1 line

polish it
Can you please go through this PR and  approve this PR for Load Trailers


optimize it
        private async void ValidateTrailerId(int trailerId, Guid accountId, CancellationToken ct)
        {
            var exsistingTrailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(trailerId, ct);
            
            if(exsistingTrailer != null && exsistingTrailer.AccountId = accountId)
                throw new InvalidTrailerIdException("Trailer Id is required.");
        }


generate a method like below
        private void SetupAssignmentToCheckOut(TrailerAssignment assignment, TrailerAppointment appointment)
            => _mocker.Setup<ITrailerAssignmentQueryInfrastructureService, Task<(TrailerAssignment, TrailerAppointment)>>(_ =>
              _.GetAssignmentWithAppointmentAsync(_accountId, _assignmentIdToCheckOut, _ct))
              .ReturnsAsync((assignment, appointment));

to mock 
        var exsistingTrailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

        public async Task<Trailer> GetTrailerByTrailerIdAsync(int trailerId, CancellationToken ct)
        {
            var model = await _trailerReadOnlyRepository.GetFirstAsync(c => c.TrailerId == trailerId, _ => _.Include(x => x.EntityStatus), ct: ct);
            return model?.ToDomain();
        }
public class Trailer
{
    public int TrailerId { get; set; }

    public Guid AccountId { get; set; }

    public int CarrierId { get; set; }

    public string TrailerNumber { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public Guid? LastUpdatedByUserId { get; set; }

    public DateTime? LastUpdatedDatetimeUtc { get; set; }

    public Carrier Carrier { get; set; }

    public Guid? TrailerAssignmentId { get; set; }

    public int? TrailerEntityStatusId { get; set; }

    public TrailerEntityStatus TrailerEntityStatus { get; set; }

    public Location Location { get; set; }

    public TrailerAssignment TrailerAssignment { get; set; }

    public TrailerAppointment TrailerAppointment { get; set; }

    public List<TrailerProperty> TrailerProperties { get; set; }
}

setup a moq obje for trailer
  public class Trailer
  {
      public int TrailerId { get; set; }

      public Guid AccountId { get; set; }

      public int CarrierId { get; set; }

      public string TrailerNumber { get; set; }

      public Guid CreatedByUserId { get; set; }

      public DateTime CreatedDatetimeUtc { get; set; }

      public Guid? LastUpdatedByUserId { get; set; }

      public DateTime? LastUpdatedDatetimeUtc { get; set; }

      public Carrier Carrier { get; set; }

      public Guid? TrailerAssignmentId { get; set; }

      public int? TrailerEntityStatusId { get; set; }

      public TrailerEntityStatus TrailerEntityStatus { get; set; }

      public Location Location { get; set; }

      public TrailerAssignment TrailerAssignment { get; set; }

      public TrailerAppointment TrailerAppointment { get; set; }

      public List<TrailerProperty> TrailerProperties { get; set; }
  }


optimize this
        public async Task<TrailerAppointment> LoadTrailerAsync(LoadTrailerRequest loadTrailerRequest, CancellationToken ct, bool validateById = false)
        {
            var (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock) = validateById
                ? await _loadTrailerUseCaseValidationService.ValidateRequestByIdAsync(loadTrailerRequest, ct)
                : await _loadTrailerUseCaseValidationService.ValidateRequestAsync(loadTrailerRequest, ct);

            var websiteTouchpoint = await _touchpointQueryInfrastructureService.GetTouchpointAsync(Touchpoints.Website, ct);

            if (trailerAssignmentToCheckOut is not null)
            {
                _trailerCheckoutPublisherInfrastructureService.PublishTrailerCheckoutMessage(trailerAssignmentToCheckOut, appointmentToComplete, websiteTouchpoint);
                loadTrailerRequest.TrailerNumber = trailerAssignmentToCheckOut.TrailerNumber;
            }

            var newTrailerAssignment = await CreateNewTrailerAssignmentAsync(loadTrailerRequest, appointment, dock, websiteTouchpoint, ct);
            appointment.OutboundTrailerAssignmentId = newTrailerAssignment.TrailerAssignmentId;
            appointment.OutboundTrailerAssignment = newTrailerAssignment;

            await _appointmentManagementInfrastructureService.UpdateOutboundTrailer(appointment.AppointmentId, appointment.OutboundTrailerAssignmentId.Value, ct);

            _appointmentPublisherInfrastructureService.PublishAppointmentUpdated(appointment);
            _trailerAssignmentPublisherInfrastructureService.PublishTrailerAssignmentMessage(newTrailerAssignment, dock);

            return appointment;
        }
        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct);
            else
                ValidateTrailerNumber(request.TrailerNumber);

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestByIdAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerByIdAssignmentForCheckOutAsync(request, ct);
            else
                ValidateTrailerId(request.TrailerId, request.AccountId, ct);

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }


        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerAssignmentForCheckOutAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(request.TrailerNumber) && request.TrailerAssignmentId.HasValue)
                throw new LoadTrailerInvalidTrailerAssignmentDataException("Only Trailer Number or Trailer AssignmentId can be provided at the same time.");

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                 throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        }

        private async Task<(TrailerAssignment, TrailerAppointment)> ValidateTrailerByIdAssignmentForCheckOutAsync(LoadTrailerRequest request, CancellationToken ct)
        {
            var exsistingTrailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

            if (exsistingTrailer == null && request.TrailerAssignmentId.HasValue)                
                throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Only Trailer Id or Trailer AssignmentId can be provided at the same time.");

            var (trailerAssignment, appointment) = await _trailerQueryInfrastructureService.GetAssignmentWithAppointmentAsync(request.AccountId, request.TrailerAssignmentId.Value, ct);

            if (trailerAssignment == null)
                throw new TrailerAssignmentNotFoundException($"Trailer Assignment with id: '{request.TrailerAssignmentId.Value}' is not found on account: '{request.AccountId}'.");

            var validStatuses = await _statusQueryInfrastructureService.GetTrailerStatusesAsync(ValidTrailerAssignmentStatuses, ct);
            if (!validStatuses.Exists(s => s.StatusId == trailerAssignment.TrailerStatusId))
                throw new LoadTrailerInvalidTrailerAssignmentStatusException($"Trailer in '{trailerAssignment.TrailerStatus.DisplayName}' status cannot be loaded.");

            return (trailerAssignment, appointment);
        } 


optimize this methood
        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct, bool validateById = false)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct, validateById);
            else
            {
                if (validateById)
                {
                    ValidateTrailerNumber(request.TrailerNumber);
                }
                else
                {
                    ValidateTrailerId(request.TrailerId, request.AccountId, ct);
                }
            }
                
            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }


can we optimize this part
        if (validateById)
            ValidateTrailerId(request.TrailerId, request.AccountId, ct);
        else
            ValidateTrailerNumber(request.TrailerNumber);


optimize it
        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct, bool validateById = false)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct, validateById);
            else
            {
                !validateById ? () => ValidateTrailerNumber(request.TrailerNumber) : () => ValidateTrailerId(request.TrailerId, request.AccountId, ct);
            }
                
            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        } 

how to moc this 
        private void SetupValidationResult(TrailerAppointment appointment, TrailerAssignment trailerAssignmentToCheckOut,
            TrailerAppointment appointmentToComplete, AccountDevice device)
            => _mocker.Setup<ILoadTrailerUseCaseValidationService, Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)>>(_ =>
                _.ValidateRequestAsync(_request, _ct, It.IsAny<bool>())
            .ReturnsAsync((appointment, trailerAssignmentToCheckOut, appointmentToComplete, device));
        public async Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)> ValidateRequestAsync(LoadTrailerRequest request, CancellationToken ct, bool validateById = false)
        {
            TrailerAssignment trailerAssignmentToCheckOut = null;
            TrailerAppointment appointmentToComplete = null;
            var appointment = await ValidateAppointmentAsync(request.AccountId, request.TrailerAppointmentId, ct);
            var dock = await ValidateDockAsync(request.AccountId, request.DockSerialNumber, ct);

            await ValidatePrefixAsync(appointment, request.Prefix, ct);

            if (request.TrailerAssignmentId.HasValue)
                (trailerAssignmentToCheckOut, appointmentToComplete) = await ValidateTrailerAssignmentForCheckOutAsync(request, ct, validateById);
            else
            {
                if (!validateById)
                     ValidateTrailerNumber(request.TrailerNumber);
                else
                     ValidateTrailerId(request.TrailerId, request.AccountId, ct);
            }

            return (appointment, trailerAssignmentToCheckOut, appointmentToComplete, dock);
        }

the bool value must be optional

is this correct
        private void SetupValidationResult(TrailerAppointment appointment, TrailerAssignment trailerAssignmentToCheckOut,
            TrailerAppointment appointmentToComplete, AccountDevice device)
            => _mocker.Setup<ILoadTrailerUseCaseValidationService, Task<(TrailerAppointment, TrailerAssignment, TrailerAppointment, AccountDevice)>>(_ =>
                _.ValidateRequestAsync(_request, _ct, It.IsAny<bool>())
            .ReturnsAsync((appointment, trailerAssignmentToCheckOut, appointmentToComplete, device));


When request.TrailerId has value AND request.TrailerAssignmentId has value - exception should be thrown
                var exsistingTrailer = await _queryInfrastructureService.GetTrailerByTrailerIdAsync(request.TrailerId, ct);

                if (exsistingTrailer == null && request.TrailerAssignmentId.HasValue)
                    throw new LoadTrailerByIdInvalidTrailerAssignmentDataException("Only Trailer Id or Trailer AssignmentId can be provided at the same time.");



polish it
it is used in validation to validate 
 if (request.TrailerId > 0 && request.TrailerAssignmentId.HasValue)
and  ValidateTrailerId(request.TrailerId, request.AccountId, ct);


11 Trailer Assignment Process

explain me this
When performing Load Trailer User must select a trailer:

utilizing existing Trailer (User should be able to see Trailer number, Carrier, Location):  

a trailer in In-Yard new Trailer status & In Yard Trailer assignment status

the following Trailers should not be available for a User to select (greyed out):

a trailer in In-Yard new Trailer status & Awaiting Dock Trailer assignment status

a trailer in In-Yard new Trailer status & Awaiting Dock - Pending Check-In Trailer assignment status

a trailer in In-Yard new Trailer status & Dock Assigned - Pending Check-In Trailer assignment status

a trailer in In-Yard new Trailer status & Dock Assigned - Dock Assigned Trailer assignment status

a trailer in At Dock new Trailer status & In Progress Trailer assignment status

Add: enter trailer number (same UX as the carrier input) 

a trailer in Checked Out new Trailer status & Checked Out Trailer assignment status should not be displayed for User selection but allowed to be entered as “new” while the same Trailer record is going to be used behind the scenes

Selection of In-Yard Trailer should do the following:

Checkout the other trailer assignment (like User does on the daily schedule manually - need to do this automatically behind the scenes) 

Assign the trailer to the new trailer assignment 

Selection of ‘Add Trailer’ should:

check if this Trailer has been in the Facility before - if yes:

update Checked out Trailer status to In Yard

if it is a new Trailer:

create a new Trailer record using Carrier & Trailer number

On confirmation of manual check-in action, assign the trailer to the new trailer assignment

The action should still support a ‘tandem’ use case as today 


12 TrailerAssignment Checkout Validation

write checkin comment in 1 line 
Both the TrailerId and TrailerAssignmentId are mandatory in the request.
TrailerId (in the request) is the Trailer we are linking to the new TrailerAssignment TA1
TrailerAssignmentId say TA0 (in the request) - is the assignment we are checking out

Need to validate the TrailerId as below:
Need to add validations around the TrailerEntity(TrailerId from the request) and associated TrailerAssignment(if any) states. Here the TrailerAssignment is TA2
Means TrailerId passed is allowed for load-trailer Action
(Example if TrailerEntity = AtDock, TrailerAssignment = InProgress we should not allow that trailer for loading)
Refer the TrailerAssignmentValidationService.ValidateTrailerAsync
If the above condition is failed then throw error
If the above condition is good then we need to checkout the TrailerAssignment TA2 linked with the TrailerId as mentioned below.

Create a new TrailerAssignment say TA1 (with TrailerNumber = Preffix + TrailerEntity.TrailerNumber)

Update the Appointment Outbound TrailerAssignmentId with the TA1

New logic update the TrailerEntity->TrailerAssignementId with TA1

Post a message to Checkout TrailerAssignmentId TA0

Post a message to Checkout TrailerAssignmentId TA2