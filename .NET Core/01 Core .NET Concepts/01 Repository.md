### Overview of Code Implementation

Your codebase follows a layered architecture typical of enterprise applications, drawing from Domain-Driven Design (DDD) principles. Here's a high-level breakdown:

1. **Domain Layer**: Contains the core business entities (models) that represent concepts like trailer assignments, tasks, etc. These are plain C# classes with properties and navigation relationships, without infrastructure concerns (e.g., no EF Core dependencies here; those are in configurations).

2. **Contracts (Interfaces)**: Define abstractions for repositories, sorting configurations, and other services. These are placed in core namespaces (e.g., `Docks.Infrastructure.Core.Sql.Common`) to allow loose coupling between layers. Interfaces ensure that infrastructure implementations can be swapped (e.g., for testing).

3. **Infrastructure Layer**: Implements the contracts using Entity Framework Core (EF Core) for SQL data access. This includes:
   - Repositories: Handle CRUD and queries.
   - Contexts: Manage database connections and model configurations.
   - Sorting Configurations: Implement sorting logic using lambdas for EF Core queries.

4. **Filters and Sorting Usage**:
   - **Filters**: Use `Expression<Func<TEntity, bool>> predicate` in repository methods (e.g., `GetListAsync`) to apply WHERE clauses dynamically. Filters are built using LINQ expressions for flexibility (e.g., `x => x.AssignedPriority > 5`).
   - **Sorting**: Uses `SortingModel` (with `SortBy` and `Order`) passed to repository methods like `ApplySorting`. The repository delegates to an injected `ISqlSortingConfiguration<TEntity>` (e.g., `DailyScheduleSortingConfiguration`), which maps `SortBy` to a lambda expression (e.g., `x => x.AssignedPriority`). This lambda is applied to the `IQueryable` using `OrderBy` or `OrderByDescending`.
   - **Pagination**: Uses `PagingModel` (with `Offset` and `Limit`) in `ApplyPaging`, applying `Skip` and `Take` to the sorted query.
   - **Integration**: In repositories like `GenericReadOnlySqlRepository`, `GetListAsync` chains filtering (`Where(predicate)`), sorting (`ApplySorting`), and pagination (`ApplyPaging`), then executes with `ToListAsync`.

5. **Dependency Injection (DI)**: In `SqlDependencies.cs`, dependencies are registered (e.g., repositories with contexts, sorting configs). This allows runtime resolution, supporting mocks in testing modes.

Below, I’ll show the code from all relevant files, categorized by layer. I’ve compiled them from the snippets you provided throughout the conversation.

### 1. Domain Layer: Models
These are entity classes representing business concepts.

#### TrailerAssignmentModel (Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments)
```csharp
using System;
using System.Collections.Generic;
using Docks.Domain.Enums;
using Docks.Infrastructure.Models.Sql.DataModels.Carriers;
using Docks.Infrastructure.Models.Sql.DataModels.DockPlans;
using Docks.Infrastructure.Models.Sql.DataModels.Drivers;
using Docks.Infrastructure.Models.Sql.DataModels.Touchpoints;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAppointments;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerReasons;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerSessions;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerStatuses;
using Docks.Infrastructure.Models.Sql.DataModels.AssignmentModes;
using Docks.Infrastructure.Models.Sql.DataModels.Yards;
using Docks.Infrastructure.Models.Sql.DataModels.Trailers;
using Docks.Infrastructure.Models.Sql.DataModels.Tasks;

namespace Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments
{
    public class TrailerAssignmentModel
    {
        public Guid TrailerAssignmentId { get; set; }

        public Guid AccountId { get; set; }

        public string SerialNumber { get; set; }

        public string TrailerNumber { get; set; }

        public string SealNumber { get; set; }

        public string DockName { get; set; }

        public LoadType? LoadType { get; set; }

        public ShipmentType? ShipmentType { get; set; }

        public ProductType? ProductType { get; set; }

        public string Comments { get; set; }

        public bool Contacted { get; set; }

        public bool? EmptyTrailer { get; set; }

        public int? AssignedPriority { get; set; }

        public int? CompletedPriority { get; set; }

        public int CarrierId { get; set; }

        public decimal CarrierDetentionFee { get; set; }

        public decimal DemurrageFee { get; set; }

        public Guid? ParentTrailerAssignmentId { get; set; }

        public bool TimeLimitEnabled { get; set; }

        public TimeSpan? TimeLimitInterval { get; set; }

        public bool TimeLimitWarningEnabled { get; set; }

        public TimeSpan? TimeLimitWarningInterval { get; set; }

        public int? TrailerStatusId { get; set; }

        public DateTime? TrailerStatusDatetimeUtc { get; set; }

        public Guid? InboundYardSpaceId { get; set; }

        public Guid? OutboundYardSpaceId { get; set; }

        public int? TouchPointId { get; set; }
     
        public DateTime? CheckInDatetimeUtc { get; set; }

        public DateTime? CheckOutDatetimeUtc { get; set; }

        public DateTime? DockAssignedDatetimeUtc { get; set; }

        public int? DemurrageTimeLimitInterval { get; set; }

        public int? DemurrageTimeLimitWarningInterval { get; set; }

        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedDatetimeUtc { get; set; }

        public Guid? LastUpdatedByUserId { get; set; }

        public DateTime? LastUpdatedDatetimeUtc { get; set; }

        public decimal? ActualTemperature { get; set; }

        public decimal? SetTemperature { get; set; }

        public bool ManuallyCreated { get; set; }

        public int? DriverConversationId { get; set; }

        public byte? AssignmentMode { get; set; }

        public int? TrailerId { get; set; }

        public AssignmentModeModel AssignmentModeModel { get; set; }

        public DriverConversationModel DriverConversation { get; set; }

        public CarrierModel Carrier { get; set; }

        public TrailerAssignmentModel ParentTrailerAssignment { get; set; }

        public TouchpointModel Touchpoint { get; set; }

        public YardSpaceModel InboundYardSpace { get; set; }

        public YardSpaceModel OutboundYardSpace { get; set; }

        public TrailerStatusModel TrailerStatus { get; set; }

        public List<TrailerStatusHistoryModel> TrailerStatusHistory { get; set; }

        public List<TrailerAssignmentIdentifierModel> Identifiers { get; set; }

        public List<TrailerAppointmentModel> TrailerAppointments { get; set; }

        public List<TrailerSessionAssignmentModel> TrailerSessionAssignments { get; set; }

        public List<DockPlanModel> DockPlans { get; set; }

        public List<TrailerReasonModel> TrailerReasons { get; set; }

        public List<TrailerAssignmentLocationModel> Locations { get; set; }

        public Guid? CheckedInTrailerAppointmentId { get; set; }

        public Guid? CheckedOutTrailerAppointmentId { get; set; }        
    }
}
```

