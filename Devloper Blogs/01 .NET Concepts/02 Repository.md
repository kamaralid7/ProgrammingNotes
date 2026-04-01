use this for your knowledge base in this repository implementation 
Let’s create concise, step-by-step notes on the **Repository Pattern** as implemented in the `GenericSqlRepositoryMock<TEntity>` class from your codebase. These notes will focus on its role, methods, and how it extends `GenericSqlRepository<TEntity>` with mock behavior, based on the provided code in `Docks.Infrastructure.Sql.Common.Repositories`. They’re designed for daily revision to help you remember its purpose and functionality in testing or development scenarios.

---

# Repository Notes: GenericSqlRepositoryMock  
**Goal**: Provide a mock implementation of the read-write repository with simulated behavior for testing/development.  
**Primary Example**: `GenericSqlRepositoryMock<TEntity>` in `Docks.Infrastructure.Sql.Common.Repositories`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Set Up the Mock Repository Structure
- **What**: Extend the read-write repository with mock features.  
- **Where**: `GenericSqlRepositoryMock<TEntity>` (inherits `GenericSqlRepository<TEntity>`).  
- **Key Code**:  
  ```csharp
  public class GenericSqlRepositoryMock<TEntity> : GenericSqlRepository<TEntity>
      where TEntity : class
  {
      private static readonly Random IdGenerator = new Random();
      private static readonly Dictionary<Type, Action<TEntity>> OnAdd = new Dictionary<Type, Action<TEntity>>
      {
          { typeof(DriverConversationModel), (TEntity _) => (_ as DriverConversationModel).DriverConversationId = IdGenerator.Next() }
      };

      public GenericSqlRepositoryMock(WritableDocksContext context) : base(context) { }
  }
  ```
- **Key Features**:  
  - **Inheritance**: Builds on `GenericSqlRepository<TEntity>` for base CRUD logic.  
  - **Constructor**: Takes `WritableDocksContext`, same as base.  
  - **Mock Setup**:  
    - `IdGenerator`: Static `Random` for generating IDs.  
    - `OnAdd`: Static dictionary mapping entity types to mock ID setters (e.g., `DriverConversationModel`).  
  - **Constraint**: `TEntity : class` ensures entity compatibility.  
- **Purpose**: Enhances the base repository with mock behavior for non-production use.

**Revision Tip**: Think of this as a "mock chef" faking cooking with pre-set ingredients (IDs).

---

## Step 2: Override Entity Addition with Mock Logic
- **What**: Modify the `Add` method to simulate ID generation.  
- **Where**: Overridden `Add`.  
- **Key Method**:  
  - **`override void Add(TEntity entity)`**:  
    - **What**: Adds an entity with mock ID generation for specific types.  
    - **Input**: `entity` (e.g., `DriverConversationModel`).  
    - **Logic**:  
      - Checks `OnAdd` for the entity type.  
      - If found, applies the action (e.g., sets `DriverConversationId` with a random ID).  
      - Calls base `Add(entity)` to stage in context.  
    - **Example**: Add a `DriverConversationModel` with a fake ID, then stage it.  
- **Purpose**: Simulates database ID assignment without real persistence.

**Revision Tip**: Picture this as "adding fake ingredients" to the dish.

---

## Step 3: Override Commit with Simulation
- **What**: Replace real database commits with a delay.  
- **Where**: Overridden `CommitAsync`.  
- **Key Method**:  
  - **`override async Task CommitAsync(CancellationToken ct)`**:  
    - **What**: Simulates saving changes with a 10ms delay.  
    - **Input**: `ct` (cancellation token).  
    - **Logic**: Uses `Task.Delay(10, ct)` instead of `SaveChangesAsync`.  
    - **Example**: "Save" a new task with a delay, no DB hit.  
- **Purpose**: Mimics database latency without actual persistence.

**Revision Tip**: See this as "pretending to serve" the dish with a fake wait.

---

## Step 4: Inherit Other Methods Unchanged
- **What**: Retain base class behavior for un-overridden methods.  
- **Where**: Methods like `Update`, `Remove`, `ExecuteProcedure`, etc.  
- **Details**:  
  - Inherited from `GenericSqlRepository<TEntity>`.  
  - Examples:  
    - `Update`: Marks entity as modified (staged in context).  
    - `Remove`: Stages entity for deletion.  
    - `GetTracked`: Retrieves tracked entities.  
  - **Note**: These stage changes in the context, but `CommitAsync` won’t persist them.  
- **Purpose**: Maintains full CRUD functionality with mock commit behavior.

**Revision Tip**: Think of these as "standard recipes" the mock chef follows, but never bakes.

---

## Step 5: Integrate with Dependency Injection
- **What**: Wire the mock repository with DI for testing scenarios.  
- **Where**: `SqlDependencies.cs`.  
- **Key Code**:  
  ```csharp
  if (CatalogExtensions.IsUnderPerformance(configuration))
  {
      SqlModule.LoadSqlRepository<IGenericSqlRepository<T>, GenericSqlRepositoryMock<T>, WritableDocksContext>(...);
  }
  ```
- **Details**:  
  - **Condition**: Registers when `IsUnderPerformanceTesting` is true (e.g., testing mode).  
  - **Dependency**: Uses `WritableDocksContext` (no additional dependencies like cache).  
- **Purpose**: Enables use in non-production environments with simulated writes.

**Revision Tip**: Imagine DI as a "special delivery" bringing mock tools for testing.

---

## Quick Recap Table
| **Step**                | **Category**             | **Key Methods**                          | **Purpose**                          |
|-------------------------|--------------------------|------------------------------------------|--------------------------------------|
| 1. Structure            | Setup                    | Constructor                              | Extend base with mock setup          |
| 2. Add Entities         | Create                   | `Add` (override)                         | Simulate ID generation               |
| 3. Commit Changes       | Persistence              | `CommitAsync` (override)                 | Simulate save with delay             |
| 4. Inherit Methods      | Base Functionality       | `Update`, `Remove`, etc.                 | Retain CRUD ops (staged only)        |
| 5. Integrate DI         | Wiring                   | DI Registration                          | Enable mock in testing mode          |

---

## Revision Tips
- **Mnemonic**: "S-A-C-I-D"  
  - **S**etup, **A**dd, **C**ommit, **I**nherit, **D**I.  
- **Visualize**:  
  - Repository = Restaurant:  
    - Setup = Mock Chef with Fake Tools (ID generator).  
    - Add = Adding Fake Ingredients (mock IDs).  
    - Commit = Pretend Serving with Delay.  
    - Inherit = Standard Recipes (not baked).  
    - DI = Testing Delivery Service.

---

These notes detail how `GenericSqlRepositoryMock<TEntity>` extends `GenericSqlRepository<TEntity>` with mock behavior, focusing on simulated ID generation and commit latency, perfect for daily revision. Want to tweak these or explore another topic (e.g., how it interacts with `WritableDocksContext`)? Let me know!

Let’s create concise, step-by-step notes on the **sorting-related interfaces and classes** in your codebase—`ISortModel<TEntity>`, `ISqlSortingConfiguration<TEntity>`, `ISqlDailyScheduleSortingConfiguration<TEntity>`, `ISqlTrailersSortingConfiguration<TEntity>`, and `MsSqlErrorCodes`—focusing on their roles and how they support sorting within the Repository Pattern. These notes are based on the provided code in `Docks.Infrastructure.Core.Sql.Common` and are designed for daily revision to help you remember their functionality.

---

# Sorting Notes: ISortModel, ISqlSortingConfiguration, and Related Interfaces  
**Goal**: Enable flexible, domain-specific sorting for SQL queries in repositories.  
**Primary Examples**: `ISortModel<TEntity>`, `ISqlSortingConfiguration<TEntity>`, `ISqlDailyScheduleSortingConfiguration<TEntity>`, `ISqlTrailersSortingConfiguration<TEntity>`, `MsSqlErrorCodes` in `Docks.Infrastructure.Core.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Define the Sort Model (ISortModel)
- **What**: Marker interface for sort model types.  
- **Where**: `ISortModel<TEntity>` in `Docks.Infrastructure.Sql.Common`.  
- **Key Code**:  
  ```csharp
  public interface ISortModel<TEntity> { }
  ```
- **Key Features**:  
  - **Empty Interface**: No methods or properties, acts as a type constraint.  
  - **Usage**: Marks a class (e.g., `TrailerAssignmentSortModel`) as a sort model for `TEntity` (e.g., `TrailerAssignmentModel`).  
- **Purpose**: Allows repositories to sort projections/DTOs alongside raw entities.

**Revision Tip**: Think of `ISortModel` as a "label" for custom sorting plates.

---

## Step 2: Define the Core Sorting Contract (ISqlSortingConfiguration)
- **What**: Interface for sorting configuration and logic.  
- **Where**: `ISqlSortingConfiguration<TEntity>` in `Docks.Infrastructure.Core.Sql.Common`.  
- **Key Code**:  
  ```csharp
  public interface ISqlSortingConfiguration<TEntity>
  {
      SortOrder SortOrder { get; set; }
      (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector);
      (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector) 
          where TSortEntity : ISortModel<TEntity>;
  }
  ```
- **Key Features**:  
  - **`SortOrder`**: Enum (`Asc`/`Desc`) to set sorting direction.  
  - **`MapSortingKeySelector(string keySelector)`**: Maps a sort key (e.g., `"AssignedPriority"`) to a lambda for `TEntity` (e.g., `x => x.AssignedPriority`).  
  - **`MapSortingKeySelector<TSortEntity>`**: Maps a key to a lambda for a sort model (e.g., `TrailerAssignmentSortModel`).  
  - **Output**: Tuple of `LambdaExpression` and `Type` for EF Core sorting.  
- **Purpose**: Provides a standard way to define sorting logic for repositories.

**Revision Tip**: Picture this as a "sorting recipe book" with instructions for entities and models.

---

## Step 3: Specialize for Daily Schedules (ISqlDailyScheduleSortingConfiguration)
- **What**: Interface for daily schedule-specific sorting.  
- **Where**: `ISqlDailyScheduleSortingConfiguration<TEntity>` in `Docks.Infrastructure.Core.Sql.Common`.  
- **Key Code**:  
  ```csharp
  public interface ISqlDailyScheduleSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
  {
  }
  ```
- **Key Features**:  
  - **Extension**: Inherits `ISqlSortingConfiguration<TEntity>` without adding new members.  
  - **Usage**: Implemented by classes like `DailyScheduleSortingConfiguration` for `TrailerAssignmentModel`.  
  - **Example**: Supports keys like `"TrailerAppointments.AppointmentDateTimeUtc"`.  
- **Purpose**: Marks a sorting configuration as tailored for daily schedule needs (e.g., appointment times).

**Revision Tip**: See this as a "daily specials" chapter in the sorting recipe book.

---

## Step 4: Specialize for Trailers (ISqlTrailersSortingConfiguration)
- **What**: Interface for trailer-specific sorting.  
- **Where**: `ISqlTrailersSortingConfiguration<TEntity>` in `Docks.Infrastructure.Core.Sql.Common`.  
- **Key Code**:  
  ```csharp
  public interface ISqlTrailersSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
  {
  }
  ```
- **Key Features**:  
  - **Extension**: Inherits `ISqlSortingConfiguration<TEntity>` with no additional members.  
  - **Usage**: Implemented by `TrailersSortingConfiguration` for `TrailerModel`.  
  - **Example**: Supports keys like `"TrailerNumber"`, `"Carrier.Name"`.  
- **Purpose**: Marks a sorting configuration for trailer-related data (e.g., trailer properties).

**Revision Tip**: Think of this as a "trailer section" in the sorting recipe book.

---

## Step 5: Handle SQL Errors (MsSqlErrorCodes)
- **What**: Static class for SQL Server error codes related to sorting/context operations.  
- **Where**: `MsSqlErrorCodes` in `Docks.Infrastructure.Core.Sql.Common`.  
- **Key Code**:  
  ```csharp
  public static class MsSqlErrorCodes
  {
      public const int ConstraintViolationCannotInsertDupKey = 2627; // Duplicate key error
  }
  ```
- **Key Features**:  
  - **Constant**: Defines error code `2627` for duplicate key violations.  
  - **Context**: Not directly tied to sorting but relevant for repository operations (e.g., writes that might affect sorted data).  
- **Purpose**: Provides error codes for handling SQL exceptions in data access.

**Revision Tip**: Picture this as a "troubleshooting guide" for kitchen mishaps (SQL errors).

---

## Step 6: Integrate with Repositories
- **What**: Use these interfaces in repository sorting logic.  
- **Where**: `GenericReadOnlySqlRepository<TEntity>` and `GenericSqlRepository<TEntity>`.  
- **Key Examples**:  
  - **Constructor**: Takes `ISqlSortingConfiguration<TEntity>` (e.g., `DailyScheduleSortingConfiguration`).  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **`ApplySorting`**: Uses `MapSortingKeySelector` to sort queries.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
    ```
  - **Specialized**: `TrailerTaskSortingConfiguration` uses `ApplyCustomSorting` instead.  
- **Purpose**: Enables dynamic, domain-specific sorting in data retrieval.

**Revision Tip**: Imagine the repository as a "chef" using the sorting recipe book to season dishes.

---

## Quick Recap Table
| **Step**                | **Component**                        | **Key Feature**                     | **Purpose**                          |
|-------------------------|--------------------------------------|-------------------------------------|--------------------------------------|
| 1. Sort Model           | `ISortModel<TEntity>`                | Marker interface                    | Label sort models                    |
| 2. Core Sorting         | `ISqlSortingConfiguration<TEntity>`  | SortOrder, MapSortingKeySelector    | Define sorting logic                 |
| 3. Daily Schedules      | `ISqlDailyScheduleSortingConfiguration`| Extends core                       | Specialize for schedules             |
| 4. Trailers             | `ISqlTrailersSortingConfiguration`   | Extends core                       | Specialize for trailers              |
| 5. Error Codes          | `MsSqlErrorCodes`                    | Error constants (e.g., 2627)        | Handle SQL errors                    |
| 6. Repository Integration| Repositories                        | Uses sorting config                | Apply sorting to queries             |

---

## Revision Tips
- **Mnemonic**: "M-C-D-T-E-R"  
  - **M**odel, **C**ore, **D**aily, **T**railers, **E**rrors, **R**epository.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - `ISortModel` = Custom Plate Label.  
    - `ISqlSortingConfiguration` = Main Recipe Book.  
    - `ISqlDailyScheduleSortingConfiguration` = Daily Specials.  
    - `ISqlTrailersSortingConfiguration` = Trailer Section.  
    - `MsSqlErrorCodes` = Troubleshooting Guide.  
    - Repository = Chef Applying Recipes.

---

These notes cover the sorting interfaces (`ISortModel`, `ISqlSortingConfiguration`, `ISqlDailyScheduleSortingConfiguration`, `ISqlTrailersSortingConfiguration`) and `MsSqlErrorCodes`, focusing on their roles and integration, perfect for daily revision. Want to tweak these or dive deeper into a specific implementation (e.g., `DailyScheduleSortingConfiguration`)? Let me know!

Let’s create concise, step-by-step notes on the **Repository Pattern** as it relates to sorting with the `DefaultSqlSortingConfiguration<TEntity>` class from your codebase. These notes will focus on its role, methods, and implementation details, based on the provided code in `Docks.Infrastructure.Sql.Common`. They’re designed for daily revision to help you remember how it provides a foundational sorting mechanism.

---

# Sorting Notes: DefaultSqlSortingConfiguration  
**Goal**: Provide a default, reflection-based sorting configuration for entities in SQL repositories.  
**Primary Example**: `DefaultSqlSortingConfiguration<TEntity>` in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Set Up the Default Sorting Configuration
- **What**: Define a base class for sorting logic.  
- **Where**: `DefaultSqlSortingConfiguration<TEntity>` (implements `ISqlSortingConfiguration<TEntity>`).  
- **Key Code**:  
  ```csharp
  public class DefaultSqlSortingConfiguration<TEntity> : ISqlSortingConfiguration<TEntity>
  {
      public virtual SortOrder SortOrder { get; set; }
  }
  ```
