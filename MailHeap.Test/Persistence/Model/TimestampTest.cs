using System.Globalization;
using MailHeap.Service.Persistence.Model;

namespace MailHeap.Test.Persistence.Model;

[TestFixture]
[TestOf(typeof(Timestamp))]
public class TimestampTest
{
    [Test]
    public void TestToString()
    {
        var dt = new Timestamp(new DateTime(2024, 7, 11, 19, 51, 33, DateTimeKind.Utc));
        Assert.That(dt.ToString("u", CultureInfo.InvariantCulture), Is.EqualTo("2024-07-11 19:51:33Z"));
    }
}