#### TrailerTaskModel (Docks.Infrastructure.Models.Sql.DataModels.Tasks)
```csharp
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
```

#### TrailerAssignmentSortModel (Docks.Infrastructure.Core.Sql.Models.SortModels)
```csharp
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments;
using Docks.Infrastructure.Sql.Common;
using System;

namespace Docks.Infrastructure.Core.Sql.Models.SortModels
{
    public class TrailerAssignmentSortModel : ISortModel<TrailerAssignmentModel>
    {
        public Guid TrailerAssignmentId { get; set; }

        public string DockName { get; set; }
    }
}
```

### 2. Contracts: Interfaces
These define abstractions for repositories and sorting.

#### IGenericSqlRepository (Docks.Infrastructure.Core.Sql.Common.Repositories)
```csharp
public interface IGenericSqlRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);
    EntityEntry<TEntity> Attach(TEntity entity);
    void Update(TEntity entity);
    void UpdateProperty<TProperty>(TEntity entity, params Expression<Func<TEntity, TProperty>>[] properties);
    void UpdateProperties(TEntity entity, params Expression<Func<TEntity, object>>[] properties);
    void AddOrUpdate(TEntity entity);
    void Remove(TEntity entity);
    Task<IList<TEntity>> ExecuteProcedure(string name, CancellationToken ct, params object[] parameters);
    Task CommitAsync(CancellationToken ct);
    TEntity GetTracked(Func<TEntity, bool> predicate);
    void ClearTrackedEntities();
}
```

#### IGenericReadOnlySqlRepository (Docks.Infrastructure.Core.Sql.Common.Repositories)
```csharp
public interface IGenericReadOnlySqlRepository<TEntity> where TEntity : class
{
    Task<IList<TEntity>> GetListAsync(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default);

    Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default);

    Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, PagingModel paging, SortingModel sorting, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default);

    IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate);

    Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default);

    Task<TEntity> GetLastAsync<TProperty>(Expression<Func<TEntity, TProperty>> keySelector, Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default);

    void ApplySortingConfig(ISqlSortingConfiguration<TEntity> sortingConfig, SortOrder sortOrder);

    IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query, PagingModel paging, SortingModel sorting);

    IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, SortingModel sorting);

    IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting) where TSortEntity : ISortModel<TEntity>;
}
```

#### ISqlSortingConfiguration (Docks.Infrastructure.Core.Sql.Common)
```csharp
using Docks.Common.Filtering.Enums;
using Docks.Infrastructure.Sql.Common;
using System;
using System.Linq.Expressions;

namespace Docks.Infrastructure.Core.Sql.Common
{
    public interface ISqlSortingConfiguration<TEntity>
    {
        SortOrder SortOrder { get; set; }

        (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector);

        (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector) where TSortEntity : ISortModel<TEntity>;
    }
}
```

#### ISqlDailyScheduleSortingConfiguration (Docks.Infrastructure.Core.Sql.Common)
```csharp
namespace Docks.Infrastructure.Core.Sql.Common
{
    public interface ISqlDailyScheduleSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
    {
    }
}
```

#### ISqlTrailersSortingConfiguration (Docks.Infrastructure.Core.Sql.Common)
```csharp
namespace Docks.Infrastructure.Core.Sql.Common
{
    public interface ISqlTrailersSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
    {
    }
}
```

#### ISortModel (Docks.Infrastructure.Sql.Common)
```csharp
namespace Docks.Infrastructure.Sql.Common
{
    public interface ISortModel<TEntity>
    {
    }
}
```

### 3. Infrastructure Layer: Repositories, Contexts, and Sorting Configurations