- **Key Features**:  
  - **Interface**: Implements `ISqlSortingConfiguration<TEntity>` for sorting contract.  
  - **`SortOrder`**: Property to set sorting direction (`Asc` or `Desc`), virtual for override.  
  - **Constraint**: Works with any `TEntity` (e.g., `TrailerTaskModel`).  
- **Purpose**: Establishes a reusable foundation for sorting configurations.

**Revision Tip**: Think of this as a "basic seasoning kit" for sorting dishes.

---

## Step 2: Implement Sorting for Entities
- **What**: Map sort keys to lambda expressions using reflection.  
- **Where**: `MapSortingKeySelector(string keySelector)`.  
- **Key Method**:  
  - **`virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)`**:  
    - **What**: Generates a sorting expression for `TEntity` based on a key.  
    - **Input**: `keySelector` (e.g., `"AssignedPriority"` or `"Trailer.TrailerNumber"`).  
    - **Logic**:  
      - Splits `keySelector` into property path (e.g., `"Trailer.TrailerNumber"` → `["Trailer", "TrailerNumber"]`).  
      - Uses reflection (`GetProperty`) to traverse path and get property type.  
      - Builds a lambda (e.g., `x => x.Trailer.TrailerNumber`) if path is valid.  
      - Returns `(null, null)` if invalid.  
    - **Example**:  
      - `"AssignedPriority"` → `(x => x.AssignedPriority, typeof(int?))`.  
      - `"InvalidKey"` → `(null, null)`.  
- **Purpose**: Provides a generic sorting mechanism for simple properties.

**Revision Tip**: Picture this as a "recipe generator" using a map (reflection) to find ingredients.

---

## Step 3: Handle Sort Models (Limited Support)
- **What**: Provide sorting for sort models, with minimal default behavior.  
- **Where**: `MapSortingKeySelector<TSortEntity>`.  
- **Key Method**:  
  - **`virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector) where TSortEntity : ISortModel<TEntity>`**:  
    - **What**: Maps a key to a sorting expression for a sort model (e.g., `TrailerAssignmentSortModel`).  
    - **Input**: `keySelector` (e.g., `"DockName"`).  
    - **Logic**: Returns `(null, null)` by default, no reflection-based logic implemented.  
    - **Example**: `"DockName"` → `(null, null)` (requires override in subclasses).  
- **Purpose**: Acts as a placeholder for sort model sorting, expecting subclasses to extend.

**Revision Tip**: See this as an "empty recipe" for custom plates, waiting for specifics.

---

## Step 4: Integrate with Repositories
- **What**: Serve as a base or fallback for repository sorting.  
- **Where**: Used in `GenericReadOnlySqlRepository<TEntity>` and `GenericSqlRepository<TEntity>`.  
- **Key Integration**:  
  - **Constructor**: Injected as `_sortingConfiguration` if no specialized config is provided.  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **`ApplySorting`**: Uses `MapSortingKeySelector` to sort queries.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
    ```
  - **Fallback**: Subclasses (e.g., `DailyScheduleSortingConfiguration`) call `base.MapSortingKeySelector` for unhandled keys.  
- **Purpose**: Provides default sorting support, extended by domain-specific configs.

**Revision Tip**: Imagine this as a "default spice" the chef uses when no special recipe is given.

---

## Step 5: Limitations and Extensions
- **What**: Understand its scope and reliance on subclasses.  
- **Details**:  
  - **Simple Properties Only**: Handles direct properties (e.g., `"AssignedPriority"`) but not complex paths (e.g., `"TrailerAppointments.First().AppointmentDateTimeUtc"`).  
  - **No Sort Model Support**: Returns `(null, null)` for `TSortEntity`, requiring override.  
  - **Extended By**: Classes like `TrailerTaskSortingConfiguration`, `DailyScheduleSortingConfiguration` add custom logic.  
- **Purpose**: Offers a starting point, not a complete solution.

**Revision Tip**: Think of it as a "starter kit" needing extra spices from others.

---

## Quick Recap Table
| **Step**                | **Category**             | **Key Methods**                          | **Purpose**                          |
|-------------------------|--------------------------|------------------------------------------|--------------------------------------|
| 1. Structure            | Setup                    | `SortOrder`                              | Define base sorting config           |
| 2. Entity Sorting       | Sorting Logic            | `MapSortingKeySelector`                  | Map keys to entity lambdas           |
| 3. Sort Model Sorting   | Sort Model Logic         | `MapSortingKeySelector<TSortEntity>`     | Placeholder for sort models          |
| 4. Repository Integration| Integration             | Used in `ApplySorting`                   | Support repository sorting           |
| 5. Limitations          | Scope                    | N/A                                      | Basic, needs extension               |

---

## Revision Tips
- **Mnemonic**: "S-E-M-R-L"  
  - **S**etup, **E**ntity, **M**odel, **R**epository, **L**imitations.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - Setup = Basic Spice Kit.  
    - Entity Sorting = Simple Spice Recipe (reflection).  
    - Model Sorting = Empty Spice Jar (needs filling).  
    - Repository = Chef Using Spices.  
    - Limitations = Starter Kit Only.

---

These notes detail how `DefaultSqlSortingConfiguration<TEntity>` provides a foundational sorting mechanism, perfect for daily revision. Want to tweak these or explore a specific subclass (e.g., `TrailerTaskSortingConfiguration`)? Let me know!




Let’s create concise, step-by-step notes on the **Repository Pattern** as it relates to sorting with the `DailyScheduleSortingConfiguration<TEntity>` class from your codebase. These notes will focus on its role, methods, and implementation details, based on the provided code in `Docks.Infrastructure.Sql.Common`. They’re designed for daily revision to help you remember how it provides domain-specific sorting for daily schedules.

---

# Sorting Notes: DailyScheduleSortingConfiguration  
**Goal**: Provide a specialized sorting configuration for daily schedules in SQL repositories.  
**Primary Example**: `DailyScheduleSortingConfiguration<TEntity>` (for `TrailerAssignmentModel`) in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Set Up the Sorting Configuration
- **What**: Define a specialized sorting class for daily schedules.  
- **Where**: `DailyScheduleSortingConfiguration<TEntity>` (inherits `DefaultSqlSortingConfiguration<TrailerAssignmentModel>`, implements `ISqlDailyScheduleSortingConfiguration<TrailerAssignmentModel>`).  
- **Key Code**:  
  ```csharp
  public class DailyScheduleSortingConfiguration : DefaultSqlSortingConfiguration<TrailerAssignmentModel>,
      ISqlDailyScheduleSortingConfiguration<TrailerAssignmentModel>
  {
      private readonly IDictionary<string, Func<(LambdaExpression Lambda, Type PropertyType)>> SortKeyMapper;
      private readonly IDailyScheduleConfiguration _dailyScheduleConfiguration;

      public DailyScheduleSortingConfiguration(IDailyScheduleConfiguration dailyScheduleConfiguration)
      {
          _dailyScheduleConfiguration = dailyScheduleConfiguration;
          SortKeyMapper = new Dictionary<string, Func<(LambdaExpression Lambda, Type PropertyType)>>
          {
              { "TrailerAppointments.AppointmentDateTimeUtc", SortByScheduledDateTime },
              { "AssignedPriority", SortByAssignedPriority },
              // ... many more keys
          };
      }
  }
  ```
- **Key Features**:  
  - **Inheritance**: Extends `DefaultSqlSortingConfiguration` for base sorting.  
  - **Interface**: Implements `ISqlDailyScheduleSortingConfiguration` for schedule-specific sorting.  
  - **Constructor**: Takes `IDailyScheduleConfiguration` (e.g., for `ChatHistoryAvailableDays`).  
  - **`SortKeyMapper`**: Dictionary mapping sort keys to sorting methods.  
- **Purpose**: Sets up a tailored sorting config for `TrailerAssignmentModel` daily schedules.

**Revision Tip**: Think of this as a "daily specials seasoning kit" for scheduling dishes.

---

## Step 2: Map Sorting Keys to Expressions
- **What**: Define how sort keys translate to sorting logic.  
- **Where**: `MapSortingKeySelector`.  
- **Key Method**:  
  - **`override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)`**:  
    - **What**: Maps a sort key to a lambda expression for `TrailerAssignmentModel`.  
    - **Input**: `keySelector` (e.g., `"TrailerAppointments.AppointmentDateTimeUtc"`).  
    - **Logic**:  
      - Checks `SortKeyMapper` for `keySelector`.  
      - If found, calls the mapped method (e.g., `SortByScheduledDateTime`).  
      - If not, falls back to `base.MapSortingKeySelector` (reflection-based).  
    - **Examples**:  
      - `"AssignedPriority"` → `(x => x.AssignedPriority.GetValueOrDefault(int.MaxValue), typeof(int))`.  
      - `"DockName"` → `(x => x.DockName, typeof(string))` (via base).  
- **Purpose**: Provides domain-specific sorting with a fallback to generic logic.

**Revision Tip**: Picture this as a "recipe lookup" finding the right seasoning for the key.

---

## Step 3: Define Sorting Methods
- **What**: Implement specific sorting logic for keys.  
- **Where**: Methods like `SortByScheduledDateTime`, `SortByAssignedPriority`.  
- **Key Examples**:  
  - **`SortByScheduledDateTime()`**:  
    - **What**: Sorts by first appointment date/time.  
    - **Logic**: `x => x.TrailerAppointments.Select(y => y.AppointmentDateTimeUtc).FirstOrDefault()`.  
    - **Output**: `(lambda, typeof(DateTime?))`.  
    - **Example**: Sort assignments by earliest appointment.  

  - **`SortByAssignedPriority()`**:  
    - **What**: Sorts by priority with null handling.  
    - **Logic**: `x => x.AssignedPriority.GetValueOrDefault(SortOrder == SortOrder.Asc ? int.MaxValue : int.MinValue)`.  
    - **Output**: `(lambda, typeof(int))`.  
    - **Example**: Sort assignments by priority, nulls last (asc) or first (desc).  

  - **`SortByUnReadMessage()`**:  
    - **What**: Sorts by unread message status with time filter.  
    - **Logic**: Uses `ChatHistoryAvailableDays` to filter, returns 2 (unread), 1 (read), 0 (none).  
    - **Output**: `(lambda, typeof(int))`.  
    - **Example**: Prioritize unread messages in recent conversations.  
- **Purpose**: Offers tailored sorting for daily schedule fields.

**Revision Tip**: See these as "special recipes" adding unique flavors to the dish.

---

## Step 4: Handle Sort Models (Default Behavior)
- **What**: Provide sorting for sort models with minimal implementation.  
- **Where**: `MapSortingKeySelector<TSortEntity>`.  
- **Key Method**:  
  - **`override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector)`**:  
    - **What**: Maps a key for a sort model (e.g., `TrailerAssignmentSortModel`).  
    - **Input**: `keySelector` (e.g., `"DockName"`).  
    - **Logic**: Delegates to `base.MapSortingKeySelector<TSortEntity>`, returning `(null, null)`.  
    - **Example**: `"DockName"` → `(null, null)` (requires subclass override).  
- **Purpose**: Acts as a placeholder, expecting extensions for sort models.

**Revision Tip**: Think of this as an "empty recipe" for custom plates, waiting for details.

---

## Step 5: Integrate with Repositories
- **What**: Use in repository sorting logic.  
- **Where**: `GenericReadOnlySqlRepository<TEntity>` or `GenericSqlRepository<TEntity>`.  
- **Key Integration**:  
  - **Constructor**: Injected as `_sortingConfiguration`.  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **`ApplySorting`**: Calls `MapSortingKeySelector` to sort queries.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
    ```
  - **SQL**: Generates `ORDER BY` clauses (e.g., `ORDER BY AssignedPriority ASC`).  
- **Purpose**: Enables dynamic, schedule-specific sorting in data retrieval.

**Revision Tip**: Imagine the repository as a "chef" using this daily specials kit to season dishes.

---

## Quick Recap Table
| **Step**                | **Category**             | **Key Methods**                          | **Purpose**                          |
|-------------------------|--------------------------|------------------------------------------|--------------------------------------|
| 1. Structure            | Setup                    | Constructor, `SortKeyMapper`             | Define schedule-specific config      |
| 2. Map Keys             | Sorting Logic            | `MapSortingKeySelector`                  | Map keys to lambdas                  |
| 3. Sorting Methods      | Specific Logic           | `SortByScheduledDateTime`, etc.          | Implement custom sorting             |
| 4. Sort Models          | Sort Model Logic         | `MapSortingKeySelector<TSortEntity>`     | Placeholder for sort models          |
| 5. Repository Integration| Integration             | Used in `ApplySorting`                   | Apply sorting to queries             |

---

## Revision Tips
- **Mnemonic**: "S-M-S-M-R"  
  - **S**etup, **M**ap, **S**orting Methods, **M**odels, **R**epository.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - Setup = Daily Specials Kit.  
    - Map = Recipe Lookup (dictionary).  
    - Sorting Methods = Special Recipes.  
    - Models = Empty Jar (needs filling).  
    - Repository = Chef Applying Spices.

---

These notes detail how `DailyScheduleSortingConfiguration<TEntity>` provides a specialized sorting mechanism for daily schedules, perfect for daily revision. Want to tweak these or explore another sorting config (e.g., `TrailerTaskSortingConfiguration`)? Let me know!

Let’s create concise, step-by-step notes on the **Repository Pattern** as it relates to sorting with the `TrailerAssigmentSortingConfiguration` class (noting the typo in your request; I’ll assume you meant `TrailerAssignmentSortingConfiguration` as per the provided code) from your codebase. These notes will focus on its role, methods, and implementation details, based on the code in `Docks.Infrastructure.Sql.Common`. They’re designed for daily revision to help you remember how it provides sorting for trailer assignments.

---

# Sorting Notes: TrailerAssignmentSortingConfiguration  
**Goal**: Provide a specialized sorting configuration for trailer assignments in SQL repositories.  
**Primary Example**: `TrailerAssigmentSortingConfiguration` (for `TrailerAssignmentModel`) in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Set Up the Sorting Configuration
- **What**: Define a specialized sorting class for trailer assignments.  
- **Where**: `TrailerAssigmentSortingConfiguration` (inherits `DefaultSqlSortingConfiguration<TrailerAssignmentModel>`).  
- **Key Code**:  
  ```csharp
  public class TrailerAssigmentSortingConfiguration : DefaultSqlSortingConfiguration<TrailerAssignmentModel>
  {
      private readonly string TrailerAppointmentDatetimeUtcSortKey = 
          $"{nameof(TrailerAssignmentModel.TrailerAppointments)}.{nameof(TrailerAppointmentModel.AppointmentDateTimeUtc)}";
      private readonly string AssignedPropertySortKey = nameof(TrailerAssignmentModel.AssignedPriority);
      private readonly string DockNameSortKey = nameof(TrailerAssignmentModel.DockName);
      // ... other keys
  }
  ```
- **Key Features**:  
  - **Inheritance**: Extends `DefaultSqlSortingConfiguration` for base sorting logic.  
  - **No Interface**: Unlike `DailyScheduleSortingConfiguration`, doesn’t explicitly implement a specialized interface (still satisfies `ISqlSortingConfiguration<TrailerAssignmentModel>` via base).  
  - **Sort Keys**: Defines specific keys (e.g., `"AssignedPriority"`, `"TrailerAppointments.AppointmentDateTimeUtc"`) as `readonly string` fields.  
- **Purpose**: Sets up a tailored sorting config for `TrailerAssignmentModel` without external dependencies.

**Revision Tip**: Think of this as a "trailer assignment seasoning kit" for sorting dishes.

---

## Step 2: Map Sorting Keys for Entities
- **What**: Map sort keys to lambda expressions for `TrailerAssignmentModel`.  
- **Where**: `MapSortingKeySelector`.  
- **Key Method**:  
  - **`override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)`**:  
    - **What**: Maps a sort key to a sorting expression.  
    - **Input**: `keySelector` (e.g., `"AssignedPriority"`).  
    - **Logic**:  
      - Uses `if` conditions to match keys case-insensitively (`StringComparison.OrdinalIgnoreCase`).  
      - Returns specific lambdas for defined keys, falls back to `base.MapSortingKeySelector` for others.  
    - **Examples**:  
      - `"AssignedPriority"`: `(x => x.AssignedPriority.GetValueOrDefault(int.MaxValue), typeof(int))`.  
      - `"TrailerAppointments.AppointmentDateTimeUtc"`: `(x => x.TrailerAppointments.Select(y => y.AppointmentDateTimeUtc).FirstOrDefault(), typeof(DateTime?))`.  
      - `"DockName"`: `(x => x.DockName, typeof(string))` (via base reflection).  
