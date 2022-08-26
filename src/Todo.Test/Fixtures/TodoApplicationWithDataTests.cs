using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.Application;
using Todo.Application.Contract.Todo;
using Todo.Application.Contract.Utils;
using Todo.Application.Utils;
using Todo.Infrastructure.EFCore;
using Todo.Infrastructure.EFCore.Repositories;
using Todo.Test.Seeds;

namespace Todo.Test.Fixtures;

public class TodoApplicationWithDataTests
{
    protected TodoDbContext? Context;
    protected ITodoApplication Application = null!;
    private ILogger _logger = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase("mr_lottery_db").Options;

        Context = new TodoDbContext(options);
        var todoRepository = new TodoRepository(Context);

        _logger = new Logger();

        var tagRepository = new TagRepository(Context);
        Application = new TodoApplication(todoRepository, tagRepository, _logger);

        SeedData.AddData(Context);
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsListOfTasks()
    {
        var tasks = await Application.GetAll();

        Assert.That(tasks, Is.Not.Empty);
    }

    [Test]
    public async Task GetById_WhenCalled_ReturnsExpectedTodo()
    {
        var result = await Application.GetById(1);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task Delete_WhenTodoExist_RemoveExpectedTodo()
    {
        Assert.That(await Application.GetById(2), Is.Not.Null);

        await Application.Delete(2);

        Assert.That(async () => await Application.GetById(2), Throws.Exception);
    }

    [Test]
    public async Task ChangeStatus_WhenTodoIsExist_StatusChanged()
    {
        await Application.ChangeStatus(1);

        var afterChangeStatus = await Application.GetById(1);

        Assert.That(afterChangeStatus.IsDone, Is.EqualTo(true));

        await Application.ChangeStatus(1);

        var afterChangeStatusSecondTime = await Application.GetById(1);

        Assert.That(afterChangeStatusSecondTime.IsDone, Is.EqualTo(false));
    }

    [Test]
    public async Task Create_WhenDataIsValid_ShowSuccessLog()
    {
        await Application.Create(new TodoCommand { Title = "test", TagId = 1 });

        Assert.That(_logger.LastError, Is.EqualTo("Todo is Created"));
    }

    [Test]
    public async Task Update_WhenDataIsValid_ShowSuccessLog()
    {
        await Application.Update(1, new TodoCommand { Title = "test", TagId = 1 });

        Assert.That(_logger.LastError, Is.EqualTo("Todo is Updated"));
    }

    [Test]
    public async Task Delete_WhenDataIsValid_ShowSuccessLog()
    {
        await Application.Delete(3);

        Assert.That(_logger.LastError, Is.EqualTo("Todo is Deleted"));
    }
}