﻿#nullable disable
using BlobHandles;
using BuildSoft.OscCore;
using BuildSoft.VRChat.Osc.Test;
using NUnit.Framework;

namespace BuildSoft.VRChat.Osc.Input.Test;

[TestOf(typeof(OscInput))]
public class OscInputTests
{
    OscClient _client = null!;
    OscServer _server = null!;

    public static IEnumerable<TestCaseData> AllOscButtonInput
        => Enum.GetValues(typeof(OscButtonInput)).Cast<OscButtonInput>().Select(item => new TestCaseData(item));
    public static IEnumerable<TestCaseData> AllOscAxisInput
        => Enum.GetValues(typeof(OscAxisInput)).Cast<OscAxisInput>().Select(item => new TestCaseData(item));
    public static IEnumerable<TestCaseData> OscAxisInputTestCases
    {
        get
        {
            foreach (var item in Enum.GetValues(typeof(OscAxisInput)))
            {
                yield return new(item, 1f) { ExpectedResult = 1f };
                yield return new(item, 2.5f) { ExpectedResult = 1f };
                yield return new(item, 0f) { ExpectedResult = 0f };
                yield return new(item, 0.25f) { ExpectedResult = 0.25f };
                yield return new(item, -1f) { ExpectedResult = -1f };
                yield return new(item, -1.001f) { ExpectedResult = -1f };
                yield return new(item, -12.34f) { ExpectedResult = -1f };
                yield return new(item, float.MaxValue) { ExpectedResult = 1f };
                yield return new(item, float.MinValue) { ExpectedResult = -1f };
            }
        }
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _client = new OscClient("127.0.0.1", OscUtility.ReceivePort);
        _server = new OscServer(OscUtility.SendPort);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _server.Dispose();
    }

    [SetUp]
    public void Setup()
    {
        OscParameter.Parameters.Clear();
    }

    [TearDown]
    public void TearDown()
    {

    }

    [TestCaseSource(nameof(AllOscButtonInput))]
    public async Task TestSend(OscButtonInput buttonInput)
    {
        OscMessageValues values = null;
        string address = null;

        void Callback(BlobString a, OscMessageValues v)
            => (address, values) = (a.ToString(), v);
        _server.AddMonitorCallback(Callback);

        buttonInput.Send();
        await TestUtility.LoopWhile(() => values == null, TestUtility.LatencyTimeout);
        Assert.AreEqual(1, values.ReadIntElementUnchecked(0));
        Assert.AreEqual(buttonInput.CreateAddress(), address);
        values = null;

        buttonInput.Send(true);
        await TestUtility.LoopWhile(() => values == null, TestUtility.LatencyTimeout);
        Assert.AreEqual(1, values.ReadIntElementUnchecked(0));
        Assert.AreEqual(buttonInput.CreateAddress(), address);
        values = null;

        buttonInput.Send(false);
        await TestUtility.LoopWhile(() => values == null, TestUtility.LatencyTimeout);
        Assert.AreEqual(0, values.ReadIntElementUnchecked(0));
        Assert.AreEqual(buttonInput.CreateAddress(), address);
        values = null;

        _server.RemoveMonitorCallback(Callback);
    }

    [TestCaseSource(nameof(OscAxisInputTestCases))]
    public async Task<float> TestSendA(OscAxisInput axisInput, float value)
    {
        OscMessageValues values = null;
        string address = null;

        void Callback(BlobString a, OscMessageValues v)
            => (address, values) = (a.ToString(), v);

        _server.AddMonitorCallback(Callback);

        axisInput.Send(value);
        await TestUtility.LoopWhile(() => values == null, TestUtility.LatencyTimeout);
        Assert.AreEqual(axisInput.CreateAddress(), address);
        Assert.IsTrue(_server.RemoveMonitorCallback(Callback));

        return values.ReadFloatElement(0);
    }

    [TestCaseSource(nameof(AllOscButtonInput))]
    public async Task TestPressRelease(OscButtonInput buttonInput)
    {
        OscMessageValues values = null;
        string address = null;

        void Callback(BlobString a, OscMessageValues v)
            => (address, values) = (a.ToString(), v);
        _server.AddMonitorCallback(Callback);

        buttonInput.Press();
        await TestUtility.LoopWhile(() => values == null, TestUtility.LatencyTimeout);
        Assert.AreEqual(buttonInput.CreateAddress(), address);
        Assert.AreEqual(1, values.ReadIntElementUnchecked(0));
        values = null;

        buttonInput.Release();
        await TestUtility.LoopWhile(() => values == null, TestUtility.LatencyTimeout);
        Assert.AreEqual(buttonInput.CreateAddress(), address);
        Assert.AreEqual(0, values.ReadIntElementUnchecked(0));
        values = null;

        _server.RemoveMonitorCallback(Callback);
    }

    [TestCaseSource(nameof(AllOscButtonInput))]
    public void TestCreateAddress(OscButtonInput buttonInput)
    {
        Assert.AreEqual("/input/" + buttonInput.ToString(), buttonInput.CreateAddress());
        Assert.AreEqual("/input/" + buttonInput.ToString(), buttonInput.CreateAddress());
    }

    [TestCaseSource(nameof(AllOscAxisInput))]
    public void TestCreateAddress(OscAxisInput axisInput)
    {
        Assert.AreEqual("/input/" + axisInput.ToString(), axisInput.CreateAddress());
        Assert.AreEqual("/input/" + axisInput.ToString(), axisInput.CreateAddress());
    }
}