- **Purpose**: Provides custom sorting for trailer assignment fields with a fallback.

**Revision Tip**: Picture this as a "recipe lookup" with a short list of special seasonings.

---

## Step 3: Map Sorting Keys for Sort Models
- **What**: Map sort keys for a sort model (e.g., `TrailerAssignmentSortModel`).  
- **Where**: `MapSortingKeySelector<TSortEntity>`.  
- **Key Method**:  
  - **`override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector)`**:  
    - **What**: Maps a key to a sorting expression for `TSortEntity`.  
    - **Input**: `keySelector` (e.g., `"DockName"`).  
    - **Logic**:  
      - Checks if `keySelector` is `"DockName"`.  
      - Returns `(x => x.DockName, typeof(string))` for `TrailerAssignmentSortModel`.  
      - Falls back to `base.MapSortingKeySelector<TSortEntity>` (returns `(null, null)`) for others.  
    - **Example**:  
      - `"DockName"`: `(x => x.DockName, typeof(string))`.  
      - `"TrailerAssignmentId"`: `(null, null)` (base behavior).  
- **Purpose**: Adds limited sort model support, focusing on `DockName`.

**Revision Tip**: See this as a "small recipe" for custom plates, with a single flavor.

---

## Step 4: Define Sorting Methods
- **What**: Implement specific sorting logic for keys.  
- **Where**: Embedded in `MapSortingKeySelector` methods (no separate methods like `DailyScheduleSortingConfiguration`).  
- **Key Examples**:  
  - **`"AssignedPriority"`:**
    - **Logic**: `x => x.AssignedPriority.GetValueOrDefault(int.MaxValue)`.  
    - **Output**: `(lambda, typeof(int))`.  
    - **Example**: Sort assignments by priority, nulls last (asc).  

  - **`"TrailerAppointments.AppointmentDateTimeUtc"`:**
    - **Logic**: `x => x.TrailerAppointments.Select(y => y.AppointmentDateTimeUtc).FirstOrDefault()`.  
    - **Output**: `(lambda, typeof(DateTime?))`.  
    - **Example**: Sort by earliest appointment time.  

  - **`"DockName"` (Sort Model):**
    - **Logic**: `x => x.DockName` (for `TrailerAssignmentSortModel`).  
    - **Output**: `(lambda, typeof(string))`.  
    - **Example**: Sort DTOs by dock name.  
- **Purpose**: Offers targeted sorting for key trailer assignment fields.

**Revision Tip**: Think of these as "quick recipes" written directly in the lookup.

---

## Step 5: Integrate with Repositories
- **What**: Use in repository sorting logic.  
- **Where**: `GenericReadOnlySqlRepository<TEntity>` or `GenericSqlRepository<TEntity>`.  
- **Key Integration**:  
  - **Constructor**: Injected as `_sortingConfiguration`.  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **`ApplySorting`**: Calls `MapSortingKeySelector` to sort queries.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
    ```
  - **SQL**: Generates `ORDER BY` (e.g., `ORDER BY AssignedPriority ASC`).  
- **Purpose**: Enables dynamic sorting for trailer assignments in data retrieval.

**Revision Tip**: Imagine the repository as a "chef" using this assignment kit to season dishes.

---

## Quick Recap Table
| **Step**                | **Category**             | **Key Methods**                          | **Purpose**                          |
|-------------------------|--------------------------|------------------------------------------|--------------------------------------|
| 1. Structure            | Setup                    | Sort Keys (`readonly string`)            | Define assignment-specific config    |
| 2. Map Entity Keys      | Entity Sorting           | `MapSortingKeySelector`                  | Map keys to entity lambdas           |
| 3. Map Sort Model Keys  | Sort Model Sorting       | `MapSortingKeySelector<TSortEntity>`     | Map keys for sort models             |
| 4. Sorting Logic        | Specific Logic           | Embedded in `MapSortingKeySelector`      | Implement custom sorting             |
| 5. Repository Integration| Integration             | Used in `ApplySorting`                   | Apply sorting to queries             |

---

## Revision Tips
- **Mnemonic**: "S-M-M-S-R"  
  - **S**etup, **M**ap Entities, **M**ap Models, **S**orting Logic, **R**epository.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - Setup = Assignment Spice Kit.  
    - Map Entities = Recipe Lookup (if statements).  
    - Map Models = Small Recipe (DockName).  
    - Sorting Logic = Quick Recipes.  
    - Repository = Chef Applying Spices.

---

These notes detail how `TrailerAssigmentSortingConfiguration` provides a specialized sorting mechanism for trailer assignments, perfect for daily revision. Want to tweak these or explore another sorting config (e.g., `TrailersSortingConfiguration`)? Let me know!

Let’s create concise, step-by-step notes on the **Repository Pattern** as it relates to sorting with the `TrailersSortingConfiguration` class from your codebase. These notes will focus on its role, methods, and implementation details, based on the provided code in `Docks.Infrastructure.Sql.Common`. They’re designed for daily revision to help you remember how it provides sorting for trailer-related data.

---

# Sorting Notes: TrailersSortingConfiguration  
**Goal**: Provide a specialized sorting configuration for trailers in SQL repositories.  
**Primary Example**: `TrailersSortingConfiguration` (for `TrailerModel`) in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Set Up the Sorting Configuration
- **What**: Define a specialized sorting class for trailers.  
- **Where**: `TrailersSortingConfiguration` (inherits `DefaultSqlSortingConfiguration<TrailerModel>`, implements `ISqlTrailersSortingConfiguration<TrailerModel>`).  
- **Key Code**:  
  ```csharp
  public class TrailersSortingConfiguration : DefaultSqlSortingConfiguration<TrailerModel>,
      ISqlTrailersSortingConfiguration<TrailerModel>
  {
      private readonly string TrailerNumberKey = nameof(TrailerModel.TrailerNumber);
      private readonly string CarrierKey = $"{nameof(TrailerModel.Carrier)}.{nameof(CarrierModel.Name)}";
      // ... other keys

      private readonly IDictionary<string, Func<(LambdaExpression Lambda, Type PropertyType)>> SortKeyMapper;

      public TrailersSortingConfiguration()
      {
          SortKeyMapper = new Dictionary<string, Func<(LambdaExpression Lambda, Type PropertyType)>>
          {
              { TrailerNumberKey, SortByTrailerNumberKey },
              { CarrierKey, SortByCarrierKey },
              // ... other mappings
          };
      }
  }
  ```
- **Key Features**:  
  - **Inheritance**: Extends `DefaultSqlSortingConfiguration` for base sorting logic.  
  - **Interface**: Implements `ISqlTrailersSortingConfiguration` for trailer-specific sorting.  
  - **Constructor**: Initializes `SortKeyMapper` with no external dependencies.  
  - **`SortKeyMapper`**: Dictionary mapping sort keys to sorting methods.  
- **Purpose**: Sets up a tailored sorting config for `TrailerModel`.

**Revision Tip**: Think of this as a "trailer seasoning kit" for sorting dishes.

---

## Step 2: Map Sorting Keys to Expressions
- **What**: Define how sort keys translate to sorting logic.  
- **Where**: `MapSortingKeySelector`.  
- **Key Method**:  
  - **`override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)`**:  
    - **What**: Maps a sort key to a lambda expression for `TrailerModel`.  
    - **Input**: `keySelector` (e.g., `"TrailerNumber"`).  
    - **Logic**:  
      - Checks `SortKeyMapper` for `keySelector`.  
      - If found, calls the mapped method (e.g., `SortByTrailerNumberKey`).  
      - If not, falls back to `base.MapSortingKeySelector` (reflection-based).  
    - **Examples**:  
      - `"TrailerNumber"`: `(x => x.TrailerNumber, typeof(string))`.  
      - `"Carrier.Name"`: `(x => x.Carrier.Name, typeof(string))`.  
      - `"InvalidKey"`: `(x => x.SomeProperty, typeof(...))` (via base, if valid).  
- **Purpose**: Provides custom sorting for trailer fields with a fallback.

**Revision Tip**: Picture this as a "recipe lookup" finding the right seasoning from a dictionary.

---

## Step 3: Define Sorting Methods
- **What**: Implement specific sorting logic for keys.  
- **Where**: Methods like `SortByTrailerNumberKey`, `SortByCarrierKey`.  
- **Key Examples**:  
  - **`SortByTrailerNumberKey()`**:  
    - **What**: Sorts by trailer number.  
    - **Logic**: `x => x.TrailerNumber`.  
    - **Output**: `(lambda, typeof(string))`.  
    - **Example**: Sort trailers by number.  

  - **`SortByCarrierKey()`**:  
    - **What**: Sorts by carrier name.  
    - **Logic**: `x => x.Carrier.Name`.  
    - **Output**: `(lambda, typeof(string))`.  
    - **Example**: Sort trailers by carrier.  

  - **`SortByAppointmentKey()`**:  
    - **What**: Sorts by first appointment date/time.  
    - **Logic**: `x => x.TrailerAssignment.TrailerAppointments.Select(y => y.AppointmentDateTimeUtc).FirstOrDefault()`.  
    - **Output**: `(lambda, typeof(DateTime?))`.  
    - **Example**: Sort by earliest appointment.  
- **Purpose**: Offers tailored sorting for trailer-related fields.

**Revision Tip**: See these as "special recipes" adding unique flavors to the dish.

---

## Step 4: Handle Sort Models (Default Behavior)
- **What**: Provide sorting for sort models with minimal implementation.  
- **Where**: `MapSortingKeySelector<TSortEntity>`.  
- **Key Method**:  
  - **`override (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector)`**:  
    - **What**: Maps a key for a sort model (e.g., a `TrailerSortModel`).  
    - **Input**: `keySelector` (e.g., `"TrailerNumber"`).  
    - **Logic**: Delegates to `base.MapSortingKeySelector<TSortEntity>`, returning `(null, null)`.  
    - **Example**: `"TrailerNumber"` → `(null, null)` (requires subclass override).  
- **Purpose**: Acts as a placeholder, expecting extensions for sort models.

**Revision Tip**: Think of this as an "empty recipe" for custom plates, waiting for details.

---

## Step 5: Integrate with Repositories
- **What**: Use in repository sorting logic.  
- **Where**: `GenericReadOnlySqlRepository<TEntity>` or `GenericSqlRepository<TEntity>`.  
- **Key Integration**:  
  - **Constructor**: Injected as `_sortingConfiguration`.  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **`ApplySorting`**: Calls `MapSortingKeySelector` to sort queries.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
    ```
  - **SQL**: Generates `ORDER BY` (e.g., `ORDER BY TrailerNumber ASC`).  
- **Purpose**: Enables dynamic, trailer-specific sorting in data retrieval.

**Revision Tip**: Imagine the repository as a "chef" using this trailer kit to season dishes.

---

## Quick Recap Table
| **Step**                | **Category**             | **Key Methods**                          | **Purpose**                          |
|-------------------------|--------------------------|------------------------------------------|--------------------------------------|
| 1. Structure            | Setup                    | Constructor, `SortKeyMapper`             | Define trailer-specific config       |
| 2. Map Keys             | Sorting Logic            | `MapSortingKeySelector`                  | Map keys to lambdas                  |
| 3. Sorting Methods      | Specific Logic           | `SortByTrailerNumberKey`, etc.           | Implement custom sorting             |
| 4. Sort Models          | Sort Model Logic         | `MapSortingKeySelector<TSortEntity>`     | Placeholder for sort models          |
| 5. Repository Integration| Integration             | Used in `ApplySorting`                   | Apply sorting to queries             |

---

## Revision Tips
- **Mnemonic**: "S-M-S-M-R"  
  - **S**etup, **M**ap, **S**orting Methods, **M**odels, **R**epository.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - Setup = Trailer Spice Kit.  
    - Map = Recipe Lookup (dictionary).  
    - Sorting Methods = Special Recipes.  
    - Models = Empty Jar (needs filling).  
    - Repository = Chef Applying Spices.

---

These notes detail how `TrailersSortingConfiguration` provides a specialized sorting mechanism for trailers, perfect for daily revision. Want to tweak these or explore another topic (e.g., `TrailerTaskSortingConfiguration`)? Let me know!


Let’s create concise, step-by-step notes on the **Repository Pattern** as it relates to sorting with the `TrailerTaskSortingConfiguration` class from your codebase. These notes will focus on its role, methods, and implementation details, based on the provided code in `Docks.Infrastructure.Sql.Common`. They’re designed for daily revision to help you remember how it provides sorting for trailer tasks.

---

# Sorting Notes: TrailerTaskSortingConfiguration  
**Goal**: Provide a specialized sorting configuration for trailer tasks in SQL repositories.  
**Primary Example**: `TrailerTaskSortingConfiguration` (for `TrailerTaskModel`) in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Set Up the Sorting Configuration
- **What**: Define a specialized sorting class for trailer tasks.  
- **Where**: `TrailerTaskSortingConfiguration` (inherits `DefaultSqlSortingConfiguration<TrailerTaskModel>`, implements `ISqlSortingConfiguration<TrailerTaskModel>`).  
- **Key Code**:  
  ```csharp
  public class TrailerTaskSortingConfiguration : DefaultSqlSortingConfiguration<TrailerTaskModel>,
      ISqlSortingConfiguration<TrailerTaskModel>
  {
      private readonly string PrioritySortKey = $"{nameof(TrailerTaskModel.AssignedPriority)}";

      public TrailerTaskSortingConfiguration()
      {
      }
  }
  ```
- **Key Features**:  
  - **Inheritance**: Extends `DefaultSqlSortingConfiguration` for base sorting logic.  
  - **Interface**: Implements `ISqlSortingConfiguration<TrailerTaskModel>` explicitly.  
  - **Constructor**: Empty, no external dependencies.  
  - **Sort Key**: Defines a single key (`"AssignedPriority"`) as a `readonly string`.  
- **Purpose**: Sets up a focused sorting config for `TrailerTaskModel`.

**Revision Tip**: Think of this as a "task seasoning kit" with one main spice.

---

## Step 2: Apply Custom Sorting Logic
- **What**: Directly apply sorting to queries instead of mapping keys.  
- **Where**: `ApplyCustomSorting`.  
- **Key Method**:  
  - **`override IQueryable<TrailerTaskModel> ApplyCustomSorting(IQueryable<TrailerTaskModel> query, SortingModel sorting)`**:  
    - **What**: Sorts the query by `AssignedPriority` with a secondary sort.  
    - **Input**: `query` (base query), `sorting` (e.g., `SortBy = "AssignedPriority"`, `Order = Asc`).  
    - **Logic**:  
      - If `sorting.SortBy == PrioritySortKey`, calls `SortByPriority`.  
      - Otherwise, falls back to `base.ApplyCustomSorting`.  
    - **Example**: Sort tasks by priority ascending, then due date.  
- **Purpose**: Provides a custom, multi-level sorting approach unique to tasks.

**Revision Tip**: Picture this as a "quick seasoning" applied directly to the dish.

---

## Step 3: Define Sorting Logic
- **What**: Implement specific sorting for `AssignedPriority`.  
- **Where**: `SortByPriority`.  
- **Key Method**:  
  - **`SortByPriority(IQueryable<TrailerTaskModel> query, SortingModel sorting)`**:  
    - **What**: Sorts by `AssignedPriority` with `TaskDueDatetimeUtc` as a tiebreaker.  
    - **Input**: `query`, `sorting`.  
    - **Logic**:  
      - If `sorting.Order == SortOrder.Asc`: `OrderBy(x => x.AssignedPriority).ThenBy(x => x.TaskDueDatetimeUtc)`.  
      - If `sorting.Order == SortOrder.Desc`: `OrderByDescending(x => x.AssignedPriority).ThenByDescending(x => x.TaskDueDatetimeUtc)`.  
    - **Output**: Sorted `IQueryable<TrailerTaskModel>`.  
    - **Example**: Sort tasks by priority ascending, then earliest due date.  
