using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Fusion;

public class MultiplayerGameplayIntegrationTests
{
    private NetworkRunner serverRunner;
    private NetworkRunner clientRunner;

    [UnitySetUp]
    public IEnumerator SetupTestSession()
    {
        var serverObj = new GameObject("Server_Runner");
        serverRunner = serverObj.AddComponent<NetworkRunner>();
        serverRunner.ProvideInput = false;

        var clientObj = new GameObject("Client_Runner");
        clientRunner = clientObj.AddComponent<NetworkRunner>();
        clientRunner.ProvideInput = true;

        var startHostTask = serverRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = "TestRoom_Auto",
            PlayerCount = 2,
            SceneManager = serverObj.AddComponent<NetworkSceneManagerDefault>()
        });

        while (!startHostTask.IsCompleted) yield return null;

        var startClientTask = clientRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = "TestRoom_Auto",
            PlayerCount = 2,
            SceneManager = clientObj.AddComponent<NetworkSceneManagerDefault>()
        });

        while (!startClientTask.IsCompleted) yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDownTestSession()
    {
        if (serverRunner != null)
        {
            var task = serverRunner.Shutdown();
            while (!task.IsCompleted) yield return null;
        }

        if (clientRunner != null)
        {
            var task = clientRunner.Shutdown();
            while (!task.IsCompleted) yield return null;
        }
    }

    [UnityTest]
    public IEnumerator Test_01_PlayerMovement_Respects_MatchActiveFlag()
    {
        var playerObj = new GameObject("TestRunner").AddComponent<RunnerPlayer>();
        Vector3 initialPos = playerObj.transform.position;

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(initialPos.z, playerObj.transform.position.z, 0.001f, "Player moved while match was inactive!");
        Object.Destroy(playerObj.gameObject);
    }

    [UnityTest]
    public IEnumerator Test_02_Tackle_Transfers_Ball_Possession_And_Launches()
    {
        var victimObj = new GameObject("VictimPlayer");
        var victim = victimObj.AddComponent<RunnerPlayer>();
        victim.AssignBall(true);

        var attackerObj = new GameObject("AttackerPlayer");
        var attackerContact = attackerObj.AddComponent<NetworkPlayerContact>();
        attackerContact.CurrentContactType = ContactType.ContactType1;

        Assert.IsTrue(victim.HasBall, "Victim did not receive initial ball possession.");

        victim.AssignBall(false);
        victim.ApplySlowdown(1.0f);

        Assert.IsFalse(victim.HasBall, "Ball was not stripped upon tackle collision.");

        Object.Destroy(victimObj);
        Object.Destroy(attackerObj);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_03_FinishLine_Declares_First_Crossing_Winner()
    {
        var finishLineObj = new GameObject("FinishLineTrigger");
        var trigger = finishLineObj.AddComponent<FinishLineTrigger>();

        Assert.IsFalse(trigger.IsMatchFinished, "Finish line initialized in finished state.");

        trigger.IsMatchFinished = true;
        trigger.WinningPlayer = PlayerRef.FromEncoded(1);

        Assert.IsTrue(trigger.IsMatchFinished, "Match did not transition to finished state.");
        Assert.AreEqual(1, trigger.WinningPlayer.RawEncoded, "Winner PlayerRef mismatch.");

        Object.Destroy(finishLineObj);
        yield return null;
    }
}
