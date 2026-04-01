also adding this 
here is the TailerTaskSortingConfiguration
using Docks.Common.Filtering;
using Docks.Infrastructure.Core.Sql.Common;
using Docks.Infrastructure.Models.Sql.DataModels.Tasks;
using System.Linq;

namespace Docks.Infrastructure.Sql.Common
{
    public class TrailerTaskSortingConfiguration : DefaultSqlSortingConfiguration<TrailerTaskModel>,
        ISqlSortingConfiguration<TrailerTaskModel>
    {
        private readonly string PrioritySortKey = $"{nameof(TrailerTaskModel.AssignedPriority)}";

        public TrailerTaskSortingConfiguration()
        {
        }

        public override IQueryable<TrailerTaskModel> ApplyCustomSorting(IQueryable<TrailerTaskModel> query, SortingModel sorting)
            => sorting.SortBy == PrioritySortKey
                ? SortByPriority(query, sorting)
                : base.ApplyCustomSorting(query, sorting);

        private static IQueryable<TrailerTaskModel> SortByPriority(IQueryable<TrailerTaskModel> query, SortingModel sorting)
            => sorting.Order == Docks.Common.Filtering.Enums.SortOrder.Asc
                ? query.OrderBy(x => x.AssignedPriority).ThenBy(x => x.TaskDueDatetimeUtc)
                : query.OrderByDescending(x => x.AssignedPriority).ThenByDescending(x => x.TaskDueDatetimeUtc);
    }
}

and its model

using Docks.Domain.Enums;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments;
using Docks.Infrastructure.Models.Sql.DataModels.Trailers;
using System;
using System.Collections.Generic;

namespace Docks.Infrastructure.Models.Sql.DataModels.Tasks
{
    public class TrailerTaskModel
    {
        public Guid TrailerTaskId { get; set; }

        public Guid AccountId { get; set; }

        public Guid? TrailerAssignmentId { get; set; }

        public int? TrailerId { get; set; }

        public int? AssignedPriority { get; set; }

        public int? CompletedPriority { get; set; }

        public Guid? SourceLocationId { get; set; }

        public Guid? TargetLocationId { get; set; }

        public Guid? AssignedAccountUserId { get; set; }

        public int TaskStatusId { get; set; }

        public Importance Importance { get; set; }

        public string Comment { get; set; }

        public DateTime? TaskDueDatetimeUtc { get; set; }

        public Guid CreatedByUserId { get; set; }

        public Guid LastUpdatedByUserId { get; set; }

        public DateTime CreatedDateTimeUtc { get; set; }

        public DateTime LastUpdatedDatetimeUtc { get; set; }

        public TaskStatusModel TaskStatus { get; set; }

        public TrailerAssignmentModel TrailerAssignment { get; set; }

        public TrailerModel Trailer { get; set; }

        public TrailerAssignmentLocationModel SourceLocation { get; set; }

        public TrailerAssignmentLocationModel TargetLocation { get; set; }

        public List<TaskStatusHistoryModel> TaskStatusHistories { get; set; }
    }
}






--------------------------------------------------


using Chamberlain.Middleware.Core.Sql;
using Chamberlain.Middleware.Infrastructure.Sql;
using Docks.Infrastructure.Sql.Configurations;
using Docks.Infrastructure.Sql.Configurations.Carriers;
using Docks.Infrastructure.Sql.Configurations.Docks;
using Docks.Infrastructure.Sql.Configurations.Docks.DockSessions;
using Docks.Infrastructure.Sql.Configurations.Docks.Sensors;
using Docks.Infrastructure.Sql.Configurations.DockPlans;
using Docks.Infrastructure.Sql.Configurations.Drivers;
using Docks.Infrastructure.Sql.Configurations.Partners;
using Docks.Infrastructure.Sql.Configurations.Rules;
using Docks.Infrastructure.Sql.Configurations.Tags;
using Docks.Infrastructure.Sql.Configurations.Touchpoints;
using Docks.Infrastructure.Sql.Configurations.TrailerAppointments;
using Docks.Infrastructure.Sql.Configurations.TrailerAppointments.Import;
using Docks.Infrastructure.Sql.Configurations.TrailerAppointments.Settings;
using Docks.Infrastructure.Sql.Configurations.TrailerAssignments;
using Docks.Infrastructure.Sql.Configurations.TrailerSessions;
using Docks.Infrastructure.Sql.Configurations.TrailerStatuses;
using Docks.Infrastructure.Sql.Configurations.Vendors;
using Docks.Infrastructure.Sql.Configurations.Yards;
using Microsoft.EntityFrameworkCore;
using Docks.Infrastructure.Sql.Configurations.SessionDefinitions;
using Chamberlain.MyQBusiness.Docks.Infrastructure.Sql.EFConfigurations;
using Docks.Infrastructure.Sql.Configurations.Faults;
using Docks.Infrastructure.Sql.Configurations.Conversations;
using Docks.Infrastructure.Sql.Configurations.CarrierTractorNumbers;
using Docks.Infrastructure.Sql.Configurations.Workflows;
using Docks.Infrastructure.Sql.Configurations.AssignmentModes;
using Docks.Infrastructure.Sql.Configurations.Jockeys;
using Docks.Infrastructure.Sql.Configurations.Trailers;
using Docks.Infrastructure.Sql.Configurations.Tasks;
using Docks.Infrastructure.Sql.Configurations.DockProperties;

