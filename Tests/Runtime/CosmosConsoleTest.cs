using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CosmosConsoleTest
{
    [Test]
    public void AddCommandTest()
    {
        Assert.DoesNotThrow(() => {
            CosmosConsoleManager.AddCommand("unit_test", testCommand);
            CosmosConsoleManager.AddCommand("unit_test", "unit test description", testCommand);
        });
    }

    [Test]
    public void RemoveCommandTest()
    {
        Assert.DoesNotThrow(() =>
            CosmosConsoleManager.RemoveCommand("help"));
    }

    public void testCommand(string[] args)
    {
        return;
    }
}