- **Purpose**: Ensures tasks are ordered by priority with a consistent secondary sort.

**Revision Tip**: See this as a "double-spice recipe" for priority and timing.

---

## Step 4: Inherit Base Sorting Methods
- **What**: Retain default sorting behavior for unhandled cases.  
- **Where**: Inherited `MapSortingKeySelector` methods (not overridden).  
- **Details**:  
  - **`MapSortingKeySelector(string keySelector)`**:  
    - Inherited from `DefaultSqlSortingConfiguration`.  
    - Uses reflection for simple properties (e.g., `"TaskDueDatetimeUtc"` → `(x => x.TaskDueDatetimeUtc, typeof(DateTime?))`).  
  - **`MapSortingKeySelector<TSortEntity>(string keySelector)`**:  
    - Returns `(null, null)` by default for sort models (e.g., a `TrailerTaskSortModel`).  
- **Purpose**: Provides fallback sorting while focusing on custom logic.

**Revision Tip**: Think of these as "basic spices" used when the special recipe isn’t needed.

---

## Step 5: Integrate with Repositories
- **What**: Use in repository sorting logic.  
- **Where**: `GenericReadOnlySqlRepository<TEntity>` or `GenericSqlRepository<TEntity>`.  
- **Key Integration**:  
  - **Constructor**: Injected as `_sortingConfiguration`.  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **Custom Use**: Likely uses `ApplyCustomSorting` instead of `MapSortingKeySelector` (based on its design).  
    ```csharp
    query = _sortingConfiguration.ApplyCustomSorting(query, sorting);
    ```
  - **SQL**: Generates `ORDER BY` (e.g., `ORDER BY AssignedPriority ASC, TaskDueDatetimeUtc ASC`).  
- **Purpose**: Enables task-specific sorting in data retrieval.

**Revision Tip**: Imagine the repository as a "chef" using this task kit to season dishes directly.

---

## Quick Recap Table
| **Step**                | **Category**             | **Key Methods**                          | **Purpose**                          |
|-------------------------|--------------------------|------------------------------------------|--------------------------------------|
| 1. Structure            | Setup                    | Constructor, `PrioritySortKey`           | Define task-specific config          |
| 2. Apply Sorting        | Sorting Logic            | `ApplyCustomSorting`                     | Apply sorting directly               |
| 3. Sorting Logic        | Specific Logic           | `SortByPriority`                         | Implement priority + due date sort   |
| 4. Inherit Base         | Fallback Logic           | `MapSortingKeySelector` (inherited)      | Provide default sorting              |
| 5. Repository Integration| Integration             | Used in `ApplyCustomSorting`             | Apply sorting to queries             |

---

## Revision Tips
- **Mnemonic**: "S-A-S-I-R"  
  - **S**etup, **A**pply, **S**orting Logic, **I**nherit, **R**epository.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - Setup = Task Spice Kit.  
    - Apply = Direct Seasoning (no mapping).  
    - Sorting Logic = Double-Spice Recipe.  
    - Inherit = Basic Spices (fallback).  
    - Repository = Chef Applying Spices.

---

These notes detail how `TrailerTaskSortingConfiguration` provides a specialized, direct sorting mechanism for trailer tasks, perfect for daily revision. Want to tweak these or explore another topic (e.g., how it differs from `MapSortingKeySelector` configs)? Let me know!


Let’s create detailed notes focusing on the **design patterns** implemented in the sorting configurations—`DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, and `TrailerTaskSortingConfiguration`—from your codebase. We’ll then compare these configurations, highlighting their differences and how they diverge from the typical `MapSortingKeySelector`-based approach. These notes are designed for daily revision to help you understand and remember their design and functionality.

---

# Sorting Design Patterns Notes: Detailed Analysis  
**Goal**: Understand design patterns in sorting configurations and their differences.  
**Primary Examples**: `DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, `TrailerTaskSortingConfiguration` in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Design Patterns Overview
- **What**: Sorting configs use design patterns to provide flexible, reusable logic.  
- **Common Patterns**:  
  - **Strategy Pattern**: Defines interchangeable sorting algorithms.  
  - **Template Method Pattern**: Provides a base structure with customizable steps.  
  - **Inheritance**: Extends base functionality for specialization.  
- **Purpose**: Enable domain-specific sorting while maintaining a consistent framework.

**Revision Tip**: Think of patterns as "cooking techniques" for sorting recipes.

---

## Step 2: DailyScheduleSortingConfiguration Design Patterns
- **What**: Specialized sorting for `TrailerAssignmentModel` daily schedules.  
- **Key Code**: Uses `SortKeyMapper` dictionary and `MapSortingKeySelector`.  
- **Design Patterns**:  
  1. **Strategy Pattern**:  
     - **How**:  
       - Implements `ISqlDailyScheduleSortingConfiguration` (extends `ISqlSortingConfiguration`).  
       - `SortKeyMapper` holds multiple sorting strategies (e.g., `SortByScheduledDateTime`, `SortByUnReadMessage`).  
       - `MapSortingKeySelector` selects the strategy based on `keySelector`.  
     - **Example**: `"AssignedPriority"` → `SortByAssignedPriority` strategy.  
     - **Benefit**: Allows swapping strategies (e.g., schedule vs. trailer sorting) at runtime.  

  2. **Template Method Pattern**:  
     - **How**:  
       - Inherits from `DefaultSqlSortingConfiguration`, overriding `MapSortingKeySelector`.  
       - Base provides a reflection-based template; this class customizes it with `SortKeyMapper`.  
     - **Example**: Fallback to `base.MapSortingKeySelector` for unhandled keys.  
     - **Benefit**: Reuses base logic while adding schedule-specific steps.  

  3. **Inheritance**:  
     - **How**: Extends `DefaultSqlSortingConfiguration` for shared reflection logic.  
     - **Example**: Inherits `SortOrder` and base sorting for simple properties.  
     - **Benefit**: Builds on a generic foundation.  

- **Purpose**: Offers a rich, extensible sorting solution for daily schedules.

**Revision Tip**: Picture this as a "master chef" with a big recipe book (dictionary) and a basic guide (base).

---

## Step 3: TrailerAssigmentSortingConfiguration Design Patterns
- **What**: Specialized sorting for `TrailerAssignmentModel` (assumed typo corrected).  
- **Key Code**: Uses `if` conditions in `MapSortingKeySelector`.  
- **Design Patterns**:  
  1. **Strategy Pattern**:  
     - **How**:  
       - Implements `ISqlSortingConfiguration` via inheritance.  
       - `MapSortingKeySelector` uses `if` statements as a simple strategy selector (e.g., `"AssignedPriority"` → specific lambda).  
     - **Example**: `"TrailerAppointments.AppointmentDateTimeUtc"` → custom lambda strategy.  
     - **Benefit**: Less dynamic than a dictionary but still offers distinct strategies.  

  2. **Template Method Pattern**:  
     - **How**:  
       - Overrides `MapSortingKeySelector` from `DefaultSqlSortingConfiguration`.  
       - Base provides reflection fallback; this class adds assignment-specific logic.  
     - **Example**: `"DockName"` falls back to base reflection.  
     - **Benefit**: Customizes the base template with assignment rules.  

  3. **Inheritance**:  
     - **How**: Extends `DefaultSqlSortingConfiguration` for base sorting capabilities.  
     - **Example**: Uses base for unhandled keys like `"SerialNumber"`.  
     - **Benefit**: Leverages generic sorting infrastructure.  

- **Purpose**: Provides a straightforward, assignment-focused sorting config.

**Revision Tip**: See this as a "line cook" with a short, fixed recipe list (if statements) and a basic guide.

---

## Step 4: TrailersSortingConfiguration Design Patterns
- **What**: Specialized sorting for `TrailerModel`.  
- **Key Code**: Uses `SortKeyMapper` dictionary and `MapSortingKeySelector`.  
- **Design Patterns**:  
  1. **Strategy Pattern**:  
     - **How**:  
       - Implements `ISqlTrailersSortingConfiguration`.  
       - `SortKeyMapper` defines multiple strategies (e.g., `SortByTrailerNumberKey`, `SortByCarrierKey`).  
       - `MapSortingKeySelector` selects the strategy dynamically.  
     - **Example**: `"Carrier.Name"` → `SortByCarrierKey` strategy.  
     - **Benefit**: Highly flexible, like `DailyScheduleSortingConfiguration`.  

  2. **Template Method Pattern**:  
     - **How**:  
       - Overrides `MapSortingKeySelector` from `DefaultSqlSortingConfiguration`.  
       - Base offers reflection template; this class uses `SortKeyMapper` for customization.  
     - **Example**: `"InvalidKey"` → base reflection.  
     - **Benefit**: Combines base logic with trailer-specific rules.  

  3. **Inheritance**:  
     - **How**: Extends `DefaultSqlSortingConfiguration` for shared functionality.  
     - **Example**: Inherits `SortOrder` and base sorting.  
     - **Benefit**: Builds on a reusable foundation.  

- **Purpose**: Offers a robust, trailer-specific sorting solution.

**Revision Tip**: Imagine this as a "trailer chef" with a recipe book (dictionary) and a basic guide.

---

## Step 5: TrailerTaskSortingConfiguration Design Patterns
- **What**: Specialized sorting for `TrailerTaskModel`.  
- **Key Code**: Uses `ApplyCustomSorting` instead of `MapSortingKeySelector`.  
- **Design Patterns**:  
  1. **Strategy Pattern**:  
     - **How**:  
       - Implements `ISqlSortingConfiguration` explicitly.  
       - `ApplyCustomSorting` acts as a strategy selector with one strategy (`SortByPriority`).  
     - **Example**: `"AssignedPriority"` → `SortByPriority` strategy.  
     - **Benefit**: Single-strategy focus, less dynamic than dictionary-based configs.  

  2. **Template Method Pattern**:  
     - **How**:  
       - Overrides `ApplyCustomSorting` from `DefaultSqlSortingConfiguration` (assumed base method).  
       - Base provides a default (possibly no-op); this class customizes with priority sorting.  
     - **Example**: Fallback to `base.ApplyCustomSorting` for other keys.  
     - **Benefit**: Extends a base template with task-specific logic.  

  3. **Inheritance**:  
     - **How**: Extends `DefaultSqlSortingConfiguration` for inherited methods (e.g., `MapSortingKeySelector`).  
     - **Example**: Uses base reflection for unhandled keys like `"TaskDueDatetimeUtc"`.  
     - **Benefit**: Reuses generic sorting when not overridden.  

- **Purpose**: Provides a direct, task-focused sorting approach.

**Revision Tip**: Picture this as a "task chef" with one special recipe (priority) and a basic guide.

---

## Step 6: Differences Between Configurations
- **What**: Compare the four sorting configurations.  
- **Key Differences**:  
  | **Aspect**             | **DailySchedule**         | **TrailerAssignment**     | **Trailers**             | **TrailerTask**          |
  |------------------------|---------------------------|---------------------------|--------------------------|--------------------------|
  | **Entity**             | `TrailerAssignmentModel`  | `TrailerAssignmentModel`  | `TrailerModel`           | `TrailerTaskModel`       |
  | **Key Selection**      | Dictionary (`SortKeyMapper`) | `if` Statements          | Dictionary (`SortKeyMapper`) | Single Key (`PrioritySortKey`) |
  | **Primary Method**     | `MapSortingKeySelector`   | `MapSortingKeySelector`   | `MapSortingKeySelector`  | `ApplyCustomSorting`     |
  | **Sort Model Support** | None (base `(null, null)`) | Limited (`DockName`)     | None (base `(null, null)`) | None (base `(null, null)`) |
  | **Complexity**         | High (many keys, logic)   | Medium (few keys)         | High (many keys)         | Low (one key, multi-level) |
  | **External Dependency**| Yes (`IDailyScheduleConfiguration`) | No                  | No                       | No                       |
  | **Sorting Levels**     | Single-level              | Single-level              | Single-level             | Multi-level (`ThenBy`)   |

- **Purpose**: Each config tailors sorting to its domain with varying approaches.

**Revision Tip**: Think of these as "chefs" with different recipe styles: big book, short list, big book, single special.

---

## Step 7: Difference from MapSortingKeySelector Configs
- **What**: Compare `TrailerTaskSortingConfiguration` (non-`MapSortingKeySelector`) vs. others.  
- **Key Differences**:  
  - **`MapSortingKeySelector` Configs (DailySchedule, TrailerAssignment, Trailers)**:  
    - **Approach**: Return `(LambdaExpression, Type)` for repositories to apply via `OrderBy`.  
    - **Flexibility**: Multiple keys mapped to lambdas, dynamic selection.  
    - **Integration**: Used in `ApplySorting` with `query.OrderBy(sortExpr, ...)`.  
    - **Example**: `"TrailerNumber"` → `(x => x.TrailerNumber, typeof(string))` → `ORDER BY TrailerNumber`.  
    - **Benefit**: Decouples sorting logic from query execution, reusable across methods.  

  - **`ApplyCustomSorting` Config (TrailerTask)**:  
    - **Approach**: Directly modifies `IQueryable` with `OrderBy`/`ThenBy`.  
    - **Flexibility**: Single key (`"AssignedPriority"`) with fixed multi-level sort.  
    - **Integration**: Likely used in a custom repository method (e.g., `query = _sortingConfiguration.ApplyCustomSorting(query, sorting)`).  
    - **Example**: `"AssignedPriority"` → `OrderBy(x => x.AssignedPriority).ThenBy(x => x.TaskDueDatetimeUtc)` → `ORDER BY AssignedPriority, TaskDueDatetimeUtc`.  
    - **Benefit**: Simplifies multi-level sorting, less overhead for single-purpose use.  

- **Purpose**: `MapSortingKeySelector` offers broader flexibility; `ApplyCustomSorting` is task-specific and direct.

**Revision Tip**: See `MapSortingKeySelector` as "recipe cards" handed to the chef, vs. `ApplyCustomSorting` as the chef seasoning directly.

---

## Quick Recap Table
| **Config**             | **Pattern**            | **Key Feature**                     | **Diff. from MapSortingKeySelector** |
|-----------------------|------------------------|-------------------------------------|--------------------------------------|
| **DailySchedule**     | Strategy, Template     | `SortKeyMapper` dictionary          | Broad, dynamic key mapping           |
| **TrailerAssignment** | Strategy, Template     | `if` statements                     | Simpler, fixed key mapping           |
| **Trailers**          | Strategy, Template     | `SortKeyMapper` dictionary          | Broad, dynamic key mapping           |
| **TrailerTask**       | Strategy, Template     | `ApplyCustomSorting`, multi-level   | Direct query mod, single-purpose     |

---

## Revision Tips
- **Mnemonic**: "S-T" (Strategy, Template) for all, plus "D" (Direct) for TrailerTask.  
- **Visualize**:  
  - Sorting = Restaurant Seasoning:  
    - Daily/Trailers = Big Recipe Book (Strategy).  
    - Assignment = Short Recipe List (Strategy).  
    - Task = Direct Seasoning (Strategy, no mapping).  
    - All = Base Guide (Template).

---

These notes detail the design patterns in each sorting config and their differences, with a focus on `MapSortingKeySelector` vs. `ApplyCustomSorting`, perfect for daily revision. Want to tweak these or explore another topic? Let me know!