namespace Docks.Infrastructure.Sql.Common.Contexts
{
    public abstract class DocksContext : BaseSqlContext
    {
        protected DocksContext(ISqlConnectionStringProvider sqlConnectionStringProvider) : base(sqlConnectionStringProvider) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DockContainerModelConfiguration());
            modelBuilder.ApplyConfiguration(new SensorTypeModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockSessionModelConfiguration());
            modelBuilder.ApplyConfiguration(new FaultHistoryModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockSessionEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new CarrierModelConfiguration());
            modelBuilder.ApplyConfiguration(new CarrierTractorNumberModelConfiguration());
            modelBuilder.ApplyConfiguration(new BlockModelConfiguration());
            modelBuilder.ApplyConfiguration(new FaultCodeModelConfiguration());
            modelBuilder.ApplyConfiguration(new IdockRegistryModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPlanModelConfiguration());
            modelBuilder.ApplyConfiguration(new DriverModelConfiguration());
            modelBuilder.ApplyConfiguration(new DriverConversationModelConfiguration());
            modelBuilder.ApplyConfiguration(new DriverStatusModelConfiguration());
            modelBuilder.ApplyConfiguration(new ArrivalActionModelConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerCalloutModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleArrivalSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleAutoAssignmentSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleDockModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleValueModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleYardModelConfiguration());
            modelBuilder.ApplyConfiguration(new TagModelConfiguration());
            modelBuilder.ApplyConfiguration(new TouchpointModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentImportBatchModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentImportErrorModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentImportModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentImportSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentDoorSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentLimitSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentCheckInHoursSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentSettingImageModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentDriverModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentDeleteAccountWhitelistModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentDriverStatusHistoryModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentGuestEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentStatusHistoryModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentTagModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAssignmentModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAssignmentIdentifierLineItemModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAssignmentIdentifierLineItemQuantityModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAssignmentIdentifierModelConfiguration());
            modelBuilder.ApplyConfiguration(new UnitOfMeasureModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerSessionAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerSessionEventModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerSessionModelConfiguration());
            modelBuilder.ApplyConfiguration(new VendorModelConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerVendorModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerStatusHistoryModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerStatusModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerEntityStatusModelConfiguration());
            modelBuilder.ApplyConfiguration(new YardLotModelConfiguration());
            modelBuilder.ApplyConfiguration(new YardSpaceModelConfiguration());
            modelBuilder.ApplyConfiguration(new YardLocationTypeModelConfiguration());
            modelBuilder.ApplyConfiguration(new SensorModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockSensorAssignmentModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAppointmentStatusModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerReasonModelConfiguration());
            modelBuilder.ApplyConfiguration(new ReasonModelConfiguration());
            modelBuilder.ApplyConfiguration(new SessionDefinitionModelConfiguration());
            modelBuilder.ApplyConfiguration(new SessionDefinitionBypassApproverModelConfiguration());
            modelBuilder.ApplyConfiguration(new SessionDefinitionStepModelConfiguration());
            modelBuilder.ApplyConfiguration(new SessionDefinitionZoneModelConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerEventSubscriptionModelConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerModelConfiguration());
            modelBuilder.ApplyConfiguration(new PartnerDockSettingModelConfiguration());
            modelBuilder.ApplyConfiguration(new ConversationSimSmsModelConfiguration());
            modelBuilder.ApplyConfiguration(new DriverCheckInModelConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowDriverCarrierResponseModelConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowDriverCustomResponseModelConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowDriverCustomQuestionModelConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowSettingsModelConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowLanguageSettingsModelConfiguration());
            modelBuilder.ApplyConfiguration(new AppointmentPanelStatusMonitoringModelConfiguration());
            modelBuilder.ApplyConfiguration(new RuleRemainingYardModelConfiguration());
            modelBuilder.ApplyConfiguration(new AssignmentModeModelConfiguration());
            modelBuilder.ApplyConfiguration(new AccountUserJockeyModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerTasksModelConfiguration());
            modelBuilder.ApplyConfiguration(new TaskStatusHistoryModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerAssignmentLocationsModelConfiguration());
            modelBuilder.ApplyConfiguration(new TaskStatusModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockAccountPropertyModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPropertiesListValueModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPropertyCategoryModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPropertyDataTypeModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPropertyModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPropertyValidationRuleModelConfiguration());
            modelBuilder.ApplyConfiguration(new DockPropertyValidationRuleTypeModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerSettingsModelConfiguration());
            modelBuilder.ApplyConfiguration(new TrailerPropertiesValueModelConfiguration());

