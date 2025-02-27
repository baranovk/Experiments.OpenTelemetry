using Autofac;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Library1;
using Microsoft.Extensions.Logging;
using Moq;

namespace Experiments.OpenTelemetry.Tests.Unit;

internal sealed class AutofacTests
{
    [Test]
    public void Container_Should_ResolveActivityWithParameters()
    {
        var builder = new ContainerBuilder();

        var mockLogger = new Mock<ILogger>();
        var mockScheduler = new Mock<IActivityScheduler>();
        var mockSource = new Mock<IWorkItemSource>();

        builder.RegisterInstance(mockLogger.Object).As<ILogger>();
        builder.RegisterInstance(mockScheduler.Object).As<IActivityScheduler>();
        builder.RegisterInstance(mockSource.Object).As<IWorkItemSource>();
        builder.RegisterType<Library1Activity>();
        var scope = builder.Build();

        var activityUid = "Library_1_Activity";

        var activity = scope.Resolve<Library1Activity>(
            new NamedParameter("uid", activityUid),
            new NamedParameter("workItemBatchUid", Guid.NewGuid())
        );

        Assert.That(activity.Uid, Is.EqualTo(activityUid));
    }
}