Let’s create detailed, step-by-step notes on the **overall design patterns** used across the sorting configurations—`DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, and `TrailerTaskSortingConfiguration`—and their implementations within your codebase. We’ll focus on how these patterns are applied holistically, tying them to the broader repository and context framework in `Docks.Infrastructure.Sql.Common`. These notes are designed for daily revision to help you understand and remember the design structure.

---

# Overall Design Patterns Notes: Sorting Configurations  
**Goal**: Understand and implement a cohesive sorting system using design patterns in repositories.  
**Primary Examples**: `DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, `TrailerTaskSortingConfiguration` in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Strategy Pattern
- **What**: Defines a family of interchangeable algorithms (sorting strategies).  
- **Where**: Core to all sorting configurations via `ISqlSortingConfiguration<TEntity>`.  
- **Implementation**:  
  - **Interface**: `ISqlSortingConfiguration<TEntity>` specifies the contract:  
    ```csharp
    public interface ISqlSortingConfiguration<TEntity>
    {
        SortOrder SortOrder { get; set; }
        (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector);
        (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector);
    }
    ```
  - **Concrete Strategies**:  
    - `DailyScheduleSortingConfiguration`: Uses `SortKeyMapper` dictionary for multiple strategies (e.g., `SortByScheduledDateTime`).  
      ```csharp
      SortKeyMapper["AssignedPriority"] = SortByAssignedPriority;
      ```
    - `TrailerAssigmentSortingConfiguration`: Uses `if` statements as a strategy selector (e.g., `"AssignedPriority"` → specific lambda).  
      ```csharp
      if (AssignedPropertySortKey.Equals(keySelector, ...)) return (x => x.AssignedPriority.GetValueOrDefault(...), typeof(int));
      ```
    - `TrailersSortingConfiguration`: Uses `SortKeyMapper` for trailer strategies (e.g., `SortByTrailerNumberKey`).  
      ```csharp
      SortKeyMapper[TrailerNumberKey] = SortByTrailerNumberKey;
      ```
    - `TrailerTaskSortingConfiguration`: Uses `ApplyCustomSorting` with a single strategy (`SortByPriority`).  
      ```csharp
      if (sorting.SortBy == PrioritySortKey) return SortByPriority(query, sorting);
      ```
  - **Repository Use**: Injected into `GenericReadOnlySqlRepository` via `_sortingConfiguration`, swapped at runtime.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    ```
- **How It Works**:  
  - Each config provides a different sorting "strategy" (e.g., by priority, appointment time).  
  - Repositories call the strategy method (`MapSortingKeySelector` or `ApplyCustomSorting`) to apply sorting.  
- **Purpose**: Enables dynamic, domain-specific sorting with interchangeable implementations.

**Revision Tip**: Think of Strategy as a "recipe book" where each config is a chapter of sorting recipes.

---

## Step 2: Template Method Pattern
- **What**: Defines a skeleton algorithm in a base class, allowing subclasses to customize steps.  
- **Where**: Base class `DefaultSqlSortingConfiguration<TEntity>` and its overrides.  
- **Implementation**:  
  - **Base Template**: `DefaultSqlSortingConfiguration` provides default sorting:  
    ```csharp
    public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)
    {
        var propertyPath = keySelector.Split('.');
        var propertyType = propertyPath.Aggregate(typeof(TEntity), (tp, key) => tp?.GetProperty(key)?.PropertyType);
        if (propertyType != null) return (Expression.Lambda(...), propertyType);
        return default;
    }
    ```
  - **Customizations**:  
    - `DailyScheduleSortingConfiguration`: Overrides `MapSortingKeySelector` with `SortKeyMapper`.  
      ```csharp
      return SortKeyMapper.TryGetValue(keySelector, out var sortFunc) ? sortFunc() : base.MapSortingKeySelector(keySelector);
      ```
    - `TrailerAssigmentSortingConfiguration`: Overrides with `if` conditions.  
      ```csharp
      if (AssignedPropertySortKey.Equals(keySelector, ...)) return (...); else return base.MapSortingKeySelector(keySelector);
      ```
    - `TrailersSortingConfiguration`: Overrides with `SortKeyMapper`.  
      ```csharp
      return SortKeyMapper.TryGetValue(keySelector, out var sortFunc) ? sortFunc() : base.MapSortingKeySelector(keySelector);
      ```
    - `TrailerTaskSortingConfiguration`: Overrides `ApplyCustomSorting` (assumed base method exists).  
      ```csharp
      return sorting.SortBy == PrioritySortKey ? SortByPriority(query, sorting) : base.ApplyCustomSorting(query, sorting);
      ```
  - **Repository Use**: Base methods are called when custom logic doesn’t apply, ensuring a fallback.  
- **How It Works**:  
  - Base class sets the "template" (e.g., reflection for `MapSortingKeySelector`).  
  - Subclasses customize the "steps" (e.g., dictionary lookup, direct sorting).  
- **Purpose**: Provides a consistent sorting structure with room for specialization.

**Revision Tip**: Picture Template as a "basic recipe" with blanks for each chef to fill.

---

## Step 3: Inheritance
- **What**: Extends a base class to inherit and specialize functionality.  
- **Where**: All configs inherit from `DefaultSqlSortingConfiguration<TEntity>`.  
- **Implementation**:  
  - **Base Class**: `DefaultSqlSortingConfiguration` offers reflection-based sorting and `SortOrder`.  
    ```csharp
    public virtual SortOrder SortOrder { get; set; }
    ```
  - **Subclasses**:  
    - `DailyScheduleSortingConfiguration`: Adds `SortKeyMapper` and schedule logic.  
    - `TrailerAssigmentSortingConfiguration`: Adds `if`-based logic and sort model support.  
    - `TrailersSortingConfiguration`: Adds `SortKeyMapper` for trailer fields.  
    - `TrailerTaskSortingConfiguration`: Adds `ApplyCustomSorting` for priority sorting.  
  - **Repository Use**: Inherited methods (e.g., base `MapSortingKeySelector`) serve as fallbacks.  
- **How It Works**:  
  - Subclasses inherit generic sorting (e.g., reflection for simple properties).  
  - They override to add domain-specific behavior (e.g., multi-level sorting in `TrailerTask`).  
- **Purpose**: Reuses base functionality while tailoring to specific needs.

**Revision Tip**: See Inheritance as a "family cookbook" where kids tweak the parent’s recipes.

---

## Step 4: Dependency Injection (DI)
- **What**: Injects dependencies to decouple components.  
- **Where**: Used across repositories and configs via DI registration.  
- **Implementation**:  
  - **Registration**: In `SqlDependencies.cs`:  
    ```csharp
    serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<TrailerAssignmentModel>), typeof(DailyScheduleSortingConfiguration));
    // Similar for others
    ```
  - **Injection**: Repositories take `ISqlSortingConfiguration<TEntity>`:  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **Specific Use**:  
    - `DailyScheduleSortingConfiguration`: Injects `IDailyScheduleConfiguration`.  
      ```csharp
      public DailyScheduleSortingConfiguration(IDailyScheduleConfiguration dailyScheduleConfiguration)
      ```
    - Others: No additional dependencies beyond base context.  
- **How It Works**:  
  - DI container resolves configs (e.g., `TrailersSortingConfiguration`) at runtime.  
  - Repositories use the injected config without hardcoding its type.  
- **Purpose**: Enables runtime flexibility and testability (e.g., swapping mock configs).

**Revision Tip**: Imagine DI as a "delivery service" bringing the right seasoning kit to the chef.

---

## Step 5: Comparison and Differences
- **What**: Compare how patterns manifest across configs.  
- **Key Differences**:  
  | **Pattern**         | **DailySchedule**         | **TrailerAssignment**     | **Trailers**             | **TrailerTask**          |
  |--------------------|---------------------------|---------------------------|--------------------------|--------------------------|
  | **Strategy**       | Dictionary-based, many strategies | `if`-based, fewer strategies | Dictionary-based, many strategies | Single strategy, direct |
  | **Template**       | Customizes `MapSortingKeySelector` | Customizes `MapSortingKeySelector` | Customizes `MapSortingKeySelector` | Customizes `ApplyCustomSorting` |
  | **Inheritance**    | Extends base fully         | Extends base fully         | Extends base fully        | Extends base, shifts focus |
  | **DI**             | Uses external config       | No extra dependencies      | No extra dependencies     | No extra dependencies    |

- **How They Differ**:  
  - **Strategy**:  
    - `DailySchedule` and `Trailers`: Broad, dynamic with dictionaries.  
    - `TrailerAssignment`: Simpler, static with `if` conditions.  
    - `TrailerTask`: Minimal, single-strategy with direct query mod.  
  - **Template**:  
    - Most use `MapSortingKeySelector` for lambda mapping.  
    - `TrailerTask` uses `ApplyCustomSorting` for direct `IQueryable` manipulation.  
  - **Inheritance**: All leverage base reflection, but `TrailerTask` shifts to a different method.  
  - **DI**: Only `DailySchedule` adds an external dependency (`IDailyScheduleConfiguration`).  

**Revision Tip**: Picture each as a "chef” with unique tools: big book, short list, big book, single spice.

---

## Step 6: Implementation in Context
- **What**: How patterns tie into repositories and contexts.  
- **Implementation**:  
  - **Repositories**:  
    - `GenericReadOnlySqlRepository`: Uses Strategy via `_sortingConfiguration` injection.  
      ```csharp
      query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
      ```
    - `GenericSqlRepository`: Inherits read-only capabilities, applies writes.  
  - **Contexts**:  
    - `ReadOnlyDocksContext`: Supports read-only sorting queries.  
    - `WritableDocksContext`: Supports write operations that might affect sorted data.  
  - **Execution**: EF Core translates lambdas or direct `OrderBy` to SQL `ORDER BY`.  
- **How It Works**:  
  - Strategy provides sorting logic.  
  - Template ensures a consistent base.  
  - Inheritance extends functionality.  
  - DI wires it all together.  
- **Purpose**: Creates a cohesive, flexible sorting system across data access layers.

**Revision Tip**: See this as a "restaurant chain" with chefs (configs), kitchens (contexts), and service (repos).

---

## Quick Recap Table
| **Pattern**         | **Implementation**                     | **Purpose**                          |
|--------------------|----------------------------------------|--------------------------------------|
| **Strategy**       | Interchangeable sorting via configs    | Dynamic, domain-specific sorting     |
| **Template**       | Base `DefaultSqlSortingConfiguration`  | Consistent structure, custom steps   |
| **Inheritance**    | Extends base for specialization        | Reuse and tailor functionality       |
| **DI**             | Injects configs into repositories      | Flexibility and testability          |

---

## Revision Tips
- **Mnemonic**: "S-T-I-D"  
  - **S**trategy, **T**emplate, **I**nheritance, **D**I.  
- **Visualize**:  
  - Sorting = Restaurant:  
    - Strategy = Recipe Book (configs).  
    - Template = Basic Cooking Guide (base).  
    - Inheritance = Family Recipes (extended).  
    - DI = Delivery Service (injection).

---

These notes provide a detailed overview of design patterns across the sorting configurations, their implementations, and differences, perfect for daily revision. Want to tweak these or dive deeper into a specific pattern’s use? Let me know!

Let’s create detailed, step-by-step notes on the **overall design patterns** used across the sorting configurations—`DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, and `TrailerTaskSortingConfiguration`—and their implementations within your codebase. We’ll focus on how these patterns are applied holistically, tying them to the broader repository and context framework in `Docks.Infrastructure.Sql.Common`. These notes are designed for daily revision to help you understand and remember the design structure.

---