            base.OnModelCreating(modelBuilder);
        }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                Microsoft.Extensions.Logging.ConsoleLoggerExtensions.AddConsole(builder);
                Microsoft.Extensions.Logging.FilterLoggingBuilderExtensions.AddFilter(builder,
                    (category, logLevel) => category == DbLoggerCategory.Database.Command.Name
                                            && logLevel == Microsoft.Extensions.Logging.LogLevel.Information);
            });

            optionsBuilder
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(loggerFactory);
        }
#endif
    }
}

#region Assembly Chamberlain.Middleware.Infrastructure.Sql, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\Kamarali\.nuget\packages\chamberlain.middleware.infrastructure.sql\8.0.1.118\lib\net8.0\Chamberlain.Middleware.Infrastructure.Sql.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using Chamberlain.Middleware.Core.Exceptions;
using Chamberlain.Middleware.Core.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Chamberlain.Middleware.Infrastructure.Sql;

public abstract class BaseSqlContext : DbContext
{
    private readonly ISqlConnectionStringProvider _connectionStringProvider;

    protected BaseSqlContext(ISqlConnectionStringProvider sqlConnectionStringProvider)
    {
        _connectionStringProvider = sqlConnectionStringProvider ?? throw new ArgumentException("sqlConnectionStringProvider");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _connectionStringProvider.GetConnectionString(GetType().Name) ?? throw new MyQApplicationConfigurationException("Unable to find connection string: " + GetType().Name);
        optionsBuilder.UseSqlServer(connectionString, delegate (SqlServerDbContextOptionsBuilder builder)
        {
            builder.EnableRetryOnFailure();
        });
        base.OnConfiguring(optionsBuilder);
    }
}
#if false // Decompilation log
'265' items in cache
------------------
Resolve: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.dll'
------------------
Resolve: 'Microsoft.EntityFrameworkCore, Version=8.0.7.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.EntityFrameworkCore, Version=8.0.7.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\Kamarali\.nuget\packages\microsoft.entityframeworkcore\8.0.7\lib\net8.0\Microsoft.EntityFrameworkCore.dll'
------------------
Resolve: 'Chamberlain.Middleware.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Chamberlain.Middleware.Core, Version=0.1.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\Kamarali\.nuget\packages\chamberlain.middleware.core\8.0.1.118\lib\net8.0\Chamberlain.Middleware.Core.dll'
------------------
Resolve: 'Microsoft.EntityFrameworkCore.SqlServer, Version=8.0.7.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.EntityFrameworkCore.SqlServer, Version=8.0.7.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\Kamarali\.nuget\packages\microsoft.entityframeworkcore.sqlserver\8.0.7\lib\net8.0\Microsoft.EntityFrameworkCore.SqlServer.dll'
------------------
Resolve: 'Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\Kamarali\.nuget\packages\microsoft.extensions.dependencyinjection.abstractions\8.0.1\lib\net8.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll'
------------------
Resolve: 'Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5'
Found single assembly: 'Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5'
Load from: 'C:\Users\Kamarali\.nuget\packages\microsoft.data.sqlclient\5.2.1\ref\net8.0\Microsoft.Data.SqlClient.dll'
------------------
Resolve: 'Microsoft.Extensions.Configuration.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.Extensions.Configuration.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\Kamarali\.nuget\packages\microsoft.extensions.configuration.abstractions\8.0.0\lib\net8.0\Microsoft.Extensions.Configuration.Abstractions.dll'
------------------
Resolve: 'System.Collections.Concurrent, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections.Concurrent, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Collections.Concurrent.dll'
------------------
Resolve: 'System.Data.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Data.Common, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Data.Common.dll'
------------------
Resolve: 'Microsoft.EntityFrameworkCore.Abstractions, Version=8.0.7.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.EntityFrameworkCore.Abstractions, Version=8.0.7.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\Kamarali\.nuget\packages\microsoft.entityframeworkcore.abstractions\8.0.7\lib\net8.0\Microsoft.EntityFrameworkCore.Abstractions.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.InteropServices, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.14\ref\net8.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif


will explore Chamberlain.Middleware.Core in some time