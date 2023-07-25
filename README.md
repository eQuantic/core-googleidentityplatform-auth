# eQuantic.Core.Api.Crud Library

The **eQuantic Core API CRUD** provides all the implementation needed to publish CRUD APIs.

To install **eQuantic.Core.Api.Crud**, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console)
```dos
Install-Package eQuantic.Core.Api.Crud
```

## Example of implementation

### The data entities
```csharp
[Table("orders")]
public class OrderData : EntityDataBase
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public virtual ICollection<OrderItemData> Items { get; set; } = new HashSet<OrderItemData>();
}

[Table("orderItems")]
public class OrderItemData : EntityDataBase, IWithReferenceId<OrderItemData, int>
{
    [Key]
    public int Id { get; set; }
    public int OrderId { get; set; }
    
    [ForeignKey(nameof(OrderId))]
    public virtual OrderData? Order { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
```

### The models
```csharp
public class Order
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

### The request models
```csharp
public class OrderRequest
{
    public DateTime? Date { get; set; }
}

public class OrderItemRequest
{
    public string? Name { get; set; }
}
```
### The mappers

```csharp
public class OrderMapper : IMapper<OrderData, Order>, IMapper<OrderRequest, OrderData>
{
    public Order? Map(OrderData? source)
    {
        return Map(source, new Order());
    }

    public Order? Map(OrderData? source, Order? destination)
    {
        if (source == null)
        {
            return null;
        }

        if (destination == null)
        {
            return Map(source);
        }

        destination.Id = source.Id;
        destination.Date = source.Date;

        return destination;
    }

    public OrderData? Map(OrderRequest? source)
    {
        return Map(source, new OrderData());
    }

    public OrderData? Map(OrderRequest? source, OrderData? destination)
    {
        if (source == null)
        {
            return null;
        }

        if (destination == null)
        {
            return Map(source);
        }
        
        destination.Date = source.Date ?? DateTime.UtcNow;

        return destination;
    }
}
```
### The services
```csharp
public interface IOrderService : ICrudServiceBase<Order, OrderRequest>
{
    
}

[MapCrudEndpoints]
public class OrderService : CrudServiceBase<Order, OrderRequest, OrderData, UserData>, IOrderService
{
    public OrderService(IQueryableUnitOfWork unitOfWork, IMapperFactory mapperFactory) : base(unitOfWork, mapperFactory)
    {
    }
}
```

### The `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Services.AddDbContext<ExampleDbContext>(opt =>
    opt.UseInMemoryDatabase("ExampleDb"));
        
builder.Services.AddQueryableRepositories<ExampleUnitOfWork>(opt =>
{
    opt.FromAssembly(assembly)
        .AddLifetime(ServiceLifetime.Scoped);
});

builder.Services
    .AddMappers(opt => opt.FromAssembly(assembly))
    .AddTransient<IExampleService, ExampleService>()
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
    .AddFilterModelBinder()
    .AddSortModelBinder();

builder.Services
    .AddEndpointsApiExplorer()
    .AddApiDocumentation(opt => opt.WithTitle("Example API"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocumentation();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.MapCrud<Order, OrderRequest, IOrderService>();

app.Run();
```

or

```csharp
...
app.MapAllCrud(assembly);
app.Run();
```