#### GenericReadOnlySqlRepository (Docks.Infrastructure.Sql.Common.Repositories)
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Chamberlain.Middleware.Infrastructure.Sql;
using Docks.Common.Exceptions.Filtering;
using Docks.Common.Extensions;
using Docks.Common.Filtering;
using Docks.Common.Filtering.Enums;
using Docks.Infrastructure.Core.Sql.Common;
using Docks.Infrastructure.Core.Sql.Common.Repositories;
using Docks.Infrastructure.Sql.Common.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Docks.Infrastructure.Sql.Common.Repositories
{
    public class GenericReadOnlySqlRepository<TEntity> : BaseSqlRepository<ReadOnlyDocksContext>,
        IGenericReadOnlySqlRepository<TEntity> where TEntity : class
    {
        private ISqlSortingConfiguration<TEntity> _sortingConfiguration;

        public GenericReadOnlySqlRepository(ReadOnlyDocksContext context,
            ISqlSortingConfiguration<TEntity> sortingConfiguration) : base(context)
        {
            _sortingConfiguration = sortingConfiguration;
        }

        public async Task<IList<TEntity>> GetListAsync(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            CancellationToken ct = default)
        {
            var query = ApplyInclude(AsNoTracking(), include);

            var result = await query.ToListAsync(ct);

            return result;
        }

        public virtual async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var query = ApplyInclude(AsNoTracking(), include).Where(predicate);

            var result = await query.ToListAsync(ct);

            return result;
        }

        public virtual async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, PagingModel paging, SortingModel sorting,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var query = ApplyInclude(AsNoTracking(), include).Where(predicate);

            query = ApplySorting(query, sorting);

            query = ApplyPaging(query, paging, sorting);

            var result = await query.ToListAsync(ct);

            return result;
        }

        public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate) => AsNoTracking().Where(predicate);

        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) => await AsNoTracking().CountAsync(predicate, ct);

        public virtual async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var query = ApplyInclude(AsNoTracking(), include).Where(predicate);

            var result = await query.FirstOrDefaultAsync(ct);

            return result;
        }

        public async Task<TEntity> GetLastAsync<TProperty>(Expression<Func<TEntity, TProperty>> keySelector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var query = ApplyInclude(AsNoTracking(), include)
                .Where(predicate)
                .OrderByDescending(keySelector);

            var result = await query.FirstOrDefaultAsync(ct);

            return result;
        }

        public void ApplySortingConfig(ISqlSortingConfiguration<TEntity> sortingConfig, SortOrder sortOrder)
        {
            _sortingConfiguration = sortingConfig;
            _sortingConfiguration.SortOrder = sortOrder;
        }

        public IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, SortingModel sorting)
        {
            if (!SortingIsSpecified(sorting))
                return query;

            var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);

            return ApplySorting(query, sorting, sortExpr, sortFieldType);
        }

        public IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting) where TSortEntity : ISortModel<TEntity>
        {
            if (!SortingIsSpecified(sorting)) return query;

            var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector<TSortEntity>(sorting.SortBy);

            return ApplySorting(query, sorting, sortExpr, sortFieldType);
        }

        public IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query, PagingModel paging, SortingModel sorting)
        {
            if (paging == null || (!paging.Limit.HasValue && paging.Offset = 0))
                return query;

            if (paging.Limit.GetValueOrDefault() <= 0)
                throw new InvalidPagingLimitException($"Limit for pagination should be greater than 0");

            if (!SortingIsSpecified(sorting))
                throw new PaginationNotSupportedException("Sql pagination supported only together with sql sorting.");

            query = query.Skip(paging.Offset);

            return paging.Limit.HasValue ? query.Take(paging.Limit.Value) : query;
        }

        #region Private methods

        private IQueryable<TEntity> AsQueryable() => Context.Set<TEntity>();

        private IQueryable<TEntity> AsNoTracking() => AsQueryable().AsNoTracking();

        private static IQueryable<TEntity> ApplyInclude(IQueryable<TEntity> query,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include)
        {
            if (include == null) return query;

            query = include(query);

            return query;
        }

        private static bool SortingIsSpecified(SortingModel sorting) => sorting != null && !string.IsNullOrEmpty(sorting.SortBy);

        private static IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting, LambdaExpression sortExpr, Type sortFieldType)
        {
            if (sortExpr != null)
            {
                return query.OrderBy(sortExpr, sortFieldType, sorting.Order);
            }

            throw new SortingNotSupportedException(
                $"Specified sorting not supported. Type ({typeof(TEntity)}), " +
                $"Field ({sorting.SortBy}), Order ({sorting.Order}).");
        }

        #endregion
    }
}
```

#### GenericReadOnlySqlRepositoryMock (Docks.Infrastructure.Sql.Common.Repositories)
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Docks.Infrastructure.Models.Sql.DataModels.Drivers;
using Docks.Infrastructure.Sql.Common.Contexts;
using Microsoft.Extensions.Caching.Memory;

namespace Docks.Infrastructure.Sql.Common.Repositories
{
    public class GenericReadOnlySqlRepositoryMock<TEntity> : GenericReadOnlySqlRepository<TEntity>
        where TEntity : class
    {
        private readonly IMemoryCache _cache;

        public GenericReadOnlySqlRepositoryMock(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration,
            IMemoryCache cache) : base(context, sortingConfiguration)
        {
            _cache = cache;
        }

        public override async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var key = $"{nameof(GenericSqlRepository<TEntity>)}.{nameof(GetListAsync)}.{predicate}.{include}";
            await Task.Delay(10, ct);
            return await _cache.GetOrCreateAsync(key, async e => await base.GetListAsync(predicate, include, ct));
        }

        public override async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, PagingModel paging, SortingModel sorting,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var key = $"{nameof(GenericSqlRepository<TEntity>)}.{nameof(GetListAsync)}.{predicate}.{include}.{paging.Limit}_{paging.Offset}.{sorting.SortBy}_{sorting.Order}";
            await Task.Delay(10, ct);
            return await _cache.GetOrCreateAsync(key, async e => await base.GetListAsync(predicate, paging, sorting, include, ct));
        }

        public override async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null, CancellationToken ct = default)
        {
            var key = $"{nameof(GenericSqlRepository<TEntity>)}.{nameof(GetFirstAsync)}.{predicate}.{include}";
            await Task.Delay(10, ct);
            return await _cache.GetOrCreateAsync(key, async e => await base.GetFirstAsync(predicate, include, ct));
        }
    }
}
```