# Overall Design Patterns Notes: Sorting Configurations  
**Goal**: Understand and implement a cohesive sorting system using design patterns in repositories.  
**Primary Examples**: `DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, `TrailerTaskSortingConfiguration` in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Strategy Pattern
- **What**: Defines a family of interchangeable algorithms (sorting strategies).  
- **Where**: Core to all sorting configurations via `ISqlSortingConfiguration<TEntity>`.  
- **Implementation**:  
  - **Interface**: `ISqlSortingConfiguration<TEntity>` specifies the contract:  
    ```csharp
    public interface ISqlSortingConfiguration<TEntity>
    {
        SortOrder SortOrder { get; set; }
        (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector);
        (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector<TSortEntity>(string keySelector);
    }
    ```
  - **Concrete Strategies**:  
    - `DailyScheduleSortingConfiguration`: Uses `SortKeyMapper` dictionary for multiple strategies (e.g., `SortByScheduledDateTime`).  
      ```csharp
      SortKeyMapper["AssignedPriority"] = SortByAssignedPriority;
      ```
    - `TrailerAssigmentSortingConfiguration`: Uses `if` statements as a strategy selector (e.g., `"AssignedPriority"` → specific lambda).  
      ```csharp
      if (AssignedPropertySortKey.Equals(keySelector, ...)) return (x => x.AssignedPriority.GetValueOrDefault(...), typeof(int));
      ```
    - `TrailersSortingConfiguration`: Uses `SortKeyMapper` for trailer strategies (e.g., `SortByTrailerNumberKey`).  
      ```csharp
      SortKeyMapper[TrailerNumberKey] = SortByTrailerNumberKey;
      ```
    - `TrailerTaskSortingConfiguration`: Uses `ApplyCustomSorting` with a single strategy (`SortByPriority`).  
      ```csharp
      if (sorting.SortBy == PrioritySortKey) return SortByPriority(query, sorting);
      ```
  - **Repository Use**: Injected into `GenericReadOnlySqlRepository` via `_sortingConfiguration`, swapped at runtime.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    ```
- **How It Works**:  
  - Each config provides a different sorting "strategy" (e.g., by priority, appointment time).  
  - Repositories call the strategy method (`MapSortingKeySelector` or `ApplyCustomSorting`) to apply sorting.  
- **Purpose**: Enables dynamic, domain-specific sorting with interchangeable implementations.

**Revision Tip**: Think of Strategy as a "recipe book" where each config is a chapter of sorting recipes.

---

## Step 2: Template Method Pattern
- **What**: Defines a skeleton algorithm in a base class, allowing subclasses to customize steps.  
- **Where**: Base class `DefaultSqlSortingConfiguration<TEntity>` and its overrides.  
- **Implementation**:  
  - **Base Template**: `DefaultSqlSortingConfiguration` provides default sorting:  
    ```csharp
    public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector)
    {
        var propertyPath = keySelector.Split('.');
        var propertyType = propertyPath.Aggregate(typeof(TEntity), (tp, key) => tp?.GetProperty(key)?.PropertyType);
        if (propertyType != null) return (Expression.Lambda(...), propertyType);
        return default;
    }
    ```
  - **Customizations**:  
    - `DailyScheduleSortingConfiguration`: Overrides `MapSortingKeySelector` with `SortKeyMapper`.  
      ```csharp
      return SortKeyMapper.TryGetValue(keySelector, out var sortFunc) ? sortFunc() : base.MapSortingKeySelector(keySelector);
      ```
    - `TrailerAssigmentSortingConfiguration`: Overrides with `if` conditions.  
      ```csharp
      if (AssignedPropertySortKey.Equals(keySelector, ...)) return (...); else return base.MapSortingKeySelector(keySelector);
      ```
    - `TrailersSortingConfiguration`: Overrides with `SortKeyMapper`.  
      ```csharp
      return SortKeyMapper.TryGetValue(keySelector, out var sortFunc) ? sortFunc() : base.MapSortingKeySelector(keySelector);
      ```
    - `TrailerTaskSortingConfiguration`: Overrides `ApplyCustomSorting` (assumed base method exists).  
      ```csharp
      return sorting.SortBy == PrioritySortKey ? SortByPriority(query, sorting) : base.ApplyCustomSorting(query, sorting);
      ```
  - **Repository Use**: Base methods are called when custom logic doesn’t apply, ensuring a fallback.  
- **How It Works**:  
  - Base class sets the "template" (e.g., reflection for `MapSortingKeySelector`).  
  - Subclasses customize the "steps" (e.g., dictionary lookup, direct sorting).  
- **Purpose**: Provides a consistent sorting structure with room for specialization.

**Revision Tip**: Picture Template as a "basic recipe" with blanks for each chef to fill.

---

## Step 3: Inheritance
- **What**: Extends a base class to inherit and specialize functionality.  
- **Where**: All configs inherit from `DefaultSqlSortingConfiguration<TEntity>`.  
- **Implementation**:  
  - **Base Class**: `DefaultSqlSortingConfiguration` offers reflection-based sorting and `SortOrder`.  
    ```csharp
    public virtual SortOrder SortOrder { get; set; }
    ```
  - **Subclasses**:  
    - `DailyScheduleSortingConfiguration`: Adds `SortKeyMapper` and schedule logic.  
    - `TrailerAssigmentSortingConfiguration`: Adds `if`-based logic and sort model support.  
    - `TrailersSortingConfiguration`: Adds `SortKeyMapper` for trailer fields.  
    - `TrailerTaskSortingConfiguration`: Adds `ApplyCustomSorting` for priority sorting.  
  - **Repository Use**: Inherited methods (e.g., base `MapSortingKeySelector`) serve as fallbacks.  
- **How It Works**:  
  - Subclasses inherit generic sorting (e.g., reflection for simple properties).  
  - They override to add domain-specific behavior (e.g., multi-level sorting in `TrailerTask`).  
- **Purpose**: Reuses base functionality while tailoring to specific needs.

**Revision Tip**: See Inheritance as a "family cookbook" where kids tweak the parent’s recipes.

---

## Step 4: Dependency Injection (DI)
- **What**: Injects dependencies to decouple components.  
- **Where**: Used across repositories and configs via DI registration.  
- **Implementation**:  
  - **Registration**: In `SqlDependencies.cs`:  
    ```csharp
    serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<TrailerAssignmentModel>), typeof(DailyScheduleSortingConfiguration));
    // Similar for others
    ```
  - **Injection**: Repositories take `ISqlSortingConfiguration<TEntity>`:  
    ```csharp
    public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
    ```
  - **Specific Use**:  
    - `DailyScheduleSortingConfiguration`: Injects `IDailyScheduleConfiguration`.  
      ```csharp
      public DailyScheduleSortingConfiguration(IDailyScheduleConfiguration dailyScheduleConfiguration)
      ```
    - Others: No additional dependencies beyond base context.  
- **How It Works**:  
  - DI container resolves configs (e.g., `TrailersSortingConfiguration`) at runtime.  
  - Repositories use the injected config without hardcoding its type.  
- **Purpose**: Enables runtime flexibility and testability (e.g., swapping mock configs).

**Revision Tip**: Imagine DI as a "delivery service" bringing the right seasoning kit to the chef.

---

## Step 5: Comparison and Differences
- **What**: Compare how patterns manifest across configs.  
- **Key Differences**:  
  | **Pattern**         | **DailySchedule**         | **TrailerAssignment**     | **Trailers**             | **TrailerTask**          |
  |--------------------|---------------------------|---------------------------|--------------------------|--------------------------|
  | **Strategy**       | Dictionary-based, many strategies | `if`-based, fewer strategies | Dictionary-based, many strategies | Single strategy, direct |
  | **Template**       | Customizes `MapSortingKeySelector` | Customizes `MapSortingKeySelector` | Customizes `MapSortingKeySelector` | Customizes `ApplyCustomSorting` |
  | **Inheritance**    | Extends base fully         | Extends base fully         | Extends base fully        | Extends base, shifts focus |
  | **DI**             | Uses external config       | No extra dependencies      | No extra dependencies     | No extra dependencies    |

- **How They Differ**:  
  - **Strategy**:  
    - `DailySchedule` and `Trailers`: Broad, dynamic with dictionaries.  
    - `TrailerAssignment`: Simpler, static with `if` conditions.  
    - `TrailerTask`: Minimal, single-strategy with direct query mod.  
  - **Template**:  
    - Most use `MapSortingKeySelector` for lambda mapping.  
    - `TrailerTask` uses `ApplyCustomSorting` for direct `IQueryable` manipulation.  
  - **Inheritance**: All leverage base reflection, but `TrailerTask` shifts to a different method.  
  - **DI**: Only `DailySchedule` adds an external dependency (`IDailyScheduleConfiguration`).  

**Revision Tip**: Picture each as a "chef” with unique tools: big book, short list, big book, single spice.

---

## Step 6: Implementation in Context
- **What**: How patterns tie into repositories and contexts.  
- **Implementation**:  
  - **Repositories**:  
    - `GenericReadOnlySqlRepository`: Uses Strategy via `_sortingConfiguration` injection.  
      ```csharp
      query = query.OrderBy(sortExpr, sortFieldType, sorting.Order);
      ```
    - `GenericSqlRepository`: Inherits read-only capabilities, applies writes.  
  - **Contexts**:  
    - `ReadOnlyDocksContext`: Supports read-only sorting queries.  
    - `WritableDocksContext`: Supports write operations that might affect sorted data.  
  - **Execution**: EF Core translates lambdas or direct `OrderBy` to SQL `ORDER BY`.  
- **How It Works**:  
  - Strategy provides sorting logic.  
  - Template ensures a consistent base.  
  - Inheritance extends functionality.  
  - DI wires it all together.  
- **Purpose**: Creates a cohesive, flexible sorting system across data access layers.

**Revision Tip**: See this as a "restaurant chain" with chefs (configs), kitchens (contexts), and service (repos).

---

## Quick Recap Table
| **Pattern**         | **Implementation**                     | **Purpose**                          |
|--------------------|----------------------------------------|--------------------------------------|
| **Strategy**       | Interchangeable sorting via configs    | Dynamic, domain-specific sorting     |
| **Template**       | Base `DefaultSqlSortingConfiguration`  | Consistent structure, custom steps   |
| **Inheritance**    | Extends base for specialization        | Reuse and tailor functionality       |
| **DI**             | Injects configs into repositories      | Flexibility and testability          |

---

## Revision Tips
- **Mnemonic**: "S-T-I-D"  
  - **S**trategy, **T**emplate, **I**nheritance, **D**I.  
- **Visualize**:  
  - Sorting = Restaurant:  
    - Strategy = Recipe Book (configs).  
    - Template = Basic Cooking Guide (base).  
    - Inheritance = Family Recipes (extended).  
    - DI = Delivery Service (injection).

---

These notes provide a detailed overview of design patterns across the sorting configurations, their implementations, and differences, perfect for daily revision. Want to tweak these or dive deeper into a specific pattern’s use? Let me know!


Let’s create comprehensive, step-by-step notes providing a **broader overview of design patterns** used across your entire codebase, focusing on the sorting configurations (`DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, `TrailerTaskSortingConfiguration`), repositories (`GenericReadOnlySqlRepository`, `GenericSqlRepository`, etc.), contexts, and related components. We’ll cover key patterns like **Repository**, **Unit of Work**, **Dependency Injection (DI)**, and others observed, explaining their implementation in your docks management system. These notes are designed for daily revision to help you grasp the overall design structure.

---

# Overall Design Patterns Notes: Broader Overview  
**Goal**: Understand the architectural patterns driving the docks management system’s data access and sorting.  
**Primary Examples**: Sorting configs, repositories, contexts in `Docks.Infrastructure.Sql.Common` and related namespaces.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Repository Pattern
- **What**: Abstracts data access into a collection-like interface, hiding database details.  
- **Where**: `IGenericReadOnlySqlRepository<TEntity>`, `IGenericSqlRepository<TEntity>`, and their implementations (`GenericReadOnlySqlRepository`, `GenericSqlRepository`, mocks).  
- **Implementation**:  
  - **Interfaces**: Define CRUD and query operations:  
    ```csharp
    public interface IGenericReadOnlySqlRepository<TEntity>
    {
        Task<IList<TEntity>> GetListAsync(...);
        Task<TEntity> GetFirstAsync(...);
    }
    public interface IGenericSqlRepository<TEntity>
    {
        void Add(TEntity entity);
        Task CommitAsync(...);
    }
    ```
  - **Concrete Classes**:  
    - `GenericReadOnlySqlRepository`: Handles reads with `ReadOnlyDocksContext`.  
      ```csharp
      public async Task<IList<TEntity>> GetListAsync(...) { return await query.ToListAsync(); }
      ```
    - `GenericSqlRepository`: Adds writes with `WritableDocksContext`.  
      ```csharp
      public void Add(TEntity entity) => Context.Set<TEntity>().Add(entity);
      ```
    - **Mocks**: `GenericReadOnlySqlRepositoryMock`, `GenericSqlRepositoryMock` simulate behavior (e.g., caching, delays).  
  - **How It Works**:  
    - Repositories act as intermediaries between business logic and EF Core.  
    - Sorting configs plug into `ApplySorting` or `ApplyCustomSorting` for query customization.  
- **Purpose**: Simplifies data access, promotes testability, and separates concerns.

**Revision Tip**: Think of Repository as a "data waiter" serving dishes (entities) from the kitchen (database).

---

## Step 2: Unit of Work Pattern
- **What**: Manages a set of changes as a single transaction, coordinating repositories.  
- **Where**: EF Core `DbContext` (`DocksContext`, `ReadOnlyDocksContext`, `WritableDocksContext`) acts as the Unit of Work.  
- **Implementation**:  
  - **Contexts**:  
    - `DocksContext`: Base for model configuration.  
      ```csharp
      protected override void OnModelCreating(ModelBuilder modelBuilder) { modelBuilder.ApplyConfiguration(...); }
      ```
    - `WritableDocksContext`: Tracks changes, commits via `SaveChangesAsync`.  
      ```csharp
      public async Task CommitAsync(...) => await Context.SaveChangesAsync(ct);
      ```
  - **Repositories**: Use the context as a shared Unit of Work:  
    - `GenericSqlRepository`: Stages changes (e.g., `Add`, `Update`), commits via `CommitAsync`.  
    - No explicit `UnitOfWork` class; `DbContext` serves this role implicitly.  
  - **How It Works**:  
    - Repositories operate on a shared `DbContext` instance (e.g., `WritableDocksContext`).  
    - Changes are tracked (adds, updates, deletes) until `CommitAsync` persists them.  
    - Sorting affects query order but not the transaction itself.  
- **Purpose**: Ensures atomicity and consistency across multiple operations.

**Revision Tip**: Picture Unit of Work as a "kitchen manager" coordinating orders (changes) before serving (commit).

---

## Step 3: Dependency Injection (DI) Pattern
- **What**: Injects dependencies to decouple components and enable runtime flexibility.  
- **Where**: Throughout repositories, contexts, and sorting configs via `SqlDependencies.cs`.  
- **Implementation**:  
  - **Registration**:  
    ```csharp
    SqlModule.LoadSqlRepository<IGenericReadOnlySqlRepository<T>, GenericReadOnlySqlRepository<T>, ReadOnlyDocksContext>(...);
    serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<TrailerTaskModel>), typeof(TrailerTaskSortingConfiguration));
    ```
  - **Injection**:  
    - Repositories: Take contexts and sorting configs.  
      ```csharp
      public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
      ```
    - Contexts: Take `ISqlConnectionStringProvider`.  
      ```csharp
      protected DocksContext(ISqlConnectionStringProvider sqlConnectionStringProvider)
      ```
    - Sorting Configs: `DailyScheduleSortingConfiguration` takes `IDailyScheduleConfiguration`.  
      ```csharp
      public DailyScheduleSortingConfiguration(IDailyScheduleConfiguration dailyScheduleConfiguration)
      ```
  - **Mocks**: Add `IMemoryCache` for testing.  
    ```csharp
    public GenericReadOnlySqlRepositoryMock(..., IMemoryCache cache)
    ```
  - **How It Works**:  
    - DI container (e.g., `IServiceCollection`) resolves dependencies at runtime.  
    - Sorting configs are swapped (e.g., real vs. mock) based on environment (`IsUnderPerformance`).  
- **Purpose**: Enhances modularity, testability, and configuration flexibility.

**Revision Tip**: See DI as a "delivery service" bringing tools (contexts, configs) to chefs (repos).

---

## Step 4: Strategy Pattern
- **What**: Defines interchangeable algorithms (e.g., sorting strategies).  
- **Where**: Sorting configs via `ISqlSortingConfiguration<TEntity>`.  
- **Implementation**:  
  - **Interface**: `ISqlSortingConfiguration<TEntity>` defines the strategy contract.  
    ```csharp
    (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector);
    ```
  - **Concrete Strategies**:  
    - `DailyScheduleSortingConfiguration`: Dictionary-based (`SortKeyMapper`) with many strategies.  
    - `TrailerAssigmentSortingConfiguration`: `if`-based, fewer strategies.  
    - `TrailersSortingConfiguration`: Dictionary-based, trailer-specific strategies.  
    - `TrailerTaskSortingConfiguration`: Single strategy via `ApplyCustomSorting`.  
  - **Repository Use**: Injected as `_sortingConfiguration`, called for sorting.  
    ```csharp
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(sorting.SortBy);
    ```
  - **How It Works**:  
    - Each config provides a sorting algorithm (e.g., by priority, appointment time).  
    - Repositories apply the chosen strategy dynamically.  
- **Purpose**: Allows flexible, domain-specific sorting implementations.

**Revision Tip**: Think of Strategy as a "recipe book" with different sorting chapters.

---

## Step 5: Template Method Pattern
- **What**: Provides a base algorithm skeleton with customizable steps.  
- **Where**: `DefaultSqlSortingConfiguration<TEntity>` and its subclasses.  
- **Implementation**:  
  - **Base Template**: `DefaultSqlSortingConfiguration` offers reflection-based sorting.  
    ```csharp
    public virtual (LambdaExpression Lambda, Type PropertyType) MapSortingKeySelector(string keySelector) { ... }
    ```
  - **Customizations**:  
    - `DailyScheduleSortingConfiguration`: Uses `SortKeyMapper` to override.  
    - `TrailerAssigmentSortingConfiguration`: Uses `if` conditions.  
    - `TrailersSortingConfiguration`: Uses `SortKeyMapper`.  
    - `TrailerTaskSortingConfiguration`: Overrides `ApplyCustomSorting` (assumed base exists).  
  - **Repository Use**: Base methods act as fallbacks when custom logic doesn’t apply.  
  - **How It Works**:  
    - Base defines the sorting process (e.g., reflection).  
    - Subclasses customize steps (e.g., dictionary lookup, direct sorting).  
- **Purpose**: Ensures a consistent sorting framework with domain-specific extensions.

**Revision Tip**: Picture Template as a "basic recipe" with blanks for chefs to fill.

---

## Step 6: Inheritance
- **What**: Extends base classes for specialization and reuse.  
- **Where**: Sorting configs, repositories, contexts.  
- **Implementation**:  
  - **Sorting Configs**: All inherit from `DefaultSqlSortingConfiguration`.  
    ```csharp
    public class DailyScheduleSortingConfiguration : DefaultSqlSortingConfiguration<TrailerAssignmentModel>
    ```
  - **Repositories**: `GenericReadOnlySqlRepositoryMock` inherits from `GenericReadOnlySqlRepository`.  
    ```csharp
    public class GenericReadOnlySqlRepositoryMock<TEntity> : GenericReadOnlySqlRepository<TEntity>
    ```
  - **Contexts**: `DocksContext` inherits from `BaseSqlContext`.  
    ```csharp
    public abstract class DocksContext : BaseSqlContext
    ```
  - **How It Works**:  
    - Base provides generic functionality (e.g., reflection sorting, context setup).  
    - Subclasses add domain-specific logic (e.g., trailer sorting, mock behavior).  
- **Purpose**: Promotes code reuse and hierarchical specialization.

**Revision Tip**: See Inheritance as a "family cookbook" where kids tweak the parent’s recipes.

---

## Step 7: Facade Pattern (Influence)
- **What**: Simplifies complex subsystems behind a unified interface.  
- **Where**: Repositories and contexts (influence, not strict).  
- **Implementation**:  
  - **Repositories**: Hide EF Core complexity (e.g., `IQueryable`, SQL generation).  
    ```csharp
    public async Task<IList<TEntity>> GetListAsync(...) { return await query.ToListAsync(); }
    ```
  - **Contexts**: Wrap EF Core setup (e.g., `OnModelCreating`, `OnConfiguring`).  
    ```csharp
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { ... }
    ```
  - **How It Works**:  
    - Callers use simple methods (e.g., `GetListAsync`) without managing EF Core directly.  
    - Sorting configs integrate seamlessly via injected interfaces.  
- **Purpose**: Reduces complexity for business logic layers.

**Revision Tip**: Imagine Facade as a "waiter" hiding the kitchen chaos from diners.

---

## Quick Recap Table
| **Pattern**         | **Where**                        | **Implementation**                     | **Purpose**                          |
|--------------------|----------------------------------|----------------------------------------|--------------------------------------|
| **Repository**     | Repositories                    | Abstracts CRUD via interfaces          | Simplify data access                 |
| **Unit of Work**   | Contexts (DbContext)            | Tracks and commits changes             | Ensure transactional consistency     |
| **DI**             | Repos, Contexts, Configs        | Injects dependencies via DI            | Enhance flexibility, testability     |
| **Strategy**       | Sorting Configs                 | Interchangeable sorting algorithms     | Dynamic, domain-specific sorting     |
| **Template**       | Sorting Configs (Base)          | Base sorting with custom steps         | Consistent, extensible sorting       |
| **Inheritance**    | Configs, Repos, Contexts        | Extends base for specialization        | Reuse and tailor functionality       |
| **Facade**         | Repos, Contexts (Influence)     | Simplifies EF Core interaction         | Reduce complexity                    |

---

## Revision Tips
- **Mnemonic**: "R-U-D-S-T-I-F"  
  - **R**epository, **U**nit of Work, **D**I, **S**trategy, **T**emplate, **I**nheritance, **F**acade.  
- **Visualize**:  
  - System = Restaurant:  
    - Repository = Waiter (data access).  
    - Unit of Work = Kitchen Manager (transactions).  
    - DI = Delivery Service (dependencies).  
    - Strategy = Recipe Book (sorting).  
    - Template = Basic Guide (base config).  
    - Inheritance = Family Recipes (extensions).  
    - Facade = Friendly Waiter (simplicity).

---

These notes provide a broad overview of design patterns across your codebase, focusing on their implementation in sorting configs, repositories, and contexts, perfect for daily revision. Want to tweak these or dive deeper into a specific pattern’s application? Let me know!

Let’s examine whether the **Builder Pattern** is used in your codebase, particularly in the context of the sorting configurations, repositories, and contexts we’ve explored (e.g., `DailyScheduleSortingConfiguration`, `GenericReadOnlySqlRepository`, `DocksContext`, etc.). I’ll provide a step-by-step analysis to determine its presence, explain what the Builder Pattern is, and then update our broader design pattern notes to reflect this. These notes will remain concise and suitable for daily revision.

---

# Analysis: Is the Builder Pattern Used Here?

## Step 1: Understand the Builder Pattern
- **What**: A creational pattern that separates the construction of a complex object from its representation, allowing step-by-step building with a fluent or structured API.  
- **Key Features**:  
  - A `Builder` class with methods to set properties incrementally.  
  - A `Build()` method to produce the final object.  
  - Often used for objects with many optional parameters or complex setup (e.g., `StringBuilder`, EF Core’s `ModelBuilder`).  
- **Example**:  
  ```csharp
  var builder = new CarBuilder();
  Car car = builder.SetColor("Red").SetWheels(4).Build();
  ```
- **Purpose**: Simplifies creation of objects with multiple configuration steps.

**Revision Tip**: Think of Builder as a "recipe assembler" mixing ingredients step-by-step.

---

## Step 2: Examine Sorting Configurations
- **Where**: `DailyScheduleSortingConfiguration`, `TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, `TrailerTaskSortingConfiguration`.  
- **Analysis**:  
  - **Construction**:  
    - `DailyScheduleSortingConfiguration` takes `IDailyScheduleConfiguration` in its constructor and initializes `SortKeyMapper` internally:  
      ```csharp
      public DailyScheduleSortingConfiguration(IDailyScheduleConfiguration dailyScheduleConfiguration)
      {
          _dailyScheduleConfiguration = dailyScheduleConfiguration;
          SortKeyMapper = new Dictionary<string, Func<(LambdaExpression Lambda, Type PropertyType)>> { ... };
      }
      ```
    - Others (`TrailerAssigmentSortingConfiguration`, `TrailersSortingConfiguration`, `TrailerTaskSortingConfiguration`) have simple constructors with no parameters beyond base requirements.  
  - **No Builder**:  
    - No separate `Builder` class or fluent API for step-by-step construction.  
    - Configuration is fixed in the constructor (e.g., `SortKeyMapper` or key definitions).  
    - No `Build()` method or incremental setup exposed.  
