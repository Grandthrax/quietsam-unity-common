using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using QuietSam.Common;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;


public class ThirdPersonPlayerControllerTests : InputTestFixture
{
     [SetUp]
    public override void Setup() => base.Setup();      // important
    [TearDown]
    public override void TearDown() => base.TearDown();// important


    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator MovePlayer_WithWASD_ShouldMovePlayer()
    {
        BuildTestScene();

        var playerController = GameObject.Find("Player").GetComponent<ThirdPerson3dPlayerController>();

        // change input to make sure the player moves
        var keyboard = SetupKeyboard();
        var playerPos = playerController.transform.position;


        //check the play moves forward on w
        // Press 'W'
        Press(keyboard.wKey);


        yield return null;
        Assert.IsTrue(keyboard.wKey.isPressed);
        Release(keyboard.wKey);
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(playerController.transform.position.z > playerPos.z);
        var newPlayerPos = playerController.transform.position;
        yield return new WaitForFixedUpdate();
        Assert.IsTrue(newPlayerPos.z == playerController.transform.position.z);

        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }

    [UnityTest]
    public IEnumerator JumpPlayer_WithSpace_ShouldJumpPlayer()
    {
        BuildTestScene();

        var playerController = GameObject.Find("Player").GetComponent<ThirdPerson3dPlayerController>();
        var keyboard = SetupKeyboard();
        var playerPos = playerController.transform.position;

        //check the player jumps on space
        Press(keyboard.spaceKey);
        yield return null;
        Assert.IsTrue(keyboard.spaceKey.isPressed);
        Release(keyboard.spaceKey);
        yield return null;
        Assert.IsTrue(playerController.transform.position.y > playerPos.y);

    }

    void BuildTestScene()
    {
        //create camera behind the player looking at the player
        var camera = new GameObject("Camera");
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 10, -10);
        camera.transform.LookAt(new Vector3(0, 0, 0));
        //create inputmanager
        var inputManager = new GameObject("InputManager");
        inputManager.AddComponent<InputManager>();
        //put some grund below the player
        var ground = new GameObject("Ground");
        ground.AddComponent<BoxCollider>();
        ground.transform.position = new Vector3(0, -1, 0);
        ground.transform.localScale = new Vector3(100, 0.1f, 100);
        //create player
        var player = new GameObject("Player");
        player.AddComponent<CharacterController>();
        player.AddComponent<ThirdPerson3dPlayerController>();

    }

    Keyboard SetupKeyboard()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();

        return keyboard;
    }
}