#### GenericSqlRepository (Docks.Infrastructure.Sql.Common.Repositories)
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Chamberlain.Middleware.Infrastructure.Sql;
using Docks.Infrastructure.Core.Sql.Common.Repositories;
using Docks.Infrastructure.Sql.Common.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Docks.Infrastructure.Sql.Common.Repositories
{
    public class GenericSqlRepository<TEntity> : BaseSqlRepository<WritableDocksContext>,
        IGenericSqlRepository<TEntity> where TEntity : class
    {
        public GenericSqlRepository(WritableDocksContext context) : base(context) { }

        public virtual void Add(TEntity entity) => Context.Set<TEntity>().Add(entity);

        public EntityEntry<TEntity> Attach(TEntity entity) => Context.Set<TEntity>().Attach(entity);

        public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

        public void UpdateProperty<TProperty>(TEntity entity, params Expression<Func<TEntity, TProperty>>[] properties)
        {
            var entry = Context.Set<TEntity>().Attach(entity);
            foreach (var prop in properties)
                entry.Property(prop).IsModified = true;
        }

        public void UpdateProperties(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
        {
            var entry = Context.Set<TEntity>().Attach(entity);
            foreach (var prop in properties)
                entry.Property(prop).IsModified = true;
        }

        public void AddOrUpdate(TEntity entity)
        {
            if (Context.Set<TEntity>().Contains(entity))
                Context.Set<TEntity>().Update(entity);
            else
                Context.Set<TEntity>().Add(entity);
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Attach(entity);
            Context.Set<TEntity>().Remove(entity);
        }

        public async Task<IList<TEntity>> ExecuteProcedure(string name, CancellationToken ct, params object[] parameters)
        {
            var query = FormattableStringFactory.Create($"exec {name}", parameters);
            var result = Context.Set<TEntity>().FromSql(query);
            return await result.ToListAsync(ct);
        }

        public virtual async Task CommitAsync(CancellationToken ct) => await Context.SaveChangesAsync(ct);

        public TEntity GetTracked(Func<TEntity, bool> predicate)
            => Context.ChangeTracker.Entries<TEntity>().Select(e => e.Entity).FirstOrDefault(predicate);

        public void ClearTrackedEntities() => Context.ChangeTracker.Clear();
    }
}
```

#### GenericSqlRepositoryMock (Docks.Infrastructure.Sql.Common.Repositories)
```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docks.Infrastructure.Models.Sql.DataModels.Drivers;
using Docks.Infrastructure.Sql.Common.Contexts;

namespace Docks.Infrastructure.Sql.Common.Repositories
{
    public class GenericSqlRepositoryMock<TEntity> : GenericSqlRepository<TEntity>
        where TEntity : class
    {
        private static readonly Random IdGenerator = new Random();
        private static readonly Dictionary<Type, Action<TEntity>> OnAdd = new Dictionary<Type, Action<TEntity>>
        {
            {typeof(DriverConversationModel), (TEntity _) => (_ as DriverConversationModel).DriverConversationId = IdGenerator.Next()}
        };

        public GenericSqlRepositoryMock(WritableDocksContext context) : base(context) { }

        public override void Add(TEntity entity)
        {
            if(OnAdd.TryGetValue(typeof(TEntity), out var onAdd))
                onAdd(entity);
            Context.Set<TEntity>().Add(entity);
        }

        public override async Task CommitAsync(CancellationToken ct) => await Task.Delay(10, ct);
    }
}
```

#### DocksContext (Docks.Infrastructure.Sql.Common.Contexts)
```csharp
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
```

#### ReadOnlyDocksContext and WritableDocksContext (Docks.Infrastructure.Sql.Common.Contexts)
```csharp
    public class ReadOnlyDocksContext : DocksContext
    {
        public ReadOnlyDocksContext(ISqlConnectionStringProvider provider) : base(provider) { }
    }

    public class WritableDocksContext : DocksContext
    {
        public WritableDocksContext(ISqlConnectionStringProvider provider) : base(provider) { }
    } 
```

#### BaseSqlContext (Chamberlain.Middleware.Infrastructure.Sql)
```csharp
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
```

#### DefaultSqlSortingConfiguration (Docks.Infrastructure.Sql.Common)
```csharp
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Docks.Common.Filtering.Enums;
using Docks.Infrastructure.Core.Sql.Common;

namespace Docks.Infrastructure.Sql.Common
{
    public class DefaultSqlSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
    {
        public virtual SortOrder SortOrder { get; set; }

        public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)
        {
            var type = typeof(TEntity);
            var propertyPath = keySelector.Split('.');
            var propertyType = propertyPath.Aggregate(type, (tp, key)
                => tp?.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)?.PropertyType);

            if (propertyType != null)
            {
                var parameter = Expression.Parameter(type, "x");
                var expressionBody = propertyPath.Aggregate<string, Expression>(parameter, Expression.Property);
                var sortExpression = Expression.Lambda(expressionBody, parameter);

                return (sortExpression, propertyType);
            }

            return default;
        }

        public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector) where TSortEntity : ISortModel<TEntity> => default;
    }
}
```

#### DailyScheduleSortingConfiguration (Docks.Infrastructure.Sql.Common)
```csharp
using Docks.Common.Configuration.Core.InfrastructureServices.TrailerAssignments;
using Docks.Domain.Drivers;
using Docks.Domain.Enums;
using Docks.Domain.TrailerAssignments;
using Docks.Domain.Vendors;
using Docks.Infrastructure.Core.Sql.Common;
using Docks.Infrastructure.Models.Sql.DataModels.Drivers;
using Docks.Infrastructure.Models.Sql.DataModels.Touchpoints;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAppointments;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerStatuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Docks.Infrastructure.Sql.Common
{
    public class DailyScheduleSortingConfiguration : DefaultSqlSortingConfiguration<TrailerAssignmentModel>,
        ISqlDailyScheduleSortingConfiguration<TrailerAssignmentModel>
    {
        // ... (full code as provided earlier, including sort keys, SortKeyMapper, constructor, MapSortingKeySelector, and all SortBy methods)
    }
}
```

#### TrailerAssigmentSortingConfiguration (Docks.Infrastructure.Sql.Common)
```csharp
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Docks.Common.Filtering.Enums;
using Docks.Infrastructure.Core.Sql.Common;