- **Conclusion**: **Builder Pattern is not used here.** Sorting configs are constructed directly via constructors.

**Revision Tip**: See these as "pre-mixed spices," not assembled step-by-step.

---

## Step 3: Examine Repositories
- **Where**: `GenericReadOnlySqlRepository<TEntity>`, `GenericSqlRepository<TEntity>`, and their mock variants.  
- **Analysis**:  
  - **Construction**:  
    - `GenericReadOnlySqlRepository` takes `ReadOnlyDocksContext` and `ISqlSortingConfiguration<TEntity>`:  
      ```csharp
      public GenericReadOnlySqlRepository(ReadOnlyDocksContext context, ISqlSortingConfiguration<TEntity> sortingConfiguration)
      ```
    - `GenericSqlRepositoryMock` takes `WritableDocksContext`:  
      ```csharp
      public GenericSqlRepositoryMock(WritableDocksContext context) : base(context) { }
      ```
  - **No Builder**:  
    - Dependencies are injected directly via constructor injection (DI).  
    - No fluent API or separate builder class for incremental setup.  
    - Construction is straightforward, with no complex object assembly.  
- **Conclusion**: **Builder Pattern is not used here.** Repositories rely on DI for instantiation.

**Revision Tip**: Picture repos as "chefs" handed tools directly, not built step-by-step.

---

## Step 4: Examine Contexts
- **Where**: `DocksContext`, `ReadOnlyDocksContext`, `WritableDocksContext`, and `BaseSqlContext`.  
- **Analysis**:  
  - **Construction**:  
    - `BaseSqlContext` takes `ISqlConnectionStringProvider`:  
      ```csharp
      protected BaseSqlContext(ISqlConnectionStringProvider sqlConnectionStringProvider)
      ```
    - `DocksContext` extends this with model configurations:  
      ```csharp
      protected DocksContext(ISqlConnectionStringProvider sqlConnectionStringProvider) : base(sqlConnectionStringProvider) { }
      ```
  - **EF Core Configuration**:  
    - `OnModelCreating` applies many configurations:  
      ```csharp
      modelBuilder.ApplyConfiguration(new TrailerTaskModelConfiguration());
      ```
    - `OnConfiguring` sets up SQL Server options:  
      ```csharp
      optionsBuilder.UseSqlServer(connectionString, builder => builder.EnableRetryOnFailure());
      ```
  - **Builder-Like Influence**:  
    - EF Core’s `ModelBuilder` and `DbContextOptionsBuilder` internally use a builder-like API (e.g., chaining methods like `EnableRetryOnFailure()`).  
    - However, this is EF Core’s implementation, not your codebase’s direct use of a custom Builder Pattern.  
    - Your contexts don’t expose a separate builder class or fluent API for external use.  
- **Conclusion**: **Builder Pattern is not directly used here.** Contexts rely on DI and EF Core’s internal builders.

**Revision Tip**: See contexts as "kitchens" set up by a manager (DI), not built incrementally by you.

---

## Step 5: Overall Assessment
- **What**: Check for Builder Pattern across the system.  
- **Findings**:  
  - **Sorting Configs**: Use constructors with fixed setups (e.g., dictionaries, `if` conditions).  
  - **Repositories**: Depend on DI for instantiation, no step-by-step construction.  
  - **Contexts**: Leverage EF Core’s internal builders (`ModelBuilder`, `DbContextOptionsBuilder`) but don’t implement a custom Builder Pattern.  
  - **No Custom Builder**: No evidence of a dedicated `Builder` class, fluent API, or `Build()` method for complex object creation in your code.  
- **Conclusion**: **The Builder Pattern is not explicitly used in your codebase.** Other patterns (e.g., Repository, DI) dominate, and EF Core provides builder-like functionality internally.

**Revision Tip**: Think of your system as a "ready-made kitchen" (no custom assembly line).

---

## Updated Overall Design Patterns Notes
Here’s the revised broader overview, incorporating the Builder Pattern analysis:

---

# Overall Design Patterns Notes: Broader Overview  
**Goal**: Understand the architectural patterns driving the docks management system’s data access and sorting.  
**Primary Examples**: Sorting configs, repositories, contexts in `Docks.Infrastructure.Sql.Common` and related namespaces.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Repository Pattern
- **What**: Abstracts data access into a collection-like interface.  
- **Implementation**: `GenericReadOnlySqlRepository`, `GenericSqlRepository` use `ReadOnlyDocksContext` and `WritableDocksContext` for queries and CRUD.  
- **Purpose**: Simplifies data access, promotes testability.

**Revision Tip**: Think of Repository as a "data waiter" serving dishes.

---

## Step 2: Unit of Work Pattern
- **What**: Manages changes as a single transaction.  
- **Implementation**: `DbContext` (`WritableDocksContext`) tracks and commits changes via `SaveChangesAsync`.  
- **Purpose**: Ensures atomicity across operations.

**Revision Tip**: Picture Unit of Work as a "kitchen manager" coordinating orders.

---

## Step 3: Dependency Injection (DI) Pattern
- **What**: Injects dependencies for decoupling.  
- **Implementation**: `SqlDependencies.cs` registers configs, contexts, repos (e.g., `SqlModule.LoadSqlRepository`).  
- **Purpose**: Enhances flexibility, testability.

**Revision Tip**: See DI as a "delivery service" bringing tools to chefs.

---

## Step 4: Strategy Pattern
- **What**: Defines interchangeable algorithms (sorting).  
- **Implementation**: Sorting configs (`DailyScheduleSortingConfiguration`, etc.) use `MapSortingKeySelector` or `ApplyCustomSorting`.  
- **Purpose**: Allows dynamic, domain-specific sorting.

**Revision Tip**: Think of Strategy as a "recipe book" with sorting chapters.

---

## Step 5: Template Method Pattern
- **What**: Provides a base algorithm with custom steps.  
- **Implementation**: `DefaultSqlSortingConfiguration` offers reflection-based sorting, overridden by configs.  
- **Purpose**: Ensures consistency with specialization.

**Revision Tip**: Picture Template as a "basic recipe" with blanks to fill.

---

## Step 6: Inheritance
- **What**: Extends base classes for reuse.  
- **Implementation**: Configs inherit from `DefaultSqlSortingConfiguration`, repos from `BaseSqlRepository`.  
- **Purpose**: Promotes code reuse and tailoring.

**Revision Tip**: See Inheritance as a "family cookbook" with tweaks.

---

## Step 7: Facade Pattern (Influence)
- **What**: Simplifies complex subsystems.  
- **Implementation**: Repos and contexts hide EF Core complexity (e.g., `GetListAsync`).  
- **Purpose**: Reduces complexity for callers.

**Revision Tip**: Imagine Facade as a "waiter" hiding kitchen chaos.

---

## Step 8: Builder Pattern (Not Used)
- **What**: Constructs complex objects step-by-step (not present).  
- **Implementation**:  
  - No custom `Builder` class or fluent API in your code.  
  - EF Core’s `ModelBuilder` and `DbContextOptionsBuilder` provide builder-like behavior internally.  
- **Purpose**: Would simplify complex object creation (not needed here due to DI).

**Revision Tip**: Think of Builder as an "absent assembler" replaced by direct setups.

---

## Quick Recap Table
| **Pattern**         | **Where**                        | **Implementation**                     | **Purpose**                          |
|--------------------|----------------------------------|----------------------------------------|--------------------------------------|
| **Repository**     | Repositories                    | Abstracts CRUD via interfaces          | Simplify data access                 |
| **Unit of Work**   | Contexts (DbContext)            | Tracks and commits changes             | Ensure transactional consistency     |
| **DI**             | Repos, Contexts, Configs        | Injects dependencies via DI            | Enhance flexibility, testability     |
| **Strategy**       | Sorting Configs                 | Interchangeable sorting algorithms     | Dynamic, domain-specific sorting     |
| **Template**       | Sorting Configs (Base)          | Base sorting with custom steps         | Consistent, extensible sorting       |
| **Inheritance**    | Configs, Repos, Contexts        | Extends base for specialization        | Reuse and tailor functionality       |
| **Facade**         | Repos, Contexts (Influence)     | Simplifies EF Core interaction         | Reduce complexity                    |
| **Builder**        | Not Used                        | EF Core internal use only              | (Would) Simplify complex creation   |

---

## Revision Tips
- **Mnemonic**: "R-U-D-S-T-I-F-B"  
  - **R**epository, **U**nit of Work, **D**I, **S**trategy, **T**emplate, **I**nheritance, **F**acade, **B**uilder (absent).  
- **Visualize**:  
  - System = Restaurant:  
    - Repository = Waiter.  
    - Unit of Work = Manager.  
    - DI = Delivery.  
    - Strategy = Recipe Book.  
    - Template = Basic Guide.  
    - Inheritance = Family Recipes.  
    - Facade = Friendly Waiter.  
    - Builder = Missing Assembler.

---

These updated notes provide a broad overview of design patterns in your codebase, confirming the absence of a custom Builder Pattern while noting EF Core’s internal use. Perfect for daily revision! Want to tweak these or explore another aspect? Let me know!

Let’s refine the notes to explicitly highlight how the **Unit of Work Pattern** is used alongside the **Repository Pattern** in your codebase, focusing on the generic implementations (`GenericReadOnlySqlRepository<TEntity>` and `GenericSqlRepository<TEntity>`). We’ll integrate this into a broader overview of design patterns, ensuring the notes are concise, step-by-step, and suitable for daily revision. The focus will be on their combined implementation and interaction, based on the provided code.

---

# Overall Design Patterns Notes: Unit of Work and Generic Repository  
**Goal**: Understand how Unit of Work and Repository patterns collaborate in generic data access.  
**Primary Examples**: `GenericReadOnlySqlRepository<TEntity>`, `GenericSqlRepository<TEntity`, and contexts in `Docks.Infrastructure.Sql.Common`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Repository Pattern with Generic Implementation
- **What**: Abstracts data access into a generic, collection-like interface.  
- **Where**: `IGenericReadOnlySqlRepository<TEntity>` and `IGenericSqlRepository<TEntity>` with their implementations.  
- **Implementation**:  
  - **Interfaces**: Define read-only and CRUD operations:  
    ```csharp
    public interface IGenericReadOnlySqlRepository<TEntity>
    {
        Task<IList<TEntity>> GetListAsync(...);
    }
    public interface IGenericSqlRepository<TEntity>
    {
        void Add(TEntity entity);
        Task CommitAsync(...);
    }
    ```
  - **Generic Classes**:  
    - `GenericReadOnlySqlRepository`: Uses `ReadOnlyDocksContext` for queries.  
      ```csharp
      public async Task<IList<TEntity>> GetListAsync(...) { return await query.ToListAsync(); }
      ```
    - `GenericSqlRepository`: Uses `WritableDocksContext` for CRUD.  
      ```csharp
      public void Add(TEntity entity) => Context.Set<TEntity>().Add(entity);
      ```
- **How It Works**:  
  - Acts as a facade over EF Core, providing a unified API for `TEntity` (e.g., `TrailerTaskModel`).  
  - Delegates persistence to the context (Unit of Work).  
- **Purpose**: Simplifies data access, promotes testability.

**Revision Tip**: Think of Repository as a "data waiter" serving dishes from the kitchen.

---

## Step 2: Unit of Work Pattern with Generic Repository
- **What**: Manages a set of changes as a single transaction, coordinating generic repositories.  
- **Where**: EF Core `DbContext` (`WritableDocksContext`, `ReadOnlyDocksContext`) as the Unit of Work, used by repositories.  
- **Implementation**:  
  - **Contexts**:  
    - `WritableDocksContext`: Tracks changes and commits them.  
      ```csharp
      public class WritableDocksContext : DocksContext
      {
          public WritableDocksContext(ISqlConnectionStringProvider provider) : base(provider) { }
      }
      ```
    - `ReadOnlyDocksContext`: Read-only, no tracking by default.  
      ```csharp
      public class ReadOnlyDocksContext : DocksContext { ... }
      ```
  - **Generic Repository Integration**:  
    - `GenericSqlRepository`:  
      - Stages changes (e.g., `Add`, `Update`) in the context:  
        ```csharp
        public void Add(TEntity entity) => Context.Set<TEntity>().Add(entity);
        public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);
        ```
      - Commits via `CommitAsync`:  
        ```csharp
        public async Task CommitAsync(CancellationToken ct) => await Context.SaveChangesAsync(ct);
        ```
    - `GenericReadOnlySqlRepository`: Uses context for queries, no commits:  
      ```csharp
      public async Task<IList<TEntity>> GetListAsync(...) { return await Context.Set<TEntity>().ToListAsync(); }
      ```
  - **How It Works**:  
    - Repositories operate on a shared `DbContext` instance (e.g., `WritableDocksContext`).  
    - `WritableDocksContext` tracks all changes (adds, updates, deletes) as a unit.  
    - `CommitAsync` persists the unit to the database; `ReadOnlyDocksContext` skips tracking for efficiency.  