namespace Docks.Infrastructure.Sql.Common
{
    public class TrailerAssigmentSortingConfiguration : DefaultSqlSortingConfiguration<TrailerAssignmentModel>
    {
        // ... (full code as provided earlier, including sort keys, MapSortingKeySelector, and MapSortingKeySelector<TSortEntity>)
    }
}
```

#### TrailersSortingConfiguration (Docks.Infrastructure.Sql.Common)
```csharp
using Docks.Infrastructure.Core.Sql.Common;
using Docks.Infrastructure.Models.Sql.DataModels.Carriers;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAppointments;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments;
using Docks.Infrastructure.Models.Sql.DataModels.Trailers;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerStatuses;
using Docks.Infrastructure.Models.Sql.DataModels.Yards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Docks.Infrastructure.Sql.Common
{
    public class TrailersSortingConfiguration : DefaultSqlSortingConfiguration<TrailerModel>,
        ISqlTrailersSortingConfiguration<TrailerModel>
    {
        // ... (full code as provided earlier, including sort keys, SortKeyMapper, constructor, MapSortingKeySelector, and all SortBy methods)
    }
}
```

#### TrailerTaskSortingConfiguration (Docks.Infrastructure.Sql.Common)
```csharp
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
```

#### SqlDependencies (Docks.Dependencies.Infrastructure.Sql)
```csharp
using Chamberlain.Middleware.Infrastructure.Sql;
using Docks.Dependencies.Common;
using Docks.Dependencies.Configuration;
using Docks.Infrastructure.Core.Sql.Common;
using Docks.Infrastructure.Core.Sql.Common.Repositories;
using Docks.Infrastructure.Models.Sql.DataModels.TrailerAssignments;
using Docks.Infrastructure.Models.Sql.DataModels.Trailers;
using Docks.Infrastructure.Sql.Common;
using Docks.Infrastructure.Sql.Common.Contexts;
using Docks.Infrastructure.Sql.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Docks.Dependencies.Infrastructure.Sql
{
    public static class SqlDependencies
    {
        public static IServiceCollection RegisterSqlRepository<T>(this IServiceCollection serviceCollection, 
            IConfiguration configuration) where T : class
        {
            serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<>), typeof(DefaultSqlSortingConfiguration<>));
            serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<TrailerAssignmentModel>), typeof(TrailerAssigmentSortingConfiguration));
            serviceCollection.AddTransient(typeof(ISqlTrailersSortingConfiguration<TrailerModel>), typeof(TrailersSortingConfiguration));
            RegisterDailyScheduleSortingConfiguration(serviceCollection);


            if (CatalogExtensions.IsUnderPerformance(configuration))
            {
                serviceCollection.RegisterMemoryCache();

                SqlModule.LoadSqlRepository<
                    IGenericReadOnlySqlRepository<T>,
                    GenericReadOnlySqlRepositoryMock<T>,
                    ReadOnlyDocksContext>(serviceCollection);

                SqlModule.LoadSqlRepository<
                    IGenericSqlRepository<T>,
                    GenericSqlRepositoryMock<T>,
                    WritableDocksContext>(serviceCollection);
            }
            else
            {
                SqlModule.LoadSqlRepository<
                    IGenericReadOnlySqlRepository<T>,
                    GenericReadOnlySqlRepository<T>,
                    ReadOnlyDocksContext>(serviceCollection);

                SqlModule.LoadSqlRepository<
                    IGenericSqlRepository<T>,
                    GenericSqlRepository<T>,
                    WritableDocksContext>(serviceCollection);
            }

            return serviceCollection;
        }

        private static void RegisterDailyScheduleSortingConfiguration(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(typeof(ISqlDailyScheduleSortingConfiguration<TrailerAssignmentModel>), typeof(DailyScheduleSortingConfiguration));
            serviceCollection.RegisterDailyScheduleConfiguration();
        }
    }
}
```

### 4. Filters and Sorting Usage
- **Filters**:  
  - Implemented using `Expression<Func<TEntity, bool>> predicate` in repository methods like `GetListAsync`. This allows dynamic WHERE clauses.  
  - Example in `GenericReadOnlySqlRepository`:  
    ```csharp
    var query = AsNoTracking().Where(predicate);
    ```

- **Sorting**:  
  - Uses `SortingModel` (from `Docks.Common.Filtering`) with `SortBy` and `Order`.  
  - Repository delegates to `ISqlSortingConfiguration` via `ApplySorting` or `ApplyCustomSorting`.  
  - Example in `GenericReadOnlySqlRepository`:  
    ```csharp
    query = ApplySorting(query, sorting);
    ```

- **Pagination**:  
  - Uses `PagingModel` with `Offset` and `Limit`.  
  - Example in `GenericReadOnlySqlRepository`:  
    ```csharp
    query = ApplyPaging(query, paging, sorting); // Skip and Take
    ```

This architecture ensures the domain remains clean, contracts define behavior, and infrastructure handles implementation details like EF Core and sorting. Filters and sorting are applied in repositories to build dynamic queries.

If you need more details or code from additional files, let me know!



Thanks for the detailed write-up. What you’ve documented is internally consistent and, overall, a solid enterprise-style EF Core + DDD-inspired data access architecture. I’ll focus on **validation**, **notable strengths**, and **important observations / improvement opportunities**, rather than restating what you already explained.

---

## High-Level Assessment

✅ **Architecture correctness**
Your description accurately reflects what the code is doing. The layering, responsibilities, and interaction patterns line up cleanly:

* Domain models are persistence-aware but not persistence-dependent
* Repositories encapsulate query composition (filtering, sorting, paging)
* Sorting is externalized via configuration objects (good separation of concerns)
* DI cleanly selects implementations (mock vs real, sorting variants)

This is a **mature, scalable design** for a large EF Core codebase.

---

## Strengths Worth Calling Out

### 1. Sorting Configuration Pattern (Very Strong)

Using `ISqlSortingConfiguration<TEntity>` with:

* `MapSortingKeySelector`
* optional `TSortEntity` support
* injected per-repository

…is a **clean and extensible alternative** to:

* giant `switch` statements
* leaking sorting logic into controllers
* fragile string-based reflection everywhere

This is especially good for:

* protecting allowed sort fields
* custom composite sorts (e.g., priority + due date)
* multi-context reuse (DailySchedule vs generic trailers)

---

### 2. Repository Query Pipeline Is Predictable

The ordering of operations is correct and intentional:

```csharp
Where(predicate)
→ ApplySorting
→ ApplyPaging
→ Execute
```

You also explicitly enforce:

```csharp
Pagination requires sorting
```

This avoids unstable SQL paging — a mistake many systems make.

---

### 3. Read / Write Context Split

Using:

* `ReadOnlyDocksContext`
* `WritableDocksContext`

is an excellent choice and positions you well for:

* read replicas
* CQRS-style optimizations later
* clearer intent in code reviews

---

### 4. Expression-Based Filtering

Using `Expression<Func<TEntity, bool>>` instead of `Func<TEntity, bool>`:

* keeps queries server-side
* allows composition
* avoids accidental client evaluation

This is the correct EF Core pattern.

---

## Important Observations & Improvement Opportunities

These aren’t “bugs” so much as **things worth tightening**.

---

### 1. ❗ Bug in ApplyPaging Guard Clause

```csharp
if (paging == null || (!paging.Limit.HasValue && paging.Offset = 0))
```

🔴 **Bug**: `paging.Offset = 0` is an assignment, not a comparison.

This will:

* mutate the paging model
* always evaluate to `false`
* cause paging logic to behave incorrectly

✅ Should be:

```csharp
paging.Offset == 0
```

This is worth fixing immediately.

---

### 2. Sorting Configuration Is Mutable (Thread Safety Risk)

```csharp
private ISqlSortingConfiguration<TEntity> _sortingConfiguration;
```

and later:

```csharp
_sortingConfiguration = sortingConfig;
_sortingConfiguration.SortOrder = sortOrder;
```

Because repositories are typically **scoped**, this is probably safe today — but:

* the sorting configuration itself is mutable
* if lifetime ever changes (e.g., singleton), this will break
* `SortOrder` being stateful is unnecessary

🔧 **Recommendation**:
Make sorting configuration **stateless** and pass `SortOrder` as a method argument rather than a property.

---

### 3. DefaultSqlSortingConfiguration Reflection Is Powerful but Dangerous

```csharp
propertyPath.Aggregate(type, (tp, key) =>
    tp?.GetProperty(key, ...)?.PropertyType);