- **Purpose**: Ensures transactional consistency across generic repository operations.

**Revision Tip**: Picture Unit of Work as a "kitchen manager" tracking orders (changes) for the waiter (repository).

---

## Step 3: Combining Unit of Work and Repository
- **What**: Use Unit of Work (`DbContext`) as the persistence layer for Repository operations.  
- **Where**: `GenericSqlRepository` and `GenericReadOnlySqlRepository` with contexts.  
- **Implementation**:  
  - **Shared Context**:  
    - `GenericSqlRepository` uses `WritableDocksContext` as its Unit of Work:  
      ```csharp
      public class GenericSqlRepository<TEntity> : BaseSqlRepository<WritableDocksContext>
      {
          public GenericSqlRepository(WritableDocksContext context) : base(context) { }
      }
      ```
    - `GenericReadOnlySqlRepository` uses `ReadOnlyDocksContext`:  
      ```csharp
      public class GenericReadOnlySqlRepository<TEntity> : BaseSqlRepository<ReadOnlyDocksContext>
      ```
  - **Workflow**:  
    - **Create/Update/Delete**: `GenericSqlRepository` stages changes in `WritableDocksContext` (e.g., `Add`, `Remove`).  
    - **Commit**: `CommitAsync` calls `SaveChangesAsync` on the context, finalizing the unit.  
      ```csharp
      var repo = new GenericSqlRepository<TrailerTaskModel>(context);
      repo.Add(new TrailerTaskModel { ... });
      await repo.CommitAsync(); // Persists all changes
      ```
    - **Read**: `GenericReadOnlySqlRepository` queries without modifying the unit.  
      ```csharp
      var repo = new GenericReadOnlySqlRepository<TrailerTaskModel>(context);
      var tasks = await repo.GetListAsync(x => x.IsCompleted == false);
      ```
  - **How It Works**:  
    - Repositories act as clients of the Unit of Work (`DbContext`).  
    - `WritableDocksContext` coordinates changes across multiple repository calls.  
    - Sorting (e.g., via `TrailerTaskSortingConfiguration`) integrates but doesn’t affect the unit’s transaction.  
- **Purpose**: Combines abstraction (Repository) with transactional control (Unit of Work).

**Revision Tip**: See this as a "waiter" (repo) and "manager" (unit) team serving and tracking orders.

---

## Step 4: Dependency Injection (DI) Pattern
- **What**: Injects dependencies to decouple components, including Unit of Work and Repository.  
- **Where**: `SqlDependencies.cs` wires repositories and contexts.  
- **Implementation**:  
  - **Registration**:  
    ```csharp
    SqlModule.LoadSqlRepository<IGenericSqlRepository<T>, GenericSqlRepository<T>, WritableDocksContext>(...);
    SqlModule.LoadSqlRepository<IGenericReadOnlySqlRepository<T>, GenericReadOnlySqlRepository<T>, ReadOnlyDocksContext>(...);
    ```
  - **Injection**:  
    - Repositories take contexts as their Unit of Work:  
      ```csharp
      public GenericSqlRepository(WritableDocksContext context) : base(context) { }
      ```
    - Sorting configs (e.g., `DailyScheduleSortingConfiguration`) may take additional dependencies:  
      ```csharp
      public DailyScheduleSortingConfiguration(IDailyScheduleConfiguration dailyScheduleConfiguration)
      ```
  - **How It Works**:  
    - DI container resolves `WritableDocksContext` or `ReadOnlyDocksContext` as the Unit of Work.  
    - Repositories use the injected context to perform operations within the unit.  
- **Purpose**: Enables flexible instantiation and testing (e.g., mock contexts).

**Revision Tip**: Imagine DI as a "delivery service" bringing the manager (context) to the waiter (repo).

---

## Step 5: Strategy Pattern (Sorting Integration)
- **What**: Provides interchangeable sorting algorithms within repositories.  
- **Where**: Sorting configs (`TrailerTaskSortingConfiguration`, etc.) and repositories.  
- **Implementation**:  
  - **Configs**: Implement `ISqlSortingConfiguration<TEntity>` with strategies:  
    - `TrailerTaskSortingConfiguration`: `ApplyCustomSorting` for priority sorting.  
      ```csharp
      query.OrderBy(x => x.AssignedPriority).ThenBy(x => x.TaskDueDatetimeUtc);
      ```
    - Others: Use `MapSortingKeySelector` for lambda-based strategies.  
  - **Repositories**: Integrate via `_sortingConfiguration`:  
    ```csharp
    query = _sortingConfiguration.ApplyCustomSorting(query, sorting); // TrailerTask
    // or
    var (sortExpr, sortFieldType) = _sortingConfiguration.MapSortingKeySelector(...);
    ```
  - **How It Works**:  
    - Unit of Work (`DbContext`) provides the queryable data.  
    - Repository applies the Strategy (sorting) to shape the query.  
- **Purpose**: Adds flexibility to Repository queries without altering Unit of Work.

**Revision Tip**: See Strategy as a "recipe book" the waiter uses to season dishes.

---

## Quick Recap Table
| **Pattern**         | **Where**                        | **Implementation**                     | **Purpose**                          |
|--------------------|----------------------------------|----------------------------------------|--------------------------------------|
| **Repository**     | `GenericReadOnlySqlRepository`, `GenericSqlRepository` | Abstracts CRUD via interfaces | Simplify data access                 |
| **Unit of Work**   | `WritableDocksContext`, `ReadOnlyDocksContext` | Tracks and commits changes    | Ensure transactional consistency     |
| **DI**             | Repos, Contexts, Configs        | Injects contexts and configs           | Enhance flexibility, testability     |
| **Strategy**       | Sorting Configs                 | Interchangeable sorting algorithms     | Dynamic, domain-specific sorting     |

---

## Revision Tips
- **Mnemonic**: "R-U-D-S"  
  - **R**epository, **U**nit of Work, **D**I, **S**trategy.  
- **Visualize**:  
  - System = Restaurant:  
    - Repository = Waiter (data access).  
    - Unit of Work = Manager (context tracking).  
    - DI = Delivery Service (dependencies).  
    - Strategy = Recipe Book (sorting).  

---

These notes emphasize how the **Unit of Work** (`DbContext`) and **Repository** (`Generic*SqlRepository`) patterns work together in your codebase, alongside **DI** and **Strategy** for sorting, perfect for daily revision. Want to tweak these or explore another pattern in this context? Let me know!


It seems like there might be a typo in your request. I assume you meant "Load IoC Containers" (Inversion of Control Containers), referring to how Inversion of Control (IoC) or Dependency Injection (DI) containers are loaded and utilized in your codebase. In the context of your docks management system, this likely relates to how dependencies (like repositories, contexts, and sorting configurations) are registered and resolved using a DI framework, such as Microsoft.Extensions.DependencyInjection. Let’s create concise, step-by-step notes on this, focusing on the implementation in `SqlDependencies.cs` and its integration with the Repository, Unit of Work, and sorting patterns. These notes are designed for daily revision to help you remember the process.

---

# IoC Container Notes: Loading and Implementation  
**Goal**: Understand how IoC containers manage dependency injection for repositories, contexts, and sorting configurations.  
**Primary Example**: `SqlDependencies.cs` in `Docks.Dependencies.Infrastructure.Sql`, using `Microsoft.Extensions.DependencyInjection`.  
**Current Date**: April 01, 2025 (for context).

---

## Step 1: Define the IoC Container
- **What**: An IoC container (e.g., `IServiceCollection`) manages dependency registration and resolution.  
- **Where**: `SqlDependencies.cs` uses `IServiceCollection` from ASP.NET Core’s DI framework.  
- **Key Code**:  
  ```csharp
  public static IServiceCollection RegisterSqlRepository<T>(this IServiceCollection serviceCollection, IConfiguration configuration)
  ```
- **Key Features**:  
  - **Extension Method**: Extends `IServiceCollection` for custom registrations.  
  - **Configuration**: Takes `IConfiguration` to adjust behavior (e.g., `IsUnderPerformance`).  
- **Purpose**: Centralizes dependency setup for the application.

**Revision Tip**: Think of the IoC container as a "supply room" storing tools (dependencies) for chefs.

---

## Step 2: Register Sorting Configurations
- **What**: Add sorting configurations to the IoC container for injection.  
- **Where**: `RegisterSqlRepository<T>` in `SqlDependencies.cs`.  
- **Key Code**:  
  ```csharp
  serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<>), typeof(DefaultSqlSortingConfiguration<>));
  serviceCollection.AddTransient(typeof(ISqlSortingConfiguration<TrailerAssignmentModel>), typeof(TrailerAssigmentSortingConfiguration));
  serviceCollection.AddTransient(typeof(ISqlTrailersSortingConfiguration<TrailerModel>), typeof(TrailersSortingConfiguration));
  serviceCollection.AddTransient(typeof(ISqlDailyScheduleSortingConfiguration<TrailerAssignmentModel>), typeof(DailyScheduleSortingConfiguration));
  ```
- **Key Features**:  
  - **Transient Lifetime**: New instance per request (`AddTransient`).  
  - **Generic and Specific**: Registers both generic (`DefaultSqlSortingConfiguration`) and specialized configs.  
- **How It Works**:  
  - Sorting configs are registered as implementations of `ISqlSortingConfiguration<TEntity>`.  
  - Resolved by repositories (e.g., `GenericReadOnlySqlRepository`) via constructor injection.  
- **Purpose**: Ensures sorting strategies are available to repositories.

**Revision Tip**: Picture this as "stocking the supply room" with seasoning kits (sorting configs).

---

## Step 3: Register Generic Repositories
- **What**: Add generic repositories to the IoC container with their Unit of Work (contexts).  
- **Where**: `RegisterSqlRepository<T>` with `SqlModule.LoadSqlRepository`.  
- **Key Code**:  
  ```csharp
  if (CatalogExtensions.IsUnderPerformance(configuration))
  {
      serviceCollection.RegisterMemoryCache();
      SqlModule.LoadSqlRepository<IGenericReadOnlySqlRepository<T>, GenericReadOnlySqlRepositoryMock<T>, ReadOnlyDocksContext>(serviceCollection);
      SqlModule.LoadSqlRepository<IGenericSqlRepository<T>, GenericSqlRepositoryMock<T>, WritableDocksContext>(serviceCollection);
  }
  else
  {
      SqlModule.LoadSqlRepository<IGenericReadOnlySqlRepository<T>, GenericReadOnlySqlRepository<T>, ReadOnlyDocksContext>(serviceCollection);
      SqlModule.LoadSqlRepository<IGenericSqlRepository<T>, GenericSqlRepository<T>, WritableDocksContext>(serviceCollection);
  }
  ```
- **Key Features**:  
  - **Conditional Registration**: Switches between real (`Generic*SqlRepository`) and mock (`Generic*SqlRepositoryMock`) based on `IsUnderPerformance`.  
  - **Scoped Lifetime**: Assumed in `SqlModule.LoadSqlRepository` (typically `AddScoped` for EF Core contexts).  
  - **Contexts as Unit of Work**: Pairs `ReadOnlyDocksContext` with read-only repo, `WritableDocksContext` with read-write repo.  
- **How It Works**:  
  - Repositories are registered with their specific `DbContext` (Unit of Work).  
  - DI resolves these when a service requests `IGenericSqlRepository<T>`.  
- **Purpose**: Provides repositories with their Unit of Work, enabling data access.

**Revision Tip**: See this as "assigning chefs" (repos) with kitchens (contexts) from the supply room.

---

## Step 4: Load the IoC Container
- **What**: Build the service provider to resolve dependencies at runtime.  
- **Where**: Application startup (e.g., `Startup.cs` or `Program.cs`, not fully shown but inferred).  
- **Key Code (Inferred)**:  
  ```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.RegisterSqlRepository<TrailerTaskModel>(Configuration);
      // Other registrations...
  }

  // In Program.cs or similar
  var serviceProvider = services.BuildServiceProvider();
  var repo = serviceProvider.GetService<IGenericSqlRepository<TrailerTaskModel>>();
  ```
- **Key Features**:  
  - **`RegisterSqlRepository`**: Extension method populates `IServiceCollection`.  
  - **`BuildServiceProvider`**: Creates the IoC container (`IServiceProvider`) to resolve instances.  
- **How It Works**:  
  - Startup calls `RegisterSqlRepository` to load all dependencies.  
  - `IServiceProvider` resolves repos, contexts, and sorting configs as needed (e.g., via constructor injection).  
- **Purpose**: Initializes the IoC container for the application lifecycle.

**Revision Tip**: Imagine this as "opening the supply room" to hand out tools when needed.

---

## Step 5: Integrate with Design Patterns
- **What**: Combine IoC with Repository, Unit of Work, and Strategy patterns.  
- **Where**: Across repositories, contexts, and sorting configs.  
- **Implementation**:  
  - **Repository Pattern**:  
    - IoC provides `GenericSqlRepository` with `WritableDocksContext` as its Unit of Work.  
      ```csharp
      public GenericSqlRepository(WritableDocksContext context) : base(context) { }
      ```
  - **Unit of Work Pattern**:  
    - `WritableDocksContext` tracks changes staged by repositories, resolved via IoC.  
      ```csharp
      await repo.CommitAsync(); // Uses injected context
      ```
  - **Strategy Pattern (Sorting)**:  
    - IoC injects sorting configs (e.g., `TrailerTaskSortingConfiguration`) into repositories.  
      ```csharp
      public GenericReadOnlySqlRepository(..., ISqlSortingConfiguration<TEntity> sortingConfiguration)
      ```
  - **How It Works**:  
    - IoC container wires repositories with their contexts (Unit of Work) and sorting strategies.  
    - Repositories use these to perform operations (e.g., `GetListAsync` with sorting).  
- **Purpose**: Orchestrates patterns for a cohesive data access system.

**Revision Tip**: See IoC as a "supply manager" coordinating waiters (repos), kitchens (contexts), and recipes (sorting).

---

## Quick Recap Table
| **Step**                | **Focus**                  | **Key Code**                          | **Purpose**                          |
|-------------------------|----------------------------|---------------------------------------|--------------------------------------|
| 1. Define IoC           | Container Setup            | `IServiceCollection`                  | Centralize dependency management     |
| 2. Register Sorting     | Sorting Configs            | `AddTransient` for configs            | Provide sorting strategies           |
| 3. Register Repos       | Repositories & Contexts    | `LoadSqlRepository` with mocks/real   | Wire repos with Unit of Work         |
| 4. Load Container       | Build Provider             | `BuildServiceProvider`                | Resolve dependencies at runtime      |
| 5. Integrate Patterns   | Combine with Patterns      | Injection into repos/contexts         | Enable cohesive data access          |

---

## Revision Tips
- **Mnemonic**: "D-R-R-L-I"  
  - **D**efine, **R**egister Sorting, **R**egister Repos, **L**oad, **I**ntegrate.  
- **Visualize**:  
  - IoC = Restaurant Supply Room:  
    - Define = Setting Up the Room.  
    - Register Sorting = Stocking Spices.  
    - Register Repos = Assigning Chefs and Kitchens.  
    - Load = Opening the Room.  
    - Integrate = Chefs Using Supplies.

---

These notes detail how the IoC container (`IServiceCollection`) is loaded and implemented in your codebase, focusing on its role in supporting the Repository, Unit of Work, and Strategy patterns. Perfect for daily revision! Want to tweak these or explore another aspect (e.g., specific DI registrations)? Let me know!