```

Pros:

* flexible
* minimal configuration

Cons:

* silent failure if property name is wrong
* runtime-only validation
* no protection against sorting by navigation collections

🔧 Recommendation:
Use this only as a fallback. For core queries, explicit mappings (as you do in DailyScheduleSortingConfiguration) are safer.

---

### 4. ISortModel<T> Is Currently a Marker Only

```csharp
public interface ISortModel<TEntity> { }
```

Right now this exists purely to constrain generic sorting. That’s fine, but:

* It doesn’t express *why* a sort model exists
* There’s no enforced mapping or projection relationship

🔧 Optional improvement:
Introduce a convention or documentation standard stating:

> “Sort models represent flattened, SQL-safe projections used only for sorting.”

No code change required — just clarity.

---

### 5. Repository Mock Caching Keys Are Risky

```csharp
$"{predicate}.{include}"
```

Expression `.ToString()`:

* is not guaranteed stable
* can differ across runs
* may cause cache misses or collisions

🟡 Acceptable for performance testing, but not for correctness-sensitive scenarios.

---

## Conceptual Alignment Check

You are **not doing classic DDD**, and that’s fine.

This is closer to:

* **DDD-inspired layering**
* **Anemic domain model**
* **Repository-as-query-builder**

That matches:

* EF Core realities
* reporting-heavy workloads
* sorting/paging/filtering use cases

Trying to force aggregates or domain services here would make this worse, not better.

---

## Summary

**What you have is good.**
More importantly, it’s **consistent**, **testable**, and **scales with complexity**.

If you want, next we can:

* Review one sorting configuration end-to-end (e.g., DailySchedule)
* Propose a cleaner stateless sorting API
* Add compile-time safety to `SortBy` keys
* Evaluate performance implications of deep includes + sorting
* Discuss how this design holds up under CQRS / read replicas

Just tell me what you want to dig into next.



### OrderBy(LambdaExpression) Extension
This appears to be a custom extension method on `IQueryable<T>` to dynamically apply `OrderBy` or `OrderByDescending` using a provided `LambdaExpression`, `Type`, and `SortOrder`. It's not standard LINQ; it's likely a helper to handle dynamic sorting in repositories (seen in `ApplySorting` private method).

#### Explanation
- **Purpose**: Allows repositories to sort queries based on runtime lambdas from sorting configs (e.g., `DailyScheduleSortingConfiguration` returns `x => x.AssignedPriority`).  
- **Implementation**:  
  - It uses reflection to call `Queryable.OrderBy` or `Queryable.OrderByDescending` generically.  
  - Handles the property type (`sortFieldType`) to make the method generic (e.g., `OrderBy<T, int>` for `AssignedPriority`).  
  - Applies `sorting.Order` to choose ascending or descending.  

- **Missing Code Inference**: Based on common C# patterns (from web search results like  and ), the extension is likely defined in a static class (e.g., `QueryableExtensions` in `Docks.Common.Extensions` or similar):
  ```csharp
  public static class QueryableExtensions
  {
      public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, LambdaExpression sortExpr, Type propertyType, SortOrder order)
      {
          if (sortExpr == null) throw new ArgumentNullException(nameof(sortExpr));

          var methodName = order == SortOrder.Asc ? "OrderBy" : "OrderByDescending";

          var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propertyType);

          var method = typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
              .First(m => m.Name == methodName && m.GetParameters().Length == 2);

          method = method.MakeGenericMethod(typeof(T), propertyType);

          return (IQueryable<T>)method.Invoke(null, new object[] { source, sortExpr });
      }
  }
  ```
  - **How It Works**:  
    - Creates a generic `Func<T, PropertyType>` delegate type.  
    - Uses reflection to invoke the correct `Queryable` method.  
    - Example Call: `query.OrderBy(x => x.AssignedPriority, typeof(int), SortOrder.Asc)` → Translates to `query.OrderBy(x => x.AssignedPriority)`.  
  - **SQL Output**: EF Core converts to `ORDER BY AssignedPriority ASC`.  

- **Usage in Code**: Seen in `GenericReadOnlySqlRepository`'s private `ApplySorting`:  
  ```csharp
  private static IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting, LambdaExpression sortExpr, Type sortFieldType)
  {
      if (sortExpr != null)
      {
          return query.OrderBy(sortExpr, sortFieldType, sorting.Order);
      }
      throw new SortingNotSupportedException(...);
  }
  ```
- **Why Custom?**: Enables dynamic sorting without knowing the property type at compile time.

**Revision Tip**: Think of this as a "dynamic chef's knife" cutting (sorting) data with runtime tools.

---

### SortingModel
This is a simple model for passing sorting instructions to repositories.

- **Namespace**: `Docks.Common.Filtering`.  
- **Code**:  
  ```csharp
  using Docks.Common.Filtering.Enums;

  namespace Docks.Common.Filtering
  {
      public class SortingModel
      {
          public string SortBy { get; set; } // e.g., "AssignedPriority"
          public SortOrder Order { get; set; } // e.g., Asc or Desc
      }
  }
  ```
- **Explanation**:  
  - `SortBy`: String key for the property (e.g., "TrailerAppointments.AppointmentDateTimeUtc").  
  - `Order`: Enum for direction (from `Docks.Common.Filtering.Enums.SortOrder`).  
  - **Usage**: Passed to `ApplySorting` in repositories.  
    - Example: `SortingModel sorting = new SortingModel { SortBy = "AssignedPriority", Order = SortOrder.Asc };`  
    - Repository checks `SortingIsSpecified(sorting)`: `sorting != null && !string.IsNullOrEmpty(sorting.SortBy)`.  
- **Purpose**: Encapsulates sorting params for flexible query building.

**Revision Tip**: See this as a "seasoning order slip" with property and direction.

---

### PagingModel
This is a simple model for passing pagination instructions to repositories.

- **Namespace**: `Docks.Common.Filtering`.  
- **Code**:  
  ```csharp
  namespace Docks.Common.Filtering
  {
      public class PagingModel
      {
          public int Offset { get; set; } // e.g., 10 (skip 10 records)
          public int? Limit { get; set; } // e.g., 5 (take 5 records)
      }
  }
  ```
- **Explanation**:  
  - `Offset`: Number of records to skip.  
  - `Limit`: Optional maximum records to return.  
  - **Usage**: Passed to `ApplyPaging` in repositories.  
    - Example: `PagingModel paging = new PagingModel { Offset = 10, Limit = 5 };`  
    - Repository applies `query.Skip(paging.Offset).Take(paging.Limit.Value)`.  
    - Throws if `Limit <= 0` or no sorting specified (`PaginationNotSupportedException`).  
- **Purpose**: Encapsulates pagination params for efficient data retrieval.

**Revision Tip**: Think of this as a "portion order slip" with skip and take.

---

### ApplyCustomSorting Base Method
This is a virtual method in the base `DefaultSqlSortingConfiguration<TEntity>`, allowing subclasses to override for direct query sorting.

- **Missing Code Inference**: The base method is not in the provided snippets, but based on patterns, it's likely defined as:
  ```csharp
  public virtual IQueryable<TEntity> ApplyCustomSorting(IQueryable<TEntity> query, SortingModel sorting)
  {
      return query; // No-op default, or perhaps throws if not overridden
  }
  ```
- **Explanation**:  
  - **Purpose**: Provides a hook for configs to sort queries directly (e.g., multi-level sorting with `ThenBy`).  
  - **Usage**: In `TrailerTaskSortingConfiguration`:  
    ```csharp
    public override IQueryable<TrailerTaskModel> ApplyCustomSorting(IQueryable<TrailerTaskModel> query, SortingModel sorting)
        => sorting.SortBy == PrioritySortKey ? SortByPriority(query, sorting) : base.ApplyCustomSorting(query, sorting);
    ```
    - If not "AssignedPriority", falls back to base (returns query unchanged).  
  - **Repository Integration**: Likely called in a custom `ApplySorting` variant:  
    ```csharp
    query = _sortingConfiguration.ApplyCustomSorting(query, sorting);
    ```
- **Why?**: Allows configs like `TrailerTaskSortingConfiguration` to apply complex sorting (e.g., `OrderBy...ThenBy`) without lambda limitations.

**Revision Tip**: Picture this as a "direct seasoning" method, overriding the base "do nothing".

---

### BaseSqlRepository<T>
This is the base class for repositories, providing shared EF Core logic.

- **Missing Code Inference**: Based on usage in `GenericReadOnlySqlRepository` and `GenericSqlRepository`, it's likely:
  ```csharp
  namespace Docks.Infrastructure.Sql.Common.Repositories
  {
      public abstract class BaseSqlRepository<TContext> : IRepository where TContext : DbContext
      {
          protected TContext Context { get; }

          protected BaseSqlRepository(TContext context)
          {
              Context = context ?? throw new ArgumentNullException(nameof(context));
          }

          protected IQueryable<TEntity> AsQueryable<TEntity>() where TEntity : class => Context.Set<TEntity>();

          // Other shared methods, e.g., for includes or tracking
      }
  }
  ```
- **Explanation**:  
  - **Purpose**: Centralizes context management and basic query helpers (e.g., `AsQueryable`, `AsNoTracking`).  
  - **Usage**:  
    - `GenericReadOnlySqlRepository`: Extends with `ReadOnlyDocksContext`.  
      ```csharp
      private IQueryable<TEntity> AsNoTracking() => AsQueryable().AsNoTracking();
      ```
    - **Properties**: `Context` is protected for access to `Set<TEntity>()`.  
- **Purpose**: Reduces duplication in repository implementations.

**Revision Tip**: Think of this as a "kitchen base" with shared tools for all chefs (repos).

---

### SqlModule.LoadSqlRepository
This is a helper method to register repositories and contexts in the IoC container.

- **Missing Code Inference**: Based on usage in `SqlDependencies.cs`, it's likely in `Chamberlain.Middleware.Infrastructure.Sql`:
  ```csharp
  public static class SqlModule
  {
      public static IServiceCollection LoadSqlRepository<TInterface, TImplementation, TContext>(
          IServiceCollection services)
          where TInterface : class
          where TImplementation : class, TInterface
          where TContext : DocksContext
      {
          // Register the context
          services.AddScoped<TContext>(sp => new TContext(sp.GetRequiredService<ISqlConnectionStringProvider>()));

          // Register the repository
          services.AddScoped<TInterface, TImplementation>();

          return services;
      }
  }
  ```
- **Explanation**:  
  - **Purpose**: Registers the repository interface with its implementation and context in `IServiceCollection`.  
  - **Usage**: Called in `SqlDependencies.cs`:  
    ```csharp
    SqlModule.LoadSqlRepository<IGenericReadOnlySqlRepository<T>, GenericReadOnlySqlRepository<T>, ReadOnlyDocksContext>(...);
    ```
  - **Logic**:  
    - Adds `TContext` as scoped, injecting `ISqlConnectionStringProvider`.  
    - Adds `TInterface` to `TImplementation` as scoped.  
  - **Why?**: Simplifies DI registration for generic types.

**Revision Tip**: See this as a "supply loader" stocking the room with repos and kitchens.

---

### ApplyPaging Bug
- **Code**: In `GenericReadOnlySqlRepository<TEntity>`'s `ApplyPaging`:  
  ```csharp
  if (paging == null || (!paging.Limit.HasValue && paging.Offset = 0))
      return query;
  ```
- **Bug Explanation**:  
  - "paging.Offset = 0" is an assignment, which is always true and sets Offset to 0.  
  - **Corrected**: Should be "paging.Offset == 0" for comparison.  
  - **Impact**: Always skips pagination if Limit is null, even if Offset > 0 (sets Offset to 0).  
  - **Fix Suggestion**: Change to `paging.Offset == 0`.  
  - **Missing Code**: No missing code; this is a typo in the provided snippet.

**Revision Tip**: Remember as a "typo trap" in conditional checks.

---

### Sort Key Validation
- **Code**: In `GenericReadOnlySqlRepository<TEntity>`'s private `ApplySorting`:  
  ```csharp
  private static IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting, LambdaExpression sortExpr, Type sortFieldType)
  {
      if (sortExpr != null)
      {
          return query.OrderBy(sortExpr, sortFieldType, sorting.Order);
      }

      throw new SortingNotSupportedException($"Specified sorting not supported. Type ({typeof(TEntity)}), Field ({sorting.SortBy}), Order ({sorting.Order}).");
  }
  ```
- **Explanation**:  
  - **Validation**: If the sorting config returns `(null, null)` from `MapSortingKeySelector` (unsupported key), throws `SortingNotSupportedException`.  
  - **Usage**: Ensures invalid sort keys (e.g., `"InvalidField"`) are caught at runtime.  
  - **Fallback in Configs**: Base `DefaultSqlSortingConfiguration` returns `default` if reflection fails. Subclasses (e.g., `DailyScheduleSortingConfiguration`) fall back to base if key not in `SortKeyMapper`.  
  - **Purpose**: Prevents silent failures in sorting; validates keys dynamically.

**Revision Tip**: Think of this as a "spice check" throwing an error if the ingredient is missing.

---

### Missing Code Summary
- **BaseSqlRepository<T>**: Inferred as abstract with `protected TContext Context { get; }` and helpers like `AsQueryable`.  
- **ApplyCustomSorting Base Method**: Inferred in `DefaultSqlSortingConfiguration` as `virtual IQueryable<TEntity> ApplyCustomSorting(IQueryable<TEntity> query, SortingModel sorting) => query;`.  
- **OrderBy Extension**: Inferred in an extensions class as the dynamic `OrderBy` method shown above.  
- **SqlModule.LoadSqlRepository**: Inferred as the DI registration helper shown above.  

If you provide more details or files, I can refine this further. Let me know!



```csharp
// From GenericReadOnlySqlRepository<TEntity> in Docks.Infrastructure.Sql.Common.Repositories

public IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, SortingModel sorting)
{
    if (!SortingIsSpecified(sorting))
        return query;

    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);

    return ApplySorting(query, sorting, sortExpr, sortFieldType);
}

public IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting) where TSortEntity : ISortModel<TEntity>
{
    if (!SortingIsSpecified(sorting)) return query;

    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector<TSortEntity>(sorting.SortBy);

    return ApplySorting(query, sorting, sortExpr, sortFieldType);
}

private static IQueryable<TSortEntity> ApplySorting<TSortEntity>(IQueryable<TSortEntity> query, SortingModel sorting, LambdaExpression sortExpr, Type sortFieldType)
{
    if (sortExpr != null)
    {
        return query.OrderBy(sortExpr, sortFieldType, sorting.Order);
    }

    throw new SortingNotSupportedException(
        $"Specified sorting not supported. Type ({typeof(TEntity)}), " +
        $"Field ({sorting.SortBy}), Order ({sorting.Order}).");
}

private static bool SortingIsSpecified(SortingModel sorting) => sorting != null && !string.IsNullOrEmpty(sorting.SortBy);
```

```csharp
// DefaultSqlSortingConfiguration<TEntity> in Docks.Infrastructure.Sql.Common

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Docks.Common.Filtering.Enums;
using Docks.Infrastructure.Core.Sql.Common;

namespace Docks.Infrastructure.Sql.Common
{
    public class DefaultSqlSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
    {
        public virtual SortOrder SortOrder { get; set; }

        public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)
        {
            var type = typeof(TEntity);
            var propertyPath = keySelector.Split('.');
            var propertyType = propertyPath.Aggregate(type, (tp, key)
                => tp?.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public)?.PropertyType);

            if (propertyType != null)
            {
                var parameter = Expression.Parameter(type, "x");
                var expressionBody = propertyPath.Aggregate<string, Expression>(parameter, Expression.Property);
                var sortExpression = Expression.Lambda(expressionBody, parameter);

                return (sortExpression, propertyType);
            }

            return default;
        }

        public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector) where TSortEntity : ISortModel<TEntity> => default;
    }
}